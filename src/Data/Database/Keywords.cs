// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

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
	public class KeywordDb : DatabaseJsonIndexed<string, KeywordData>
	{
		private Dictionary<ushort, KeywordData> IdEntries = new Dictionary<ushort, KeywordData>();

		public KeywordData Find(ushort id)
		{
			return this.IdEntries.GetValueOrDefault(id);
		}

		protected override void ReadEntry(JObject entry)
		{
			if (entry.Count < 2)
				throw new DatabaseWarningException("Missing mandatory data");

			var info = new KeywordData();
			info.Id = entry.ReadUShort("id");
			info.Name = entry.ReadString("name");

			this.Entries[info.Name] = info;
			this.IdEntries[info.Id] = info;
		}
	}
}
