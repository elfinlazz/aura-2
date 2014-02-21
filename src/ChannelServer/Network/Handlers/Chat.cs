// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Shared.Network;
using Aura.Channel.Network.Sending;
using Aura.Shared.Util;

namespace Aura.Channel.Network.Handlers
{
	public partial class ChannelServerHandlers : PacketHandlerManager<ChannelClient>
	{
		[PacketHandler(Op.Chat)]
		public void Chat(ChannelClient client, Packet packet)
		{
			var unkByte = packet.GetByte();
			var message = packet.GetString();

			var creature = client.GetCreature(packet.Id);
			if (creature == null)
				return;

			// Don't send message if it's a valid command
			if (ChannelServer.Instance.CommandProcessor.Process(client, creature, message))
				return;

			Send.Chat(creature, message);
		}
	}
}
