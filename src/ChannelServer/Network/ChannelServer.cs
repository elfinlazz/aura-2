// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Handlers;
using Aura.Shared.Network;

namespace Aura.Channel.Network
{
	public class ChannelServer : DefaultServer<ChannelClient>
	{
		public static readonly ChannelServer Instance = new ChannelServer();

		public ServerInfoManager Servers { get; private set; }

		private ChannelServer()
			: base()
		{
			this.Handlers = new ChannelServerHandlers();
			this.Handlers.AutoLoad();

			this.Servers = new ServerInfoManager();
		}
	}
}
