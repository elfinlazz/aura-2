// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Channel.World.Entities;
using Aura.Shared.Network;

namespace Aura.Channel.Network.Sending
{
	public static partial class Send
	{
		/// <summary>
		/// Sends GmcpOpen to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		public static void GmcpOpen(Creature creature)
		{
			var packet = new Packet(Op.GmcpOpen, creature.EntityId);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends GmcpInvisibilityR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		public static void GmcpInvisibilityR(Creature creature, bool success)
		{
			var packet = new Packet(Op.GmcpInvisibilityR, creature.EntityId);
			packet.PutByte(success);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends GmcpNpcListR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="npcs"></param>
		public static void GmcpNpcListR(Creature creature, ICollection<Creature> npcs)
		{
			var packet = new Packet(Op.GmcpNpcListR, creature.EntityId);
			packet.PutInt(npcs.Count);
			foreach (var npc in npcs)
			{
				var pos = npc.GetPosition();

				packet.PutInt(npc.RegionId);
				packet.PutString(npc.Name); // Name
				packet.PutString(npc.Name); // Local Name
				packet.PutString("{0} @ {1}/{2}", npc.RegionId, pos.X, pos.Y); // Location
			}

			creature.Client.Send(packet);
		}
	}
}
