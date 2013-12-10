// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Shared.Util.Configuration;
using Aura.Channel.Util.Configuration.Files;

namespace Aura.Channel.Util.Configuration
{
	public class ChannelConf : BaseConf
	{
		/// <summary>
		/// channel.conf
		/// </summary>
		public ChannelConfFile Channel { get; protected set; }

		/// <summary>
		/// channel.conf
		/// </summary>
		public CommandsConfFile Commands { get; protected set; }

		/// <summary>
		/// channel.conf
		/// </summary>
		public WorldConfFile World { get; protected set; }

		public ChannelConf()
		{
			this.Channel = new ChannelConfFile();
			this.Commands = new CommandsConfFile();
			this.World = new WorldConfFile();
		}

		public override void Load()
		{
			this.LoadDefault();

			this.Channel.Load();
			this.Commands.Load();
			this.World.Load();
		}
	}
}
