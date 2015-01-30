// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Aura.Data.Database
{
	[Serializable]
	public class CharCardData
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public int SetId { get; set; }
		public List<int> Races { get; set; }
		public int TradeItem { get; set; }
		public int TradePoints { get; set; }

		public CharCardData()
		{
			this.Races = new List<int>();
		}

		public bool Enabled(int race)
		{
			return this.Races.Contains(race);
		}
	}

	/// <summary>
	/// Indexed by char card id.
	/// </summary>
	public class CharCardDb : DatabaseJsonIndexed<int, CharCardData>
	{
		protected override void ReadEntry(JObject entry)
		{
			entry.AssertNotMissing("id", "set", "allowed");

			var info = new CharCardData();
			info.Id = entry.ReadInt("id");
			info.SetId = entry.ReadInt("set");
			info.TradeItem = entry.ReadInt("tradeItem");
			info.TradePoints = entry.ReadInt("tradePoints");

			var races = entry.ReadInt("allowed");
			if ((races & 0x01) != 0) info.Races.Add(10001);
			if ((races & 0x02) != 0) info.Races.Add(10002);
			if ((races & 0x04) != 0) info.Races.Add(9001);
			if ((races & 0x08) != 0) info.Races.Add(9002);
			if ((races & 0x10) != 0) info.Races.Add(8001);
			if ((races & 0x20) != 0) info.Races.Add(8002);

			this.Entries[info.Id] = info;
		}
	}
}
