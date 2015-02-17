// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Aura.Data.Database
{
	[Serializable]
	public class PropsDbData : TaggableData
	{
		public int Id { get; set; }
		public string Name { get; set; }
	}

	public class PropsDb : DatabaseJsonIndexed<int, PropsDbData>
	{
		protected override void ReadEntry(JObject entry)
		{
			entry.AssertNotMissing("id", "name", "tags");

			var data = new PropsDbData();
			data.Id = entry.ReadInt("id");
			data.Name = entry.ReadString("name");
			data.Tags = entry.ReadString("tags");

			this.Entries[data.Id] = data;
		}
	}
}
