// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Collections.Generic;
using System.Linq;

namespace Aura.Shared.Network
{
	public class ServerInfoManager
	{
		private Dictionary<string, ServerInfo> _servers;

		public ServerInfoManager()
		{
			this._servers = new Dictionary<string, ServerInfo>();
		}

		public List<ServerInfo> List
		{
			get
			{
				lock (_servers)
					return _servers.Values.ToList();
			}
		}

		/// <summary>
		/// Returns true if a server with that name exists.
		/// </summary>
		/// <param name="serverName"></param>
		/// <returns></returns>
		public bool Has(string serverName)
		{
			return _servers.ContainsKey(serverName);
		}

		/// <summary>
		/// Returns server info or null, if it doesn't exist.
		/// </summary>
		/// <param name="serverName"></param>
		/// <returns></returns>
		public ServerInfo GetServer(string serverName)
		{
			ServerInfo result;
			_servers.TryGetValue(serverName, out result);
			return result;
		}

		/// <summary>
		/// Returns channel info from server or null, if server or channel
		/// doesn't exist.
		/// </summary>
		/// <param name="serverName"></param>
		/// <param name="channelName"></param>
		/// <returns></returns>
		public ChannelInfo GetChannel(string serverName, string channelName)
		{
			var server = this.GetServer(serverName);
			if (server == null)
				return null;

			return server.Get(channelName);
		}

		/// <summary>
		/// Returns channel info or null, if channel doesn't exist.
		/// </summary>
		/// <param name="fullName"></param>
		/// <returns></returns>
		public ChannelInfo GetChannel(string fullName)
		{
			lock (this.List)
			{
				foreach (var server in this.List)
				{
					foreach (var channel in server.Channels.Values)
					{
						if (channel.FullName == fullName)
							return channel;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Adds server with this name, if it doesn't exist already,
		/// and returns the new or existing server info for it.
		/// </summary>
		/// <param name="serverName"></param>
		/// <returns></returns>
		public ServerInfo Add(string serverName)
		{
			if (!this.Has(serverName))
			{
				var result = new ServerInfo(serverName);
				_servers.Add(serverName, result);
				return result;
			}

			return this.GetServer(serverName);
		}

		/// <summary>
		/// Adds channel and server, if it doesn't exist yet.
		/// Replaces channel if it already exists.
		/// </summary>
		/// <param name="channel"></param>
		public void Add(ChannelInfo channel)
		{
			var server = this.Add(channel.ServerName);
			server.Channels[channel.Name] = channel;
		}
	}
}
