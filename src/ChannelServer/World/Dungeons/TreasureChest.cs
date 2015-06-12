// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.World.Entities;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;

namespace Aura.Channel.World.Dungeons
{
	public class TreasureChest
	{
		public const int ChestPropId = 10201;

		/// <summary>
		/// List of items in this chest.
		/// </summary>
		public List<Item> Items { get; private set; }

		/// <summary>
		/// Creates new treasure chest.
		/// </summary>
		public TreasureChest()
		{
			this.Items = new List<Item>();
		}

		/// <summary>
		/// Adds item to chest.
		/// </summary>
		/// <param name="item"></param>
		public void Add(Item item)
		{
			if (item == null)
				return;

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

		/// <summary>
		/// Returns chest prop with behavior.
		/// </summary>
		/// <param name="region"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="direction"></param>
		/// <returns></returns>
		public Prop CreateProp(Region region, int x, int y, float direction)
		{
			var result = new Prop(ChestPropId, region.Id, x, y, direction, 1, 0, "closed_identified");
			result.Extensions.Add(new ConfirmationPropExtension("", Localization.Get("Do you wish to open this chest?")));
			result.Behavior = (creature, prop) =>
			{
				// Make sure the chest was still closed when it was clicked.
				// No security violation because it could be caused by lag.
				if (prop.State == "open")
					return;

				if (!creature.Inventory.Has(70028)) // Treasure Chest Key
				{
					// Unofficial
					Send.Notice(creature, Localization.Get("You don't have a key."));
					return;
				}

				prop.SetState("open");

				creature.Inventory.Remove(70028);

				var rnd = RandomProvider.Get();
				foreach (var item in this.Items)
				{
					var pos = new Position(x, y).GetRandomInRange(50, rnd);
					item.Drop(region, pos);
				}
			};

			return result;
		}
	}
}
