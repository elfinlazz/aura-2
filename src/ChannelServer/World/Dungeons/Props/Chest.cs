// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using Aura.Channel.Network.Sending;
using Aura.Channel.World.Entities;
using Aura.Mabi;
using Aura.Shared.Util;

namespace Aura.Channel.World.Dungeons.Props
{
	public class Chest : Prop
	{
		public List<Item> Items { get; private set; }
		public string InternalName;

		public Chest(int id, int regionId, int x, int y, float direction, float scale = 1f, float altitude = 0,
			string state = "closed", string name = "", string title = "", string intName = "")
			: base (id, regionId, x, y, direction, scale, altitude, state, name, title)
		{
			this.Items = new List<Item>();
			this.InternalName = intName;
			this.Behavior = DefaultChestBehavior;
		}

		private void DefaultChestBehavior(Creature creature, Prop prop)
		{
			if (this.State == "open")
				return;

			this.SetState("open");

			foreach (var item in this.Items)
				item.Drop(this.Region, new Position((int)this.Info.X, (int)this.Info.Y));
		}

		/// <summary>
		/// Adds item to chest.
		/// </summary>
		/// <param name="item"></param>
		public void Add(Item item)
		{
			this.Items.Add(item);
		}

		/// <summary>
		/// Adds gold stacks based on amount to chest.
		/// </summary>
		/// <param name="amount"></param>
		public void AddGold(int amount)
		{
			while (amount > 0)
			{
				var n = Math.Min(1000, amount);
				amount -= n;

				var gold = Item.CreateGold(n);
				this.Add(gold);
			}
		}

		public static Chest CreateChest(int x, int y, float direction, int propId = 10201, int regionId=0, string name = "")
		{
			direction = MabiMath.DegreeToRadian((int)direction);
			return new Chest(propId, regionId, x, y, direction, intName: name);
		}
	}

	public class TreasureChest_temp : Chest
	{
		private TreasureChest_temp(int id, int regionId, int x, int y, float direction, float scale = 1f, float altitude = 0, 
			string state = "", string name = "", string title = "")
			: base (id, regionId, x, y, direction, scale, altitude, state, name, title)
		{
			this.Behavior = TreasureChestBehavior + this.Behavior;
		}

		private void TreasureChestBehavior(Creature creature, Prop prop)
		{
			// Make sure the chest was still closed when it was clicked.
			// No security violation because it could be caused by lag.
			if (this.State == "open")
				return;

			if (!creature.Inventory.Has(70028)) // Treasure Chest Key
			{
				// Unofficial
				Send.Notice(creature, Localization.Get("You don't have a key."));
				return;
			}
			creature.Inventory.Remove(70028);
		}
	}

}
