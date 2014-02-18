// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Shared.Mabi.Structs;
using Aura.Data.Database;
using Aura.Shared.Mabi.Const;
using Aura.Data;
using Aura.Shared.Util;

namespace Aura.Channel.Skills
{
	public class Skill
	{
		private int _race;

		public SkillInfo Info;
		public SkillData SkillData { get; protected set; }
		public SkillRankData RankData { get; protected set; }

		/// <summary>
		/// Returns true if skill has enough experience and is below max rank.
		/// </summary>
		public bool IsRankable { get { return (this.Info.Experience >= 100000 && this.Info.Rank < this.Info.MaxRank); } }

		public Skill(SkillId id, SkillRank rank, int race, int exp = 0)
		{
			this.Info.Id = id;
			this.Info.Rank = rank;
			this.Info.MaxRank = rank;
			this.Info.Experience = exp;
			_race = race;

			this.Info.Flag = SkillFlags.ShowAllConditions | SkillFlags.Shown;

			// The conditions are set to the max and are reduced afterwards,
			// making them "Complete" once they reach 0. Setting all to 1
			// to prevent "Perfect Training" spam, when adding exp.
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
			this.SkillData = AuraData.SkillDb.Find((int)this.Info.Id);
			if (this.SkillData == null)
				throw new Exception("Skill.LoadRankData: Skill data not found for '" + this.Info.Id.ToString() + "'.");

			if ((this.RankData = this.SkillData.GetRankData((byte)this.Info.Rank, _race)) == null)
			{
				if ((this.RankData = this.SkillData.GetFirstRankData(_race)) == null)
					throw new Exception("Skill.LoadRankData: No rank data found for '" + this.Info.Id.ToString() + "@" + this.Info.Rank.ToString() + "'.");

				Log.Warning("Skill.LoadRankData: Missing rank data for '{0},{1}', using '{2}' instead.", this.Info.Id, this.Info.Rank, (SkillRank)this.RankData.Rank);
			}

			this.Info.MaxRank = (SkillRank)this.SkillData.MaxRank;

			if (this.IsRankable)
				this.Info.Flag |= SkillFlags.Rankable;
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
	}
}
