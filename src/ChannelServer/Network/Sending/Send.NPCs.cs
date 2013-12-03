// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Shared.Network;
using Aura.Channel.World.Entities;

namespace Aura.Channel.Network.Sending
{
	public static partial class Send
	{
		/// <summary>
		/// Sends NpcTalk to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="xml"></param>
		public static void NpcTalk(Creature creature, string xml)
		{
			var packet = new Packet(Op.NpcTalk, creature.EntityId);
			packet.PutString(xml);
			packet.PutBin();

			creature.Client.Send(packet);
		}
	}
}
