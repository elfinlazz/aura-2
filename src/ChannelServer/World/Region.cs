// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using Aura.Channel.Network;
using Aura.Channel.Network.Sending;
using Aura.Channel.Util;
using Aura.Channel.World.Entities;
using Aura.Data;
using Aura.Mabi.Const;
using Aura.Shared.Network;
using Aura.Shared.Util;
using System.Threading;
using Aura.Data.Database;
using Boo.Lang.Compiler.TypeSystem;
using System.Drawing;
using Aura.Channel.Scripting.Scripts;
using Aura.Mabi.Network;

namespace Aura.Channel.World
{
	public abstract class Region
	{
		// TODO: Data?
		public const int VisibleRange = 3000;

		public static readonly Region Limbo = new Limbo();

		protected ReaderWriterLockSlim _creaturesRWLS, _propsRWLS, _clientEventsRWLS, _itemsRWLS;
		private Dictionary<int, int> _propIds;

		protected Dictionary<long, Creature> _creatures;
		protected Dictionary<long, Prop> _props;
		protected Dictionary<long, ClientEvent> _clientEvents;
		protected Dictionary<long, Item> _items;

		protected HashSet<ChannelClient> _clients;

		public RegionInfoData RegionInfoData { get; protected set; }

		/// <summary>
		/// Region's name
		/// </summary>
		public string Name { get; protected set; }

		/// <summary>
		/// Region's id
		/// </summary>
		public int Id { get; protected set; }

		/// <summary>
		/// Manager for blocking objects in the region.
		/// </summary>
		public RegionCollision Collisions { get; protected set; }

		/// <summary>
		/// Returns true if region is a dynamic region, judged by its id.
		/// </summary>
		public bool IsDynamic { get { return Math2.Between(this.Id, MabiId.DynamicRegions, MabiId.DynamicRegions + 5000); } }

		/// <summary>
		/// Returns true if region is a dungeon region, judged by its id.
		/// </summary>
		public bool IsDungeon { get { return Math2.Between(this.Id, MabiId.DungeonRegions, MabiId.DungeonRegions + 10000); } }

		/// <summary>
		/// Returns true if region is temporary, i.e. a dungeon or a dynamic region.
		/// </summary>
		public bool IsTemp { get { return (this.IsDynamic || this.IsDungeon); } }

		/// <summary>
		/// Initializes class.
		/// </summary>
		/// <param name="regionId"></param>
		protected Region(int regionId)
		{
			_creaturesRWLS = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
			_propsRWLS = new ReaderWriterLockSlim();
			_clientEventsRWLS = new ReaderWriterLockSlim();
			_itemsRWLS = new ReaderWriterLockSlim();

			_propIds = new Dictionary<int, int>();

			this.Id = regionId;

			_creatures = new Dictionary<long, Creature>();
			_props = new Dictionary<long, Prop>();
			_clientEvents = new Dictionary<long, ClientEvent>();
			_items = new Dictionary<long, Item>();

			_clients = new HashSet<ChannelClient>();

			this.Collisions = new RegionCollision();
		}

		/// <summary>
		/// Adds all props found in the client for this region and creates a list
		/// of areas.
		/// </summary>
		protected void InitializeFromData()
		{
			if (this.RegionInfoData == null || this.RegionInfoData.Areas == null)
				return;

			this.LoadProps();
			this.LoadClientEvents();
		}

		/// <summary>
		/// Adds all props found in the client for this region.
		/// </summary>
		protected void LoadProps()
		{
			foreach (var area in this.RegionInfoData.Areas)
			{
				foreach (var prop in area.Props.Values)
				{
					var add = new Prop(prop.EntityId, prop.Id, this.Id, (int)prop.X, (int)prop.Y, prop.Direction, prop.Scale, 0, "", "", "");

					// Add copy of extensions
					foreach (var para in prop.Parameters)
						add.Extensions.Add(new PropExtension(para.SignalType, para.EventType, para.Name, 0));

					// Add drop behaviour if drop type exists
					var dropType = prop.GetDropType();
					if (dropType != -1) add.Behavior = Prop.GetDropBehavior(dropType);

					// Replace default shapes with the ones loaded from region.
					add.Shapes.Clear();
					add.Shapes.AddRange(prop.Shapes.Select(a => a.GetPoints(0, 0, 0)));

					this.AddProp(add);
				}
			}
		}

