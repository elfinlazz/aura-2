// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using Newtonsoft.Json.Linq;

namespace Aura.Data.Database
{
	[Serializable]
	public class ChairData
	{
		public int ItemId { get; set; }
		public int PropId { get; set; }
		public int GiantPropId { get; set; }
		public int Effect { get; set; }
	}

	/// <summary>
	/// Indexed by item id.
	/// </summary>
	public class ChairDb : DatabaseJsonIndexed<int, ChairData>
	{
		protected override void ReadEntry(JObject entry)
		{
			entry.AssertNotMissing("itemId", "propId", "giantPropId");

			var info = new ChairData();
			info.ItemId = entry.ReadInt("itemId");
			info.PropId = entry.ReadInt("propId");
			info.GiantPropId = entry.ReadInt("giantPropId");
			info.Effect = entry.ReadInt("effect");

			this.Entries[info.ItemId] = info;
		}
	}
}
