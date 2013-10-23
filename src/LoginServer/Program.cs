// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence file in the main folder

using System;
using Aura.Login.Network;
using Aura.Shared.Util;
using Aura.Shared.Database;
using Aura.Login.Util;

namespace Aura.Login
{
	class Program
	{
		static void Main(string[] args)
		{
			CmdUtil.WriteHeader("Login Server", ConsoleColor.Magenta);
			CmdUtil.LoadingTitle();

			// Conf
			ServerUtil.LoadConf(LoginConf.Instance);

			// Database
			ServerUtil.InitDatabase(LoginConf.Instance);

			// Start
			LoginServer.Instance.Start(LoginConf.Instance.Port);

			CmdUtil.RunningTitle();

			// commands

			Console.ReadLine();
		}
	}
}
