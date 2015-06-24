// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Aura.Channel.World.Entities;
using Aura.Data;
using Aura.Mabi;
using Aura.Shared.Util;
using Aura.Channel.Util;
using Aura.Channel.Network.Sending;
using Aura.Shared.Network;
using System.Threading.Tasks;
using Aura.Mabi.Network;
using Aura.Mabi.Const;
using Aura.Channel.World.Dungeons;

namespace Aura.Channel.World
{
	public class WorldManager
	{
		private Dictionary<int, Region> _regions;

		public DungeonManager DungeonManager { get; private set; }
		public DynamicRegionManager DynamicRegions { get; private set; }
		public SpawnManager SpawnManager { get; private set; }

		/// <summary>
		/// Returns number of regions.
		/// </summary>
		public int Count { get { return _regions.Count; } }

		public WorldManager()
		{
			_regions = new Dictionary<int, Region>();

			this.DungeonManager = new DungeonManager();
			this.DynamicRegions = new DynamicRegionManager();
			this.SpawnManager = new SpawnManager();
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
			Parallel.ForEach(AuraData.RegionDb.Entries.Values, region => this.AddRegion(region.Id));
		}

		// ------------------------------------------------------------------

		/// <summary>
		/// Due time of the heartbeat timer.
		/// </summary>
		public const int HeartbeatTime = 500;
		public const int Second = 1000, Minute = Second * 60, Hour = Minute * 60;
		public const int ErinnMinute = 1500, ErinnHour = ErinnMinute * 60, ErinnDay = ErinnHour * 24;

		private bool _initialized;
		private Timer _heartbeatTimer;
		private DateTime _lastHeartbeat;
		private double _secondsTime, _minutesTime, _hoursTime, _erinnTime;
		private int _mabiTickCount;

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
			var now = new ErinnTime(DateTime.Now);
			var diff = (now.DateTime - _lastHeartbeat).TotalMilliseconds;

			if (diff != HeartbeatTime && Math.Abs(HeartbeatTime - diff) > HeartbeatTime && diff < 100000000)
			{
				Log.Debug("OMG, the server has an irregular heartbeat! ({0})", diff.ToInvariant());
			}

			// Seconds event
			if ((_secondsTime += diff) >= Second)
			{
				_secondsTime = 0;
				ChannelServer.Instance.Events.OnSecondsTimeTick(now);
			}

			// Minutes event
			if ((_minutesTime += diff) >= Minute)
			{
				_minutesTime = (now.DateTime.Second * Second + now.DateTime.Millisecond);
				ChannelServer.Instance.Events.OnMinutesTimeTick(now);

				// Mabi tick event
				if (++_mabiTickCount >= 5)
				{
					ChannelServer.Instance.Events.OnMabiTick(now);
					_mabiTickCount = 0;
				}
			}

			// Hours event
			if ((_hoursTime += diff) >= Hour)
			{
				_hoursTime = (now.DateTime.Minute * Minute + now.DateTime.Second * Second + now.DateTime.Millisecond);
				ChannelServer.Instance.Events.OnHoursTimeTick(now);
			}

			// Erinn time event
			if ((_erinnTime += diff) >= ErinnMinute)
			{
				_erinnTime = 0;
				ChannelServer.Instance.Events.OnErinnTimeTick(now);

				// TODO: Dawn/Dusk/Midnight wouldn't be called if the server had a 500+ ms hickup.

				// Erinn daytime event
				if (now.IsDawn || now.IsDusk)
				{
					ChannelServer.Instance.Events.OnErinnDaytimeTick(now);
					this.OnErinnDaytimeTick(now);
				}

				// Erinn midnight event
				if (now.IsMidnight)
					ChannelServer.Instance.Events.OnErinnMidnightTick(now);
			}

			this.UpdateEntities();

			_lastHeartbeat = now.DateTime;
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

		/// <summary>
		/// Broadcasts Eweca notice, called at 6:00 and 18:00.
		/// </summary>
		/// <param name="now"></param>
		private void OnErinnDaytimeTick(ErinnTime now)
		{
			var notice = now.IsNight
				? Localization.Get("Eweca is rising.\nMana is starting to fill the air all around.")
				: Localization.Get("Eweca has disappeared.\nThe surrounding Mana is starting to fade away.");
			Send.Notice(NoticeType.MiddleTop, notice);
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
			}

			var region = new NormalRegion(regionId);
			lock (_regions)
				_regions.Add(regionId, region);
		}

