// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
namespace Aura.Data.Database
{
	[Serializable]
	public class PetData
	{
		public int RaceId { get; set; }

		public short TimeLimit { get; set; }
		public float ExpMultiplicator { get; set; }

		public float Height { get; set; }
		public float Upper { get; set; }
		public float Lower { get; set; }

		public float Life { get; set; }
		public float Mana { get; set; }
		public float Stamina { get; set; }
		public float Str { get; set; }
		public float Int { get; set; }
		public float Dex { get; set; }
		public float Will { get; set; }
		public float Luck { get; set; }

		public short Defense { get; set; }
		public float Protection { get; set; }

		public uint Color1 { get; set; }
		public uint Color2 { get; set; }
		public uint Color3 { get; set; }
	}

	public class PetDb : DatabaseCsvIndexed<int, PetData>
	{
		[MinFieldCount(20)]
		protected override void ReadEntry(CsvEntry entry)
		{
			var info = new PetData();
			info.RaceId = entry.ReadInt();
			entry.ReadString(); // Name
			info.TimeLimit = entry.ReadShort();
			info.ExpMultiplicator = entry.ReadFloat();

			info.Height = entry.ReadFloat();
			info.Upper = entry.ReadFloat();
			info.Lower = entry.ReadFloat();

			info.Life = entry.ReadFloat();
			info.Mana = entry.ReadFloat();
			info.Stamina = entry.ReadFloat();
			info.Str = entry.ReadFloat();
			info.Int = entry.ReadFloat();
			info.Dex = entry.ReadFloat();
			info.Will = entry.ReadFloat();
			info.Luck = entry.ReadFloat();

			info.Defense = entry.ReadShort();
			info.Protection = entry.ReadFloat();

			info.Color1 = entry.ReadUIntHex();
			info.Color2 = entry.ReadUIntHex();
			info.Color3 = entry.ReadUIntHex();

			this.Entries[info.RaceId] = info;
		}
	}
}
