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
		public SkillRankData RankData { get; protected set; }

		public bool IsRankable { get { return this.Info.Experience >= 100000; } }

		public Skill(SkillId id, SkillRank rank, int race)
		{
			this.Info.Id = id;
			this.Info.Rank = rank;
			this.Info.MaxRank = rank;
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
			var skillData = AuraData.SkillDb.Find((int)this.Info.Id);
			if (skillData == null)
				throw new Exception("Skill.LoadRankData: Skill data not found for '" + this.Info.Id.ToString() + "'.");

			if ((this.RankData = skillData.GetRankData((byte)this.Info.Rank, _race)) == null)
			{
				if ((this.RankData = skillData.GetFirstRankData(_race)) == null)
					throw new Exception("Skill.LoadRankData: No rank data found for '" + this.Info.Id.ToString() + "@" + this.Info.Rank.ToString() + "'.");

				Log.Warning("Skill.LoadRankData: Missing rank data for '{0},{1}', using '{2}' instead.", this.Info.Id, this.Info.Rank, (SkillRank)this.RankData.Rank);
			}

			this.Info.MaxRank = (SkillRank)skillData.MaxRank;
		}

		/// <summary>
		/// Changes rank, resets experience, loads rank data.
		/// </summary>
		/// <param name="rank"></param>
		public void ChangeRank(SkillRank rank)
		{
			this.Info.Rank = rank;
			this.Info.Experience = 0;
			this.LoadRankData();
		}
	}
}
