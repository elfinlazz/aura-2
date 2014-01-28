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

		public bool EnableContinentWarp { get; protected set; }

		public void Load()
		{
			this.Require("system/conf/world.conf");

			this.DropRate = this.GetFloat("drop_rate", 100) / 100.0f;
			this.GoldDropRate = this.GetFloat("gold_drop_rate", 30) / 100.0f;
			this.PropDropRate = this.GetFloat("prop_drop_rate", 30) / 100.0f;

			this.EnableContinentWarp = this.GetBool("enable_continent_warp", true);
		}
	}
}
