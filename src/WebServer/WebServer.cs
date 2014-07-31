// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Shared.Util;
using Aura.Shared.Util.Commands;
using Aura.Web.Controllers;
using Aura.Web.Util;
using SharpExpress;
using SharpExpress.Engines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;

namespace Aura.Web
{
	public class WebServer : ServerMain
	{
		public static readonly WebServer Instance = new WebServer();

		private bool _running = false;

		/// <summary>
		/// Actual web server
		/// </summary>
		public WebApplication App { get; private set; }

		/// <summary>
		/// Configuration
		/// </summary>
		public WebConf Conf { get; private set; }

		/// <summary>
		/// Loads all necessary components and starts the server.
		/// </summary>
		public void Run()
		{
			if (_running)
				throw new Exception("Server is already running.");

			CliUtil.WriteHeader("Web Server", ConsoleColor.DarkRed);
			CliUtil.LoadingTitle();

			this.NavigateToRoot();

			// Check rights
			this.CheckAdmin();

			// Conf
			this.LoadConf(this.Conf = new WebConf());

			// Database
			this.InitDatabase(this.Conf);

			// Server
			this.StartWebServer();

			CliUtil.RunningTitle();
			_running = true;

			// Commands
			var commands = new ConsoleCommands();
			commands.Wait();
		}

		public void StartWebServer()
		{
			Log.Info("Starting web server...");

			this.App = new WebApplication();

			this.App.Engine("htm", new HandlebarsEngine());

			this.App.Static("user/save/");
			this.App.Static("web/public/");

			this.App.Get("/", new MainController());
			this.App.Post("/ui", new UiStorageController());
			this.App.Post("/visual-chat", new VisualChatController());
			this.App.All("/register", new RegisterController());

			try
			{
				this.App.Listen(this.Conf.Web.Port);

				Log.Status("Server ready, listening on 0.0.0.0:{0}.", this.Conf.Web.Port);
			}
			catch (HttpListenerException)
			{
				Log.Error("Failed to start web server.");
				Log.Info("The port might already be in use, make sure no other application, like other web servers or Skype, are using it or set a different port in web.conf.");
				CliUtil.Exit(1);
			}
		}

		public void CheckAdmin()
		{
			var id = WindowsIdentity.GetCurrent();
			var principal = new WindowsPrincipal(id);

			if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
			{
				Log.Error("The Web Server requires admin permissions, please restart it as admin. See the Wiki for more information.");
				CliUtil.Exit(740);
			}
		}
	}
}
