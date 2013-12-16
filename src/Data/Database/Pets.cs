// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

namespace Aura.Data.Database
{
	public class PetData
	{
		public int RaceId { get; internal set; }

		public short TimeLimit { get; internal set; }
		public float ExpMultiplicator { get; internal set; }

		public float Height { get; internal set; }
		public float Upper { get; internal set; }
		public float Lower { get; internal set; }

		public float Life { get; internal set; }
		public float Mana { get; internal set; }
		public float Stamina { get; internal set; }
		public float Str { get; internal set; }
		public float Int { get; internal set; }
		public float Dex { get; internal set; }
		public float Will { get; internal set; }
		public float Luck { get; internal set; }

		public short Defense { get; internal set; }
		public float Protection { get; internal set; }

		public uint Color1 { get; internal set; }
		public uint Color2 { get; internal set; }
		public uint Color3 { get; internal set; }
	}

	public class PetDb : DatabaseCSVIndexed<int, PetData>
	{
		protected override void ReadEntry(CSVEntry entry)
		{
			if (entry.Count < 20)
				throw new FieldCountException(20);

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

			this.Entries.Add(info.RaceId, info);
		}
	}
}
