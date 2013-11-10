// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Shared.Util.Configuration;

namespace Aura.Channel.Util.Configuration.Files
{
	public class CommandsConfFile : ConfFile
	{
		public char Prefix { get; protected set; }

		public void Load()
		{
			this.RequireAndInclude("{0}/conf/commands.conf", "system", "user");

			this.Prefix = this.GetString("commands.prefix", ">")[0];
		}
	}
}
