// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using SharpExpress;

namespace Aura.Web.Controllers
{
	public class MainController : IController
	{
		public void Index(Request req, Response res)
		{
			res.Send("Aura Web Server");
		}
	}
}
