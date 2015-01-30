// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;

namespace Aura.Shared.Util
{
	public class CliUtil
	{
		/// <summary>
		/// Prints Aura's ASCII art.
		/// </summary>
		/// <param name="title">Name of this server (for the console's title)</param>
		/// <param name="color">Color of the header</param>
		public static void WriteHeader(string title, ConsoleColor color)
		{
			if (title != null)
				Console.Title = "Aura : " + title;

			Console.ForegroundColor = color;
			Console.Write(@"                          __     __  __  _ __    __                             ");
			Console.Write(@"                        /'__`\  /\ \/\ \/\`'__\/'__`\                           ");
			Console.Write(@"                       /\ \L\.\_\ \ \_\ \ \ \//\ \L\.\_                         ");
			Console.Write(@"                       \ \__/.\_\\ \____/\ \_\\ \__/.\_\                        ");
			Console.Write(@"                        \/__/\/_/ \/___/  \/_/ \/__/\/_/                        ");
			Console.Write(@"                                                                                ");

			Console.ForegroundColor = ConsoleColor.White;
			Console.Write(@"                         by the Aura development team                           ");

			Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.Write(@"________________________________________________________________________________");

			Console.WriteLine("");
		}

		public static void LoadingTitle()
		{
			if (!Console.Title.StartsWith("* "))
				Console.Title = "* " + Console.Title;
		}

		public static void RunningTitle()
		{
			Console.Title = Console.Title.TrimStart('*', ' ');
		}

		/// <summary>
		/// Waits for the return key, and closes the application afterwards.
		/// </summary>
		/// <param name="exitCode"></param>
		/// <param name="wait"></param>
		public static void Exit(int exitCode, bool wait = true)
		{
			if (wait)
			{
				Log.Info("Press Enter to exit.");
				Console.ReadLine();
			}
			Log.Info("Exiting...");
			Environment.Exit(exitCode);
		}

		/// <summary>
		/// Returns whether the application runs with admin rights or not.
		/// </summary>
		public static bool CheckAdmin()
		{
			var id = WindowsIdentity.GetCurrent();
			var principal = new WindowsPrincipal(id);

			return principal.IsInRole(WindowsBuiltInRole.Administrator);
		}
	}
}
