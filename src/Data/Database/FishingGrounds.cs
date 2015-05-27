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
	public class FishingGroundData
	{
		/// <summary>
		/// Name of the fishing ground.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Priority by which grounds are selected.
		/// </summary>
		/// <remarks>
		/// When you try to fish, the server selects one of several
		/// potential grounds, that contain various items.
		/// For example, during an event you could create a new ground
		/// that overrides all others.
		/// </remarks>
		public int Priority { get; set; }

		// Conditions
		// We have a limited amount of possible conditions,
		// and they are rather simple, it's easier to not
		// abstract them.

		/// <summary>
		/// Locations are which this ground is available.
		/// </summary>
		public string[] Locations { get; set; }

		/// <summary>
		/// Event during which this ground is available.
		/// </summary>
		public string Event { get; set; }

		/// <summary>
		/// Percentage chance for this ground to be available.
		/// </summary>
		public float Chance { get; set; }

		/// <summary>
		/// Rod using which this ground becomes available.
		/// </summary>
		public int Rod { get; set; }

		/// <summary>
		/// Bait using which this ground becomes available.
		/// </summary>
		public int Bait { get; set; }

		/// <summary>
		/// Items that can be fished from this ground.
		/// </summary>
		public DropData[] Items { get; set; }

		/// <summary>
		/// Cumulative drop chance of all items.
		/// </summary>
		public double TotalItemChance { get; set; }
	}

	/// <summary>
	/// Database of fishing grounds.
	/// </summary>
	public class FishingGroundsDb : DatabaseJsonIndexed<string, FishingGroundData>
	{
		protected override void ReadEntry(JObject entry)
		{
			entry.AssertNotMissing("name", "conditions", "items");

			var data = new FishingGroundData();
			data.Name = entry.ReadString("name");
			data.Priority = entry.ReadInt("priority");

			var entryConditions = (JObject)entry["conditions"];

			if (entryConditions.ContainsKeys("location"))
				data.Locations = ((JArray)entryConditions["location"]).Select(a => (string)a).ToArray();
			else
				data.Locations = new string[0];

			data.Event = entryConditions.ReadString("event");
			data.Chance = entryConditions.ReadFloat("chance");
			data.Rod = entryConditions.ReadInt("rod");
			data.Bait = entryConditions.ReadInt("bait");

			var items = new List<DropData>();
			foreach (var entryItemObj in entry["items"])
			{
				// Items might be commented
				var entryItem = entryItemObj as JObject;
				if (entryItem == null)
					continue;

				entryItem.AssertNotMissing("itemId", "chance");

				var item = new DropData();
				item.ItemId = entryItem.ReadInt("itemId");
				item.Chance = entryItem.ReadFloat("chance");

				if (entryItem.ContainsKey("color1")) item.Color1 = entryItem.ReadUInt("color1");
				if (entryItem.ContainsKey("color2")) item.Color2 = entryItem.ReadUInt("color2");
				if (entryItem.ContainsKey("color3")) item.Color3 = entryItem.ReadUInt("color3");

				items.Add(item);
			}
			data.Items = items.ToArray();
			data.TotalItemChance = items.Sum(a => a.Chance);

			if (data.Items.Length == 0)
				throw new DatabaseErrorException("Item list must not be empty.");

			this.Entries[data.Name] = data;
		}
	}
}
