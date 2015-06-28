// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Shared.Network;
using Aura.Channel.Network.Sending;
using Aura.Shared.Util;
using Aura.Mabi.Network;
using Aura.Mabi.Const;

namespace Aura.Channel.Network.Handlers
{
	public partial class ChannelServerHandlers : PacketHandlerManager<ChannelClient>
	{
		[PacketHandler(Op.Chat)]
		public void Chat(ChannelClient client, Packet packet)
		{
			var unkByte = packet.GetByte();
			var message = packet.GetString();

			var creature = client.GetCreatureSafe(packet.Id);

			if (!creature.Can(Locks.Speak))
				return;

			// Don't send message if it's a valid command
			if (ChannelServer.Instance.CommandProcessor.Process(client, creature, message))
				return;

			Send.Chat(creature, message);
		}

		[PacketHandler(Op.VisualChat)]
		public void VisualChat(ChannelClient client, Packet packet)
		{
			var url = packet.GetString();
			var width = packet.GetShort();
			var height = packet.GetShort();

			var creature = client.GetCreatureSafe(packet.Id);

			Send.VisualChat(creature, url, width, height);
		}
	}
}
