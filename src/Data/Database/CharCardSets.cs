// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence file in the main folder

using System.Collections.Generic;

namespace Aura.Data.Database
{
	public class CharCardSetInfo
	{
		public int SetId { get; internal set; }
		public int Race { get; internal set; }
		public int Class { get; internal set; }
		public byte Pocket { get; internal set; }
		public uint Color1 { get; internal set; }
		public uint Color2 { get; internal set; }
		public uint Color3 { get; internal set; }
	}

	public class CharCardSetDb : DatabaseCSV<CharCardSetInfo>
	{
		public List<CharCardSetInfo> Find(uint setId, uint race)
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