		/// <summary>
		/// Adds all props found in the client for this region.
		/// </summary>
		protected void LoadClientEvents()
		{
			foreach (var area in this.RegionInfoData.Areas)
			{
				foreach (var clientEvent in area.Events.Values)
				{
					var add = new ClientEvent(clientEvent.Id, clientEvent);
					this.AddClientEvent(add);
				}
			}
		}

		/// <summary>
		/// Returns event by id or null if it doesn't exist.
		/// </summary>
		/// <param name="eventId"></param>
		/// <returns></returns>
		public ClientEvent GetClientEvent(long eventId)
		{
			ClientEvent result;

			_clientEventsRWLS.EnterReadLock();
			try
			{
				_clientEvents.TryGetValue(eventId, out result);
			}
			finally
			{
				_clientEventsRWLS.ExitReadLock();
			}

			return result;
		}

		/// <summary>
		/// Returns event by name or null if it doesn't exist.
		/// </summary>
		/// <param name="eventId"></param>
		/// <returns></returns>
		public ClientEvent GetClientEvent(string eventName)
		{
			return this.GetClientEvent(a => a.Data.Name == eventName);
		}

		/// <summary>
		/// Adds client event to region.
		/// </summary>
		/// <param name="clientEvent"></param>
		private void AddClientEvent(ClientEvent clientEvent)
		{
			_clientEventsRWLS.EnterWriteLock();
			try
			{
				if (_clientEvents.ContainsKey(clientEvent.EntityId))
					throw new ArgumentException("A client event with id '" + clientEvent.EntityId.ToString("X16") + "' already exists.");

				_clientEvents.Add(clientEvent.EntityId, clientEvent);
			}
			finally
			{
				_clientEventsRWLS.ExitWriteLock();
			}

			// Add collisions
			this.Collisions.Add(clientEvent);
		}

		/// <summary>
		/// Returns first event that matches the predicate.
		/// </summary>
		/// <param name="eventId"></param>
		/// <returns></returns>
		public ClientEvent GetClientEvent(Func<ClientEvent, bool> predicate)
		{
			_clientEventsRWLS.EnterReadLock();
			try
			{
				return _clientEvents.Values.FirstOrDefault(predicate);
			}
			finally
			{
				_clientEventsRWLS.ExitReadLock();
			}
		}


		/// <summary>
		/// Returns a list of events that start with the given path,
		/// e.g. "Uladh_main/field_Tir_S_aa/fish_tircho_stream_", to get all
		/// fishing events starting with that name.
		/// </summary>
		/// <param name="eventPath"></param>
		/// <returns></returns>
		public List<ClientEvent> GetMatchingEvents(string eventPath)
		{
			var split = eventPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
			if (split.Length != 3)
				throw new ArgumentException("Invalid event path, expected 3 segments.");

			var result = new List<ClientEvent>();
			//var eventName = split[2];

			// We either have to look for the name or the path, but it's
			// technically possible that two areas have events with the same
			// name, so the path is safer.

			_clientEventsRWLS.EnterReadLock();
			try
			{
				// TODO: Cache

				var events = _clientEvents.Values.Where(a => a.Data.Path.StartsWith(eventPath));
				result.AddRange(events);
			}
			finally
			{
				_clientEventsRWLS.ExitReadLock();
			}

			return result;
		}

