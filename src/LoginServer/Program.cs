// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using Aura.Login.Network;
using Aura.Login.Util;
using Aura.Shared.Util;

namespace Aura.Login
{
	class Program
	{
		static void Main(string[] args)
		{
			CliUtil.WriteHeader("Login Server", ConsoleColor.Magenta);
			CliUtil.LoadingTitle();

			ServerUtil.NavigateToRoot();

			// Conf
			ServerUtil.LoadConf(LoginConf.Instance);

			// Database
			ServerUtil.InitDatabase(LoginConf.Instance);

			// Data
			ServerUtil.LoadData(DataLoad.LoginServer, false);

			// Localization
			ServerUtil.LoadLocalization(LoginConf.Instance);

			// Debug
			LoginServer.Instance.Servers.Add("Aura");

			// Start
			LoginServer.Instance.Start(LoginConf.Instance.Port);

			CliUtil.RunningTitle();

			// Commands
			var commands = new LoginConsoleCommands();
			commands.Wait();
		}
	}
}
