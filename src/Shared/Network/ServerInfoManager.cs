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

		public bool Has(string serverName)
		{
			return _servers.ContainsKey(serverName);
		}

		public ServerInfo Get(string serverName)
		{
			ServerInfo result;
			_servers.TryGetValue(serverName, out result);
			return result;
		}

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

		public void Add(ChannelInfo channel)
		{
			var server = this.Add(channel.ServerName);
			server.Channels.Add(channel.Name, channel);
		}
	}
}
