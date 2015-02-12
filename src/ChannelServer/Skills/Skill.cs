// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Channel.Network.Sending;
using Aura.Channel.World.Entities;
using Aura.Data;
using Aura.Data.Database;
using Aura.Shared.Mabi.Const;
using Aura.Shared.Mabi.Structs;
using Aura.Shared.Util;
using Aura.Channel.Util.Configuration.Files;
using System.Globalization;

namespace Aura.Channel.Skills
{
	public class Skill
	{
		private Creature _creature;
		private int _race;

		/// <summary>
		/// Information about the skill, serialized to packets.
		/// </summary>
		public SkillInfo Info;

		/// <summary>
		/// Data about the skill, loaded from the db.
		/// </summary>
		public SkillData Data { get; protected set; }

		/// <summary>
		/// Data about the skill's current rank, loaded from the db.
		/// </summary>
		public SkillRankData RankData { get; protected set; }

		/// <summary>
		/// The skills current state.
		/// </summary>
		public SkillState State { get; set; }

		/// <summary>
		/// Holds time at which the skill is fully loaded.
		/// </summary>
		public DateTime CastEnd { get; set; }

		private int _stack = 0;
		/// <summary>
		/// Loaded stack count.
		/// </summary>
		public int Stacks
		{
			get { return _stack; }
			set
			{
				_stack = Math2.Clamp(0, sbyte.MaxValue, value);
				Send.SkillStackSet(_creature, this.Info.Id, _stack);
			}
		}

		/// <summary>
		/// Returns true if skill has enough experience and is below max rank.
		/// </summary>
		public bool IsRankable { get { return (this.Info.Experience >= 100000 && this.Info.Rank < this.Info.MaxRank); } }

		/// <summary>
		/// New Skill.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="id"></param>
		/// <param name="rank"></param>
		/// <param name="race"></param>
		public Skill(Creature creature, SkillId id, SkillRank rank, int race)
		{
			_creature = creature;
			_race = race;

			this.Info.Id = id;
			this.Info.Rank = rank;
			this.Info.MaxRank = rank;

			this.Info.Flag = SkillFlags.Shown;

			// The conditions are set to the max and are reduced afterwards,
			// making them "Complete" once they reach 0. Initializing to 1
			// in case of problems.
			this.Info.ConditionCount1 = 1;
			this.Info.ConditionCount2 = 1;
			this.Info.ConditionCount3 = 1;
			this.Info.ConditionCount4 = 1;
			this.Info.ConditionCount5 = 1;
			this.Info.ConditionCount6 = 1;
			this.Info.ConditionCount7 = 1;
			this.Info.ConditionCount8 = 1;
			this.Info.ConditionCount9 = 1;

			this.LoadRankData();
		}

