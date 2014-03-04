// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections;
using Aura.Channel.Network;
using Aura.Channel.Network.Sending;
using Aura.Channel.Util;
using Aura.Channel.World;
using Aura.Channel.World.Entities;
using Aura.Shared.Mabi.Const;
using Aura.Shared.Network;
using Aura.Shared.Util;

namespace Aura.Channel.Scripting.Scripts
{
	public abstract class BaseScript : IDisposable
	{
		private const int PropDropRadius = 50;

		public string ScriptFilePath { get; set; }

		protected ScriptVariables GlobalVars { get { return ChannelServer.Instance.ScriptManager.GlobalVars; } }

		public virtual void Load()
		{
		}

		/// <summary>
		/// Adds subscribtions based on "On" attribute on methods.
		/// </summary>
		public void AutoLoadEvents()
		{
			var type = this.GetType();
			var methods = this.GetType().GetMethods();
			foreach (var method in methods)
			{
				var attrs = method.GetCustomAttributes(typeof(OnAttribute), false);
				if (attrs.Length == 0)
					continue;

				var attr = attrs[0] as OnAttribute;

				var eventHandlerInfo = ChannelServer.Instance.Events.GetType().GetEvent(attr.Event);
				if (eventHandlerInfo == null)
				{
					Log.Error("AutoLoadEvents: Unknown event '{0}' on '{1}.{2}'.", attr.Event, type.Name, method.Name);
					continue;
				}

				try
				{
					eventHandlerInfo.AddEventHandler(ChannelServer.Instance.Events, Delegate.CreateDelegate(eventHandlerInfo.EventHandlerType, this, method));
				}
				catch (Exception ex)
				{
					Log.Exception(ex, "AutoLoadEvents: Failed to subscribe '{1}.{2}' to '{0}'.", attr.Event, type.Name, method.Name);
				}
			}
		}

		/// <summary>
		/// Unsubscribes from all auto subscribed events.
		/// </summary>
		public virtual void Dispose()
		{
			var type = this.GetType();
			var methods = this.GetType().GetMethods();
			foreach (var method in methods)
			{
				var attrs = method.GetCustomAttributes(typeof(OnAttribute), false);
				if (attrs.Length == 0)
					continue;

				var attr = attrs[0] as OnAttribute;

				var eventHandlerInfo = ChannelServer.Instance.Events.GetType().GetEvent(attr.Event);
				if (eventHandlerInfo == null)
				{
					// Erroring on load should be enough.
					//Log.Error("Dispose: Unknown event '{0}' on '{1}.{2}'.", attr.Event, type.Name, method.Name);
					continue;
				}

				try
				{
					eventHandlerInfo.RemoveEventHandler(ChannelServer.Instance.Events, Delegate.CreateDelegate(eventHandlerInfo.EventHandlerType, this, method));
				}
				catch (Exception ex)
				{
					Log.Exception(ex, "Dispose: Failed to unsubscribe '{1}.{2}' from '{0}'.", attr.Event, type.Name, method.Name);
				}
			}

			this.CleanUp();
		}

		/// <summary>
		/// Called from Dispose, use for cleaning up before reload.
		/// </summary>
		protected virtual void CleanUp()
		{
		}

		/// <summary>
		/// Creates creature spawn area.
		/// </summary>
		/// <param name="raceId"></param>
		/// <param name="amount"></param>
		/// <param name="regionId"></param>
		/// <param name="coordinates"></param>
		protected void CreatureSpawn(int raceId, int amount, int regionId, params int[] coordinates)
		{
			ChannelServer.Instance.ScriptManager.AddCreatureSpawn(new CreatureSpawn(raceId, amount, regionId, coordinates));
		}

		/// <summary>
		/// Returns random number between 0.0 and 100.0.
		/// </summary>
		/// <returns></returns>
		protected double Random()
		{
			var rnd = RandomProvider.Get();
			return (100 * rnd.NextDouble());
		}

		/// <summary>
		/// Returns random number between 0 and max-1.
		/// </summary>
		/// <param name="max">Exclusive upper bound</param>
		/// <returns></returns>
		protected int Random(int max)
		{
			var rnd = RandomProvider.Get();
			return rnd.Next(max);
		}

		/// <summary>
		/// Returns random number between min and max-1.
		/// </summary>
		/// <param name="min">Inclusive lower bound</param>
		/// <param name="max">Exclusive upper bound</param>
		/// <returns></returns>
		protected int Random(int min, int max)
		{
			var rnd = RandomProvider.Get();
			return rnd.Next(min, max);
		}

		/// <summary>
		/// Adds hook for NPC.
		/// </summary>
		/// <param name="npcName"></param>
		/// <param name="hookName"></param>
		/// <param name="func"></param>
		protected void AddHook(string npcName, string hookName, ScriptHook func)
		{
			ChannelServer.Instance.ScriptManager.AddHook(npcName, hookName, func);
		}

