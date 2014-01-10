// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

namespace Aura.Shared.Util.Configuration.Files
{
	/// <summary>
	/// Represents localization.conf
	/// </summary>
	public class LocalizationConfFile : ConfFile
	{
		public string Language { get; protected set; }

		public void Load()
		{
			this.Require("system/conf/localization.conf");

			this.Language = this.GetString("language", "us");
		}
	}
}