		/// <summary>
		/// Loads rank data, based on current rank.
		/// </summary>
		public void LoadRankData()
		{
			this.Data = AuraData.SkillDb.Find((int)this.Info.Id);
			if (this.Data == null)
				throw new Exception("Skill.LoadRankData: Skill data not found for '" + this.Info.Id.ToString() + "'.");

			if ((this.RankData = this.Data.GetRankData((byte)this.Info.Rank, _race)) == null)
			{
				if ((this.RankData = this.Data.GetFirstRankData(_race)) == null)
					throw new Exception("Skill.LoadRankData: No rank data found for '" + this.Info.Id.ToString() + "@" + this.Info.Rank.ToString() + "'.");

				Log.Warning("Skill.LoadRankData: Missing rank data for '{0},{1}', using '{2}' instead.", this.Info.Id, this.Info.Rank, (SkillRank)this.RankData.Rank);
			}

			this.Info.MaxRank = (SkillRank)this.Data.MaxRank;

			this.Info.ConditionCount1 = (short)this.RankData.Conditions[0].Count;
			this.Info.ConditionCount2 = (short)this.RankData.Conditions[1].Count;
			this.Info.ConditionCount3 = (short)this.RankData.Conditions[2].Count;
			this.Info.ConditionCount4 = (short)this.RankData.Conditions[3].Count;
			this.Info.ConditionCount5 = (short)this.RankData.Conditions[4].Count;
			this.Info.ConditionCount6 = (short)this.RankData.Conditions[5].Count;
			this.Info.ConditionCount7 = (short)this.RankData.Conditions[6].Count;
			this.Info.ConditionCount8 = (short)this.RankData.Conditions[7].Count;
			this.Info.ConditionCount9 = (short)this.RankData.Conditions[8].Count;

			if (this.RankData.Conditions[0].Visible) this.Info.Flag |= SkillFlags.ShowCondition1;
			if (this.RankData.Conditions[1].Visible) this.Info.Flag |= SkillFlags.ShowCondition2;
			if (this.RankData.Conditions[2].Visible) this.Info.Flag |= SkillFlags.ShowCondition3;
			if (this.RankData.Conditions[3].Visible) this.Info.Flag |= SkillFlags.ShowCondition4;
			if (this.RankData.Conditions[4].Visible) this.Info.Flag |= SkillFlags.ShowCondition5;
			if (this.RankData.Conditions[5].Visible) this.Info.Flag |= SkillFlags.ShowCondition6;
			if (this.RankData.Conditions[6].Visible) this.Info.Flag |= SkillFlags.ShowCondition7;
			if (this.RankData.Conditions[7].Visible) this.Info.Flag |= SkillFlags.ShowCondition8;
			if (this.RankData.Conditions[8].Visible) this.Info.Flag |= SkillFlags.ShowCondition9;
		}

		/// <summary>
		/// Changes rank, resets experience, loads rank data.
		/// </summary>
		/// <param name="rank"></param>
		public void ChangeRank(SkillRank rank)
		{
			this.Info.Rank = rank;
			this.Info.Experience = 0;
			this.Info.Flag &= ~SkillFlags.Rankable;
			this.LoadRankData();
		}

		/// <summary>
		/// Increases training condition count.
		/// </summary>
		/// <param name="condition"></param>
		/// <param name="amount"></param>
		public void Train(int condition, int amount = 1)
		{
			// Only characters can train skills.
			if (!_creature.IsCharacter)
				return;

			var bonus = "";

			// Apply skill exp multiplier
			if (ChannelServer.Instance.Conf.World.SkillExpRate != 1)
			{
				amount = (int)(amount * ChannelServer.Instance.Conf.World.SkillExpRate);
				bonus = string.Format(Localization.Get(" (Skill Exp Rate Bonus: x{0})"), ChannelServer.Instance.Conf.World.SkillExpRate.ToString(CultureInfo.InvariantCulture));
			}

			// Change count and reveal the condition
			if (amount > 0)
			{
				switch (condition)
				{
					case 1: this.Info.ConditionCount1 = (short)Math.Max(0, this.Info.ConditionCount1 - amount); this.Info.Flag |= SkillFlags.ShowCondition1; break;
					case 2: this.Info.ConditionCount2 = (short)Math.Max(0, this.Info.ConditionCount2 - amount); this.Info.Flag |= SkillFlags.ShowCondition2; break;
					case 3: this.Info.ConditionCount3 = (short)Math.Max(0, this.Info.ConditionCount3 - amount); this.Info.Flag |= SkillFlags.ShowCondition3; break;
					case 4: this.Info.ConditionCount4 = (short)Math.Max(0, this.Info.ConditionCount4 - amount); this.Info.Flag |= SkillFlags.ShowCondition4; break;
					case 5: this.Info.ConditionCount5 = (short)Math.Max(0, this.Info.ConditionCount5 - amount); this.Info.Flag |= SkillFlags.ShowCondition5; break;
					case 6: this.Info.ConditionCount6 = (short)Math.Max(0, this.Info.ConditionCount6 - amount); this.Info.Flag |= SkillFlags.ShowCondition6; break;
					case 7: this.Info.ConditionCount7 = (short)Math.Max(0, this.Info.ConditionCount7 - amount); this.Info.Flag |= SkillFlags.ShowCondition7; break;
					case 8: this.Info.ConditionCount8 = (short)Math.Max(0, this.Info.ConditionCount8 - amount); this.Info.Flag |= SkillFlags.ShowCondition8; break;
					case 9: this.Info.ConditionCount9 = (short)Math.Max(0, this.Info.ConditionCount9 - amount); this.Info.Flag |= SkillFlags.ShowCondition9; break;
					default:
						Log.Error("Skill.Train: Unknown training condition ({0})", condition);
						break;
				}
			}

			var exp = this.UpdateExperience();
			if (exp > 0)
				Send.SkillTrainingUp(_creature, this, exp, bonus);
		}