		/// <summary>
		/// Returns id of area at the given coordinates and adjusts it if region is dynamic, or 0 if area wasn't found.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public int GetAreaId(int x, int y)
		{
			var areaId = 0;

			foreach (var area in this.RegionInfoData.Areas)
			{
				if (x >= Math.Min(area.X1, area.X2) && x < Math.Max(area.X1, area.X2) && y >= Math.Min(area.Y1, area.Y2) && y < Math.Max(area.Y1, area.Y2))
					areaId = area.Id;
			}

			return areaId;
		}

		/// <summary>
		/// Updates all entites, removing dead ones, updating visibility, etc.
		/// </summary>
		public void UpdateEntities()
		{
			this.RemoveOverdueEntities();
			this.UpdateVisibility();
		}

		/// <summary>
		/// Removes expired entities. 
		/// </summary>
		private void RemoveOverdueEntities()
		{
			var now = DateTime.Now;

			// Get all expired entities
			var disappear = new List<Entity>();

			_creaturesRWLS.EnterReadLock();
			try
			{
				disappear.AddRange(_creatures.Values.Where(a => a.DisappearTime > DateTime.MinValue && a.DisappearTime < now));
			}
			finally
			{
				_creaturesRWLS.ExitReadLock();
			}

			_itemsRWLS.EnterReadLock();
			try
			{
				disappear.AddRange(_items.Values.Where(a => a.DisappearTime > DateTime.MinValue && a.DisappearTime < now));
			}
			finally
			{
				_itemsRWLS.ExitReadLock();
			}

			_propsRWLS.EnterReadLock();
			try
			{
				disappear.AddRange(_props.Values.Where(a => a.DisappearTime > DateTime.MinValue && a.DisappearTime < now));
			}
			finally
			{
				_propsRWLS.ExitReadLock();
			}

			// Remove them from the region
			foreach (var entity in disappear)
				entity.Disappear();
		}

		/// <summary>
		/// Updates visible entities on all clients.
		/// </summary>
		private void UpdateVisibility()
		{
			_creaturesRWLS.EnterReadLock();
			try
			{
				foreach (var creature in _creatures.Values)
				{
					var pc = creature as PlayerCreature;

					// Only update player creatures
					if (pc == null)
						continue;

					pc.LookAround();
				}
			}
			finally
			{
				_creaturesRWLS.ExitReadLock();
			}
		}

		/// <summary>
		/// Returns a list of visible entities, from the view point of creature.
		/// </summary>
		/// <param name="creature"></param>
		public List<Entity> GetVisibleEntities(Creature creature)
		{
			var result = new List<Entity>();
			var pos = creature.GetPosition();

			// Players don't see anything else while they're watching a cutscene.
			// This automatically (de)spawns entities (from LookAround) while watching.
			if (creature.Temp.CurrentCutscene == null || !creature.IsPlayer)
			{
				_creaturesRWLS.EnterReadLock();
				try
				{
					result.AddRange(_creatures.Values.Where(a => a.GetPosition().InRange(pos, VisibleRange) && !a.Conditions.Has(ConditionsA.Invisible)));
				}
				finally
				{
					_creaturesRWLS.ExitReadLock();
				}

				_itemsRWLS.EnterReadLock();
				try
				{
					result.AddRange(_items.Values.Where(a => a.GetPosition().InRange(pos, VisibleRange)));
				}
				finally
				{
					_itemsRWLS.ExitReadLock();
				}
			}

			_propsRWLS.EnterReadLock();
			try
			{
				// Send all props of a region, so they're visible from afar.
				// While client props are visible as well they don't have to
				// be sent, the client already has them.
				//
				// ^^^^^^^^^^^^^^^^^^ This caused a bug with client prop states
				// not being set until the prop was used by a player while
				// the creature was in the region (eg windmill) so we'll count
				// all props as visible. -- Xcelled
				//
				// ^^^^^^^^^^^^^^^^^^ That causes a huge EntitiesAppear packet,
				// because there are thousands of client props. We only need
				// the ones that make a difference. Added check for
				// state and XML. [exec]

				result.AddRange(_props.Values.Where(a => a.ServerSide || a.ModifiedClientSide));
			}
			finally
			{
				_propsRWLS.ExitReadLock();
			}

			return result;
		}

