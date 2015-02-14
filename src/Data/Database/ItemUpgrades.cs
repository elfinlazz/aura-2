// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Aura.Data.Database
{
	[Serializable]
	public class ItemUpgradeData
	{
		public int Id { get; set; }
		public string Ident { get; set; }
		public string Name { get; set; }
		public int Exp { get; set; }
		public int Gold { get; set; }
		public int UpgradeMin { get; set; }
		public int UpgradeMax { get; set; }
		public string Filter { get; set; }
		public List<string> Npcs { get; set; }
		public Dictionary<string, float> Effects { get; set; }

		public ItemUpgradeData()
		{
			this.Npcs = new List<string>();
			this.Effects = new Dictionary<string, float>();
		}
	}

	public class ItemUpgradesDb : DatabaseJsonIndexed<string, ItemUpgradeData>
	{
		protected override void ReadEntry(JObject entry)
		{
			entry.AssertNotMissing("id", "ident", "name", "exp", "gold", "upgradeMin", "upgradeMax", "filter");

			var data = new ItemUpgradeData();
			data.Id = entry.ReadInt("id");
			data.Ident = entry.ReadString("ident");
			data.Name = entry.ReadString("name");
			data.Exp = entry.ReadInt("exp");
			data.Gold = entry.ReadInt("gold");
			data.UpgradeMin = entry.ReadInt("upgradeMin");
			data.UpgradeMax = entry.ReadInt("upgradeMax");
			data.Filter = entry.ReadString("filter");

			if (entry.ContainsKeys("npcs"))
			{
				foreach (var npc in entry["npcs"])
					data.Npcs.Add((string)npc);
			}

			if (entry.ContainsKeys("effects"))
			{
				foreach (var effect in (IDictionary<string, JToken>)entry["effects"])
					data.Effects[effect.Key] = (float)effect.Value;
			}

			this.Entries[data.Name] = data;
		}
	}
}
