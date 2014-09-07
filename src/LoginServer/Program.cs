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
			var c = new MabiCrypto(0x0, false);
			var s = new MabiCrypto(0x0, true);

			var p = new byte[25];

			s.FromServer(p, 6, p.Length);
			c.FromServer(p, 6, p.Length);

			Debug.Assert(p.All(b => b == 0));

			c.FromClient(p, 6, p.Length);
			s.FromClient(p, 6, p.Length);

			Debug.Assert(p.All(b => b == 0));

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
