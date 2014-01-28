// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Login.Database;
using Aura.Shared.Network;

namespace Aura.Login.Network.Handlers
{
	public partial class LoginServerHandlers : PacketHandlerManager<LoginClient>
	{
		/// <summary>
		/// ?
		/// </summary>
		/// <remarks>
		/// No idea what this is. Answer contains a single 0 byte,
		/// possibly a list of some kind. Nothing special happens
		/// when the byte is modified.
		/// </remarks>
		/// <example>
		/// No parameters.
		/// </example>
		[PacketHandler(Op.LoginUnk)]
		public void LoginUnk(LoginClient client, Packet packet)
		{
			Send.LoginUnkR(client, 0);
		}
	}
}
