// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Login.Network;
using SharpExpress;

namespace Aura.Login.Web
{
	public class BroadcastController : IController
	{
		public void Index(Request req, Response res)
		{
			if (!LoginServer.Instance.Conf.Login.IsTrustedSource(req.ClientIp))
				return;

			var msg = req.Parameter("msg", null);
			if (!string.IsNullOrWhiteSpace(msg))
				Send.Internal_Broadcast(msg);
		}
	}
}
