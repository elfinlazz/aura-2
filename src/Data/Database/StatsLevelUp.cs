// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence file in the main folder

using System;
using System.Linq;

namespace Aura.Data.Database
{
	public class StatsLevelUpInfo
	{
		public byte Age { get; internal set; }
		public short Race { get; internal set; }

		public short AP { get; internal set; }
		public float Life { get; internal set; }
		public float Mana { get; internal set; }
		public float Stamina { get; internal set; }
		public float Str { get; internal set; }
		public float Int { get; internal set; }
		public float Dex { get; internal set; }
		public float Will { get; internal set; }
		public float Luck { get; internal set; }
	}

	public class StatsLevelUpDb : DatabaseCSV<StatsLevelUpInfo>
	{
		/// <summary>
		/// Returns the age info for the given race
		/// at the given age, or null.
		/// </summary>
		/// <param name="race"></param>
		/// <param name="age"></param>
		/// <returns></returns>
		public StatsLevelUpInfo Find(int race, byte age)
		{
			race = (race & ~3);
			return this.Entries.FirstOrDefault(a => a.Race == race && a.Age == Math.Min((byte)25, age));
		}

		protected override void ReadEntry(CSVEntry entry)
		{
			if (entry.Count < 11)
				throw new FieldCountException(11);

			var info = new StatsLevelUpInfo();
			info.Age = entry.ReadByte();
			info.Race = entry.ReadShort();
			info.AP = entry.ReadShort();
			info.Life = entry.ReadFloat();
			info.Mana = entry.ReadFloat();
			info.Stamina = entry.ReadFloat();
			info.Str = entry.ReadFloat();
			info.Int = entry.ReadFloat();
			info.Dex = entry.ReadFloat();
			info.Will = entry.ReadFloat();
			info.Luck = entry.ReadFloat();

			this.Entries.Add(info);
		}
	}
}
