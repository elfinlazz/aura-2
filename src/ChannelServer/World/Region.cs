// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Channel.World.Entities;
using Aura.Shared.Util;
using Aura.Shared.Network;
using Aura.Channel.Network.Sending;

namespace Aura.Channel.World
{
	public class Region
	{
		public int Id { get; protected set; }

		protected Dictionary<long, Creature> _creatures;
		protected Dictionary<long, Prop> _props;
		protected Dictionary<long, Item> _items;

		public Region(int id)
		{
			this.Id = id;

			_creatures = new Dictionary<long, Creature>();
			_props = new Dictionary<long, Prop>();
			_items = new Dictionary<long, Item>();
		}

		/// <summary>
		/// Adds creature to region, sends EntityAppears.
		/// </summary>
		/// <param name="creature"></param>
		public void AddCreature(Creature creature)
		{
			lock (_creatures)
				_creatures.Add(creature.EntityId, creature);

			creature.Region = this;

			Send.EntityAppears(creature);

			Log.Status("Creatures in region {0}: {1}", this.Id, _creatures.Count);
		}

		/// <summary>
		/// Removes creature from region, sends EntityDisappears.
		/// </summary>
		/// <param name="creature"></param>
		public void RemoveCreature(Creature creature)
		{
			lock (_creatures)
				_creatures.Remove(creature.EntityId);

			Send.EntityDisappears(creature);

			creature.Region = null;

			Log.Status("Creatures in region {0}: {1}", this.Id, _creatures.Count);
		}

		/// <summary>
		/// Returns new list of all entities within range of source.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="range"></param>
		/// <returns></returns>
		public List<Entity> GetEntitiesInRange(Entity source, int range = -1)
		{
			if (range < 0)
				range = 2500;

			var result = new List<Entity>();

			lock (_creatures)
				result.AddRange(_creatures.Values.Where(a => a.GetPosition().InRange(source.GetPosition(), range)));

			lock (_items)
				result.AddRange(_items.Values.Where(a => a.GetPosition().InRange(source.GetPosition(), range)));

			lock (_props)
				result.AddRange(_props.Values);

			return result;
		}

		/// <summary>
		/// Broadcasts packet in region.
		/// </summary>
		/// <param name="packet"></param>
		public void Broadcast(Packet packet)
		{
			lock (_creatures)
			{
				// TODO: Don't send to the same client twice
				foreach (var creature in _creatures.Values)
					creature.Client.Send(packet);
			}
		}

		/// <summary>
		/// Broadcasts packet to all creatures in range of source.
		/// </summary>
		/// <param name="packet"></param>
		/// <param name="source"></param>
		/// <param name="sendToSource"></param>
		/// <param name="range"></param>
		public void Broadcast(Packet packet, Entity source, bool sendToSource = true, int range = -1)
		{
			if (range < 0)
				range = 2500; // TODO: read from region data

			var pos = source.GetPosition();

			lock (_creatures)
			{
				// TODO: Don't send to the same client twice
				foreach (var creature in _creatures.Values.Where(a => a.GetPosition().InRange(pos, range)))
				{
					if (creature == source && !sendToSource)
						continue;

					creature.Client.Send(packet);
				}
			}
		}
	}
}
