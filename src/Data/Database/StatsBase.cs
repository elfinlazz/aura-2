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
		public ushort Race { get; internal set; }
		public byte Age { get; internal set; }

		public byte AP { get; internal set; }
		public byte Life { get; internal set; }
		public byte Mana { get; internal set; }
		public byte Stamina { get; internal set; }
		public byte Str { get; internal set; }
		public byte Int { get; internal set; }
		public byte Dex { get; internal set; }
		public byte Will { get; internal set; }
		public byte Luck { get; internal set; }
	}

	public class StatsBaseDb : DatabaseCSVIndexed<int, Dictionary<int, StatsBaseData>>
	{
		/// <summary>
		/// Returns the age info (base stats) for the given race
		/// at the given age, or null.
		/// </summary>
		/// <param name="raceId">0 = Human, 1 = Elf, 2 = Giant</param>
		/// <param name="age">10-17</param>
		/// <returns></returns>
		public StatsBaseData Find(int raceId, byte age)
		{
			raceId = (raceId & ~3);

			var race = this.Entries.GetValueOrDefault(raceId);
			if (race == null)
				return null;

			return race.GetValueOrDefault(age);
		}

		[MinFieldCount(11)]
		protected override void ReadEntry(CSVEntry entry)
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
