// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using Aura.Channel.Network.Sending;
using Aura.Channel.World.Entities;
using Aura.Mabi;
using Aura.Shared.Util;
using Aura.Channel.World.Dungeons.Puzzles;

namespace Aura.Channel.World.Dungeons.Props
{
	public class Chest : DungeonProp
	{
		protected List<Item> _items;

		public bool IsOpen { get { return (this.State == "open"); } }

		public Chest(int propId, string name)
			: base(propId, name)
		{
			_items = new List<Item>();

			this.Name = name;
			this.Behavior = this.DefaultBehavior;
		}

		public Chest(Puzzle puzzle, string name)
			: this(puzzle.Dungeon.Data.ChestId, name)
		{
		}

		protected virtual void DefaultBehavior(Creature creature, Prop prop)
		{
			if (this.State == "open")
				return;

			this.SetState("open");
			this.DropItems();
		}

		public void DropItems()
		{
			lock (_items)
			{
				foreach (var item in _items)
					item.Drop(this.Region, this.GetPosition());
				_items.Clear();
			}
		}

		/// <summary>
		/// Adds item to chest.
		/// </summary>
		/// <param name="item"></param>
		public void Add(Item item)
		{
			lock (_items)
				_items.Add(item);
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
	}

	public class LockedChest : Chest
	{
		public string LockName { get; protected set; }

		public LockedChest(int propId, string name, string lockName)
			: base(propId, name)
		{
			this.LockName = lockName;
			this.State = "closed";
			this.Extensions.Add(new ConfirmationPropExtension("", Localization.Get("Do you wish to open this chest?"), null, "haskey(" + lockName + ")"));
		}

		public LockedChest(Puzzle puzzle, string name, string key)
			: this(puzzle.Dungeon.Data.ChestId, name, key)
		{
		}

		protected override void DefaultBehavior(Creature creature, Prop prop)
		{
			// Make sure the chest was still closed when it was clicked.
			// No security violation because it could be caused by lag.
			if (prop.State == "open")
				return;

			// Check key
			var key = creature.Inventory.GetItem(a => a.Info.Id == 70028 && a.MetaData1.GetString("prop_to_unlock") == this.LockName);
			if (key == null)
			{
				Send.Notice(creature, Localization.Get("There is no matching key."));
				return;
			}

			// Remove key
			creature.Inventory.Remove(key);

			// Open and drop
			prop.SetState("open");
			this.DropItems();
		}
	}

	public class TreasureChest : LockedChest
	{
		public TreasureChest()
			: base(10201, "TreasureChest", "chest")
		{
			this.State = "closed_identified";
		}
	}
}
