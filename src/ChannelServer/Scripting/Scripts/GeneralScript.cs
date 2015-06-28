// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections;
using Aura.Channel.Network;
using Aura.Channel.Network.Sending;
using Aura.Channel.Util;
using Aura.Channel.World;
using Aura.Channel.World.Entities;
using Aura.Data.Database;
using Aura.Mabi.Const;
using Aura.Shared.Network;
using Aura.Shared.Util;
using System.Threading;
using System.Collections.Generic;
using Aura.Shared.Util.Commands;
using Aura.Shared.Scripting.Scripts;
using Aura.Data;

namespace Aura.Channel.Scripting.Scripts
{
	/// <summary>
	/// General purpose script with various helpful methods and the
	/// auto subscription feature.
	/// </summary>
	public abstract class GeneralScript : IDisposable, IScript, IAutoLoader
	{
		private const int PropDropRadius = 50;

		protected ScriptVariables GlobalVars { get { return ChannelServer.Instance.ScriptManager.GlobalVars; } }

		public virtual bool Init()
		{
			this.Load();
			return true;
		}

		public virtual void Load()
		{
		}

		/// <summary>
		/// Adds subscriptions based on "On" attribute on methods.
		/// </summary>
		public void AutoLoad()
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

		// Functions
		// ------------------------------------------------------------------

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
		/// Returns a random string from the given ones.
		/// </summary>
		/// <param name="strings"></param>
		[Obsolete("Use Rnd instead.", true)]
		public string RndStr(params string[] strings)
		{
			return this.Rnd(strings);
		}

		/// <summary>
		/// Returns a random value from the given ones.
		/// </summary>
		/// <param name="values"></param>
		public T Rnd<T>(params T[] values)
		{
			if (values == null || values.Length == 0)
				throw new ArgumentException("values might not be null or empty.");

			return values[this.Random(values.Length)];
		}

		/// <summary>
		/// Returns the params as array (syntactic sugar).
		/// </summary>
		/// <param name="coordinates"></param>
		protected T[] A<T>(params T[] coordinates)
		{
			return coordinates;
		}

		/// <summary>
		/// Returns true if feature is enabled.
		/// </summary>
		/// <remarks>
		/// TODO: Make another more general script base class for this and Random?
		/// </remarks>
		/// <param name="featureName"></param>
		/// <returns></returns>
		protected bool IsEnabled(string featureName)
		{
			return AuraData.FeaturesDb.IsEnabled(featureName);
		}

		#region Extension

