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

		public Dictionary<int, Dictionary<int, SkillRankData>> RankData { get; internal set; }

		public SkillRankData GetRankData(int rank, int raceId)
		{
			if (this.RankData == null)
				return null;

			raceId = raceId & ~3;

			// Check race specific first, fall back to default (0).
			var skill = this.RankData.GetValueOrDefault(raceId);
			if (skill == null)
				if ((skill = this.RankData.GetValueOrDefault(0)) == null)
					return null;

			return skill.GetValueOrDefault(rank);
		}

		public SkillRankData GetFirstRankData(int raceId)
		{
			if (this.RankData == null)
				return null;

			raceId = raceId & ~3;

			// Check race specific first, fall back to default (0).
			var skill = this.RankData.GetValueOrDefault(raceId);
			if (skill == null)
				if ((skill = this.RankData.GetValueOrDefault(0)) == null)
					return null;

			return skill.Values.First();
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

			info.RankData = AuraData.SkillRankDb.Find(info.Id);
			if (info.RankData != null && info.RankData.Count > 0)
			{
				var rankList = info.RankData.Values.First();
				info.MaxRank = rankList.Values.Last().Rank;
			}

			if (info.RankData == null)
				throw new DatabaseWarningException("No rank data found for skill '{0}'.", info.Id);

			this.Entries[info.Id] = info;
		}
	}
}