		/// <summary>
		/// Updates exp and returns gained amount.
		/// </summary>
		/// <returns></returns>
		public float UpdateExperience()
		{
			var result = this.Info.Experience / 1000f;
			var exp = 0f;
			exp += ((this.RankData.Conditions[0].Count - this.Info.ConditionCount1) * this.RankData.Conditions[0].Exp);
			exp += ((this.RankData.Conditions[1].Count - this.Info.ConditionCount2) * this.RankData.Conditions[1].Exp);
			exp += ((this.RankData.Conditions[2].Count - this.Info.ConditionCount3) * this.RankData.Conditions[2].Exp);
			exp += ((this.RankData.Conditions[3].Count - this.Info.ConditionCount4) * this.RankData.Conditions[3].Exp);
			exp += ((this.RankData.Conditions[4].Count - this.Info.ConditionCount5) * this.RankData.Conditions[4].Exp);
			exp += ((this.RankData.Conditions[5].Count - this.Info.ConditionCount6) * this.RankData.Conditions[5].Exp);
			exp += ((this.RankData.Conditions[6].Count - this.Info.ConditionCount7) * this.RankData.Conditions[6].Exp);
			exp += ((this.RankData.Conditions[7].Count - this.Info.ConditionCount8) * this.RankData.Conditions[7].Exp);
			exp += ((this.RankData.Conditions[8].Count - this.Info.ConditionCount9) * this.RankData.Conditions[8].Exp);
			this.Info.Experience = (int)(exp * 1000);

			if (this.IsRankable)
				this.Info.Flag |= SkillFlags.Rankable;

			return (exp - result);
		}

		/// <summary>
		/// Activates given flag(s).
		/// </summary>
		/// <param name="flags"></param>
		public void Activate(SkillFlags flags)
		{
			this.Info.Flag |= flags;
		}

		/// <summary>
		/// Deativates given flag(s).
		/// </summary>
		/// <param name="flags"></param>
		public void Deactivate(SkillFlags flags)
		{
			this.Info.Flag &= ~flags;
		}

		/// <summary>
		/// Returns true if skill has the given flags.
		/// </summary>
		/// <param name="flags"></param>
		public bool Has(SkillFlags flags)
		{
			return ((this.Info.Flag & flags) != 0);
		}

		/// <summary>
		/// Returns cast time of skill, specific for its creature.
		/// </summary>
		/// <returns></returns>
		public int GetCastTime()
		{
			var dynamic = (ChannelServer.Instance.Conf.World.CombatSystem == CombatSystem.Dynamic);

			// Characters/Dynamic
			if (_creature.IsCharacter && dynamic)
				return this.RankData.NewLoadTime;

			// Monsters/Pets
			return this.RankData.LoadTime;
		}
	}

	/// <summary>
	/// Current state of a skill.
	/// </summary>
	public enum SkillState
	{
		None,
		Prepared,
		Ready,
		Used,
		Completed,
		Canceled,
	}
}
