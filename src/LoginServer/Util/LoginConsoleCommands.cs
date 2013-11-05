// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Shared.Util;
using Aura.Shared.Util.Commands;

namespace Aura.Login.Util
{
	public class LoginConsoleCommands : ConsoleCommands
	{
		public LoginConsoleCommands()
		{
			this.Add("shutdown", "Orders all servers to shut down", HandleShutDown);
		}

		private CommandResult HandleShutDown(string command, string[] args)
		{
			Log.Info("...");

			return CommandResult.Okay;
		}
	}
}
