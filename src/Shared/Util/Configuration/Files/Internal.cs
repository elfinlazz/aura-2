// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

namespace Aura.Shared.Util.Configuration.Files
{
	/// <summary>
	/// Represents internal.conf
	/// </summary>
	public class InternalConfFile : ConfFile
	{
		public string Password { get; protected set; }

		public void Load()
		{
			this.RequireAndInclude("{0}/conf/internal.conf", "system", "user");

			this.Password = this.GetString("internal.password", "change_me");
		}
	}
}
