// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Channel.Util;
using Aura.Channel.World.Quests;
using Aura.Channel.Network.Sending;
using Aura.Shared.Util;
using Aura.Shared.Mabi.Const;

namespace Aura.Channel.World.Entities.Creatures
{
	public class CreatureQuests
	{
		private Creature _creature;

		private Dictionary<int, Quest> _quests;

		public CreatureQuests(Creature creature)
		{
			_creature = creature;
			_quests = new Dictionary<int, Quest>();
		}

		/// <summary>
		/// Adds quest.
		/// </summary>
		/// <param name="quest"></param>
		public void Add(Quest quest)
		{
			lock (_quests)
				_quests[quest.Id] = quest;
		}

		/// <summary>
		/// Returns true if creature has quest (completed or not).
		/// </summary>
		/// <param name="questId"></param>
		/// <returns></returns>
		public bool Has(int questId)
		{
			lock (_quests)
				return _quests.ContainsKey(questId);
		}

		/// <summary>
		/// Returns quest or null.
		/// </summary>
		/// <param name="questId"></param>
		/// <returns></returns>
		public Quest Get(int questId)
		{
			Quest result;
			lock (_quests)
				_quests.TryGetValue(questId, out result);
			return result;
		}

		/// <summary>
		/// Returns quest or null.
		/// </summary>
		/// <param name="uniqueId"></param>
		/// <returns></returns>
		public Quest Get(long uniqueId)
		{
			lock (_quests)
				return _quests.Values.FirstOrDefault(a => a.UniqueId == uniqueId);
		}

		/// <summary>
		/// Calls <see cref="Get(long)"/>. If the result is null, throws <see cref="SevereViolation"/>.
		/// </summary>
		/// <param name="uniqueId"></param>
		/// <returns></returns>
		public Quest GetSafe(long uniqueId)
		{
			var q = this.Get(uniqueId);

			if (q == null)
				throw new SevereViolation("Creature does not have quest 0x{0:X}", uniqueId);

			return q;
		}

		/// <summary>
		/// Returns true if quest is complete.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public bool IsComplete(int id)
		{
			var quest = this.Get(id);
			return (quest != null && quest.State == QuestState.Complete);
		}

		/// <summary>
		/// Returns new list of quests.
		/// </summary>
		/// <returns></returns>
		public ICollection<Quest> GetList()
		{
			return _quests.Values.ToArray();
		}

		/// <summary>
		/// Returns new list of incomplete quests.
		/// </summary>
		/// <returns></returns>
		public ICollection<Quest> GetIncompleteList()
		{
			return _quests.Values.Where(a => a.State != QuestState.Complete).ToArray();
		}

		/// <summary>
		/// Starts quest
		/// </summary>
		/// <param name="questId"></param>
		public void Start(int questId)
		{
			// Remove quest if it's aleady there and not completed,
			// or it will be shown twice till next relog.
			var existingQuest = this.Get(questId);
			if (existingQuest != null && existingQuest.State < QuestState.Complete)
				this.GiveUp(existingQuest);

			// Check for quest script
			var questScript = ChannelServer.Instance.ScriptManager.GetQuestScript(questId);
			if (questScript == null)
				throw new Exception("Quest '" + questId.ToString() + "' does not exist.");

			var quest = new Quest(questId);
			this.Add(quest);

			// Owl
			Send.QuestOwlNew(_creature, quest.UniqueId);

			// Quest item (required to complete quests)
			_creature.Inventory.Add(quest.QuestItem, Pocket.Quests);

			// Quest info
			Send.NewQuest(_creature, quest);
		}

		/// <summary>
		/// Finishes objective for quest, returns false if quest doesn't exist
		/// or doesn't have the objective.
		/// </summary>
		/// <param name="questId"></param>
		/// <param name="objective"></param>
		public bool Finish(int questId, string objective)
		{
			var quest = this.Get(questId);
			if (quest == null) return false;

			var progress = quest.GetProgress(objective);
			if (progress == null)
				throw new Exception("Quest.Finish: No progress found for objective '" + objective + "'.");

			quest.SetDone(objective);

			Send.QuestUpdate(_creature, quest);

			return true;
		}

		/// <summary>
		/// Completes and removes quest, if it exists.
		/// </summary>
		/// <param name="questId"></param>
		public bool Complete(int questId)
		{
			var quest = this.Get(questId);
			if (quest == null) return false;

			return this.Complete(quest);
		}

		/// <summary>
		/// Completes and removes quest, if it exists.
		/// </summary>
		/// <param name="quest"></param>
		public bool Complete(Quest quest)
		{
			var success = this.Complete(quest, true);
			if (success)
			{
				quest.State = QuestState.Complete;

				ChannelServer.Instance.Events.OnPlayerCompletesQuest(_creature, quest.Id);
			}
			return success;
		}

		/// <summary>
		/// Completes and removes quest without rewards, if it exists.
		/// </summary>
		/// <param name="quest"></param>
		/// <returns></returns>
		public bool GiveUp(Quest quest)
		{
			var success = this.Complete(quest, false);
			if (success)
				lock (_quests)
					_quests.Remove(quest.Id);
			return success;
		}

		/// <summary>
		/// Completes and removes quest, if it exists.
		/// </summary>
		/// <param name="quest"></param>
		/// <param name="rewards">Shall rewards be given?</param>
		private bool Complete(Quest quest, bool rewards)
		{
			if (!_quests.ContainsValue(quest))
				return false;

			if (rewards)
			{
				// Owl
				Send.QuestOwlComplete(_creature, quest.UniqueId);

				// Rewards
				foreach (var reward in quest.Data.Rewards)
				{
					try
					{
						reward.Reward(_creature, quest);
					}
					catch (NotImplementedException)
					{
						Log.Unimplemented("Quest.Complete: Reward '{0}'.", reward.Type);
					}
				}
			}

			_creature.Inventory.Remove(quest.QuestItem);

			// Remove from quest log.
			Send.QuestClear(_creature, quest.UniqueId);

			return true;
		}

		/// <summary>
		/// Returns true if the quest is in progress.
		/// </summary>
		/// <param name="questId"></param>
		/// <param name="objective"></param>
		/// <returns></returns>
		public bool IsActive(int questId, string objective = null)
		{
			var quest = this.Get(questId);
			if (quest == null) return false;

			var current = quest.CurrentObjective;
			if (current == null) return false;

			if (objective != null && current.Ident != objective)
				return false;

			return (quest.State == QuestState.InProgress);
		}
	}
}
