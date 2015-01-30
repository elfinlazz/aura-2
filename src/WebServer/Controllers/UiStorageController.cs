// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Shared.Util;
using SharpExpress;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Aura.Web.Controllers
{
	public class UiStorageController : IController
	{
		/// <remarks>
		/// Parameters:
		/// 
		///   Files:
		///     ui  ^[0-9]{16}\.xml$  XML file containing the ui settings
		/// 
		///   Post:
		///     char_id          long    Id of the character
		///     name_server      string  Name of the server
		///     ui_load_success  bool    Whether loading was successful
		/// 
		/// Security:
		///   Since the client doesn't give us anything but char id and
		///   server name, the settings could easily be overwritten
		///   by anybody.
		/// </remarks>
		public void Index(Request req, Response res)
		{
			// Get file
			var file = req.Files.FirstOrDefault(a => a.Name == "ui");
			if (file == null || !file.HasData)
			{
				Log.Error("UiStorageController: Missing file.");
				return;
			}

			// Check file name
			if (!Regex.IsMatch(file.FileName, @"^[0-9]{16}\.xml$"))
			{
				Log.Error("UiStorageController: Invalid file name '{0}'.", file.FileName);
				return;
			}

			var charId = req.Parameter("char_id", null);
			var serverName = req.Parameter("name_server", null);
			var loadSuccess = req.Parameter("ui_load_success", null);

			// Check parameters
			if (!Regex.IsMatch(charId, @"^[0-9]{16}$") || !Regex.IsMatch(charId, @"^[0-9A-Za-z_ ]+$"))
			{
				Log.Error("UiStorageController: Invalid character id ({0}) or server name ({1}).", charId, serverName);
				return;
			}

			var group = charId.Substring(charId.Length - 3);

			// Move file
			try
			{
				file.MoveTo("user/save/ui/" + serverName + "/" + group + "/" + file.FileName);
			}
			catch (Exception ex)
			{
				Log.Exception(ex, "UiStorageController: Failed to move file.");
			}

			Log.Info("Character '{0}' uploaded his UI settings.", charId);

			// Success
			res.Send("1");
		}
	}
}