		/// <summary>
		/// Adds creature to region, sends EntityAppears.
		/// </summary>
		public void AddCreature(Creature creature)
		{
			//if (creature.Region != Region.Limbo)
			//	creature.Region.RemoveCreature(creature);

			_creaturesRWLS.EnterWriteLock();
			try
			{
				if (_creatures.ContainsKey(creature.EntityId))
					throw new ArgumentException("A creature with id '" + creature.EntityId.ToString("X16") + "' already exists.");

				_creatures.Add(creature.EntityId, creature);
			}
			finally
			{
				_creaturesRWLS.ExitWriteLock();
			}

			creature.Region = this;
			ChannelServer.Instance.Events.OnPlayerEntersRegion(creature);

			// Save reference to client if it's mainly controlling this creature.
			if (creature.Client.Controlling == creature)
			{
				lock (_clients)
					_clients.Add(creature.Client);
			}

			// Send appear packets, so there's no delay.
			Send.EntityAppears(creature);

			// Remove Spawned state, so effect only plays the first time.
			// This probably only works because of the EntityAppears above,
			// otherwise the state would be gone by the time LookAround
			// kicks in. Maybe we need a better solution.
			creature.State &= ~CreatureStates.Spawned;

			//if (creature.EntityId < MabiId.Npcs)
			//	Log.Status("Creatures currently in region {0}: {1}", this.Id, _creatures.Count);
		}

		/// <summary>
		/// Removes creature from region, sends EntityDisappears.
		/// </summary>
		public virtual void RemoveCreature(Creature creature)
		{
			_creaturesRWLS.EnterWriteLock();
			try
			{
				_creatures.Remove(creature.EntityId);
			}
			finally
			{
				_creaturesRWLS.ExitWriteLock();
			}

			// TODO: Technically not required? Handled by LookAround.
			Send.EntityDisappears(creature);

			ChannelServer.Instance.Events.OnPlayerLeavesRegion(creature);

			// Update visible entities before leaving the region, so the client
			// gets and up-to-date list.
			var playerCreature = creature as PlayerCreature;
			if (playerCreature != null)
				playerCreature.LookAround();

			creature.Region = Region.Limbo;

			if (creature.Client.Controlling == creature)
				lock (_clients)
					_clients.Remove(creature.Client);

			//if (creature.EntityId < MabiId.Npcs)
			//	Log.Status("Creatures currently in region {0}: {1}", this.Id, _creatures.Count);
		}

		/// <summary>
		/// Returns creature by entityId, or null, if it doesn't exist.
		/// </summary>
		public Creature GetCreature(long entityId)
		{
			Creature creature;

			_creaturesRWLS.EnterReadLock();
			try
			{
				_creatures.TryGetValue(entityId, out creature);
			}
			finally
			{
				_creaturesRWLS.ExitReadLock();
			}

			return creature;
		}

		/// <summary>
		/// Returns true if creature with given entity id exists.
		/// </summary>
		/// <param name="entityId"></param>
		/// <returns></returns>
		public bool CreatureExists(long entityId)
		{
			_creaturesRWLS.EnterReadLock();
			try
			{
				return _creatures.ContainsKey(entityId);
			}
			finally
			{
				_creaturesRWLS.ExitReadLock();
			}
		}

		/// <summary>
		/// Returns list of creatures that match predicate.
		/// </summary>
		public ICollection<Creature> GetCreatures(Func<Creature, bool> predicate)
		{
			_creaturesRWLS.EnterReadLock();
			try
			{
				return _creatures.Values.Where(predicate).ToList();
			}
			finally
			{
				_creaturesRWLS.ExitReadLock();
			}
		}

