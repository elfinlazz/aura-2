// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Shared.Network;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;

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
		/// 001 [................] String : Aura
		/// 002 [............0000] Short  : 0
		/// 003 [............0000] Short  : 0
		/// 004 [..............01] Byte   : 1
		/// 005 [........00000001] Int    : 1
		/// 006 [................] String : Ch1
		/// 007 [........00000001] Int    : 1
		/// 008 [........00000000] Int    : 0
		/// 009 [........00000000] Int    : 0
		/// 010 [............0000] Short  : 0
		/// 011 [................] String : 127.0.0.1
		/// 012 [........00002B0C] Int    : 11020
		/// 013 [........00000000] Int    : 0
		/// </example>
		[PacketHandler(Op.Internal.ChannelStatus)]
		public void Internal_ChannelStatus(ChannelClient client, Packet packet)
		{
			var serverCount = packet.GetByte();
			for (int i = 0; i < serverCount; ++i)
			{
				var serverName = packet.GetString();
				var unk1 = packet.GetShort();
				var unk2 = packet.GetShort();
				var unk3 = packet.GetByte();

				var channelCount = packet.GetInt();
				for (int j = 0; j < channelCount; ++j)
				{
					var channelName = packet.GetString();
					var state = (ChannelState)packet.GetInt();
					var events = (ChannelEvent)packet.GetInt();
					var unk4 = packet.GetInt();
					var stress = packet.GetShort();
					var host = packet.GetString();
					var port = packet.GetInt();
					var users = packet.GetInt();

					// Add channel
					var channel = new ChannelInfo(channelName, serverName, host, port);
					channel.State = state;
					channel.Events = events;
					channel.Users = users;

					ChannelServer.Instance.ServerList.Add(channel);
				}
			}
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
