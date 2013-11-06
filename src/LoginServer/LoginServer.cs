// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using Aura.Login.Network;
using Aura.Login.Network.Handlers;
using Aura.Login.Util;
using Aura.Shared.Network;
using Aura.Shared.Util;

namespace Aura.Login
{
	public class LoginServer : ServerMain
	{
		public static readonly LoginServer Instance = new LoginServer();

		private bool _running = false;

		/// <summary>
		/// Instance of the actual server component.
		/// </summary>
		private DefaultServer<LoginClient> Server { get; set; }

		/// <summary>
		/// List of servers and channels.
		/// </summary>
		public ServerInfoManager ServerList { get; private set; }

		/// <summary>
		/// Configuration
		/// </summary>
		public LoginConf Conf { get; private set; }

		private LoginServer()
		{
			this.Server = new DefaultServer<LoginClient>();
			this.Server.Handlers = new LoginServerHandlers();
			this.Server.Handlers.AutoLoad();

			this.ServerList = new ServerInfoManager();
		}

		/// <summary>
		/// Loads all necessary components and starts the server.
		/// </summary>
		public void Run()
		{
			if (_running)
				throw new Exception("Server is already running.");

			CliUtil.WriteHeader("Login Server", ConsoleColor.Magenta);
			CliUtil.LoadingTitle();

			this.NavigateToRoot();

			// Conf
			this.LoadConf(this.Conf = new LoginConf());

			// Database
			this.InitDatabase(this.Conf);

			// Data
			this.LoadData(DataLoad.LoginServer, false);

			// Localization
			this.LoadLocalization(this.Conf);

			// Debug
			this.ServerList.Add("Aura");

			// Start
			this.Server.Start(this.Conf.Port);

			CliUtil.RunningTitle();
			_running = true;

			// Commands
			var commands = new LoginConsoleCommands();
			commands.Wait();
		}
	}
}
