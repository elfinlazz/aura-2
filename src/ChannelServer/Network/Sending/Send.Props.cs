// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Channel.World.Entities;
using Aura.Shared.Network;
using Aura.Channel.World;
using Aura.Channel.Network.Sending.Helpers;

namespace Aura.Channel.Network.Sending
{
	public static partial class Send
	{
		/// <summary>
		/// Broadcasts HittingProp in range of creature.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="propEntityId"></param>
		public static void HittingProp(Creature creature, long propEntityId)
		{
			var pos = creature.GetPosition();

			var packet = new Packet(Op.HittingProp, creature.EntityId);
			packet.PutLong(propEntityId);
			packet.PutInt(2000);
			packet.PutFloat(pos.X);
			packet.PutFloat(pos.Y);

			creature.Region.Broadcast(packet, creature);
		}

		/// <summary>
		/// Sends HitPropR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		public static void HitPropR(Creature creature)
		{
			var packet = new Packet(Op.HitPropR, creature.EntityId);
			packet.PutByte(true);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends TouchPropR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		public static void TouchPropR(Creature creature)
		{
			var packet = new Packet(Op.TouchPropR, creature.EntityId);
			packet.PutByte(true);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Broadcasts prop update in its region.
		/// </summary>
		/// <param name="prop"></param>
		public static void PropUpdate(Prop prop)
		{
			var packet = new Packet(Op.PropUpdate, prop.EntityId);
			packet.AddPropUpdateInfo(prop);

			prop.Region.Broadcast(packet);
		}
	}
}
