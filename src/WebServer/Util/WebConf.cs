// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Shared.Util;
using Aura.Shared.Util.Configuration;
using System.Collections.Generic;

namespace Aura.Web.Util
{
	public class WebConf : BaseConf
	{
		/// <summary>
		/// web.conf
		/// </summary>
		public WebConfFile Web { get; protected set; }

		public WebConf()
		{
			this.Web = new WebConfFile();
		}

		public override void Load()
		{
			this.LoadDefault();
			this.Web.Load();
		}
	}

	/// <summary>
	/// Represents web.conf
	/// </summary>
	public class WebConfFile : ConfFile
	{
		public int Port { get; protected set; }

		public void Load()
		{
			this.Require("system/conf/web.conf");

			this.Port = this.GetInt("port", 80);
		}
	}
}
