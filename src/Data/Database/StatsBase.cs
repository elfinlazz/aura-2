// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;

namespace Aura.Data.Database
{
	[Serializable]
	public class StatsBaseData
	{
		public ushort Race { get; set; }
		public byte Age { get; set; }

		public byte AP { get; set; }
		public byte Life { get; set; }
		public byte Mana { get; set; }
		public byte Stamina { get; set; }
		public byte Str { get; set; }
		public byte Int { get; set; }
		public byte Dex { get; set; }
		public byte Will { get; set; }
		public byte Luck { get; set; }
	}

	public class StatsBaseDb : DatabaseCsvIndexed<int, Dictionary<int, StatsBaseData>>
	{
		/// <summary>
		/// Returns the age info (base stats) for the given race
		/// at the given age, or null.
		/// </summary>
		/// <param name="raceId"></param>
		/// <param name="age">10-17</param>
		/// <returns></returns>
		public StatsBaseData Find(int raceId, int age)
		{
			raceId = (raceId & ~3);

			var race = this.Entries.GetValueOrDefault(raceId);
			if (race == null)
				return null;

			return race.GetValueOrDefault(age);
		}

		[MinFieldCount(11)]
		protected override void ReadEntry(CsvEntry entry)
		{
			var info = new StatsBaseData();
			info.Age = entry.ReadByte();
			info.Race = entry.ReadUShort();
			info.AP = entry.ReadByte();
			info.Life = entry.ReadByte();
			info.Mana = entry.ReadByte();
			info.Stamina = entry.ReadByte();
			info.Str = entry.ReadByte();
			info.Int = entry.ReadByte();
			info.Dex = entry.ReadByte();
			info.Will = entry.ReadByte();
			info.Luck = entry.ReadByte();

			if (!this.Entries.ContainsKey(info.Race))
				this.Entries[info.Race] = new Dictionary<int, StatsBaseData>();

			this.Entries[info.Race][info.Age] = info;
		}
	}
}
