// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using Aura.Shared.Util;

namespace Aura.Channel.Scripting.Scripts
{
	public abstract partial class BaseScript : IDisposable
	{
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
					//Log.Error("AutoLoadEvents: Unknown event '{0}' on '{1}.{2}'.", attr.Event, type.Name, method.Name);
					continue;
				}

				try
				{
					eventHandlerInfo.RemoveEventHandler(ChannelServer.Instance.Events, Delegate.CreateDelegate(eventHandlerInfo.EventHandlerType, this, method));
				}
				catch (Exception ex)
				{
					Log.Exception(ex, "AutoLoadEvents: Failed to unsubscribe '{1}.{2}' from '{0}'.", attr.Event, type.Name, method.Name);
				}
			}
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