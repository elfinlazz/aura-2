// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;

namespace Aura.Data.Database
{
	[Serializable]
	public class PropDropData
	{
		public int Type { get; set; }
		public List<PropDropItemInfo> Items { get; set; }

		public PropDropData()
		{
			this.Items = new List<PropDropItemInfo>();
		}

		public PropDropData(int type)
			: this()
		{
			this.Type = type;
		}

		/// <summary>
		/// Returns a random item id from the list, based on the weight (chance).
		/// </summary>
		/// <param name="rand"></param>
		/// <returns></returns>
		public PropDropItemInfo GetRndItem(Random rand)
		{
			var total = this.Items.Sum(cls => cls.Chance);

			var randVal = rand.NextDouble() * total;
			var i = 0;
			for (; randVal > 0; ++i)
				randVal -= this.Items[i].Chance;

			return this.Items[i - 1];
		}
	}

	[Serializable]
	public class PropDropItemInfo
	{
		public int Type { get; set; }
		public int ItemClass { get; set; }
		public ushort Amount { get; set; }
		public float Chance { get; set; }
	}

	public class PropDropDb : DatabaseCsvIndexed<int, PropDropData>
	{
		[MinFieldCount(3)]
		protected override void ReadEntry(CsvEntry entry)
		{
			var info = new PropDropItemInfo();
			info.Type = entry.ReadInt();
			info.ItemClass = entry.ReadInt();
			info.Amount = entry.ReadUShort();
			info.Chance = entry.ReadFloat();

			var ii = AuraData.ItemDb.Find(info.ItemClass);
			if (ii == null)
				throw new Exception(string.Format("Unknown item id '{0}'.", info.ItemClass));

			if (info.Amount > ii.StackMax)
				info.Amount = ii.StackMax;

			// The file contains PropDropItemInfo, here we organize it into PropDropInfo structs.
			if (!this.Entries.ContainsKey(info.Type))
				this.Entries.Add(info.Type, new PropDropData(info.Type));
			this.Entries[info.Type].Items.Add(info);
		}
	}
}
