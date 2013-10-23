// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Shared.Util;

namespace Aura.Login.Util
{
	public class LoginConf : BaseConf
	{
		public static readonly LoginConf Instance = new LoginConf();

		// Login
		public int Port;

		private LoginConf()
		{
		}

		public void Load()
		{
			this.LoadLog();
			this.LoadDatabase();
			this.LoadLogin();
		}

		public void LoadLogin()
		{
			this.Port = this.GetInt("login.port", 11000);
		}
	}
}
