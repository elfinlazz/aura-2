// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Shared.Network;
using Aura.Channel.World;

namespace Aura.Channel.Network.Handlers
{
	public partial class ChannelServerHandlers : PacketHandlerManager<ChannelClient>
	{
		/// <summary>
		/// Simple walking/running.
		/// </summary>
		/// <example>
		/// 001 [........0000321F] Int    : 12831
		/// 002 [........0000966F] Int    : 38511
		/// 003 [..............01] Byte   : 1
		/// 004 [..............00] Byte   : 0
		/// </example>
		[PacketHandler(Op.Run, Op.Walk)]
		public void Run(ChannelClient client, Packet packet)
		{
			var x = packet.GetInt();
			var y = packet.GetInt();

			var creature = client.GetCreature(packet.Id);
			if (creature == null)
				return;

			var from = creature.GetPosition();
			var to = new Position(x, y);

#if false
			// Old Aura code ahead
			// Collision
			MabiVertex intersection;
			if (WorldManager.Instance.FindCollisionInTree(creature.Region, pos, dest, out intersection))
			{
				//Logger.Debug("intersection " + intersection);
				// TODO: Uhm... do something.
			}
#endif

			//WorldManager.Instance.ActivateMobs(creature, from, to);

			creature.Move(to, (packet.Op == Op.Walk));
		}
	}
}
