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
			try
			{
				LoginConf.Instance.RequireAndInclude("../../{0}/conf/login.conf", "system", "user");
				LoginConf.Instance.Load();

				// Log
				if (LoginConf.Instance.Archive)
					Log.Archive = "../../log/archive/";
				Log.LogFile = "../../log/login.txt";
				Log.Hide |= LoginConf.Instance.Hide;

				Log.Info("Read configuration.");
			}
			catch (Exception ex)
			{
				Log.Exception(ex, "Unable to read configuration. ({0})", ex.Message);
				CmdUtil.Exit(1);
			}

			// Database
			try
			{
				AuraDb.Instance.Init(LoginConf.Instance.Host, LoginConf.Instance.User, LoginConf.Instance.Pass, LoginConf.Instance.Db);

				Log.Info("Initialized database.");
			}
			catch (Exception ex)
			{
				Log.Exception(ex, "Unable to open database connection. ({0})", ex.Message);
				CmdUtil.Exit(1);
			}

			// Start
			LoginServer.Instance.Start(LoginConf.Instance.Port);

			CmdUtil.RunningTitle();

			// commands

			Console.ReadLine();
		}
	}
}
