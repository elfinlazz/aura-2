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

		private Dictionary<PtjType, PtjTrackRecord> _ptjRecords;

		public CreatureQuests(Creature creature)
		{
			_creature = creature;
			_quests = new Dictionary<int, Quest>();
			_ptjRecords = new Dictionary<PtjType, PtjTrackRecord>();
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
		/// Returns quest or null.
		/// </summary>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public Quest Get(Func<Quest, bool> predicate)
		{
			lock (_quests)
				return _quests.Values.FirstOrDefault(predicate);
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
			lock (_quests)
				return _quests.Values.ToArray();
		}

		/// <summary>
		/// Returns new list of incomplete quests.
		/// </summary>
		/// <returns></returns>
		public ICollection<Quest> GetIncompleteList()
		{
			lock (_quests)
				return _quests.Values.Where(a => a.State != QuestState.Complete).ToArray();
		}

		/// <summary>
		/// Starts quest
		/// </summary>
		/// <param name="questId"></param>
		public void Start(int questId, bool owl)
		{
			// Remove quest if it's aleady there and not completed,
			// or it will be shown twice till next relog.
			var existingQuest = this.Get(questId);
			if (existingQuest != null && existingQuest.State < QuestState.Complete)
				this.GiveUp(existingQuest);

			var quest = new Quest(questId);
			this.Start(quest, owl);
		}

		/// <summary>
		/// Starts quest, sending it to the client and adding the quest item
		/// to the creature's inventory.
		/// </summary>
		/// <param name="quest"></param>
		public void Start(Quest quest, bool owl)
		{
			this.Add(quest);

			// Owl
			if (owl)
				Send.QuestOwlNew(_creature, quest.UniqueId);

			// Quest item (required to complete quests)
			_creature.Inventory.Add(quest.QuestItem, Pocket.Quests);

			// Quest info
			Send.NewQuest(_creature, quest);

			// Start PTJ clock
			if (quest.Data.Type == QuestType.Deliver)
				Send.QuestStartPtj(_creature, quest.UniqueId);

			// Initial objective check, for things like collect and reach rank,
			// that may be done already.
			quest.Data.CheckCurrentObjective(_creature);
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
		public bool Complete(int questId, bool owl)
		{
			var quest = this.Get(questId);
			if (quest == null) return false;

			return this.Complete(quest, owl);
		}

		/// <summary>
		/// Completes and removes quest, if it exists.
		/// </summary>
		/// <param name="quest"></param>
		public bool Complete(Quest quest, bool owl)
		{
			return this.Complete(quest, 0, owl);
		}

		/// <summary>
		/// Completes and removes quest, if it exists.
		/// </summary>
		/// <param name="quest"></param>
		public bool Complete(Quest quest, int rewardGroup, bool owl)
		{
			var success = this.EndQuest(quest, rewardGroup, owl);
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
			var success = this.EndQuest(quest, -1, false);
			if (success)
				lock (_quests)
					_quests.Remove(quest.Id);

			return success;
		}

		/// <summary>
		/// Completes and removes quest, if it exists, giving the rewards
		/// in the process, if warranted.
		/// </summary>
		/// <param name="quest"></param>
		/// <param name="rewardGroup">Reward group to use, set to -1 for no rewards.</param>
		/// <param name="owl">Show owl delivering the rewards?</param>
		/// <returns></returns>
		private bool EndQuest(Quest quest, int rewardGroup, bool owl)
		{
			if (!_quests.ContainsValue(quest))
				return false;

			var result = quest.GetResult();

			// Increase PTJ done/success
			if (quest.Data.Type == QuestType.Deliver)
				this.ModifyPtjTrackRecord(quest.Data.PtjType, +1, (result == QuestResult.Perfect ? +1 : 0));

			// Rewards
			if (rewardGroup != -1)
			{
				var rewards = quest.Data.GetRewards(rewardGroup, result);
				if (rewards.Count == 0)
					Log.Warning("CreatureQuests.EndQuest: No rewards for group '{0}' at result '{1}' in quest '{2}'.", rewardGroup, result, quest.Id);
				else
					this.GiveRewards(quest, rewards, owl);
			}

			_creature.Inventory.Remove(quest.QuestItem);

			// Remove from quest log.
			Send.QuestClear(_creature, quest.UniqueId);

			// Update PTJ stuff and stop clock
			if (quest.Data.Type == QuestType.Deliver)
			{
				var record = this.GetPtjTrackRecord(quest.Data.PtjType);

				Send.QuestUpdatePtj(_creature, quest.Data.PtjType, record.Done, record.Success);
				Send.QuestEndPtj(_creature);
			}

			return true;
		}

		private void GiveRewards(Quest quest, ICollection<QuestReward> rewards, bool owl)
		{
			if (rewards.Count == 0)
				return;

			if (owl)
				Send.QuestOwlComplete(_creature, quest.UniqueId);

			foreach (var reward in rewards)
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

		/// <summary>
		/// Modifies track record, changing success, done, and last change.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="done"></param>
		/// <param name="success"></param>
		public void ModifyPtjTrackRecord(PtjType type, int done, int success)
		{
			var record = this.GetPtjTrackRecord(type);

			record.Done += done;
			record.Success += success;
			record.LastChange = DateTime.Now;
		}

		/// <summary>
		/// Returns new list of all track records.
		/// </summary>
		/// <returns></returns>
		public PtjTrackRecord[] GetPtjTrackRecords()
		{
			lock (_ptjRecords)
				return _ptjRecords.Values.ToArray();
		}

		/// <summary>
		/// Returns track record for type.
		/// </summary>
		/// <returns></returns>
		public PtjTrackRecord GetPtjTrackRecord(PtjType type)
		{
			PtjTrackRecord record;
			lock (_ptjRecords)
				if (!_ptjRecords.TryGetValue(type, out record))
					_ptjRecords[type] = (record = new PtjTrackRecord(type, 0, 0, DateTime.MinValue));

			return record;
		}

		/// <summary>
		/// Returns current PTJ quest or null.
		/// </summary>
		/// <returns></returns>
		public Quest GetPtjQuest()
		{
			return this.Get(a => a.Data.Type == QuestType.Deliver && a.State != QuestState.Complete);
		}
	}
}