		/// <summary>
		/// Returns list of creatures that match predicate.
		/// </summary>
		public ICollection<NPC> GetNpcs(Func<NPC, bool> predicate)
		{
			var result = new List<NPC>();

			_creaturesRWLS.EnterReadLock();
			try
			{
				foreach (var creature in _creatures.Values)
				{
					var npc = creature as NPC;
					if (npc == null || !predicate(npc))
						continue;

					result.Add(npc);
				}
			}
			finally
			{
				_creaturesRWLS.ExitReadLock();
			}

			return result;
		}

		/// <summary>
		/// Returns creature by name, or null, if it doesn't exist.
		/// </summary>
		public Creature GetCreature(string name)
		{
			_creaturesRWLS.EnterReadLock();
			try
			{
				return _creatures.Values.FirstOrDefault(a => a.Name == name);
			}
			finally
			{
				_creaturesRWLS.ExitReadLock();
			}
		}

		/// <summary>
		/// Returns NPC by entityId, or null, if no NPC with that id exists.
		/// </summary>
		public NPC GetNpc(long entityId)
		{
			return this.GetCreature(entityId) as NPC;
		}

		/// <summary>
		/// Returns NPC by entity id, throws SevereViolation exception if
		/// NPC doesn't exist.
		/// </summary>
		/// <param name="entityId"></param>
		/// <returns></returns>
		public NPC GetNpcSafe(long entityId)
		{
			var npc = this.GetNpc(entityId);

			if (npc == null)
				throw new SevereViolation("Tried to get a nonexistant NPC");

			return npc;
		}

		/// <summary>
		/// Returns creature by entity id, throws SevereViolation exception if
		/// creature doesn't exist.
		/// </summary>
		/// <param name="entityId"></param>
		/// <returns></returns>
		public Creature GetCreatureSafe(long entityId)
		{
			var creature = this.GetCreature(entityId);

			if (creature == null)
				throw new SevereViolation("Tried to get a nonexistant creature");

			return creature;
		}

		/// <summary>
		/// Returns first player creature with the given name, or null.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public PlayerCreature GetPlayer(string name)
		{
			_creaturesRWLS.EnterReadLock();
			try
			{
				return _creatures.Values.FirstOrDefault(a => a is PlayerCreature && a.Name == name) as PlayerCreature;
			}
			finally
			{
				_creaturesRWLS.ExitReadLock();
			}
		}

		/// <summary>
		/// Returns all player creatures in range.
		/// </summary>
		/// <param name="pos"></param>
		/// <param name="range"></param>
		/// <returns></returns>
		public List<Creature> GetPlayersInRange(Position pos, int range = VisibleRange)
		{
			_creaturesRWLS.EnterReadLock();
			try
			{
				return _creatures.Values.Where(a => a.IsPlayer && a.GetPosition().InRange(pos, range)).ToList();
			}
			finally
			{
				_creaturesRWLS.ExitReadLock();
			}
		}

		/// <summary>
		/// Returns all player creatures in region.
		/// </summary>
		/// <returns></returns>
		public List<Creature> GetAllPlayers()
		{
			_creaturesRWLS.EnterReadLock();
			try
			{
				return _creatures.Values.Where(a => a.IsPlayer).ToList();
			}
			finally
			{
				_creaturesRWLS.ExitReadLock();
			}
		}

		/// <summary>
		/// Returns amount of players in region.
		/// </summary>
		/// <returns></returns>
		public int CountPlayers()
		{
			_creaturesRWLS.EnterReadLock();
			try
			{
				// Count any player creatures that are directly controlled,
				// filtering creatures with masters (pets/partners).
				return _creatures.Values.Count(a => a is PlayerCreature && a.Master == null);
			}
			finally
			{
				_creaturesRWLS.ExitReadLock();
			}
		}

		/// <summary>
		/// Returns all visible creatures in range of entity, excluding itself.
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="range"></param>
		/// <returns></returns>
		public ICollection<Creature> GetVisibleCreaturesInRange(Entity entity, int range = VisibleRange)
		{
			return this.GetCreatures(a => a != entity && a.GetPosition().InRange(entity.GetPosition(), range) && !a.Conditions.Has(ConditionsA.Invisible));
		}

