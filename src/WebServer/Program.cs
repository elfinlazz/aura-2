// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aura.Web
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				WebServer.Instance.Run();
			}
			catch (Exception ex)
			{
				Log.Exception(ex, "An exception occured while starting the server.");
				CliUtil.Exit(1);
			}
		}
	}
}
