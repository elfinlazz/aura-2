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
		public ServerInfo Get(string serverName)
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
		public ChannelInfo Get(string serverName, string channelName)
		{
			var server = this.Get(serverName);
			if (server == null)
				return null;

			return server.Get(channelName);
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

			return this.Get(serverName);
		}

		/// <summary>
		/// Adds channel and server if it doesn't exist yet.
		/// </summary>
		/// <param name="channel"></param>
		public void Add(ChannelInfo channel)
		{
			var server = this.Add(channel.ServerName);
			server.Channels.Add(channel.Name, channel);
		}
	}
}