		/// <summary>
		///  Spawns prop, sends EntityAppears.
		/// </summary>
		public void AddProp(Prop prop)
		{
			// Generate prop id if it doesn't have one yet.
			if (prop.EntityId == 0)
				prop.EntityId = this.GetNewPropEntityId(prop);

			_propsRWLS.EnterWriteLock();
			try
			{
				if (_props.ContainsKey(prop.EntityId))
					throw new ArgumentException("A prop with id '" + prop.EntityId.ToString("X16") + "' already exists.");

				_props.Add(prop.EntityId, prop);
			}
			finally
			{
				_propsRWLS.ExitWriteLock();
			}

			prop.Region = this;

			// Add collisions
			this.Collisions.Add(prop);

			Send.EntityAppears(prop);
		}

		/// <summary>
		/// Generates entity id for prop.
		/// </summary>
		/// <param name="prop"></param>
		/// <returns></returns>
		private long GetNewPropEntityId(Prop prop)
		{
			var regionId = this.Id;
			var areaId = this.GetAreaId((int)prop.Info.X, (int)prop.Info.Y);
			var propId = 0;

			lock (_propIds)
			{
				if (!_propIds.ContainsKey(areaId))
					_propIds[areaId] = 1;

				propId = _propIds[areaId]++;

				if (propId >= ushort.MaxValue)
					throw new Exception("Max prop id reached in region '" + regionId + "', area '" + areaId + "'.");
			}

			var result = MabiId.ServerProps;
			result |= (long)regionId << 32;
			result |= (long)areaId << 16;
			result |= (ushort)propId;

			return result;
		}

		/// <summary>
		/// Despawns prop, sends EntityDisappears.
		/// </summary>
		public void RemoveProp(Prop prop)
		{
			if (!prop.ServerSide)
			{
				Log.Error("RemoveProp: Client side props can't be removed.");
				prop.DisappearTime = DateTime.MinValue;
				return;
			}

			_propsRWLS.EnterWriteLock();
			try
			{
				_props.Remove(prop.EntityId);
			}
			finally
			{
				_propsRWLS.ExitWriteLock();
			}

			// Remove collisions
			this.Collisions.Remove(prop.EntityId);

			Send.PropDisappears(prop);

			prop.Region = Region.Limbo;
		}

		/// <summary>
		/// Returns prop or null.
		/// </summary>
		public Prop GetProp(long entityId)
		{
			Prop result;

			_propsRWLS.EnterReadLock();
			try
			{
				_props.TryGetValue(entityId, out result);
			}
			finally
			{
				_propsRWLS.ExitReadLock();
			}

			return result;
		}

		/// <summary>
		/// Returns prop or null.
		/// </summary>
		public IList<Prop> GetProps(Func<Prop, bool> predicate)
		{
			var result = new List<Prop>();

			_propsRWLS.EnterReadLock();
			try
			{
				result.AddRange(_props.Values.Where(predicate));
			}
			finally
			{
				_propsRWLS.ExitReadLock();
			}

			return result;
		}

		/// <summary>
		///  Adds item, sends EntityAppears.
		/// </summary>
		public void AddItem(Item item)
		{
			_itemsRWLS.EnterWriteLock();
			try
			{
				if (_items.ContainsKey(item.EntityId))
					throw new ArgumentException("An item with id '" + item.EntityId.ToString("X16") + "' already exists.");

				_items.Add(item.EntityId, item);
			}
			finally
			{
				_itemsRWLS.ExitWriteLock();
			}

			item.Region = this;

			Send.EntityAppears(item);
		}

		/// <summary>
		/// Despawns item, sends EntityDisappears.
		/// </summary>
		public void RemoveItem(Item item)
		{
			_itemsRWLS.EnterWriteLock();
			try
			{
				_items.Remove(item.EntityId);
			}
			finally
			{
				_itemsRWLS.ExitWriteLock();
			}

			Send.EntityDisappears(item);

			item.Region = Region.Limbo;
		}

