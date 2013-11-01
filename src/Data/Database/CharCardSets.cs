// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence file in the main folder

using System.Collections.Generic;

namespace Aura.Data.Database
{
	public class CharCardSetInfo
	{
		public int SetId { get; set; }
		public int Race { get; set; }
		public int Class { get; set; }
		public byte Pocket { get; set; }
		public uint Color1 { get; set; }
		public uint Color2 { get; set; }
		public uint Color3 { get; set; }
	}

	public class CharCardSetDb : DatabaseCSV<CharCardSetInfo>
	{
		public List<CharCardSetInfo> Find(int setId, int race)
		{
			return this.Entries.FindAll(a => a.SetId == setId && a.Race == race);
		}

		protected override void ReadEntry(CSVEntry entry)
		{
			if (entry.Count < 7)
				throw new FieldCountException(7);

			var info = new CharCardSetInfo();
			info.SetId = entry.ReadInt();
			info.Race = entry.ReadInt();
			info.Class = entry.ReadInt();
			info.Pocket = entry.ReadByte();
			info.Color1 = entry.ReadUIntHex();
			info.Color2 = entry.ReadUIntHex();
			info.Color3 = entry.ReadUIntHex();

			this.Entries.Add(info);
		}
	}
}
