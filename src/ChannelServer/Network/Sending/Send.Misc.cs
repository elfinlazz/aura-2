// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Channel.World.Entities;
using Aura.Shared.Network;
using Aura.Channel.World;
using Aura.Shared.Mabi.Const;

namespace Aura.Channel.Network.Sending
{
	public static partial class Send
	{
		/// <summary>
		/// Sends MoonGateInfoRequestR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		public static void MoonGateInfoRequestR(Creature creature)
		{
			var packet = new Packet(Op.MoonGateInfoRequestR, creature.EntityId);
			//packet.PutString("_moongate_tara_west");
			//packet.PutString("_moongate_tirchonaill");

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends MailsRequestR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		public static void MailsRequestR(Creature creature)
		{
			var packet = new Packet(Op.MailsRequestR, creature.EntityId);
			// ...

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends SosButtonRequestR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="enabled"></param>
		public static void SosButtonRequestR(Creature creature, bool enabled)
		{
			var packet = new Packet(Op.SosButtonRequestR, creature.EntityId);
			packet.PutByte(enabled);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends HomesteadInfoRequestR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		public static void HomesteadInfoRequestR(Creature creature)
		{
			var packet = new Packet(Op.HomesteadInfoRequestR, creature.EntityId);
			packet.PutByte(0);
			packet.PutByte(0);
			packet.PutByte(1);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Broadcasts Effect in range of creature.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="parameters"></param>
		public static void Effect(Creature creature, int effectId, params object[] parameters)
		{
			var packet = new Packet(Op.Effect, creature.EntityId);
			packet.PutInt(effectId);
			foreach (var p in parameters)
			{
				if (p is byte) packet.PutByte((byte)p);
				else if (p is short) packet.PutShort((short)p);
				else if (p is int) packet.PutInt((int)p);
				else if (p is long) packet.PutLong((long)p);
				else if (p is float) packet.PutFloat((float)p);
				else if (p is string) packet.PutString((string)p);
				else
					throw new Exception("Unsupported effect parameter: " + p.GetType());
			}

			creature.Region.Broadcast(packet, creature);
		}

		/// <summary>
		/// Sends Disappear to creature's client.
		/// </summary>
		/// <remarks>
		/// Should this be broadcasted? What does it even do? TODO.
		/// </remarks>
		/// <param name="creature"></param>
		public static void Disappear(Creature creature)
		{
			var packet = new Packet(Op.Disappear, MabiId.Channel);
			packet.PutLong(creature.EntityId);

			creature.Client.Send(packet);
		}
	}
}
