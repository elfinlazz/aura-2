// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;

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

		/// <summary>
		/// Returns channel info or null, if it doesn't exist.
		/// </summary>
		/// <param name="channelName"></param>
		/// <returns></returns>
		public ChannelInfo Get(string channelName)
		{
			ChannelInfo result;
			this.Channels.TryGetValue(channelName, out result);
			return result;
		}
	}

	public class ChannelInfo
	{
		public string Name { get; set; }
		public string ServerName { get; set; }
		public string FullName { get; set; }
		public string Host { get; set; }
		public int Port { get; set; }
		public DateTime LastUpdate { get; set; }

		/// <summary>
		/// Status of the channel
		/// </summary>
		/// <remarks>
		/// This needs to be update to reflect the stress value.
		/// </remarks>
		public ChannelState State { get; set; }

		/// <summary>
		/// What events are going on
		/// </summary>
		public ChannelEvent Events { get; set; }

		/// <summary>
		/// 0-100%, in increments of 5%.
		/// </summary>
		public short Stress
		{
			get
			{
				var val = (this.Users * 100) / this.MaxUsers;
				return (short)(val - val % 5);
			}
		}

		/// <summary>
		/// Current users
		/// </summary>
		public int Users { get; set; }

		/// <summary>
		/// Max users able to connect
		/// </summary>
		public int MaxUsers { get; set; }

		public ChannelInfo(string name, string server, string ip, int port)
		{
			this.Name = name;
			this.ServerName = server;
			this.FullName = name + "@" + server;
			this.Host = ip;
			this.Port = port;

			this.State = ChannelState.Normal;
			this.Events = ChannelEvent.None;

			this.LastUpdate = DateTime.MinValue;
		}
	}

	public enum ChannelState
	{
		/// <summary>
		/// Server is offline for maint
		/// </summary>
		Maintenance = 0,
		/// <summary>
		/// Server is online and stress is [0,40) (less than 40%)
		/// </summary>
		Normal = 1,
		/// <summary>
		/// Server is online and stress is [40,70) (between 40% and 70%)
		/// </summary>
		Busy = 2,
		/// <summary>
		/// Server is online and stress is [70,95) (between 70% and 95%)
		/// </summary>
		Full = 3,
		/// <summary>
		/// Server is online and stress is [95,+∞) (greater than 95%)
		/// 
		/// In this state, the client won't allow you to move to the channel
		/// </summary>
		Bursting = 4,
		/// <summary>
		/// This state has never been directly observed. Maybe used internally,
		/// or possibly if a channel crashes.
		/// 
		/// Shows up as [Maintenance] clientside.
		/// </summary>
		Booting = 5,
		/// <summary>
		/// Any other value is interpreted as [Error], unknown if this affects
		/// client behavior.
		/// </summary>
		Error = 6		
	}

	[Flags]
	public enum ChannelEvent
	{
		None = 0,
		Event = 1,
		PvP = 2
	}
}
