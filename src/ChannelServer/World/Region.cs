// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using Aura.Channel.Network;
using Aura.Channel.Network.Sending;
using Aura.Channel.World.Entities;
using Aura.Data;
using Aura.Shared.Mabi.Const;
using Aura.Shared.Network;
using Aura.Shared.Util;
using System.Threading;
using Aura.Data.Database;
using Boo.Lang.Compiler.TypeSystem;

namespace Aura.Channel.World
{
	public class Region
	{
		// TODO: Data?
		public const int VisibleRange = 3000;

		protected ReaderWriterLockSlim _creaturesRWLS, _propsRWLS, _itemsRWLS;

		protected Dictionary<long, Creature> _creatures;
		protected Dictionary<long, Prop> _props;
		protected Dictionary<long, Item> _items;

		protected HashSet<ChannelClient> _clients;

		protected RegionData _regionData;

		/// <summary>
		/// Region's id
		/// </summary>
		public int Id { get; protected set; }

		/// <summary>
		/// Manager for blocking objects in the region.
		/// </summary>
		public RegionCollision Collisions { get; protected set; }

		public Region(int id)
		{
			_creaturesRWLS = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
			_propsRWLS = new ReaderWriterLockSlim();
			_itemsRWLS = new ReaderWriterLockSlim();

			this.Id = id;

			_creatures = new Dictionary<long, Creature>();
			_props = new Dictionary<long, Prop>();
			_items = new Dictionary<long, Item>();

			_clients = new HashSet<ChannelClient>();

			_regionData = AuraData.RegionInfoDb.Find(this.Id);
			if (_regionData == null)
			{
				Log.Warning("Region: No data found for '{0}'.", this.Id);
				return;
			}

			this.Collisions = new RegionCollision(_regionData.X1, _regionData.Y1, _regionData.X2, _regionData.Y2);
			this.Collisions.Init(_regionData);

			this.LoadClientProps();
		}

		/// <summary>
		/// Adds all props found in the client for this region.
		/// </summary>
		private void LoadClientProps()
		{
			if (_regionData == null || _regionData.Areas == null)
				return;

			foreach (var area in _regionData.Areas.Values)
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
			{
				if (entity.Is(DataType.Creature))
				{
					var creature = entity as Creature;
					this.RemoveCreature(creature);
					creature.Dispose();

					// Respawn
					var npc = creature as NPC;
					if (npc != null && npc.SpawnId > 0)
						ChannelServer.Instance.ScriptManager.Spawn(npc.SpawnId, 1);
				}
				else if (entity.Is(DataType.Item))
				{
					this.RemoveItem(entity as Item);
				}
				else if (entity.Is(DataType.Prop))
				{
					this.RemoveProp(entity as Prop);
				}
			}
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

			if (creature.EntityId < MabiId.Npcs)
				Log.Status("Creatures currently in region {0}: {1}", this.Id, _creatures.Count);
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

			if (creature.EntityId < MabiId.Npcs)
				Log.Status("Creatures currently in region {0}: {1}", this.Id, _creatures.Count);
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
			foreach (var npc in npcs) { this.RemoveCreature(npc); npc.Dispose(); }
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
				return _creatures.Values.OfType<NPC>()
					.Count(npc => npc.AI != null && npc.AI.State == Scripting.Scripts.AiScript.AiState.Aggro && npc.Race == raceId && npc.Target == target);
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
}