		/// <summary>
		/// Adds region to world.
		/// </summary>
		/// <param name="region"></param>
		public void AddRegion(Region region)
		{
			lock (_regions)
			{
				if (_regions.ContainsKey(region.Id))
				{
					Log.Warning("Region '{0}' already exists.", region.Id);
					return;
				}

				_regions.Add(region.Id, region);
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
		/// Returns region by name, or null if it doesn't exist.
		/// </summary>
		/// <param name="regionId"></param>
		/// <returns></returns>
		public Region GetRegion(string regionName)
		{
			lock (_regions)
				return _regions.Values.FirstOrDefault(a => a.Name == regionName);
		}

		/// <summary>
		/// Returns true if region exists.
		/// </summary>
		/// <param name="regionId"></param>
		/// <returns></returns>
		public bool HasRegion(int regionId)
		{
			lock (_regions)
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
			var regionId = (int)((id >> 32) & ~0xFFFF0000);
			var region = this.GetRegion(regionId);
			if (region == null)
				return null;

			return region.GetProp(id);
		}

		/// <summary>
		/// Returns player creature with the given name, or null.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public PlayerCreature GetPlayer(string name)
		{
			lock (_regions)
				return _regions.Values.Select(region => region.GetPlayer(name)).FirstOrDefault(creature => creature != null);
		}

		/// <summary>
		/// Returns all players in all regions.
		/// </summary>
		/// <returns></returns>
		public List<Creature> GetAllPlayers()
		{
			var result = new List<Creature>();

			lock (_regions)
				foreach (var region in _regions.Values)
					result.AddRange(region.GetAllPlayers());

			return result;
		}

		/// <summary>
		/// Returns amount of players in all regions.
		/// </summary>
		/// <returns></returns>
		public int CountPlayers()
		{
			lock (_regions)
				return _regions.Values.Sum(region => region.CountPlayers());
		}

		/// <summary>
		/// Returns creature from any region by id, or null.
		/// </summary>
		/// <param name="entityId"></param>
		/// <returns></returns>
		public Creature GetCreature(long entityId)
		{
			lock (_regions)
				return _regions.Values.Select(region => region.GetCreature(entityId)).FirstOrDefault(creature => creature != null);
		}

		/// <summary>
		/// Returns creature from any region by name, or null.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public Creature GetCreature(string name)
		{
			lock (_regions)
				return _regions.Values.Select(region => region.GetCreature(name)).FirstOrDefault(creature => creature != null);
		}

		/// <summary>
		/// Returns list of all creatures in all regions.
		/// </summary>
		public ICollection<Creature> GetAllCreatures()
		{
			var result = new List<Creature>();

			lock (_regions)
				foreach (var region in _regions.Values)
					result.AddRange(region.GetCreatures(_ => true));

			return result;
		}

		/// <summary>
		/// Returns NPC from any region by id, or null.
		/// </summary>
		/// <param name="entityId"></param>
		/// <returns></returns>
		public NPC GetNpc(long entityId)
		{
			lock (_regions)
				return _regions.Values.Select(region => region.GetNpc(entityId)).FirstOrDefault(creature => creature != null);
		}

		/// <summary>
		/// Returns NPC from any region by name, or null.
		/// </summary>
		/// <param name="entityId"></param>
		/// <returns></returns>
		public NPC GetNpc(string name)
		{
			lock (_regions)
				return (NPC)_regions.Values.Select(region => region.GetCreature(name)).FirstOrDefault(creature => creature != null && creature is NPC);
		}

		/// <summary>
		/// Returns collection of all good, normal NPCs.
		/// </summary>
		/// <returns></returns>
		public ICollection<Creature> GetAllGoodNpcs()
		{
			var result = new List<Creature>();

			lock (_regions)
				foreach (var region in _regions.Values)
					region.GetAllGoodNpcs(ref result);

			return result;
		}

		/// <summary>
		/// Removes all NPCs, props, etc from all regions.
		/// </summary>
		public void RemoveScriptedEntities()
		{
			// Make a copy of region list, the following loop could modify
			// the current regions.
			List<Region> regions;
			lock (_regions)
				regions = _regions.Values.ToList();

			foreach (var region in regions)
				region.RemoveScriptedEntities();
		}

		/// <summary>
		/// Broadcasts packet in all regions.
		/// </summary>
		/// <param name="packet"></param>
		public void Broadcast(Packet packet)
		{
			lock (_regions)
				foreach (var region in _regions.Values)
					region.Broadcast(packet);
		}
	}
}
