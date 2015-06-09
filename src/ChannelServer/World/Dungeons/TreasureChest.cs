// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.World.Entities;
using Aura.Shared.Util;
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
		/// Returns chest prop with behavior.
		/// </summary>
		/// <param name="region"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="direction"></param>
		/// <returns></returns>
		public Prop CreateProp(Region region, int x, int y, float direction)
		{
			var result = new Prop(ChestPropId, region.Id, x, y, direction);
			result.Extensions.Add(new ConfirmationPropExtension("", Localization.Get("Do you wish to open this chest?")));
			result.Behavior = (creature, prop) =>
			{
				// TODO: if key

				var rnd = RandomProvider.Get();
				foreach (var item in this.Items)
				{
					var pos = new Position(x, y).GetRandomInRange(50, rnd);
					item.Drop(region, pos);
				}

				prop.SetState("open");
			};

			return result;
		}
	}
}
