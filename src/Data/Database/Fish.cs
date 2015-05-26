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
	public class FishData
	{
		/// <summary>
		/// Item id of the fish
		/// </summary>
		public int ItemId { get; set; }

		/// <summary>
		/// Name of the prop to display
		/// </summary>
		public string PropName { get; set; }

		/// <summary>
		/// Size of the displayed prop
		/// </summary>
		public int PropSize { get; set; }

		/// <summary>
		/// Min size of the caught fish.
		/// </summary>
		public int SizeMin { get; set; }

		/// <summary>
		/// Max size of the caught fish.
		/// </summary>
		public int SizeMax { get; set; }
	}

	/// <summary>
	/// Database of fish.
	/// </summary>
	public class FishDb : DatabaseJsonIndexed<int, FishData>
	{
		protected override void ReadEntry(JObject entry)
		{
			entry.AssertNotMissing("itemId", "prop", "propSize");

			var data = new FishData();
			data.ItemId = entry.ReadInt("itemId");
			data.PropName = entry.ReadString("prop");
			data.PropSize = entry.ReadInt("propSize");
			data.SizeMin = entry.ReadInt("sizeMin");
			data.SizeMax = entry.ReadInt("sizeMax");

			this.Entries[data.ItemId] = data;
		}
	}
}
