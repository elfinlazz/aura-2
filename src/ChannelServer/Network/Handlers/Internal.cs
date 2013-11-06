// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Shared.Network;
using Aura.Shared.Util;
using Aura.Channel.Network.Sending;

namespace Aura.Channel.Network.Handlers
{
	public partial class ChannelServerHandlers : PacketHandlerManager<ChannelClient>
	{
		[PacketHandler(Op.Internal.ServerIdentifyR)]
		public void Internal_ServerIdentifyR(ChannelClient client, Packet packet)
		{
			var success = packet.GetBool();

			if (!success)
			{
				Log.Error("Server identification failed, check the password.");
				return;
			}

			client.State = ClientState.LoggedIn;

			Send.Internal_ChannelStatus();
		}
	}
}
