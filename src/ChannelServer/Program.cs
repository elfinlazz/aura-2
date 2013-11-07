// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using Aura.Channel.Network;
using Aura.Channel.Util;
using Aura.Shared.Util;

namespace Aura.Channel
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				ChannelServer.Instance.Run();
			}
			catch (Exception ex)
			{
				Log.Exception(ex, "An exception occured while starting the server.");
				CliUtil.Exit(1);
			}
		}
	}
}