		/// <summary>
		/// Returns item or null.
		/// </summary>
		public Item GetItem(long entityId)
		{
			Item result;

			_itemsRWLS.EnterReadLock();
			try
			{
				_items.TryGetValue(entityId, out result);
			}
			finally
			{
				_itemsRWLS.ExitReadLock();
			}

			return result;
		}

		/// <summary>
		/// Returns a list of all items on the floor.
		/// </summary>
		public List<Item> GetAllItems()
		{
			List<Item> result;

			_itemsRWLS.EnterReadLock();
			try
			{
				result = new List<Item>(_items.Values);
			}
			finally
			{
				_itemsRWLS.ExitReadLock();
			}

			return result;
		}

		/// <summary>
		/// Returns new list of all entities within range of source.
		/// </summary>
		public List<Entity> GetEntitiesInRange(Entity source, int range = -1)
		{
			if (range < 0)
				range = VisibleRange;

			var result = new List<Entity>();

			_creaturesRWLS.EnterReadLock();
			try
			{
				result.AddRange(_creatures.Values.Where(a => a.GetPosition().InRange(source.GetPosition(), range)));
			}
			finally
			{
				_creaturesRWLS.ExitReadLock();
			}

			_itemsRWLS.EnterReadLock();
			try
			{
				result.AddRange(_items.Values.Where(a => a.GetPosition().InRange(source.GetPosition(), range)));
			}
			finally
			{
				_itemsRWLS.ExitReadLock();
			}

			_propsRWLS.EnterReadLock();
			try
			{
				// All props are visible, but not all of them are in range.
				result.AddRange(_props.Values.Where(a => a.GetPosition().InRange(source.GetPosition(), range)));
			}
			finally
			{
				_propsRWLS.ExitReadLock();
			}

			return result;
		}

		/// <summary>
		/// Returns new list of all creatures within range of position.
		/// </summary>
		public List<Creature> GetCreaturesInRange(Position pos, int range)
		{
			var result = new List<Creature>();

			_creaturesRWLS.EnterReadLock();
			try
			{
				result.AddRange(_creatures.Values.Where(a => a.GetPosition().InRange(pos, range)));
			}
			finally
			{
				_creaturesRWLS.ExitReadLock();
			}

			return result;
		}

		/// <summary>
		/// Returns new list of all creatures within the specified polygon.
		/// </summary>
		public List<Creature> GetCreaturesInPolygon(params Point[] points)
		{
			var result = new List<Creature>();

			_creaturesRWLS.EnterReadLock();
			try
			{
				result.AddRange(_creatures.Values.Where(a => a.GetPosition().InPolygon(points)));
			}
			finally
			{
				_creaturesRWLS.ExitReadLock();
			}

			return result;
		}

		/// <summary>
		/// Removes all scripted entites from this region.
		/// </summary>
		public void RemoveScriptedEntities()
		{
			// Get NPCs
			var npcs = new List<Creature>();
			_creaturesRWLS.EnterReadLock();
			try { npcs.AddRange(_creatures.Values.Where(a => a is NPC)); }
			finally { _creaturesRWLS.ExitReadLock(); }

			// Get server side props
			var props = new List<Prop>();
			_propsRWLS.EnterReadLock();
			try { props.AddRange(_props.Values.Where(a => a.ServerSide)); }
			finally { _propsRWLS.ExitReadLock(); }

			// Remove all
			foreach (var npc in npcs) { npc.Dispose(); this.RemoveCreature(npc); }
			foreach (var prop in props) this.RemoveProp(prop);
		}

