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

namespace Aura.Channel.World
{
	public class Region
	{
		// TODO: Data?
		public const int VisibleRange = 5000;

		public int Id { get; protected set; }

		protected Dictionary<long, Creature> _creatures;
		protected Dictionary<long, Prop> _props;
		protected Dictionary<long, Item> _items;

		protected HashSet<ChannelClient> _clients;

		public Region(int id)
		{
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
			lock (_creatures)
				disappear.AddRange(_creatures.Values.Where(a => a.DisappearTime > DateTime.MinValue && a.DisappearTime < now));
			lock (_items)
				disappear.AddRange(_items.Values.Where(a => a.DisappearTime > DateTime.MinValue && a.DisappearTime < now));
			lock (_props)
				disappear.AddRange(_props.Values.Where(a => a.DisappearTime > DateTime.MinValue && a.DisappearTime < now));

			// Remove them from the region
			foreach (var entity in disappear)
			{
				if (entity.DataType == DataType.Creature)
				{
					this.RemoveCreature(entity as Creature);

					// Respawn
					//var npc = entity as NPC;
					//if (npc != null && npc.SpawnId > 0)
					//{
					//    ScriptManager.Instance.Spawn(npc.SpawnId, 1);
					//}
				}
				else if (entity.DataType == DataType.Item)
				{
					this.RemoveItem(entity as Item);
				}
				else if (entity.DataType == DataType.Prop)
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
			lock (_creatures)
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
		}

		/// <summary>
		/// Returns a list of visible entities, from the view point of creature.
		/// </summary>
		/// <param name="creature"></param>
		public List<Entity> GetVisibleEntities(Creature creature)
		{
			var result = new List<Entity>();
			var pos = creature.GetPosition();

			lock (_creatures)
				result.AddRange(_creatures.Values.Where(a => a.GetPosition().InRange(pos, VisibleRange) && !a.Has(CreatureConditionA.Invisible)));
			lock (_props)
				result.AddRange(_props.Values.Where(a => a.GetPosition().InRange(pos, VisibleRange)));
			lock (_items)
				result.AddRange(_items.Values.Where(a => a.GetPosition().InRange(pos, VisibleRange)));

			return result;
		}

		/// <summary>
		/// Adds creature to region, sends EntityAppears.
		/// </summary>
		public void AddCreature(Creature creature)
		{
			lock (_creatures)
				_creatures.Add(creature.EntityId, creature);

			if (creature.Region != null)
				creature.Region.RemoveCreature(creature);

			creature.Region = this;

			// Save reference to client if it's controlling mainly this creature.
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
			lock (_creatures)
				_creatures.Remove(creature.EntityId);

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
			lock (_creatures)
				_creatures.TryGetValue(entityId, out creature);
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
			return _creatures.Values.FirstOrDefault(a => a is PlayerCreature && a.Name == name) as PlayerCreature;
		}

		/// <summary>
		///  Spawns prop, sends EntityAppears.
		/// </summary>
		public void AddProp(Prop prop)
		{
			lock (_props)
				_props.Add(prop.EntityId, prop);

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

			lock (_props)
				_props.Remove(prop.EntityId);

			Send.EntityDisappears(prop);

			prop.Region = null;
		}

		/// <summary>
		/// Returns prop or null.
		/// </summary>
		public Prop GetProp(long entityId)
		{
			Prop result;
			lock (_props)
				_props.TryGetValue(entityId, out result);
			return result;
		}

		/// <summary>
		///  Adds item, sends EntityAppears.
		/// </summary>
		public void AddItem(Item item)
		{
			lock (_items)
				_items.Add(item.EntityId, item);

			item.Region = this;

			Send.EntityAppears(item);
		}

		/// <summary>
		/// Despawns item, sends EntityDisappears.
		/// </summary>
		public void RemoveItem(Item item)
		{
			lock (_items)
				_items.Remove(item.EntityId);

			Send.EntityDisappears(item);

			item.Region = null;
		}

		/// <summary>
		/// Returns item or null.
		/// </summary>
		public Item GetItem(long entityId)
		{
			Item result;
			lock (_items)
				_items.TryGetValue(entityId, out result);
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

			lock (_creatures)
				result.AddRange(_creatures.Values.Where(a => a.GetPosition().InRange(source.GetPosition(), range)));

			lock (_items)
				result.AddRange(_items.Values.Where(a => a.GetPosition().InRange(source.GetPosition(), range)));

			// All props spawned by the server, without range check.
			lock (_props)
				result.AddRange(_props.Values.Where(a => a.ServerSide));

			return result;
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
