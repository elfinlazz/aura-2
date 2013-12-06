// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Shared.Util;
using Aura.Channel.World.Entities;
using Aura.Channel.World;

namespace Aura.Channel.Scripting.Scripts
{
	public abstract partial class BaseScript
	{
		/// <summary>
		/// Creates prop and spawns it.
		/// </summary>
		protected void SpawnProp(int id, int regionId, int x, int y, float direction, PropFunc behavior = null)
		{
			var region = WorldManager.Instance.GetRegion(regionId);
			if (region == null)
			{
				Log.Error("SpawnProp: Region '{0}' doesn't exist.", regionId);
				return;
			}

			var prop = new Prop(id, regionId, x, y, direction);
			prop.Behavior = behavior;
			prop.ServerSide = true;

			region.AddProp(prop);
		}

		/// <summary>
		/// Sets behavior for the prop with entityId.
		/// </summary>
		protected void SetPropBehavior(long entityId, PropFunc behavior)
		{
			var prop = WorldManager.Instance.GetProp(entityId);
			if (prop == null)
			{
				Log.Error("SetPropBehavior: Prop '{0}' doesn't exist.", entityId.ToString("X16"));
				return;
			}

			prop.Behavior = behavior;
		}
	}
}