// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Aura.Data.Database
{
	[Serializable]
	public class RaceSkillData
	{
		public int RaceId { get; set; }
		public ushort SkillId { get; set; }
		public byte Rank { get; set; }
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

			var rank = entry.ReadString();
			if (rank == "N") info.Rank = 0;
			else info.Rank = (byte)(16 - int.Parse(rank, NumberStyles.HexNumber));

			if (!this.Entries.ContainsKey(info.RaceId))
				this.Entries[info.RaceId] = new List<RaceSkillData>();

			this.Entries[info.RaceId].Add(info);
		}
	}
}
