// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Shared.Util.Configuration;
using Aura.Channel.Util.Configuration.Files;

namespace Aura.Channel.Util.Configuration
{
	public sealed class ChannelConf : BaseConf
	{
		/// <summary>
		/// autoban.conf
		/// </summary>
		public AutobanConfFile Autoban { get; private set; }

		/// <summary>
		/// channel.conf
		/// </summary>
		public ChannelConfFile Channel { get; private set; }

		/// <summary>
		/// channel.conf
		/// </summary>
		public CommandsConfFile Commands { get; private set; }

		/// <summary>
		/// channel.conf
		/// </summary>
		public WorldConfFile World { get; private set; }

		public ChannelConf()
		{
			this.Autoban = new AutobanConfFile();
			this.Channel = new ChannelConfFile();
			this.Commands = new CommandsConfFile();
			this.World = new WorldConfFile();
		}

		public override void Load()
		{
			this.LoadDefault();

			this.Autoban.Load();
			this.Channel.Load();
			this.Commands.Load();
			this.World.Load();
		}
	}
}
