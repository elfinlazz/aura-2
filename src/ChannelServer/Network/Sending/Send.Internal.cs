// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Shared.Mabi;
using Aura.Shared.Network;

namespace Aura.Channel.Network.Sending
{
	public static partial class Send
	{
		/// <summary>
		/// Sends Internal.ServerIdentify to login server.
		/// </summary>
		public static void Internal_ServerIdentify()
		{
			var packet = new Packet(Op.Internal.ServerIdentify, 0);
			packet.PutString(Password.Hash(ChannelServer.Instance.Conf.Internal.Password));

			ChannelServer.Instance.LoginServer.Send(packet);
		}

		/// <summary>
		/// Sends Internal.ChannelStatus to login server.
		/// </summary>
		public static void Internal_ChannelStatus()
		{
			var cur = ChannelServer.Instance.World.CountPlayers();
			var max = ChannelServer.Instance.Conf.Channel.MaxUsers;

			var packet = new Packet(Op.Internal.ChannelStatus, 0);
			packet.PutString(ChannelServer.Instance.Conf.Channel.ChannelServer);
			packet.PutString(ChannelServer.Instance.Conf.Channel.ChannelName);
			packet.PutString(ChannelServer.Instance.Conf.Channel.ChannelHost);
			packet.PutInt(ChannelServer.Instance.Conf.Channel.ChannelPort);
			packet.PutInt(cur);
			packet.PutInt(max);
			packet.PutInt((int)ChannelState.Normal);

			ChannelServer.Instance.LoginServer.Send(packet);
		}

		/// <summary>
		/// Sends Internal.Broadcast to login server.
		/// </summary>
		public static void Internal_Broadcast(string message)
		{
			var packet = new Packet(Op.Internal.BroadcastNotice, 0);
			packet.PutString(message);

			ChannelServer.Instance.LoginServer.Send(packet);
		}
	}
}
