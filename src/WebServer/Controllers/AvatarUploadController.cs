// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Shared.Util;
using SharpExpress;
using System.Linq;
using System.Text.RegularExpressions;

namespace Aura.Web.Controllers
{
	/// <remarks>
	/// Parameters:
	///   Files:
	///     userfile : snapshot.jpg
	///     usertext : snapshot.txt
	///     
	///   Post:
	///     user_id : admin
	///     name_char : Zerono
	///     name_server : Aura
	///     char_id : 4503599627370498
	///     gender : M
	///     age : 23
	///     key :
	///     title : 60001
	///     name_mate :
	/// 
	/// Response:
	///   "1" for success
	/// </remarks>
	public class AvatarUploadController : IController
	{
		public void Index(Request req, Response res)
		{
			var charId = req.Parameter("char_id");
			var serverName = req.Parameter("name_server");
			var userFile = req.Files.FirstOrDefault(file => file.Name == "userfile");
			var userText = req.Files.FirstOrDefault(file => file.Name == "usertext");

			if (charId == null || !Regex.IsMatch(charId, @"^[0-9]+$") || serverName == null || !Regex.IsMatch(serverName, @"^[0-9A-Za-z_]+$") || userFile == null || userText == null)
				return;

			var key = charId.Substring(charId.Length - 3);
			var folder = "user/save/avatar/" + serverName + "/" + key + "/" + charId + "/";

			if (userFile.HasData) userFile.MoveTo(folder + "snapshot.jpg");
			if (userText.HasData) userText.MoveTo(folder + "snapshot.txt");

			Log.Info("Character '{0}' uploaded a snapshot of himself.", charId);

			res.Send("1");
		}
	}
}
