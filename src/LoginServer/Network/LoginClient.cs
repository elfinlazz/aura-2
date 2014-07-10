// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Login.Database;
using Aura.Shared.Network;

namespace Aura.Login.Network
{
	public class LoginClient : DefaultClient
	{
		public Account Account { get; set; }
	}
}
