// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Shared.Database;

namespace Aura.Shared.Util
{
	public static class ServerUtil
	{
		public static void LoadConf(BaseConf conf)
		{
			try
			{
				conf.Load();
				Log.Info("Read configuration.");
			}
			catch (Exception ex)
			{
				Log.Exception(ex, "Unable to read configuration. ({0})", ex.Message);
				CmdUtil.Exit(1);
			}
		}

		public static void InitDatabase(BaseConf conf)
		{
			try
			{
				AuraDb.Instance.Init(conf.Host, conf.User, conf.Pass, conf.Db);

				Log.Info("Initialized database.");
			}
			catch (Exception ex)
			{
				Log.Exception(ex, "Unable to open database connection. ({0})", ex.Message);
				CmdUtil.Exit(1);
			}
		}
	}
}
