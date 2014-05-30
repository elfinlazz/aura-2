// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Linq;

namespace Aura.Data.Database
{
	[Serializable]
	public class TitleData
	{
		public ushort Id { get; set; }
		public string Name { get; set; }
		public Dictionary<string, float> Effects { get; set; }

		public TitleData()
		{
			this.Effects = new Dictionary<string, float>();
		}
	}

	/// <summary>
	/// Indexed by title id.
	/// </summary>
	public class TitleDb : DatabaseJsonIndexed<int, TitleData>
	{
		protected override void ReadEntry(JObject entry)
		{
			entry.AssertNotMissing("id", "name");

			var data = new TitleData();
			data.Id = entry.ReadUShort("id");
			data.Name = entry.ReadString("name");

			// Effects
			if (entry["effects"] != null)
			{
				foreach (var effect in (IDictionary<string, JToken>)entry["effects"])
					data.Effects[effect.Key] = (float)effect.Value;
			}

			this.Entries[data.Id] = data;
		}
	}
}
