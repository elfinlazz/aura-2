// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Data.Database
{
	[Serializable]
	public class FeatureData
	{
		public string Name { get; set; }
		public bool Enabled { get; set; }
	}

	/// <summary>
	/// Indexed by feature name.
	/// </summary>
	public class FeaturesDb : DatabaseJsonIndexed<string, FeatureData>
	{
		public bool IsEnabled(string featureName)
		{
			var entry = this.Entries.GetValueOrDefault(featureName);
			if (entry == null) return false;

			return entry.Enabled;
		}

		protected override void ReadEntry(JObject entry)
		{
			this.ParseObjectRecursive(entry, true);
		}

		private void ParseObjectRecursive(JObject entry, bool parentEnabled)
		{
			entry.AssertNotMissing("name", "enabled");

			var data = new FeatureData();
			data.Name = entry.ReadString("name");
			data.Enabled = entry.ReadBool("enabled") && parentEnabled;

			this.Entries[data.Name] = data;

			// Stop if there are no children
			if (!entry.ContainsKeys("children"))
				return;

			foreach (JObject child in entry["children"].Where(a => a.Type == JTokenType.Object))
				this.ParseObjectRecursive(child, data.Enabled);
		}
	}
}
