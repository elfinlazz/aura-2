// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using Aura.Shared.Mabi.Const;
using Aura.Shared.Util;
using Aura.Channel.Scripting.Scripts;
using Aura.Channel.World.Entities;

namespace Aura.Channel.World.Quests
{
	public class Quest
	{
		private static long _questId = MabiId.QuestsTmp;

		private OrderedDictionary<string, QuestObjectiveProgress> _progresses;

		/// <summary>
		/// Unique id of this quests.
		/// </summary>
		public long UniqueId { get; set; }

		/// <summary>
		/// General id to identify this quests.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// State the quest is in.
		/// </summary>
		public QuestState State { get; set; }

		/// <summary>
		/// Returns quest script
		/// </summary>
		public QuestScript Data { get; protected set; }

		/// <summary>
		/// Returns true if all objectives are done.
		/// </summary>
		public bool IsDone
		{
			get
			{
				return _progresses.Values.All(progress => progress.Done);
			}
		}

		/// <summary>
		/// Returns progress for current objective or null,
		/// if all objectives are done.
		/// </summary>
		public QuestObjectiveProgress CurrentObjective
		{
			get
			{
				return _progresses.Values.FirstOrDefault(progress => !progress.Done);
			}
		}

		/// <summary>
		/// Returns progress for current objective or last one,
		/// if all objectives are done.
		/// </summary>
		public QuestObjectiveProgress CurrentObjectiveOrLast
		{
			get
			{
				foreach (var progress in _progresses.Values)
					if (!progress.Done)
						return progress;
				return _progresses[_progresses.Count - 1];
			}
		}

		/// <summary>
		/// Holds quest item, after it was generated.
		/// </summary>
		public Item QuestItem { get; set; }

		public Quest(int questId, long uniqueId, QuestState state)
		{
			this.Id = questId;
			this.UniqueId = uniqueId;
			this.State = state;

			this.Data = ChannelServer.Instance.ScriptManager.GetQuestScript(this.Id);
			if (this.Data == null)
				throw new Exception("Quest '" + questId.ToString() + "' does not exist.");

			_progresses = new OrderedDictionary<string, QuestObjectiveProgress>();
			foreach (var objective in this.Data.Objectives)
				_progresses[objective.Key] = new QuestObjectiveProgress(objective.Key);
			_progresses[0].Unlocked = true;
		}

		public Quest(int questId)
			: this(questId, Interlocked.Increment(ref _questId), QuestState.InProgress)
		{
			this.GenerateQuestItem();
		}

		/// <summary>
		/// Returns progress for objective.
		/// </summary>
		/// <param name="objective"></param>
		/// <returns></returns>
		public QuestObjectiveProgress GetProgress(string objective)
		{
			QuestObjectiveProgress result;
			_progresses.TryGetValue(objective, out result);
			return result;
		}

		/// <summary>
		/// Returns list of all objective progresses.
		/// </summary>
		/// <remarks>
		/// Values in our generic OrderedDictioanry aleady creates a copy.
		/// </remarks>
		/// <returns></returns>
		public ICollection<QuestObjectiveProgress> GetList()
		{
			return _progresses.Values;
		}

		/// <summary>
		/// Sets objective done and unlocks the next one.
		/// </summary>
		/// <param name="objective"></param>
		public void SetDone(string objective)
		{
			if (!_progresses.ContainsKey(objective))
				throw new Exception("SetDone: No progress found for objective '" + objective + "'.");

			for (int i = 0; i < _progresses.Count; ++i)
			{
				_progresses[i].Done = true;
				_progresses[i].Unlocked = false;
				if (_progresses[i].Count < this.Data.Objectives[i].Amount)
					_progresses[i].Count = this.Data.Objectives[i].Amount;

				if (_progresses[i].Ident != objective)
					continue;

				if (i + 1 < _progresses.Count)
					_progresses[i + 1].Unlocked = true;
				break;
			}
		}

		/// <summary>
		/// Sets objective undone.
		/// </summary>
		/// <param name="objective"></param>
		public void SetUndone(string objective)
		{
			if (!_progresses.ContainsKey(objective))
				throw new Exception("SetUndone: No progress found for objective '" + objective + "'.");

			for (int i = _progresses.Count - 1; i >= 0; ++i)
			{
				_progresses[i].Done = false;
				_progresses[i].Unlocked = (_progresses[i].Ident == objective);
				_progresses[i].Count = 0;

				if (_progresses[i].Unlocked)
					break;
			}
		}

		/// <summary>
		/// Generates, caches, and returns new quest item.
		/// </summary>
		/// <remarks>
		/// Officialy quest item ids are always
		/// unique quest id - offset, that doesn't work
		/// with our auto-incrementing table though,
		/// so we use normal ids. Seems to work fine.
		/// </remarks>
		private Item GenerateQuestItem(int itemId = 70024)
		{
			this.QuestItem = new Item(itemId);
			//this.QuestItem.EntityId = (this.UniqueId - MabiId.QuestItemOffset);
			this.QuestItem.QuestId = this.UniqueId;
			this.QuestItem.MetaData1.Parse(this.Data.MetaData.ToString());

			return this.QuestItem;
		}
	}

	public class QuestObjectiveProgress
	{
		public string Ident { get; set; }
		public int Count { get; set; }
		public bool Done { get; set; }
		public bool Unlocked { get; set; }

		public QuestObjectiveProgress(string objective)
		{
			this.Ident = objective;
		}
	}

	/// <summary>
	/// State of a quest
	/// </summary>
	public enum QuestState { InProgress, Complete }
}
