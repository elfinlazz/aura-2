// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Shared.Util.Configuration;

namespace Aura.Channel.Util.Configuration.Files
{
	public class ChannelConfFile : ConfFile
	{
		public string LoginHost { get; protected set; }
		public int LoginPort { get; protected set; }

		public string ChannelServer { get; protected set; }
		public string ChannelName { get; protected set; }
		public string ChannelHost { get; protected set; }
		public int ChannelPort { get; protected set; }
		public int MaxUsers { get; protected set; }

		public void Load()
		{
			this.Require("system/conf/channel.conf");

			this.LoginHost = this.GetString("login_host", "127.0.0.1");
			this.LoginPort = this.GetInt("login_port", 11000);

			this.ChannelServer = this.GetString("channel_server", "Aura");
			this.ChannelName = this.GetString("channel_name", "Ch1");
			this.ChannelHost = this.GetString("channel_host", "127.0.0.1");
			this.ChannelPort = this.GetInt("channel_port", 11020);
			this.MaxUsers = this.GetInt("max_users", 20);
		}
	}
}
