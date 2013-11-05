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
			CliUtil.WriteHeader("Channel Server", ConsoleColor.Magenta);
			CliUtil.LoadingTitle();

			ServerUtil.NavigateToRoot();

			// Conf
			ServerUtil.LoadConf(ChannelConf.Instance);

			// Database
			ServerUtil.InitDatabase(ChannelConf.Instance);

			// Data
			ServerUtil.LoadData(DataLoad.WorldServer, false);

			// Localization
			ServerUtil.LoadLocalization(ChannelConf.Instance);

			// Start
			ChannelServer.Instance.Start(ChannelConf.Instance.Port);

			CliUtil.RunningTitle();

			Console.ReadLine();
		}
	}
}
