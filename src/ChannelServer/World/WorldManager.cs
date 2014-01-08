// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Data;
using Aura.Channel.World.Entities;
using Aura.Shared.Util;
using System.Threading;
using Aura.Shared.Mabi;

namespace Aura.Channel.World
{
	public class WorldManager
	{
		public static readonly WorldManager Instance = new WorldManager();

		private Dictionary<int, Region> _regions;

		public int Count { get { return _regions.Count; } }

		private WorldManager()
		{
			_regions = new Dictionary<int, Region>();
		}

		/// <summary>
		/// Initializes world (regions, heartbeat, etc)
		/// </summary>
		public void Initialize()
		{
			if (_initialized)
				throw new Exception("WorldManager should only be initialized once.");

			this.AddRegionsFromData();
			this.SetUpHeartbeat();

			_initialized = true;
		}

		/// <summary>
		/// Adds all regions found in RegionDb.
		/// </summary>
		private void AddRegionsFromData()
		{
			foreach (var region in AuraData.RegionDb.Entries.Values)
				this.AddRegion(region.Id);
		}

		// ------------------------------------------------------------------

		/// <summary>
		/// Due time of the heartbeat timer.
		/// </summary>
		public const int HeartbeatTime = 500;
		public const int Second = 1000, Minute = Second * 60, Hour = Minute * 60;
		public const int ErinnMinute = 1500;

		private bool _initialized;
		private Timer _heartbeatTimer;
		private DateTime _lastHeartbeat;
		private double _secondsTime, _minutesTime, _hoursTime, _erinnTime;

		/// <summary>
		/// Initializes heartbeat timer.
		/// </summary>
		private void SetUpHeartbeat()
		{
			var now = DateTime.Now;

			// Start timer on the next HeartbeatTime
			// (eg on the next full 500 ms) and run it regularly afterwards.
			_heartbeatTimer = new Timer(Heartbeat, null, HeartbeatTime - (now.Ticks / 10000 % HeartbeatTime), HeartbeatTime);
		}

		/// <summary>
		/// Handles regularly occuring events and raises time events.
		/// </summary>
		/// <remarks>
		/// On the first call all time events are raised,
		/// because lastHeartbeat is 0, and the events depend on the time
		/// since the last heartbeat. This also ensures that they aren't
		/// called multiple times.
		/// </remarks>
		private void Heartbeat(object _)
		{
			var now = DateTime.Now;
			var diff = (now - _lastHeartbeat).TotalMilliseconds;

			if (diff != HeartbeatTime && Math.Abs(HeartbeatTime - diff) > HeartbeatTime && diff < 100000000)
			{
				Log.Error("OMG, the server has an irregular heartbeat! ({0})", diff);
			}

			// Seconds event
			if ((_secondsTime += diff) >= Second)
			{
				_secondsTime = 0;
				// Raise seconds tick
			}

			// Minutes event
			if ((_minutesTime += diff) >= Minute)
			{
				_minutesTime = (now.Second * Second + now.Millisecond);
				// Raise minutes tick
			}

			// Hours event
			if ((_hoursTime += diff) >= Hour)
			{
				_hoursTime = (now.Minute * Minute + now.Second * Second + now.Millisecond);
				// Raise hours tick
			}

			// Erinn time event
			if ((_erinnTime += diff) >= ErinnMinute)
			{
				_erinnTime = 0;

				var et = new ErinnTime(now);

				// Erinn daytime event
				if (et.IsDawn || et.IsDusk)
				{
					// Raise Erinn daytime tick
				}

				// Erinn midnight event
				if (et.IsMidnight)
				{
					// Raise Erinn midnight tick
				}
			}

			this.UpdateEntities();

			_lastHeartbeat = now;
		}

		/// <summary>
		/// Updates all entities in all regions.
		/// </summary>
		private void UpdateEntities()
		{
			lock (_regions)
			{
				foreach (var region in _regions.Values)
					region.UpdateEntities();
			}
		}

		// ------------------------------------------------------------------

		/// <summary>
		/// Adds new region with regionId.
		/// </summary>
		/// <param name="regionId"></param>
		public void AddRegion(int regionId)
		{
			lock (_regions)
			{
				if (_regions.ContainsKey(regionId))
				{
					Log.Warning("Region '{0}' already exists.", regionId);
					return;
				}

				_regions.Add(regionId, new Region(regionId));
			}
		}

		/// <summary>
		/// Removes region with RegionId.
		/// </summary>
		/// <param name="regionId"></param>
		public void RemoveRegion(int regionId)
		{
			lock (_regions)
				_regions.Remove(regionId);
		}

		/// <summary>
		/// Returns region by id, or null if it doesn't exist.
		/// </summary>
		/// <param name="regionId"></param>
		/// <returns></returns>
		public Region GetRegion(int regionId)
		{
			Region result;
			lock (_regions)
				_regions.TryGetValue(regionId, out result);
			return result;
		}

		/// <summary>
		/// Returns true if region exists.
		/// </summary>
		/// <param name="regionId"></param>
		/// <returns></returns>
		public bool HasRegion(int regionId)
		{
			return _regions.ContainsKey(regionId);
		}

		/// <summary>
		/// Returns first prop with the given id, from any region,
		/// or null, if none was found.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public Prop GetProp(long id)
		{
			foreach (var region in _regions.Values)
			{
				var prop = region.GetProp(id);
				if (prop != null)
					return prop;
			}

			return null;
		}

		/// <summary>
		/// Returns player creature with the given name, or null.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public PlayerCreature GetPlayer(string name)
		{
			foreach (var region in _regions.Values)
			{
				var creature = region.GetPlayer(name);
				if (creature != null)
					return creature;
			}

			return null;
		}
	}
}
