// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;

namespace Aura.Data.Database
{
	[Serializable]
	public class KeywordData
	{
		public ushort Id { get; set; }
		public string Name { get; set; }
	}

	/// <summary>
	/// Indexed by keyword id.
	/// </summary>
	public class KeywordDb : DatabaseCSVIndexed<string, KeywordData>
	{
		private Dictionary<ushort, KeywordData> IdEntries = new Dictionary<ushort, KeywordData>();

		public KeywordData Find(ushort id)
		{
			return this.IdEntries.GetValueOrDefault(id);
		}

		[MinFieldCount(2)]
		protected override void ReadEntry(CSVEntry entry)
		{
			var info = new KeywordData();
			info.Id = entry.ReadUShort();
			info.Name = entry.ReadString();

			this.Entries[info.Name] = info;
			this.IdEntries[info.Id] = info;
		}
	}
}
