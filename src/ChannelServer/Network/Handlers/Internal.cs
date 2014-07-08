// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Shared.Network;
using Aura.Shared.Util;
using System;

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

		/// <summary>
		/// Sent to all connected clients by the login server,
		/// list of servers and channels
		/// </summary>
		/// <example>
		/// 001 [..............01] Byte   : 1
		/// 002 [................] String : Aura
		/// 003 [............0000] Short  : 0
		/// 004 [............0000] Short  : 0
		/// 005 [..............01] Byte   : 1
		/// 006 [........00000001] Int    : 1
		/// 007 [................] String : Ch1
		/// 008 [........00000001] Int    : 1
		/// 009 [........00000000] Int    : 0
		/// 010 [........00000000] Int    : 0
		/// 011 [............0000] Short  : 0
		/// </example>
		[PacketHandler(Op.ChannelStatus)]
		public void ChannelStatus(ChannelClient client, Packet packet)
		{
			// Read servers and channels
		}

		/// <summary>
		/// Sent to all connected clients by the login server,
		/// message to broadcast
		/// </summary>
		/// <example>
		/// 001 [................] String : test
		/// </example>
		[PacketHandler(Op.Internal.BroadcastNotice)]
		public void Broadcast(ChannelClient client, Packet packet)
		{
			var notice = packet.GetString();

			Send.Notice(NoticeType.TopRed, Math.Max(20000, notice.Length * 350), notice);
		}
	}
}
