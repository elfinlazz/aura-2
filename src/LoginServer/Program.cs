// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Shared.Util;
using Aura.Shared.Network;
using Aura.Login.Network;

namespace Aura.Login
{
	class Program
	{
		/// <summary>
		/// <author></author>
		/// </summary>
		/// <param name="args"></param>
		static void Main(string[] args)
		{
			CmdUtil.WriteHeader("Login Server", ConsoleColor.Magenta);
			CmdUtil.LoadingTitle();

			var config = new Conf();
			config.Require("../../system/conf/login.conf");
			config.Include("../../user/conf/login.conf");

			Log.Archive = "../../log/archive/";
			Log.LogFile = "../../log/login.txt";
			Log.Hide |= LogLevel.Exception;

			LoginServer.Instance.Start(11000);

			CmdUtil.RunningTitle();

			// commands

			Console.ReadLine();
		}
	}
}
