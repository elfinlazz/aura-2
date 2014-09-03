// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Login.Database;
using Aura.Shared.Mabi;
using SharpExpress;

namespace Aura.Login.Web
{
	public class CheckUserController : IController
	{
		public void Index(Request req, Response res)
		{
			if (!LoginServer.Instance.Conf.Login.IsTrustedSource(req.ClientIp))
				return;

			var name = req.Parameter("name");
			var pass = req.Parameter("pass");

			// Check parameters
			if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(pass))
			{
				res.Send("0");
				return;
			}

			// Get account
			var account = LoginServer.Instance.Database.GetAccount(name);
			if (account == null)
			{
				res.Send("0");
				return;
			}

			// Check password
			var passwordCorrect = Password.Check(pass, account.Password);

			// Response
			res.Send(passwordCorrect ? "1" : "0");
		}
	}
}
