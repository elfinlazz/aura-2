// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using Newtonsoft.Json.Linq;

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

	public class PetDb : DatabaseJsonIndexed<int, PetData>
	{
		protected override void ReadEntry(JObject entry)
		{
			entry.AssertNotMissing("raceId", "timeLimit", "exp", "height", "upper", "lower", "life", "mana", "stamina", "str", "int", "dex", "will", "luck", "defense", "protection");

			var info = new PetData();
			info.RaceId = entry.ReadInt("raceId");
			info.TimeLimit = entry.ReadShort("timeLimit");
			info.ExpMultiplicator = entry.ReadFloat("exp");

			info.Height = entry.ReadFloat("height");
			info.Upper = entry.ReadFloat("upper");
			info.Lower = entry.ReadFloat("lower");

			info.Life = entry.ReadFloat("life");
			info.Mana = entry.ReadFloat("mana");
			info.Stamina = entry.ReadFloat("stamina");
			info.Str = entry.ReadFloat("str");
			info.Int = entry.ReadFloat("int");
			info.Dex = entry.ReadFloat("dex");
			info.Will = entry.ReadFloat("will");
			info.Luck = entry.ReadFloat("luck");

			info.Defense = entry.ReadShort("defense");
			info.Protection = entry.ReadFloat("protection");

			info.Color1 = entry.ReadUInt("color1", 0x808080);
			info.Color2 = entry.ReadUInt("color2", 0x808080);
			info.Color3 = entry.ReadUInt("color3", 0x808080);

			this.Entries[info.RaceId] = info;
		}
	}
}
