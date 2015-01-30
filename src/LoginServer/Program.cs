// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Diagnostics;
using Aura.Login.Network;
using Aura.Login.Util;
using Aura.Shared.Network.Crypto;
using Aura.Shared.Util;
using System.Linq;

namespace Aura.Login
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				LoginServer.Instance.Run();
			}
			catch (Exception ex)
			{
				Log.Exception(ex, "An exception occured while starting the server.");
				CliUtil.Exit(1);
			}
		}
	}
}
