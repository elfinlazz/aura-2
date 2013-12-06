// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Channel.World.Entities;
using Aura.Shared.Network;
using Aura.Channel.World;

namespace Aura.Channel.Network.Sending
{
	public static partial class Send
	{
		/// <summary>
		/// Broadcasts ChangeStance in range of creature.
		/// </summary>
		/// <param name="creature"></param>
		public static void ChangeStance(Creature creature)
		{
			var packet = new Packet(Op.ChangeStance, creature.EntityId);
			packet.PutByte((byte)creature.BattleStance);
			packet.PutByte(1);

			creature.Region.Broadcast(packet, creature);
		}

		/// <summary>
		/// Sends ChangeStanceRequestR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		public static void ChangeStanceRequestR(Creature creature)
		{
			var packet = new Packet(Op.ChangeStanceRequestR, creature.EntityId);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Broadcasts HittingProp in range of creature.
		/// </summary>
		/// <param name="creature"></param>
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
		/// <param name="propEntityId"></param>
		public static void HitPropR(Creature creature)
		{
			var packet = new Packet(Op.HitPropR, creature.EntityId);
			packet.PutByte(true);

			creature.Client.Send(packet);
		}
	}
}
