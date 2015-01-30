// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Collections.Generic;
using Aura.Login.Database;
using Aura.Shared.Util;
using Aura.Shared.Util.Commands;

namespace Aura.Login.Util
{
	public class LoginConsoleCommands : ConsoleCommands
	{
		public LoginConsoleCommands()
		{
			this.Add("shutdown", "Orders all servers to shut down", HandleShutDown);
			this.Add("auth", "<account> <level>", "Changes authority level of account", HandleAuth);
		}

		private CommandResult HandleShutDown(string command, IList<string> args)
		{
			Log.Info("(Unimplemented)");

			return CommandResult.Okay;
		}

		private CommandResult HandleAuth(string command, IList<string> args)
		{
			if (args.Count < 3)
				return CommandResult.InvalidArgument;

			int level;
			if (!int.TryParse(args[2], out level))
				return CommandResult.InvalidArgument;

			if (!LoginServer.Instance.Database.ChangeAuth(args[1], level))
			{
				Log.Error("Failed to change auth. (Does the account exist?)");
				return CommandResult.Okay;
			}

			Log.Info("Changed auth successfully.");

			return CommandResult.Okay;
		}
	}
}
