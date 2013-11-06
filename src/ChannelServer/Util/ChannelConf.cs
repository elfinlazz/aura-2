// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Shared.Util.Configuration;

namespace Aura.Channel.Util
{
	public class ChannelConf : BaseConf
	{
		/// <summary>
		/// channel.conf
		/// </summary>
		public ChannelConfFile Channel { get; protected set; }

		public ChannelConf()
		{
			this.Channel = new ChannelConfFile();
		}

		public override void Load()
		{
			this.LoadDefault();
			this.Channel.Load();
		}
	}

	public class ChannelConfFile : ConfFile
	{
		public string Host { get; protected set; }
		public int Port { get; protected set; }

		public void Load()
		{
			this.RequireAndInclude("{0}/conf/channel.conf", "system", "user");

			this.Host = this.GetString("channel.host", "127.0.0.1");
			this.Port = this.GetInt("channel.port", 11020);
		}
	}
}
