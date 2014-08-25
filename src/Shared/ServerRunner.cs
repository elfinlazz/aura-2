// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using Aura.Shared.Network;
using Aura.Shared.Util.Commands;
using Aura.Shared.Util.Configuration;
using Aura.Shared.Util;

namespace Aura.Shared
{
	/// <summary>
	/// Abstracts commonalities for server runners
	/// </summary>
	public abstract class ServerRunner<TClient, TServer, TConf, THandlers, TCommands> : ServerMain
		where TClient : BaseClient, new()
		where TServer : BaseServer<TClient>, new()
		where TConf : BaseConf, new()
		where THandlers : PacketHandlerManager<TClient>, new ()
		where TCommands : ConsoleCommands, new()
	{
		public abstract string Name { get; }
		protected abstract ConsoleColor _headerColor { get; }
		protected abstract int _listenPort { get; }
		protected virtual DataLoad _dataLoad { get { return DataLoad.None; } }

		protected bool _running { get; set; }

		/// <summary>
		/// Instance of the actual server component.
		/// </summary>
		public TServer Server { get; protected set; }

		/// <summary>
		/// Configuration
		/// </summary>
		public TConf Conf { get; protected set; }

		protected ServerRunner()
		{
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

			this.Server = new TServer();
			this.Server.Handlers = new THandlers();
			this.Server.Handlers.AutoLoad();
		}

		/// <summary>
		/// Loads all necessary components and starts the server.
		/// </summary>
		public virtual void Run()
		{
			CliUtil.WriteHeader(this.Name, this._headerColor);
			CliUtil.LoadingTitle();

			if (_running)
				throw new Exception(this.Name + " is already running.");

			this.NavigateToRoot();

			// Conf
			this.LoadConf(this.Conf = new TConf());

			// Database
			this.InitDatabase(this.Conf);

			// Data
			this.LoadData(this._dataLoad, false);

			// Localization
			this.LoadLocalization(this.Conf);

			this.AfterSetup();

			// Start
			this.Server.Start(this._listenPort);

			this.AfterStart();

			CliUtil.RunningTitle();
			_running = true;

			// Commands
			var commands = new TCommands();
			commands.Wait();
		}

		/// <summary>
		/// Executed after basic setup but before the server
		/// starts listening.
		/// 
		/// Use this to set up additional state.
		/// </summary>
		protected virtual void AfterSetup()
		{
			
		}

		/// <summary>
		/// Executed before the server's thread blocks reading commands
		/// 
		/// Use it to run any specific post-start code
		/// </summary>
		protected virtual void AfterStart()
		{
			
		}

		/// <summary>
		/// Handler for unhandled exceptions.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			try
			{
				Log.Error("Oh no! Ferghus escaped his memory block and infected the rest of the server!");
				Log.Error("Aura has encountered an unexpected and unrecoverable error. We're going to try to save as much as we can.");
			}
			catch { }
			try
			{
				this.Server.Stop();
			}
			catch { }
			try
			{
				// save the world
			}
			catch { }
			try
			{
				Log.Exception((Exception)e.ExceptionObject);
				Log.Status("Closing server.");
			}
			catch { }

			CliUtil.Exit(1, false);
		}
	}
}
