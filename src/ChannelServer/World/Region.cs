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

namespace Aura.Channel.World
{
	public class Region
	{
		// TODO: Data?
		public const int VisibleRange = 3000;

		protected ReaderWriterLockSlim _creaturesRWLS, _propsRWLS, _itemsRWLS;

		public int Id { get; protected set; }

		protected Dictionary<long, Creature> _creatures;
		protected Dictionary<long, Prop> _props;
		protected Dictionary<long, Item> _items;

		protected HashSet<ChannelClient> _clients;

		public Region(int id)
		{
			_creaturesRWLS = new ReaderWriterLockSlim();
			_propsRWLS = new ReaderWriterLockSlim();
			_itemsRWLS = new ReaderWriterLockSlim();

			this.Id = id;

			_creatures = new Dictionary<long, Creature>();
			_props = new Dictionary<long, Prop>();
			_items = new Dictionary<long, Item>();

			_clients = new HashSet<ChannelClient>();

			this.LoadClientProps();
		}

		/// <summary>
		/// Adds all props found in the client for this region.
		/// </summary>
		private void LoadClientProps()
		{
			var props = AuraData.RegionInfoDb.Find(this.Id);
			if (props == null || props.Areas == null)
				return;

			foreach (var area in props.Areas.Values)
			{
				foreach (var prop in area.Props.Values)
				{
					var add = new Prop(prop.EntityId, "", "", "", prop.Id, this.Id, (int)prop.X, (int)prop.Y, prop.Direction, prop.Scale, 0);
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
					this.RemoveCreature(entity as Creature);

					// Respawn
					//var npc = entity as NPC;
					//if (npc != null && npc.SpawnId > 0)
					//{
					//    ScriptManager.Instance.Spawn(npc.SpawnId, 1);
					//}
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

			_creaturesRWLS.EnterReadLock();
			try
			{
				result.AddRange(_creatures.Values.Where(a => a.GetPosition().InRange(pos, VisibleRange) && !a.Has(CreatureConditionA.Invisible)));
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

			_propsRWLS.EnterReadLock();
			try
			{
				result.AddRange(_props.Values.Where(a => a.GetPosition().InRange(pos, VisibleRange) && a.ServerSide));
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

			Send.EntityDisappears(creature);

			creature.Region = null;

			lock (_clients)
				_clients.Remove(creature.Client);

			if (creature.EntityId < MabiId.Npcs)
				Log.Status("Creatures currently in region {0}: {1}", this.Id, _creatures.Count);
		}

		/// <summary>
		/// Returns creature with entityId, or null, if it doesn't exist.
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
		/// Returns NPC with entityId, or null, if no NPC with that id exists.
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
		/// <param name="range"></param>
		/// <returns></returns>
		public List<Creature> GetPlayersInRange(Position pos, int range = VisibleRange)
		{
			_creaturesRWLS.EnterReadLock();
			try
			{
				return _creatures.Values.Where(a => a.Is(EntityType.Character) || a.Is(EntityType.Pet) && a.GetPosition().InRange(pos, range)).ToList();
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

			Send.EntityDisappears(prop);

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

			// All props spawned by the server, without range check.
			_propsRWLS.EnterReadLock();
			try
			{
				result.AddRange(_props.Values.Where(a => a.ServerSide));
			}
			finally
			{
				_propsRWLS.ExitReadLock();
			}

			return result;
		}

		/// <summary>
		/// Activates AIs in range of the movement path.
		/// </summary>
		/// <param name="from"></param>
		/// <param name="to"></param>
		public void ActivateAis(Creature creature, Position from, Position to)
		{
			// Bounding rectangle
			var minX = Math.Min(from.X, to.X) - VisibleRange;
			var minY = Math.Min(from.Y, to.Y) - VisibleRange;
			var maxX = Math.Max(from.X, to.X) + VisibleRange;
			var maxY = Math.Max(from.Y, to.Y) + VisibleRange;

			// Linear movement equation
			var slope = (to.Y == from.Y ? 0.001 : (to.Y - from.Y) / (to.X - from.X));
			var b = from.Y - slope * from.X;

			// Activation
			_creaturesRWLS.EnterReadLock();
			try
			{

				foreach (NPC npc in _creatures.Values.Where(a => a.Is(EntityType.NPC)))
				{
					var pos = npc.GetPosition();
					if (!(pos.X >= minX && pos.X <= maxX && pos.Y >= minY && pos.Y <= maxY && (Math.Abs(pos.Y - (long)(slope * pos.X + b)) <= VisibleRange)))
						continue;

					var time = (from.GetDistance(to) / creature.GetSpeed());

					npc.AI.Activate(time);
				}
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
