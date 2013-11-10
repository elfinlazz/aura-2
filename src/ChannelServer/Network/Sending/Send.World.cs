// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.World.Entities;
using Aura.Shared.Mabi.Const;
using Aura.Shared.Network;
using System.Collections.Generic;
using Aura.Shared.Util;

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
			if (entity.EntityType == EntityType.Item)
				op = Op.ItemAppears;
			else if (entity.EntityType == EntityType.Prop)
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
			if (entity.EntityType == EntityType.Item)
				op = Op.ItemDisappears;

			var packet = new Packet(op, MabiId.Broadcast);
			packet.PutLong(entity.EntityId);
			packet.PutByte(0);

			entity.Region.Broadcast(packet, entity, false);
		}

		public static void EntitiesAppear(ChannelClient client, IList<Entity> entities)
		{
			var packet = new Packet(Op.EntitiesAppear, MabiId.Broadcast);
			packet.PutShort((short)entities.Count);
			foreach (var entity in entities)
			{
				// XXX: Could be cached
				var data = Packet.Empty().AddPublicEntityInfo(entity).Build(false);

				packet.PutShort((short)entity.DataType);
				packet.PutInt(data.Length);
				packet.PutBin(data);
			}

			client.Send(packet);
		}
	}
}
