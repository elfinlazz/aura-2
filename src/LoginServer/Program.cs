// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Shared.Util;

namespace Aura.LoginServer
{
	class Program
	{
		static void Main(string[] args)
		{
			var config = new Conf();
			config.Require("../../system/conf/login.conf");
			config.Include("../../user/conf/login.conf");

			Log.Archive = "../../log/archive/";
			Log.LogFile = "../../log/login.txt";
			Log.Info("test");
			Log.Error("test");
			try
			{
				var x = new int[10];
				var i = x[11];
			}
			catch (Exception ex)
			{
				Log.Exception(ex, "Failed to do foo");
			}

			Console.ReadLine();
		}
	}
}
