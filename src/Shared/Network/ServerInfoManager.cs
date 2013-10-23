// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aura.Shared.Network
{
	public class ServerInfoManager
	{
		public Dictionary<string, ServerInfo> Servers;

		public ServerInfoManager()
		{
			this.Servers = new Dictionary<string, ServerInfo>();
		}
	}
}
