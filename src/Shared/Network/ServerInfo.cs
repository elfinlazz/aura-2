// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aura.Shared.Network
{
	public class ServerInfo
	{
		public string Name { get; private set; }
		public Dictionary<string, ChannelInfo> Channels { get; private set; }

		public ServerInfo(string name)
		{
			this.Name = name;
			this.Channels = new Dictionary<string, ChannelInfo>();
		}
	}

	public class ChannelInfo
	{
		public string Name;
		public string ServerName;
		public string FullName;
		public string IP;
		public short Port;
		public DateTime LastUpdate = DateTime.MinValue;

		/// <summary>
		/// 0 = Maintenance,
		/// 1 = Normal,
		/// 2 = Busy,
		/// 3 = Full,
		/// 5 = Error
		/// </summary>
		public ChannelState State;

		/// <summary>
		/// 0 = Normal,
		/// 1 = Event,
		/// 2 = PvP,
		/// 3 = Event/PvP
		/// </summary>
		public ChannelEvent Events;

		/// <summary>
		/// 0-75
		/// </summary>
		public byte Stress;

		public ChannelInfo(string name, string server, string ip, short port)
		{
			this.Name = name;
			this.ServerName = server;
			this.FullName = name + "@" + server;
			this.IP = ip;
			this.Port = port;

			this.State = ChannelState.Normal;
			this.Events = ChannelEvent.Normal;
		}
	}

	public enum ChannelState { Maintenance, Normal, Busy, Full, Error = 5 }
	public enum ChannelEvent { Normal, Event, PvP, EventPvP }
}
