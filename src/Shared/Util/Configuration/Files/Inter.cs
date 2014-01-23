// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

namespace Aura.Shared.Util.Configuration.Files
{
	/// <summary>
	/// Represents inter.conf
	/// </summary>
	public class InterConfFile : ConfFile
	{
		public string Password { get; protected set; }

		public void Load()
		{
			this.Require("system/conf/inter.conf");

			this.Password = this.GetString("password", "change_me");
		}
	}
}
