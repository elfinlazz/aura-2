// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Aura.Shared.Util.Commands
{
	/// <summary>
	/// Generalized command manager
	/// </summary>
	/// <typeparam name="TCommand"></typeparam>
	/// <typeparam name="TFunc"></typeparam>
	public abstract class CommandManager<TCommand, TFunc>
		where TCommand : Command<TFunc>
		where TFunc : class
	{
		protected Dictionary<string, TCommand> _commands;

		protected CommandManager()
		{
			_commands = new Dictionary<string, TCommand>();
		}

		/// <summary>
		/// Adds command to list of command handlers.
		/// </summary>
		/// <param name="command"></param>
		protected void Add(TCommand command)
		{
			if (_commands.ContainsKey(command.Name))
				Log.Warning("Command '{0}' is being overwritten.", command.Name);

			_commands[command.Name] = command;
		}

		/// <summary>
		/// Returns arguments parsed from line.
		/// </summary>
		/// <remarks>
		/// Matches words and multiple lines in quotation.
		/// </remarks>
		/// <example>
		/// > command arg1 arg2 - 3 args, "command", "arg1", "arg2"
		/// > command arg1 "arg2 arg3" - 3 args, "command", "arg1", "arg2 arg3"
		/// </example>
		protected IList<string> ParseLine(string line)
		{
			// Find args, matching words and multiple words in quotation.
			var matches = Regex.Matches(line, @"(""[a-z0-9_\-\.,\+': ]+""|[a-z0-9_\-\.,\+':]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

			// Convert matches
			var args = new List<string>(matches.Count);
			for (var i = 0; i < matches.Count; i++)
				args.Add(matches[i].Groups[1].Value.Trim('"', ' '));

			return args;
		}

		/// <summary>
		/// Returns command or null, if the command doesn't exist.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public TCommand GetCommand(string name)
		{
			TCommand command;
			_commands.TryGetValue(name, out command);
			return command;
		}
	}

	/// <summary>
	/// Generalized command holder
	/// </summary>
	/// <typeparam name="TFunc"></typeparam>
	public abstract class Command<TFunc> where TFunc : class
	{
		public string Name { get; protected set; }
		public string Usage { get; protected set; }
		public string Description { get; protected set; }
		public TFunc Func { get; protected set; }

		protected Command(string name, string usage, string description, TFunc func)
		{
			if (!typeof(TFunc).IsSubclassOf(typeof(Delegate)))
				throw new InvalidOperationException(typeof(TFunc).Name + " is not a delegate type");

			this.Name = name;
			this.Usage = usage;
			this.Description = description;
			this.Func = func;
		}
	}

	public enum CommandResult { Okay, Fail, InvalidArgument, Break }
}
