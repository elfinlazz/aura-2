// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Collections.Generic;
using System.Linq;

namespace Aura.Data.Database
{
	public class SkillData
	{
		public ushort Id { get; internal set; }
		public string Name { get; internal set; }
		public ushort MasterTitle { get; internal set; }
		public byte MaxRank { get; internal set; }

		public List<SkillRankData> RankData { get; internal set; }

		public SkillData()
		{
			this.RankData = new List<SkillRankData>();
		}

		public SkillRankData GetRankData(byte level, int race)
		{
			race = race & ~3;

			SkillRankData info;
			if ((info = this.RankData.FirstOrDefault(a => a.Rank == level && a.Race == race)) == null)
			{
				if ((info = this.RankData.FirstOrDefault(a => a.Rank == level && a.Race == 0)) == null)
				{
					return null;
				}
			}

			return info;
		}
	}

	/// <summary>
	/// Indexed by skill id.
	/// Depends on: SkillRankDb
	/// </summary>
	public class SkillDb : DatabaseCSVIndexed<int, SkillData>
	{
		public List<SkillData> FindAll(string name)
		{
			name = name.ToLower();
			return this.Entries.FindAll(a => a.Value.Name.ToLower().Contains(name));
		}

		[MinFieldCount(3)]
		protected override void ReadEntry(CSVEntry entry)
		{
			var info = new SkillData();
			info.Id = entry.ReadUShort();
			info.Name = entry.ReadString();
			info.MasterTitle = entry.ReadUShort();

			info.RankData.AddRange(AuraData.SkillRankDb.Entries.FindAll(a => a.SkillId == info.Id).OrderBy(a => a.Rank));
			if (info.RankData.Count > 0)
				info.MaxRank = info.RankData.Last().Rank;

			this.Entries[info.Id] = info;
		}
	}
}
