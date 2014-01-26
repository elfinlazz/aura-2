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
		/// Sends SummonPetR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="pet">Negative response if null</param>
		public static void SummonPetR(Creature creature, Creature pet)
		{
			var packet = new Packet(Op.SummonPetR, creature.EntityId);
			packet.PutByte(pet != null);
			if (pet != null)
				packet.PutLong(pet.EntityId);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends UnsummonPetR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		/// <param name="entityId"></param>
		public static void UnsummonPetR(Creature creature, bool success, long entityId)
		{
			var packet = new Packet(Op.UnsummonPetR, creature.EntityId);
			packet.PutByte(success);
			packet.PutLong(entityId);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends PetRegister to creature's client.
		/// </summary>
		/// <remarks>
		/// TODO: Test, does this tell the client it can control this creature?
		/// </remarks>
		/// <param name="creature"></param>
		/// <param name="pet"></param>
		public static void PetRegister(Creature creature, Creature pet)
		{
			var packet = new Packet(Op.PetRegister, creature.EntityId);
			packet.PutLong(pet.EntityId);
			packet.PutByte(2); // Probably the follower type, see 5209

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends PetUnregister to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="pet"></param>
		public static void PetUnregister(Creature creature, Creature pet)
		{
			var packet = new Packet(Op.PetUnregister, creature.EntityId);
			packet.PutLong(pet.EntityId);

			creature.Client.Send(packet);
		}
	}
}
