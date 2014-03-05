// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Collections.Generic;

namespace Aura.Data.Database
{
	public class RaceSkillData
	{
		public int RaceId { get; internal set; }
		public ushort SkillId { get; internal set; }
		public byte Rank { get; internal set; }
	}

	public class RaceSkillDb : DatabaseCSVIndexed<int, List<RaceSkillData>>
	{
		public List<RaceSkillData> FindAll(int raceId)
		{
			if (!this.Entries.ContainsKey(raceId))
				return new List<RaceSkillData>();
			return this.Entries[raceId];
		}

		[MinFieldCount(3)]
		protected override void ReadEntry(CSVEntry entry)
		{
			var info = new RaceSkillData();
			info.RaceId = entry.ReadInt();
			info.SkillId = entry.ReadUShort();
			info.Rank = (byte)(16 - entry.ReadByteHex());

			if (!this.Entries.ContainsKey(info.RaceId))
				this.Entries[info.RaceId] = new List<RaceSkillData>();

			this.Entries[info.RaceId].Add(info);
		}
	}
}
