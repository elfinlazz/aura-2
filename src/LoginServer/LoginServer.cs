// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Login.Database;
using Aura.Login.Network;
using Aura.Login.Network.Handlers;
using Aura.Login.Util;
using Aura.Login.Web;
using Aura.Shared;
using Aura.Shared.Network;
using Aura.Shared.Util;
using SharpExpress;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Aura.Login
{
	public class LoginServer : ServerRunner<LoginClient, DefaultServer<LoginClient>,
		LoginConf, LoginServerHandlers, LoginConsoleCommands>
	{
		public static readonly LoginServer Instance = new LoginServer();

		public override string Name { get { return "Login Server"; } }
		protected override ConsoleColor _headerColor { get { return ConsoleColor.Magenta; } }

		protected override int _listenPort { get { return this.Conf.Login.Port; } }

		protected override DataLoad _dataLoad { get { return DataLoad.LoginServer; } }


		/// <summary>
		/// List of servers and channels.
		/// </summary>
		public ServerInfoManager ServerList { get; private set; }

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
			this.Server.ClientDisconnected += this.OnClientDisconnected;

			this.ServerList = new ServerInfoManager();

			this.ChannelClients = new List<LoginClient>();
		}

		/// <summary>
		/// Loads all necessary components
		/// </summary>
		protected override void AfterSetup()
		{
			this.CheckDatabaseUpdates();

			this.LoadWebApi();
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

				Send.ChannelStatus(this.ServerList.List);
				Send.Internal_ChannelStatus(this.ServerList.List);
			}
		}

		private void CheckDatabaseUpdates()
		{
			Log.Info("Checking for updates...");

			var files = Directory.GetFiles("sql");
			foreach (var filePath in files.Where(file => Path.GetExtension(file).ToLower() == ".sql"))
				this.RunUpdate(Path.GetFileName(filePath));
		}

		private void RunUpdate(string updateFile)
		{
			if (LoginDb.Instance.CheckUpdate(updateFile))
				return;

			Log.Info("Update '{0}' found, executing...", updateFile);

			LoginDb.Instance.RunUpdate(updateFile);
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

		public void BroadcastPlayers(Packet packet)
		{
			lock (this.Server.Clients)
			{
				foreach (var client in this.Server.Clients.Where(a => a.State == ClientState.LoggedIn && !this.ChannelClients.Contains(a)))
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
