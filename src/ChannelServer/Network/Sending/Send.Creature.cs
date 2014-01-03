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

		/// <summary>
		/// Broadcasts ForceRunTo in creature's range.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="to"></param>
		public static void ForceRunTo(Creature creature)
		{
			ForceRunTo(creature, creature.GetPosition());
		}

		/// <summary>
		/// Broadcasts ForceRunTo in creature's range.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="to"></param>
		public static void ForceRunTo(Creature creature, Position to)
		{
			var pos = creature.GetPosition();

			var packet = new Packet(Op.ForceRunTo, creature.EntityId);

			// From
			packet.PutInt(pos.X);
			packet.PutInt(pos.Y);

			// To
			packet.PutInt(to.X);
			packet.PutInt(to.Y);

			packet.PutByte(1);
			packet.PutByte(0);

			creature.Region.Broadcast(packet, creature);
		}

		/// <summary>
		/// Broadcasts ForceWalkTo in creature's range.
		/// </summary>
		/// <param name="creature"></param>
		public static void ForceWalkTo(Creature creature)
		{
			ForceWalkTo(creature, creature.GetPosition());
		}

		/// <summary>
		/// Broadcasts ForceWalkTo in creature's range.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="to"></param>
		public static void ForceWalkTo(Creature creature, Position to)
		{
			var pos = creature.GetPosition();

			var packet = new Packet(Op.ForceWalkTo, creature.EntityId);

			// From
			packet.PutInt(pos.X);
			packet.PutInt(pos.Y);

			// To
			packet.PutInt(to.X);
			packet.PutInt(to.Y);

			packet.PutByte(1);
			packet.PutByte(0);

			creature.Region.Broadcast(packet, creature);
		}

	}
}
