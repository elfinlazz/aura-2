// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using Aura.Channel.World.Entities;
using Aura.Shared.Mabi.Const;
using Aura.Shared.Network;
using Aura.Shared.Util;
using Aura.Channel.World;

namespace Aura.Channel.Network.Sending
{
	public static partial class Send
	{
		/// <summary>
		/// Broadcasts Running|Walking in range of creature.
		/// </summary>
		public static void Move(Creature creature, Position from, Position to, bool walking)
		{
			var packet = new Packet(!walking ? Op.Running : Op.Walking, creature.EntityId);
			packet.PutInt(from.X);
			packet.PutInt(from.Y);
			packet.PutInt(to.X);
			packet.PutInt(to.Y);

			creature.Region.Broadcast(packet, creature);
		}
	}
}
