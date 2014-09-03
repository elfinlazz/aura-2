// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Shared.Database;
using Aura.Shared.Mabi;
using Aura.Shared.Util;
using SharpExpress;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Aura.Web.Controllers
{
	public class RegisterController : IController
	{
		public void Index(Request req, Response res)
		{

			var name = req.Parameter("name");
			var pass1 = req.Parameter("password1");
			var pass2 = req.Parameter("password2");

			var error = "";
			var success = "";

			if (name != null && pass1 != null && pass2 != null)
			{
				if (pass1 != pass2)
				{
					error = "The passwords don't match.";
					goto L_Send;
				}

				if (name.Length < 4)
				{
					name = "";
					error = "Username too short (min. 4 characters).";
					goto L_Send;
				}

				if (pass1.Length < 6)
				{
					error = "Password too short (min. 6 characters).";
					goto L_Send;
				}

				if (!Regex.IsMatch(name, @"^[0-9A-Za-z]+$"))
				{
					error = "Username contains invalid characters.";
					goto L_Send;
				}

				if (WebServer.Instance.Database.AccountExists(name))
				{
					error = "Account already exists.";
					goto L_Send;
				}

				var passHash = Password.RawToMD5SHA256(pass1);

				WebServer.Instance.Database.CreateAccount(name, passHash);

				Log.Info("New account created: {0}", name);

				name = "";
				success = "Account created successfully.";
			}

		L_Send:
			res.Render("web/register.htm", new { error, success, name });
		}
	}
}
