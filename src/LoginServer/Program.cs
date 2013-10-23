// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence file in the main folder

using System;
using Aura.Login.Network;
using Aura.Shared.Util;
using Aura.Shared.Database;

namespace Aura.Login
{
	class Program
	{
		static void Main(string[] args)
		{
			CmdUtil.WriteHeader("Login Server", ConsoleColor.Magenta);
			CmdUtil.LoadingTitle();

			var config = new Conf();
			try
			{
				config.RequireAndInclude("../../{0}/conf/login.conf", "system", "user");

				if (config.GetBool("log.archive", true))
					Log.Archive = "../../log/archive/";
				Log.LogFile = "../../log/login.txt";
				Log.Hide |= (LogLevel)config.GetInt("log.cmd_hide", 8);

				Log.Info("Read configuration.");
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
				CmdUtil.Exit();
			}

			try
			{
				var host = config.GetString("database.host", "127.0.0.1");
				var user = config.GetString("database.user", "root");
				var pass = config.GetString("database.pas", "root");
				var db = config.GetString("database.db", "aura");
				AuraDb.Instance.Init(host, user, pass, db);

				Log.Info("Initialized database.");
			}
			catch (Exception ex)
			{
				Log.Exception(ex, "Unable to open database connection.");
				CmdUtil.Exit();
			}

			LoginServer.Instance.Start(config.GetInt("login.port", 11000));

			CmdUtil.RunningTitle();

			// commands

			Console.ReadLine();
		}
	}
}
