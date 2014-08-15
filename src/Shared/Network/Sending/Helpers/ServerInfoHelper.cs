// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;

namespace Aura.Shared.Network.Sending.Helpers
{
	public static class ServerInfoHelper
	{
		/// <summary>
		/// Adds list of server and channel information to packet.
		/// </summary>
		/// <param name="packet"></param>
		/// <param name="serverList"></param>
		/// <param name="type"></param>
		public static void AddServerList(this Packet packet, ICollection<ServerInfo> serverList, ServerInfoType type)
		{
			packet.PutByte((byte)serverList.Count);
			foreach (var server in serverList)
				packet.AddServerInfo(server, type);
		}

		/// <summary>
		/// Adds server and channel information to packet.
		/// </summary>
		/// <param name="packet"></param>
		/// <param name="server"></param>
		/// <param name="type"></param>
		public static void AddServerInfo(this Packet packet, ServerInfo server, ServerInfoType type)
		{
			packet.PutString(server.Name);
			packet.PutShort(0); // Server type?
			packet.PutShort(0);
			packet.PutByte(1);

			// Channels
			packet.PutInt((int)server.Channels.Count);
			foreach (var channel in server.Channels.Values)
				packet.AddChannelInfo(channel, type);
		}

		/// <summary>
		/// Adds channel information to packet.
		/// </summary>
		/// <param name="packet"></param>
		/// <param name="channel"></param>
		/// <param name="type"></param>
		public static void AddChannelInfo(this Packet packet, ChannelInfo channel, ServerInfoType type)
		{
			packet.PutString(channel.Name);
			packet.PutInt((int)channel.State);
			packet.PutInt((int)channel.Events);
			packet.PutInt(0); // 1 for Housing? Hidden?
			packet.PutShort(channel.Stress);

			// Channels need more information
			if (type == ServerInfoType.Internal)
			{
				packet.PutString(channel.Host);
				packet.PutInt(channel.Port);
				packet.PutInt(channel.Users);
			}
		}
	}

	public enum ServerInfoType
	{
		Client, Internal
	}
}
