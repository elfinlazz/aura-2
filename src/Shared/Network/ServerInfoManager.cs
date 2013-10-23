// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence file in the main folder

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
	}
}