		/// <summary>
		/// Activates AIs in range of the movement path.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="from"></param>
		/// <param name="to"></param>
		public void ActivateAis(Creature creature, Position from, Position to)
		{
			// Bounding rectangle
			var minX = Math.Min(from.X, to.X) - VisibleRange;
			var minY = Math.Min(from.Y, to.Y) - VisibleRange;
			var maxX = Math.Max(from.X, to.X) + VisibleRange;
			var maxY = Math.Max(from.Y, to.Y) + VisibleRange;

			// Activation
			_creaturesRWLS.EnterReadLock();
			try
			{
				foreach (var npc in _creatures.Values.OfType<NPC>())
				{
					if (npc.AI == null)
						continue;

					var pos = npc.GetPosition();
					if (!(pos.X >= minX && pos.X <= maxX && pos.Y >= minY && pos.Y <= maxY))
						continue;

					var time = (from.GetDistance(to) / creature.GetSpeed()) * 1000;

					npc.AI.Activate(time);
				}
			}
			finally
			{
				_creaturesRWLS.ExitReadLock();
			}
		}

		/// <summary>
		/// Returns amount of creatures of race that are targetting target
		/// in this region.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="raceId"></param>
		/// <returns></returns>
		public int CountAggro(Creature target, int raceId)
		{
			_creaturesRWLS.EnterReadLock();
			try
			{
				return _creatures.Values.OfType<NPC>().Count(npc =>
					!npc.IsDead &&
					npc.AI != null &&
					npc.AI.State == AiScript.AiState.Aggro &&
					npc.RaceId == raceId &&
					npc.Target == target
				);
			}
			finally
			{
				_creaturesRWLS.ExitReadLock();
			}
		}

		/// <summary>
		/// Returns amount of creatures of race that are targetting target
		/// in this region.
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public int CountAggro(Creature target)
		{
			_creaturesRWLS.EnterReadLock();
			try
			{
				return _creatures.Values.OfType<NPC>().Count(npc =>
					!npc.IsDead &&
					npc.AI != null &&
					npc.AI.State == AiScript.AiState.Aggro &&
					npc.Target == target
				);
			}
			finally
			{
				_creaturesRWLS.ExitReadLock();
			}
		}

		/// <summary>
		/// Adds all good NPCs of region to list.
		/// </summary>
		/// <param name="list"></param>
		public void GetAllGoodNpcs(ref List<Creature> list)
		{
			_creaturesRWLS.EnterReadLock();
			try
			{
				list.AddRange(_creatures.Values.Where(a => a.Has(CreatureStates.GoodNpc) && a is NPC));
			}
			finally
			{
				_creaturesRWLS.ExitReadLock();
			}
		}

		/// <summary>
		/// Broadcasts packet in region.
		/// </summary>
		public virtual void Broadcast(Packet packet)
		{
			lock (_clients)
			{
				foreach (var client in _clients)
					client.Send(packet);
			}
		}

		/// <summary>
		/// Broadcasts packet to all creatures in range of source.
		/// </summary>
		public virtual void Broadcast(Packet packet, Entity source, bool sendToSource = true, int range = -1)
		{
			if (range < 0)
				range = VisibleRange;

			var pos = source.GetPosition();

			lock (_clients)
			{
				foreach (var client in _clients)
				{
					if (!client.Controlling.GetPosition().InRange(pos, range))
						continue;

					if (client.Controlling == source && !sendToSource)
						continue;

					client.Send(packet);
				}
			}
		}
	}

	public enum RegionMode
	{
		/// <summary>
		/// Kept in the world permanently
		/// </summary>
		Permanent,

		/// <summary>
		/// Region gets removed once the last player has left.
		/// </summary>
		RemoveWhenEmpty,
	}

	public class Limbo : Region
	{
		public Limbo()
			: base(0)
		{
		}

		public override void Broadcast(Packet packet)
		{
			Log.Warning("Broadcast in Limbo.");
			Log.Debug(Environment.StackTrace);
		}

		public override void Broadcast(Packet packet, Entity source, bool sendToSource = true, int range = -1)
		{
			Log.Warning("Broadcast in Limbo from source.");
			Log.Debug(Environment.StackTrace);
		}
	}
}
