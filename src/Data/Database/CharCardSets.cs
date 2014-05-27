// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;

namespace Aura.Data.Database
{
	[Serializable]
	public class CharCardSetData
	{
		public int SetId { get; internal set; }
		public int Race { get; internal set; }
		public int Class { get; internal set; }
		public byte Pocket { get; internal set; }
		public uint Color1 { get; internal set; }
		public uint Color2 { get; internal set; }
		public uint Color3 { get; internal set; }
	}

	public class CharCardSetDb : DatabaseCSVIndexed<int, Dictionary<int, List<CharCardSetData>>>
	{
		public List<CharCardSetData> Find(int setId, int raceId)
		{
			var set = this.Entries.GetValueOrDefault(setId);
			if (set == null)
				return null;

			var raceSet = set.GetValueOrDefault(raceId);
			if (raceSet == null)
				return null;

			return raceSet;
		}

		[MinFieldCount(7)]
		protected override void ReadEntry(CSVEntry entry)
		{
			var info = new CharCardSetData();
			info.SetId = entry.ReadInt();
			info.Race = entry.ReadInt();
			info.Class = entry.ReadInt();
			info.Pocket = entry.ReadByte();
			info.Color1 = entry.ReadUIntHex();
			info.Color2 = entry.ReadUIntHex();
			info.Color3 = entry.ReadUIntHex();

			if (!this.Entries.ContainsKey(info.SetId))
				this.Entries[info.SetId] = new Dictionary<int, List<CharCardSetData>>();
			if (!this.Entries[info.SetId].ContainsKey(info.Race))
				this.Entries[info.SetId][info.Race] = new List<CharCardSetData>();

			this.Entries[info.SetId][info.Race].Add(info);
		}
	}
}
