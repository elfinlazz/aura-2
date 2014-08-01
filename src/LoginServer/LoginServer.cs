// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Aura.Login.Database;
using Aura.Login.Network;
using Aura.Login.Network.Handlers;
using Aura.Login.Util;
using Aura.Shared.Network;
using Aura.Shared.Util;
using SharpExpress;
using System.Net;
using Aura.Login.Web;

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

		/// <summary>
		/// List of connected channel clients.
		/// </summary>
		public List<LoginClient> ChannelClients { get; private set; }

		/// <summary>
		/// Web API server
		/// </summary>
		public WebApplication WebApp { get; private set; }

		private LoginServer()
		{
			this.Server = new DefaultServer<LoginClient>();
			this.Server.Handlers = new LoginServerHandlers();
			this.Server.Handlers.AutoLoad();
			this.Server.ClientDisconnected += this.OnClientDisconnected;

			this.ServerList = new ServerInfoManager();

			this.ChannelClients = new List<LoginClient>();
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

			// Check if there are any updates
			this.CheckDatabaseUpdates();

			// Data
			this.LoadData(DataLoad.LoginServer, false);

			// Localization
			this.LoadLocalization(this.Conf);

			this.LoadWebApi();

			// Start
			this.Server.Start(this.Conf.Login.Port);

			CliUtil.RunningTitle();
			_running = true;

			// Commands
			var commands = new LoginConsoleCommands();
			commands.Wait();
		}

		private void OnClientDisconnected(LoginClient client)
		{
			var update = false;

			lock (this.ChannelClients)
			{
				if (this.ChannelClients.Contains(client))
				{
					this.ChannelClients.Remove(client);
					update = true;
				}
			}

			if (update)
			{
				var channel = (client.Account != null ? this.ServerList.GetChannel(client.Account.Name) : null);
				if (channel == null)
				{
					Log.Warning("Unregistered channel disconnected.");
					return;
				}
				Log.Status("Channel '{0}' disconnected, switched to Maintenance.", client.Account.Name);
				channel.State = ChannelState.Maintenance;

				Send.ChannelUpdate();
			}
		}

		private void CheckDatabaseUpdates()
		{
			Log.Info("Checking for updates...");

			try
			{
				var files = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "sql"));
				foreach (var filePath in files.Where(a => Path.GetExtension(a) == ".sql"))
					this.RunUpdate(filePath);
			}
			catch (Exception ex)
			{
				Log.Error("Error while updating database: {0}", ex.Message);
				CliUtil.Exit(1);
			}
		}

		private void RunUpdate(string filePath)
		{
			if (LoginDb.Instance.CheckUpdate(filePath))
				return;

			var name = Path.GetFileName(filePath);

			Log.Info("Update '{0}' found, executing...", name);

			if (!LoginDb.Instance.RunUpdate(filePath))
				Log.Error("Update '{0}' failed.", name);
			else
				Log.Info("Update '{0}' successful.", name);
		}

		public void Broadcast(Packet packet)
		{
			lock (this.Server.Clients)
			{
				foreach (var client in this.Server.Clients.Where(a => a.State == ClientState.LoggedIn))
				{
					client.Send(packet);
				}
			}
		}

		public void BroadcastChannels(Packet packet)
		{
			lock (this.Server.Clients)
			{
				foreach (var client in this.Server.Clients.Where(a => a.State == ClientState.LoggedIn && this.ChannelClients.Contains(a)))
				{
					client.Send(packet);
				}
			}
		}

		private void LoadWebApi()
		{
			Log.Info("Loading Web API...");

			this.WebApp = new WebApplication();

			this.WebApp.Get(@"/status", new StatusController());
			this.WebApp.All(@"/broadcast", new BroadcastController());
			this.WebApp.All(@"/check-user", new CheckUserController());

			try
			{
				this.WebApp.Listen(this.Conf.Login.WebPort);

				Log.Info("Web API listening on 0.0.0.0:{0}", this.Conf.Login.WebPort);
			}
			catch (Exception)
			{
				Log.Error("Failed to load Web API, port already in use?");
			}
		}
	}
}
