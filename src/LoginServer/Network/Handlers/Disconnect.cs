// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Login.Database;
using Aura.Shared.Network;
using Aura.Shared.Util;

namespace Aura.Login.Network.Handlers
{
	public partial class LoginServerHandlers : PacketHandlerManager<LoginClient>
	{
		/// <summary>
		/// Sent before going back to login screen or continuing to a channel.
		/// </summary>
		/// <remarks>
		/// No response, it only informs us that the connection is being closed.
		/// </remarks>
		/// <example>
		/// 0001 [................] String : admin
		/// 0002 [4D80902C6DF00000] Long   : ?
		/// 0003 [........800000EE] Int    : 2147483886
		/// 0004 [........00000000] Int    : 0
		/// 0005 [........00000000] Int    : 0
		/// </example>
		[PacketHandler(Op.DisconnectInform)]
		public void Disconnect(LoginClient client, Packet packet)
		{
			var accountName = packet.GetString();
			var unkLong = packet.GetLong();
			var unkInt1 = packet.GetInt();
			var unkInt2 = packet.GetInt();
			var unkInt3 = packet.GetInt();

			if (accountName != client.Account.Name)
				return;

			client.Account.LoggedIn = false;
			LoginServer.Instance.Database.UpdateAccount(client.Account);

			Log.Info("'{0}' is closing the connection.", accountName);
		}
	}
}
