// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using SharpExpress;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Aura.Web.Controllers
{
	/// <remarks>
	/// Parameters:
	///   Files:
	///     visualchat.png   Chat image, max size 256x96
	///     
	///   Post:
	///     server        string  Server name
	///     characterid   long    Character's entity id
	///     charname      string  Character's name
	/// </remarks>
	public class VisualChatController : IController
	{
		public void Index(Request req, Response res)
		{
			var server = req.Parameter("server", "");
			var characterId = req.Parameter("characterid", "");
			var characterName = req.Parameter("charname", "");
			var file = req.Files.FirstOrDefault();

			// Check char name
			if (!Regex.IsMatch(characterName, @"^[0-9A-Za-z_]+$"))
				return;

			// Check file
			if (file.FileName != "visualchat.png" || file.ContentType != "image/png")
				return;

			// Move file
			var fileName = string.Format("chat_{0:yyyyMMdd_HHmmss}_{1}.png", DateTime.Now, characterName);
			file.MoveTo("user/save/visual-chat/" + fileName);

			// Response, URL to image
			res.Send("http://" + req.HttpHost + ":" + req.HttpPort + "/" + "user/save/visual-chat/" + fileName);
		}
	}
}
