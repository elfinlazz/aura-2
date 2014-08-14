// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;

namespace Aura.Data.Database
{
	[Serializable]
	public class StatsLevelUpData
	{
		public ushort Race { get; set; }
		public byte Age { get; set; }

		public short AP { get; set; }
		public float Life { get; set; }
		public float Mana { get; set; }
		public float Stamina { get; set; }
		public float Str { get; set; }
		public float Int { get; set; }
		public float Dex { get; set; }
		public float Will { get; set; }
		public float Luck { get; set; }
	}

	public class StatsLevelUpDb : DatabaseCsvIndexed<int, Dictionary<int, StatsLevelUpData>>
	{
		/// <summary>
		/// Returns the age info for the given race
		/// at the given age, or null.
		/// </summary>
		/// <param name="raceId"></param>
		/// <param name="age"></param>
		/// <returns></returns>
		public StatsLevelUpData Find(int raceId, int age)
		{
			raceId = (raceId & ~3);

			var race = this.Entries.GetValueOrDefault(raceId);
			if (race == null)
				return null;

			return race.GetValueOrDefault(Math.Min((byte)25, age));
		}

		[MinFieldCount(11)]
		protected override void ReadEntry(CsvEntry entry)
		{
			var info = new StatsLevelUpData();
			info.Age = entry.ReadByte();
			info.Race = entry.ReadUShort();
			info.AP = entry.ReadShort();
			info.Life = entry.ReadFloat();
			info.Mana = entry.ReadFloat();
			info.Stamina = entry.ReadFloat();
			info.Str = entry.ReadFloat();
			info.Int = entry.ReadFloat();
			info.Dex = entry.ReadFloat();
			info.Will = entry.ReadFloat();
			info.Luck = entry.ReadFloat();

			if (!this.Entries.ContainsKey(info.Race))
				this.Entries[info.Race] = new Dictionary<int, StatsLevelUpData>();

			this.Entries[info.Race][info.Age] = info;
		}
	}
}