		/// <summary>
		/// Adds packet handler.
		/// </summary>
		/// <param name="op"></param>
		/// <param name="handler"></param>
		protected void AddPacketHandler(int op, PacketHandlerManager<ChannelClient>.PacketHandlerFunc handler)
		{
			ChannelServer.Instance.Server.Handlers.Add(op, handler);
		}

		/// <summary>
		/// Adds command.
		/// </summary>
		/// <param name="auth"></param>
		/// <param name="charAuth"></param>
		/// <param name="name"></param>
		/// <param name="usage"></param>
		/// <param name="func"></param>
		protected void AddCommand(int auth, int charAuth, string name, string usage, GmCommandFunc func)
		{
			ChannelServer.Instance.CommandProcessor.Add(auth, charAuth, name, usage, func);
		}

		/// <summary>
		/// Creates prop and spawns it.
		/// </summary>
		protected Prop SpawnProp(int id, int regionId, int x, int y, float direction, PropFunc behavior = null)
		{
			var region = ChannelServer.Instance.World.GetRegion(regionId);
			if (region == null)
			{
				Log.Error("{1}.SpawnProp: Region '{0}' doesn't exist.", regionId, this.GetType().Name);
				return null;
			}

			var prop = new Prop(id, regionId, x, y, direction);
			prop.Behavior = behavior;

			region.AddProp(prop);

			return prop;
		}

		/// <summary>
		/// Spawns prop
		/// </summary>
		protected Prop SpawnProp(Prop prop, PropFunc behavior = null)
		{
			var region = ChannelServer.Instance.World.GetRegion(prop.RegionId);
			if (region == null)
			{
				Log.Error("{1}.SpawnProp: Region '{0}' doesn't exist.", prop.RegionId, this.GetType().Name);
				return null;
			}

			prop.Behavior = behavior;

			region.AddProp(prop);

			return prop;
		}

		/// <summary>
		/// Sets behavior for the prop with entityId.
		/// </summary>
		protected Prop SetPropBehavior(long entityId, PropFunc behavior)
		{
			var prop = ChannelServer.Instance.World.GetProp(entityId);
			if (prop == null)
			{
				Log.Error("{1}.SetPropBehavior: Prop '{0}' doesn't exist.", entityId.ToString("X16"), this.GetType().Name);
				return null;
			}

			prop.Behavior = behavior;

			return prop;
		}

		/// <summary>
		/// Returns a drop behavior.
		/// </summary>
		/// <param name="dropType"></param>
		/// <returns></returns>
		protected PropFunc PropDrop(int dropType)
		{
			return Prop.GetDropBehavior(dropType);
		}

		/// <summary>
		/// Returns a warp behavior.
		/// </summary>
		/// <remarks>
		/// The source location is ignored, it's only there for clarity.
		/// </remarks>
		/// <param name="sourceRegion"></param>
		/// <param name="sourceX"></param>
		/// <param name="sourceY"></param>
		/// <param name="region"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		protected PropFunc PropWarp(int sourceRegion, int sourceX, int sourceY, int region, int x, int y)
		{
			return this.PropWarp(region, x, y);
		}

		/// <summary>
		/// Returns a warp behavior.
		/// </summary>
		/// <param name="region"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		protected PropFunc PropWarp(int region, int x, int y)
		{
			return Prop.GetWarpBehavior(region, x, y);
		}

		/// <summary>
		/// Spawns creature(s)
		/// </summary>
		/// <param name="raceId"></param>
		/// <param name="amount"></param>
		/// <param name="regionId"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="radius"></param>
		/// <param name="effect"></param>
		protected void Spawn(int raceId, int amount, int regionId, int x, int y, int radius = 0, bool effect = false)
		{
			this.Spawn(raceId, amount, regionId, new Position(x, y), radius, effect);
		}

		/// <summary>
		/// Spawns creature(s)
		/// </summary>
		/// <param name="raceId"></param>
		/// <param name="amount"></param>
		/// <param name="regionId"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="radius"></param>
		/// <param name="effect"></param>
		protected void Spawn(int raceId, int amount, int regionId, Position pos, int radius = 0, bool effect = false)
		{
			amount = Math2.MinMax(1, 100, amount);

			var rnd = RandomProvider.Get();

			for (int i = 0; i < amount; ++i)
			{
				if (radius > 0)
					pos = pos.GetRandomInRange(radius, rnd);

				ChannelServer.Instance.ScriptManager.Spawn(raceId, regionId, pos.X, pos.Y, -1, true, effect);
			}
		}
	}

	public class OnAttribute : Attribute
	{
		public string Event { get; protected set; }

		public OnAttribute(string evnt)
		{
			this.Event = evnt;
		}
	}
}