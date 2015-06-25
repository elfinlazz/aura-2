// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Channel.World.Entities;
using Aura.Shared.Network;
using Aura.Mabi.Const;
using Aura.Mabi.Network;

namespace Aura.Channel.Network.Sending
{
	public static partial class Send
	{
		/// <summary>
		/// Sends RequestRebirthR to creature's client.
		/// </summary>
		/// <param name="vehicle"></param>
		public static void RequestRebirthR(Creature creature, bool success)
		{
			var packet = new Packet(Op.RequestRebirthR, MabiId.Login);
			packet.PutByte(success);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends EnterRebirthR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		public static void EnterRebirthR(Creature creature)
		{
			var packet = new Packet(Op.EnterRebirthR, creature.EntityId);
			packet.PutByte(0);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends PonsUpdate to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		public static void PonsUpdate(Creature creature, int ponsAmount)
		{
			var packet = new Packet(Op.PonsUpdate, MabiId.Channel);
			packet.PutByte(2);
			packet.PutInt(ponsAmount);

			creature.Client.Send(packet);
		}
	}
}
