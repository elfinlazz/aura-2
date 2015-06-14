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

		public Chest(int propId, string name)
			: base(propId, name)
		{
			_items = new List<Item>();

			this.InternalName = name;
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
		public LockedChest(int propId, string name)
			: base(propId, name)
		{
			this.Extensions.Add(new ConfirmationPropExtension("", Localization.Get("Do you wish to open this chest?")));
		}

		public LockedChest(Puzzle puzzle, string name)
			: this(puzzle.Dungeon.Data.ChestId, name)
		{
		}

		protected override void DefaultBehavior(Creature creature, Prop prop)
		{
			// Make sure the chest was still closed when it was clicked.
			// No security violation because it could be caused by lag.
			if (prop.State == "open")
				return;

			if (!creature.Inventory.Has(70028)) // Treasure Chest Key
			{
				Send.Notice(creature, Localization.Get("There is no matching key."));
				return;
			}

			prop.SetState("open");

			creature.Inventory.Remove(70028);

			this.DropItems();
		}
	}
}