		/// <summary>
		/// Adds hook for NPC.
		/// </summary>
		/// <param name="npcName"></param>
		/// <param name="hookName"></param>
		/// <param name="func"></param>
		protected void AddHook(string npcName, string hookName, NpcScriptHook func)
		{
			ChannelServer.Instance.ScriptManager.NpcScriptHooks.Add(npcName, hookName, func);
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
		/// Adds console command.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="usage"></param>
		/// <param name="description"></param>
		/// <param name="handler"></param>
		protected void AddConsoleCommand(string name, string usage, string description, ConsoleCommandFunc handler)
		{
			ChannelServer.Instance.ConsoleCommands.Add(name, usage, description, handler);
		}

		#endregion Extension

		#region Props

		/// <summary>
		/// Creates prop and spawns it.
		/// </summary>
		protected Prop SpawnProp(int id, int regionId, int x, int y, float direction, float scale, PropFunc behavior = null)
		{
			var region = ChannelServer.Instance.World.GetRegion(regionId);
			if (region == null)
			{
				Log.Error("{1}.SpawnProp: Region '{0}' doesn't exist.", regionId, this.GetType().Name);
				return null;
			}

			var prop = new Prop(id, regionId, x, y, direction, scale);
			prop.Behavior = behavior;

			region.AddProp(prop);

			return prop;
		}

		/// <summary>
		/// Creates prop and spawns it.
		/// </summary>
		protected Prop SpawnProp(int id, int regionId, int x, int y, float direction, PropFunc behavior = null)
		{
			return this.SpawnProp(id, regionId, x, y, direction, 1, behavior);
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

		#endregion Props

		#region Spawn

		/// <summary>
		/// Creates creature spawn area.
		/// </summary>
		/// <param name="raceId"></param>
		/// <param name="amount"></param>
		/// <param name="regionId"></param>
		/// <param name="coordinates"></param>
		[Obsolete("Use CreateSpawner instead.", true)]
		protected void CreatureSpawn(int raceId, int amount, int regionId, params int[] coordinates)
		{
			this.CreateSpawner(raceId, amount, regionId, 0, 10, 10, null, coordinates);
		}

		/// <summary>
		/// Creates creature spawn area.
		/// </summary>
		/// <param name="race"></param>
		/// <param name="amount"></param>
		/// <param name="region"></param>
		/// <param name="coordinates"></param>
		protected void CreateSpawner(int race, int amount, int region, int delay = 0, int delayMin = 10, int delayMax = 20, int[] titles = null, int[] coordinates = null)
		{
			ChannelServer.Instance.World.SpawnManager.Add(new CreatureSpawner(race, amount, region, delay, delayMin, delayMax, titles, coordinates));
		}

		/// <summary>
		/// Spawns creature(s)
		/// </summary>
		/// <param name="raceId"></param>
		/// <param name="amount"></param>
		/// <param name="regionId"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		protected List<Creature> Spawn(int raceId, int amount, int regionId, int x, int y)
		{
			return this.Spawn(raceId, amount, regionId, x, y, null);
		}

		/// <summary>
		/// Spawns creature(s)
		/// </summary>
		/// <param name="raceId"></param>
		/// <param name="amount"></param>
		/// <param name="regionId"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="radius">Radius around position for random spawn</param>
		protected List<Creature> Spawn(int raceId, int amount, int regionId, int x, int y, int radius)
		{
			return this.Spawn(raceId, amount, regionId, x, y, radius, true, null);
		}

		/// <summary>
		/// Spawns creature(s)
		/// </summary>
		/// <param name="raceId"></param>
		/// <param name="amount"></param>
		/// <param name="regionId"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="radius">Radius around position for random spawn</param>
		/// <param name="effect">Whether to display spawn effect</param>
		protected List<Creature> Spawn(int raceId, int amount, int regionId, int x, int y, int radius, bool effect)
		{
			return this.Spawn(raceId, amount, regionId, x, y, radius, effect, null);
		}

		/// <summary>
		/// Spawns creature(s)
		/// </summary>
		/// <param name="raceId"></param>
		/// <param name="amount"></param>
		/// <param name="regionId"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="onDeath">Runs when one of the creatures dies</param>
		protected List<Creature> Spawn(int raceId, int amount, int regionId, int x, int y, Action<Creature, Creature> onDeath)
		{
			return this.Spawn(raceId, amount, regionId, x, y, 0, true, onDeath);
		}

		/// <summary>
		/// Spawns creature(s)
		/// </summary>
		/// <param name="raceId"></param>
		/// <param name="amount"></param>
		/// <param name="regionId"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="radius">Radius around position for random spawn</param>
		/// <param name="onDeath">Runs when one of the creatures dies</param>
		protected List<Creature> Spawn(int raceId, int amount, int regionId, int x, int y, int radius, Action<Creature, Creature> onDeath)
		{
			return this.Spawn(raceId, amount, regionId, x, y, 0, true, onDeath);
		}

		/// <summary>
		/// Spawns creature(s)
		/// </summary>
		/// <param name="raceId"></param>
		/// <param name="amount"></param>
		/// <param name="regionId"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="radius">Radius around position for random spawn</param>
		/// <param name="effect">Whether to display spawn effect</param>
		/// <param name="onDeath">Runs when one of the creatures dies</param>
		protected List<Creature> Spawn(int raceId, int amount, int regionId, int x, int y, int radius, bool effect, Action<Creature, Creature> onDeath)
		{
			return this.Spawn(raceId, amount, regionId, new Position(x, y), radius, effect, onDeath);
		}

		/// <summary>
		/// Spawns creature(s)
		/// </summary>
		/// <param name="raceId"></param>
		/// <param name="amount"></param>
		/// <param name="regionId"></param>
		/// <param name="pos"></param>
		/// <param name="radius">Radius around position for random spawn</param>
		/// <param name="effect">Whether to display spawn effect</param>
		/// <param name="onDeath">Runs when one of the creatures dies</param>
		protected List<Creature> Spawn(int raceId, int amount, int regionId, Position pos, int radius, bool effect, Action<Creature, Creature> onDeath)
		{
			var result = new List<Creature>();

			amount = Math2.Clamp(1, 100, amount);

			var rnd = RandomProvider.Get();

			for (int i = 0; i < amount; ++i)
			{
				if (radius > 0)
					pos = pos.GetRandomInRange(radius, rnd);

				var creature = ChannelServer.Instance.World.SpawnManager.Spawn(raceId, regionId, pos.X, pos.Y, true, effect);

				if (onDeath != null)
					creature.Death += onDeath;

				result.Add(creature);
			}

			return result;
		}

		#endregion Spawn

		#region Client Events

		/// <summary>
		/// Adds handler for event.
		/// </summary>
		/// <remarks>
		/// Reads the region id from the event id, the region must exist first,
		/// and it must already contain the event.
		/// </remarks>
		/// <param name="eventId"></param>
		/// <param name="signal"></param>
		/// <param name="onTriggered"></param>
		public void OnClientEvent(long eventId, SignalType signal, Action<Creature, EventData> onTriggered)
		{
			// Get region
			var regionId = (ushort)((eventId >> 32) & ~0xFFFF0000);
			var region = ChannelServer.Instance.World.GetRegion(regionId);
			if (region == null)
			{
				Log.Error("OnClientEvent: Region '{0}' doesn't exist.", regionId);
				return;
			}

			// Get event
			var clientEvent = region.GetClientEvent(eventId);
			if (clientEvent == null)
			{
				Log.Error("OnClientEvent: Client event '{0}' doesn't exist in '{1}'.", eventId, regionId);
				return;
			}

			clientEvent.Handlers.Add(signal, onTriggered);
		}

		#endregion Client Events

		#region Timers

		/// <summary>
		/// Creates timer that runs action once, after x milliseconds.
		/// </summary>
		/// <param name="ms"></param>
		/// <param name="action"></param>
		/// <returns></returns>
		protected Timer SetTimeout(int ms, Action action)
		{
			Timer timer = null;
			timer = new Timer(_ =>
			{
				action();
				GC.KeepAlive(timer);
			}
			, null, ms, Timeout.Infinite);

			return timer;
		}

		/// <summary>
		/// Creates timer that runs action repeatedly, every x milliseconds.
		/// </summary>
		/// <param name="ms"></param>
		/// <param name="action"></param>
		/// <returns></returns>
		protected Timer SetInterval(int ms, Action action)
		{
			Timer timer = null;
			timer = new Timer(_ =>
			{
				action();
				GC.KeepAlive(timer);
			}
			, null, ms, ms);

			return timer;
		}

		/// <summary>
		/// Stops timer by changing due time and period.
		/// </summary>
		/// <param name="timer"></param>
		protected void StopTimer(Timer timer)
		{
			timer.Change(Timeout.Infinite, Timeout.Infinite);
		}

		#endregion Timers
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