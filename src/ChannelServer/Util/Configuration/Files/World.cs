// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Shared.Util.Configuration;

namespace Aura.Channel.Util.Configuration.Files
{
	public class WorldConfFile : ConfFile
	{
		public float DropRate { get; protected set; }
		public float GoldDropRate { get; protected set; }
		public float PropDropRate { get; protected set; }

		public void Load()
		{
			this.RequireAndInclude("{0}/conf/world.conf", "system", "user");

			this.DropRate = this.GetFloat("world.drop_rate", 100) / 100.0f;
			this.GoldDropRate = this.GetFloat("world.gold_drop_rate", 30) / 100.0f;
			this.PropDropRate = this.GetFloat("world.prop_drop_rate", 30) / 100.0f;
		}
	}
}
