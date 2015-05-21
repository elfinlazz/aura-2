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
	public class Region
	{
		// TODO: Data?
		public const int VisibleRange = 3000;

		protected ReaderWriterLockSlim _creaturesRWLS, _propsRWLS, _clientEventsRWLS, _itemsRWLS;

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
		/// Name of the region this one is based on (dynamics)
		/// </summary>
		public string BaseName { get; protected set; }

		/// <summary>
		/// Region's id
		/// </summary>
		public int Id { get; protected set; }

		/// <summary>
		/// Id of the region this one is based on (dynamics)
		/// </summary>
		public int BaseId { get; protected set; }

		/// <summary>
		/// Variation file used for this region (dynamics)
		/// </summary>
		public string Variation { get; protected set; }

		/// <summary>
		/// Returns true if this is a dynamic region.
		/// </summary>
		public bool IsDynamic { get { return this.Id != this.BaseId; } }

		/// <summary>
		/// Returns true if this region is temporary, like a dynamic region
		/// or a dungeon.
		/// </summary>
		public bool IsTemporary { get { return this.IsDynamic; } }

		/// <summary>
		/// Returns true if this region is temporary, like a dynamic region
		/// or a dungeon.
		/// </summary>
		public RegionMode Mode { get; protected set; }

		/// <summary>
		/// Manager for blocking objects in the region.
		/// </summary>
		public RegionCollision Collisions { get; protected set; }

		/// <summary>
		/// Creates new region by id.
		/// </summary>
		/// <param name="regionId"></param>
		private Region(int regionId, RegionMode mode)
		{
			_creaturesRWLS = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
			_propsRWLS = new ReaderWriterLockSlim();
			_clientEventsRWLS = new ReaderWriterLockSlim();
			_itemsRWLS = new ReaderWriterLockSlim();

			this.Id = regionId;
			this.BaseId = regionId;

			this.Mode = mode;

			_creatures = new Dictionary<long, Creature>();
			_props = new Dictionary<long, Prop>();
			_clientEvents = new Dictionary<long, ClientEvent>();
			_items = new Dictionary<long, Item>();

			_clients = new HashSet<ChannelClient>();

			this.Collisions = new RegionCollision();
		}

		/// <summary>
		/// Creates new region by id.
		/// </summary>
		/// <param name="regionId"></param>
		public static Region CreateNormal(int regionId)
		{
			var region = new Region(regionId, RegionMode.Permanent);

			region.RegionInfoData = AuraData.RegionInfoDb.Find(region.Id);
			if (region.RegionInfoData == null)
				throw new Exception("Region.CreateNormal: No region info data found for '" + region.Id + "'.");

			region.InitializeFromData();

			return region;
		}

		/// <summary>
		/// Creates new dynamic region, based on a region and variation file.
		/// Region is automatically added to the dynamic region manager.
		/// </summary>
		/// <param name="regionId"></param>
		/// <param name="variationFile"></param>
		public static Region CreateDynamic(int baseRegionId, string variationFile = "", RegionMode mode = RegionMode.RemoveWhenEmpty)
		{
			var region = new Region(baseRegionId, mode);
			region.Id = ChannelServer.Instance.World.DynamicRegions.GetFreeDynamicRegionId();
			region.Variation = variationFile;

			var baseRegionInfoData = AuraData.RegionInfoDb.Find(region.BaseId);
			if (baseRegionInfoData == null)
				throw new Exception("Region.CreateDynamic: No region info data found for '" + region.BaseId + "'.");

			region.RegionInfoData = CreateVariation(baseRegionInfoData, region.Id, variationFile);

			region.InitializeFromData();

			ChannelServer.Instance.World.DynamicRegions.Add(region);

			return region;
		}

		/// <summary>
		/// Recreates a region, based on another region and a variation file.
		/// </summary>
		/// <param name="baseRegionInfoData"></param>
		/// <param name="newRegionId"></param>
		/// <param name="variationFile"></param>
		/// <returns></returns>
		private static RegionInfoData CreateVariation(RegionInfoData baseRegionInfoData, int newRegionId, string variationFile)
		{
			var result = new RegionInfoData();
			result.Id = newRegionId;
			result.GroupId = baseRegionInfoData.GroupId;
			result.X1 = baseRegionInfoData.X1;
			result.Y1 = baseRegionInfoData.Y1;
			result.X2 = baseRegionInfoData.X2;
			result.Y2 = baseRegionInfoData.Y2;

			// TODO: Filter areas, props, and events to create, based on variation file.

			result.Areas = new List<AreaData>(baseRegionInfoData.Areas.Count);
			var i = 1;
			foreach (var originalArea in baseRegionInfoData.Areas)
			{
				var area = originalArea.Copy(false, false);
				area.Id = i++;

				// Add props
				foreach (var originalProp in originalArea.Props.Values)
				{
					var prop = originalProp.Copy();

					var id = (ulong)prop.EntityId;
					id &= ~0x0000FFFFFFFF0000U;
					id |= ((ulong)result.Id << 32);
					id |= ((ulong)baseRegionInfoData.GetAreaIndex(originalArea.Id) << 16);

					prop.EntityId = (long)id;

					area.Props.Add(prop.EntityId, prop);
				}

				// Add events
				foreach (var originalEvent in originalArea.Events.Values)
				{
					var ev = originalEvent.Copy();
					ev.RegionId = result.Id;

					var id = (ulong)ev.Id;
					id &= ~0x0000FFFFFFFF0000U;
					id |= ((ulong)result.Id << 32);
					id |= ((ulong)baseRegionInfoData.GetAreaIndex(originalArea.Id) << 16);

					ev.Id = (long)id;

					area.Events.Add(ev.Id, ev);
				}

				result.Areas.Add(area);
			}

			return result;
		}

		/// <summary>
		/// Adds all props found in the client for this region and creates a list
		/// of areas.
		/// </summary>
		protected void InitializeFromData()
		{
			if (this.RegionInfoData == null || this.RegionInfoData.Areas == null)
				return;

			var regionData = AuraData.RegionDb.Find(this.BaseId);
			if (regionData != null)
				this.BaseName = regionData.Name;

			this.Name = (this.IsDynamic ? "DynamicRegion" + this.Id : this.BaseName);

			this.Collisions.Init(this.RegionInfoData);

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
					var add = new Prop(prop.EntityId, "", "", prop.Id, this.Id, (int)prop.X, (int)prop.Y, prop.Direction, prop.Scale, 0);

					// Add drop behaviour if drop type exists
					var dropType = prop.GetDropType();
					if (dropType != -1) add.Behavior = Prop.GetDropBehavior(dropType);

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

					lock (_clientEvents)
						_clientEvents[add.EntityId] = add;
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
			if (creature.Region != null)
				creature.Region.RemoveCreature(creature);

			_creaturesRWLS.EnterWriteLock();
			try
			{
				_creatures.Add(creature.EntityId, creature);
			}
			finally
			{
				_creaturesRWLS.ExitWriteLock();
			}

			creature.Region = this;

			// Save reference to client if it's mainly controlling this creature.
			if (creature.Client.Controlling == creature)
			{
				lock (_clients)
					_clients.Add(creature.Client);
			}

			// TODO: Technically not required? Handled by LookAround.
			Send.EntityAppears(creature);

			//if (creature.EntityId < MabiId.Npcs)
			//	Log.Status("Creatures currently in region {0}: {1}", this.Id, _creatures.Count);
		}

		/// <summary>
		/// Removes creature from region, sends EntityDisappears.
		/// </summary>
		public void RemoveCreature(Creature creature)
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

			creature.Region = null;

			if (creature.Client.Controlling == creature)
				lock (_clients)
					_clients.Remove(creature.Client);

			//if (creature.EntityId < MabiId.Npcs)
			//	Log.Status("Creatures currently in region {0}: {1}", this.Id, _creatures.Count);

			// Remove dynamic region from client when he's removed from it on
			// the server, so it's recreated next time it goes to a dynamic
			// region with that id. Otherwise it will load the previous
			// region again.
			if (this.IsDynamic)
				Send.RemoveDynamicRegion(creature, this.Id);

			// Remove empty region from world
			if (this.CountPlayers() == 0 && this.Mode == RegionMode.RemoveWhenEmpty)
			{
				ChannelServer.Instance.World.RemoveRegion(this.Id);
				if (this.IsDynamic)
					ChannelServer.Instance.World.DynamicRegions.Remove(this.Id);
			}
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
		/// Returns list of creatures that match predicate.
		/// </summary>
		public ICollection<Creature> GetCreatures(Func<Creature, bool> predicate)
		{
			var result = new List<Creature>();

			_creaturesRWLS.EnterReadLock();
			try
			{
				result.AddRange(_creatures.Values.Where(predicate));
			}
			finally
			{
				_creaturesRWLS.ExitReadLock();
			}

			return result;
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
		public List<Creature> GetVisibleCreaturesInRange(Entity entity, int range = VisibleRange)
		{
			_creaturesRWLS.EnterReadLock();
			try
			{
				return _creatures.Values.Where(a => a != entity && a.GetPosition().InRange(entity.GetPosition(), range) && !a.Conditions.Has(ConditionsA.Invisible)).ToList();
			}
			finally
			{
				_creaturesRWLS.ExitReadLock();
			}
		}

		/// <summary>
		///  Spawns prop, sends EntityAppears.
		/// </summary>
		public void AddProp(Prop prop)
		{
			// TODO: Add prop shape to collisions

			_propsRWLS.EnterWriteLock();
			try
			{
				_props.Add(prop.EntityId, prop);
			}
			finally
			{
				_propsRWLS.ExitWriteLock();
			}

			prop.Region = this;

			// Add collisions
			if (prop.Shapes.Count > 0)
			{
				foreach (var shape in prop.Shapes)
					this.Collisions.Add(prop.EntityIdHex, shape);
			}

			Send.EntityAppears(prop);
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
			if (prop.Shapes.Count > 0)
				this.Collisions.Remove(prop.EntityIdHex);

			Send.PropDisappears(prop);

			prop.Region = null;
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

			item.Region = null;
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
		/// Drops item into region and makes it disappear after x seconds.
		/// Sends EntityAppears.
		/// </summary>
		public void DropItem(Item item, int x, int y)
		{
			item.Move(this.Id, x, y);
			item.DisappearTime = DateTime.Now.AddSeconds(Math.Max(60, (item.OptionInfo.Price / 100) * 60));

			this.AddItem(item);
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
					npc.Race == raceId &&
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
		public void Broadcast(Packet packet)
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
		public void Broadcast(Packet packet, Entity source, bool sendToSource = true, int range = -1)
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
}
