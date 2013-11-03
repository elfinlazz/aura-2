// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;

namespace Aura.Data.Database
{
	public class PropDropInfo
	{
		public int Type { get; internal set; }
		public List<PropDropItemInfo> Items { get; internal set; }

		public PropDropInfo()
		{
			this.Items = new List<PropDropItemInfo>();
		}

		public PropDropInfo(int type)
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
			float total = 0;
			foreach (var cls in this.Items)
				total += cls.Chance;

			var rand_val = rand.NextDouble() * total;
			int i = 0;
			for (; rand_val > 0; ++i)
				rand_val -= this.Items[i].Chance;

			return this.Items[i - 1];
		}
	}

	public class PropDropItemInfo
	{
		public int Type { get; internal set; }
		public int ItemClass { get; internal set; }
		public ushort Amount { get; internal set; }
		public float Chance { get; internal set; }
	}

	public class PropDropDb : DatabaseCSVIndexed<int, PropDropInfo>
	{
		protected override void ReadEntry(CSVEntry entry)
		{
			if (entry.Count < 3)
				throw new FieldCountException(3);

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
				this.Entries.Add(info.Type, new PropDropInfo(info.Type));
			this.Entries[info.Type].Items.Add(info);
		}
	}
}
