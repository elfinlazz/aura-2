// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Aura.Login.Network;
using Aura.Login.Network.Handlers;
using Aura.Login.Database;
using Aura.Login.Util;
using Aura.Shared.Network;
using Aura.Shared.Util;

namespace Aura.Login.Updater
{
	class UpdaterUtil
	{
		public static readonly UpdaterUtil Instance = new UpdaterUtil();

		private UpdaterUtil()
		{
		}

		public void CheckUpdates()
		{
			Log.Info("Checking for updates..");

			try
			{
				string[] fileEntries = Directory.GetFiles(Directory.GetCurrentDirectory() + "/sql");
				foreach (string fileName in fileEntries)
				{
					if (Path.GetExtension(fileName).Equals(".sql"))
					{
						this.RunUpdate(Path.GetFileName(fileName));
					}
				}
				//Log.Info("Done checking updates.");
			}
			catch (Exception ex)
			{
				Log.Exception(ex, "Error while running update files.");
				CliUtil.Exit(1);
			}
		}

		public void RunUpdate(string fileName)
		{
			if (!LoginDb.Instance.CheckUpdate(fileName))
			{
				Log.Info("Update " + fileName + " found.");
				Log.Status("Running update...");

				if (!LoginDb.Instance.RunUpdate(fileName))
					Log.Error("Update " + fileName + " failed.");
				else
					Log.Info("Update " + fileName + " has been successful.");
			}
		}
	}
}
