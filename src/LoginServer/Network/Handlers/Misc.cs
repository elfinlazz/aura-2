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

		/// <summary>
		/// Sent when player chooses to trade a character card for
		/// the items and pons.
		/// </summary>
		/// <remarks>
		/// New in NA188.
		/// </remarks>
		/// <example>
		/// 001 [................] String : Zerono
		/// 002 [................] String : Aura
		/// 003 [........00000001] Int    : 1
		/// 004 [0000000000000005] Long   : 5
		/// </example>
		/// <param name="client"></param>
		/// <param name="packet"></param>
		[PacketHandler(Op.TradeCard)]
		public void TradeCard(LoginClient client, Packet packet)
		{
			var name = packet.GetString();
			var server = packet.GetString();
			var cardType = packet.GetInt();
			var cardId = packet.GetLong();

			//var card = client.Account.GetCharacterCard(cardId);
			//if (card == null) return;

			//client.Account.DeleteCharacterCard(card);

			Send.TradeCardR(client, false);
		}
	}
}
