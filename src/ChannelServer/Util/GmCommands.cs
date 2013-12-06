// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using Aura.Channel.Network;
using Aura.Channel.Network.Sending;
using Aura.Channel.World.Entities;
using Aura.Shared.Util.Commands;
using Aura.Shared.Util;
using System.Globalization;

namespace Aura.Channel.Util
{
	public class GmCommandManager : CommandManager<GmCommand, GmCommandFunc>
	{
		public GmCommandManager()
		{
			Add(40, "warp", "", HandleWarp);
			Add(99, "test", "", HandleTest);
			Add(99, "where", "", HandleWhere);
		}

		// ------------------------------------------------------------------

		/// <summary>
		/// Adds new command.
		/// </summary>
		/// <param name="auth"></param>
		/// <param name="name"></param>
		/// <param name="usage"></param>
		/// <param name="func"></param>
		public void Add(int auth, string name, string usage, GmCommandFunc func)
		{
			this.Add(new GmCommand(auth, name, usage, func));
		}

		/// <summary>
		/// Adds alias for command.
		/// </summary>
		/// <param name="commandName"></param>
		/// <param name="alias"></param>
		public void AddAlias(string commandName, string alias)
		{
			var command = this.GetCommand(commandName);
			if (command == null)
				throw new Exception("Aliasing: Command " + commandName + " not found");

			_commands[alias] = command;
		}

		/// <summary>
		/// Tries to run command, based on message.
		/// Returns false if the message was of no interest to the
		/// command handler.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="creature"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public bool Process(ChannelClient client, Creature creature, string message)
		{
			if (message.Length < 2 || !message.StartsWith(ChannelServer.Instance.Conf.Commands.Prefix.ToString()))
				return false;

			var args = this.ParseLine(message);
			args[0].TrimStart(ChannelServer.Instance.Conf.Commands.Prefix);
			var command = this.GetCommand(args[0]);

			if (command == null || client.Account.Authority < command.Auth)
			{
				Send.ServerMessage(creature, "Unknown command '{0}'.", args[0]);
				return true;
			}

			var result = command.Func(client, creature, creature, message, args);

			if (result == CommandResult.InvalidArgument)
			{
				Send.ServerMessage(creature, "Usage: {0} {1}", command.Name, command.Usage);
				return true;
			}

			if (result == CommandResult.Fail)
			{
				Send.ServerMessage(creature, "Failed to process command.");
				return true;
			}

			return true;
		}

		// ------------------------------------------------------------------

		public CommandResult HandleTest(ChannelClient client, Creature sender, Creature target, string message, string[] args)
		{
			//for (int i = 0; i < args.Length; ++i)
			//{
			//    Send.ServerMessage(sender, "Arg{0}: {1}", i, args[i]);
			//}

			return CommandResult.Okay;
		}

		public CommandResult HandleWhere(ChannelClient client, Creature sender, Creature target, string message, string[] args)
		{
			var pos = sender.GetPosition();

			Send.ServerMessage(sender, "You're here: {0} @ {1}, {2}", sender.RegionId, pos.X, pos.Y);

			return CommandResult.Okay;
		}

		public CommandResult HandleWarp(ChannelClient client, Creature sender, Creature target, string message, string[] args)
		{
			if (args.Length < 2)
				return CommandResult.InvalidArgument;

			var pos = target.GetPosition();
			int regionId = 0, x = pos.X, y = pos.Y;

			if (!int.TryParse(args[1], out regionId))
				return CommandResult.InvalidArgument;
			if (args.Length > 2 && !int.TryParse(args[2], out x))
				return CommandResult.InvalidArgument;
			if (args.Length > 3 && !int.TryParse(args[3], out y))
				return CommandResult.InvalidArgument;

			target.Warp(regionId, x, y);

			return CommandResult.Okay;
		}
	}

	public class GmCommand : Command<GmCommandFunc>
	{
		public int Auth { get; private set; }

		public GmCommand(int auth, string name, string usage, GmCommandFunc func)
			: base(name, usage, "", func)
		{
			this.Auth = auth;
		}
	}

	public delegate CommandResult GmCommandFunc(ChannelClient client, Creature sender, Creature target, string message, string[] args);
}
