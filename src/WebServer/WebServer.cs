// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Shared.Util;
using Aura.Shared.Util.Commands;
using SharpExpress;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Aura.Web
{
	public class WebServer : ServerMain
	{
		public static readonly WebServer Instance = new WebServer();

		public WebApplication App;

		private bool _running = false;

		/// <summary>
		/// Loads all necessary components and starts the server.
		/// </summary>
		public void Run()
		{
			if (_running)
				throw new Exception("Server is already running.");

			CliUtil.WriteHeader("Web Server", ConsoleColor.DarkRed);
			CliUtil.LoadingTitle();

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

			this.App.Get("/", (req, res) => { res.Send("Aura Web Server"); });

			try
			{
				this.App.Listen(8080);

				Log.Status("Server ready, listening on *:{0}.", 8080);
			}
			catch (HttpListenerException)
			{
				Log.Error("Failed to start web server, port already in use?");
				CliUtil.Exit(1);
			}
		}
	}
}
