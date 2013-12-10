// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.World;
using Aura.Channel.World.Entities;
using Aura.Data;
using Aura.Shared.Util;

namespace Aura.Channel.Scripting.Scripts
{
	public abstract partial class BaseScript
	{
		private const int PropDropRadius = 50;

		/// <summary>
		/// Creates prop and spawns it.
		/// </summary>
		protected Prop SpawnProp(int id, int regionId, int x, int y, float direction, PropFunc behavior = null)
		{
			var region = WorldManager.Instance.GetRegion(regionId);
			if (region == null)
			{
				Log.Error("SpawnProp: Region '{0}' doesn't exist.", regionId);
				return null;
			}

			var prop = new Prop(id, regionId, x, y, direction);
			prop.Behavior = behavior;
			prop.ServerSide = true;

			region.AddProp(prop);

			return prop;
		}

		/// <summary>
		/// Sets behavior for the prop with entityId.
		/// </summary>
		protected Prop SetPropBehavior(long entityId, PropFunc behavior)
		{
			var prop = WorldManager.Instance.GetProp(entityId);
			if (prop == null)
			{
				Log.Error("SetPropBehavior: Prop '{0}' doesn't exist.", entityId.ToString("X16"));
				return null;
			}

			prop.Behavior = behavior;

			return prop;
		}

		// Behaviors
		// ------------------------------------------------------------------

		protected PropFunc PropDrop(int dropType)
		{
			return (creature, prop) =>
			{
				if (RandomProvider.Get().NextDouble() > ChannelServer.Instance.Conf.World.PropDropRate)
					return;

				var dropInfo = AuraData.PropDropDb.Find(dropType);
				if (dropInfo == null)
				{
					Log.Warning("PropDrop Behavior: Unknown prop drop type '{0}'.", dropType);
					return;
				}

				var rnd = RandomProvider.Get();
				var pos = creature.GetPosition();

				// Get random item from potential drops
				var dropItemInfo = dropInfo.GetRndItem(rnd);
				var item = new Item(dropItemInfo.ItemClass);
				item.Info.Amount = dropItemInfo.Amount > 1 ? (ushort)rnd.Next(1, dropItemInfo.Amount) : (ushort)1;

				// Get random drop position
				var x = rnd.Next(pos.X - PropDropRadius, pos.X + PropDropRadius + 1);
				var y = rnd.Next(pos.Y - PropDropRadius, pos.Y + PropDropRadius + 1);

				prop.Region.DropItem(item, x, y);
			};
		}

		protected PropFunc PropWarp(int region, int x, int y)
		{
			return (creature, prop) =>
			{
				creature.Warp(region, x, y);
			};
		}
	}
}