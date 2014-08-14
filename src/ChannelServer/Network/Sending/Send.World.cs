// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Collections.Generic;
using System.Linq;
using Aura.Channel.World.Entities;
using Aura.Shared.Mabi.Const;
using Aura.Shared.Network;
using Aura.Shared.Util;
using Aura.Channel.Network.Sending.Helpers;

namespace Aura.Channel.Network.Sending
{
	public static partial class Send
	{
		/// <summary>
		/// Broadcasts EntityAppears|ItemAppears|PropAppears in entity's region.
		/// </summary>
		/// <param name="entity"></param>
		public static void EntityAppears(Entity entity)
		{
			var op = Op.EntityAppears;
			if (entity is Item)
				op = Op.ItemAppears;
			else if (entity is Prop)
				op = Op.PropAppears;

			var packet = new Packet(op, MabiId.Broadcast);
			packet.AddPublicEntityInfo(entity);

			entity.Region.Broadcast(packet, entity, false);
		}

		/// <summary>
		/// Broadcasts EntityDisappears|ItemDisappears in entity's region.
		/// </summary>
		/// <param name="entity"></param>
		public static void EntityDisappears(Entity entity)
		{
			var op = Op.EntityDisappears;
			if (entity is Item)
				op = Op.ItemDisappears;

			var packet = new Packet(op, MabiId.Broadcast);
			packet.PutLong(entity.EntityId);
			packet.PutByte(0);

			entity.Region.Broadcast(packet, entity, false);
		}

		/// <summary>
		/// Broadcasts PropDisappears in prop's region.
		/// </summary>
		/// <param name="prop"></param>
		public static void PropDisappears(Prop prop)
		{
			var packet = new Packet(Op.PropDisappears, MabiId.Broadcast);
			packet.PutLong(prop.EntityId);

			prop.Region.Broadcast(packet, prop, false);
		}

		/// <summary>
		/// Sends EntitiesAppear to client, unless entities is empty.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="entities"></param>
		public static void EntitiesAppear(ChannelClient client, IEnumerable<Entity> entities)
		{
			// Count() is much faster then creating a list, speed being
			// important in this method.
			var count = (short)entities.Count();
			if (count < 1)
				return;

			var packet = new Packet(Op.EntitiesAppear, MabiId.Broadcast);
			packet.PutShort(count);
			foreach (var entity in entities)
			{
				var data = Packet.Empty().AddPublicEntityInfo(entity).Build();

				packet.PutShort((short)entity.DataType);
				packet.PutInt(data.Length);
				packet.PutBin(data);
			}

			client.Send(packet);
		}

		/// <summary>
		/// Sends EntitiesDisappear to client, unless entities is empty.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="entities"></param>
		public static void EntitiesDisappear(ChannelClient client, IEnumerable<Entity> entities)
		{
			var count = (short)entities.Count();
			if (count < 1)
				return;

			var packet = new Packet(Op.EntitiesDisappear, MabiId.Broadcast);
			packet.PutShort(count);
			foreach (var entity in entities)
			{
				packet.PutShort((short)entity.DataType);
				packet.PutLong(entity.EntityId);
			}

			client.Send(packet);
		}
	}
}
