// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Shared.Network;
using Aura.Shared.Util;

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
