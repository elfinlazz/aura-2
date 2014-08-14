// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.World.Entities;
using Aura.Shared.Network;
using System;

namespace Aura.Channel.Network.Sending.Helpers
{
	public static class EntityHelper
	{
		public static Packet AddPublicEntityInfo(this Packet packet, Entity entity)
		{
			switch (entity.DataType)
			{
				case DataType.Creature: packet.AddCreatureInfo(entity as Creature, CreaturePacketType.Public); break;
				case DataType.Item: packet.AddItemInfo(entity as Item, ItemPacketType.Public); break;
				case DataType.Prop: packet.AddPropInfo(entity as Prop); break;
				default:
					throw new Exception("Unknown entity type '" + entity.GetType().ToString() + "', '" + entity.DataType + "'.");
			}

			return packet;
		}
	}
}
