// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Linq;
using Aura.Data;
using Aura.Login.Database;
using Aura.Shared.Network;
using Aura.Shared.Util;

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

			// Check card
			var card = client.Account.GetCharacterCard(cardId);
			if (card == null)
			{
				Log.Warning("TradeCard: User '{0}' tried to trade a card he doesn't have.", client.Account.Name);
				goto L_Fail;
			}

			// Check target
			var character = client.Account.Characters.FirstOrDefault(a => a.Name == name);
			if (character == null)
			{
				Log.Warning("TradeCard: User '{0}' tried to trade a card with an invalid character.", client.Account.Name);
				goto L_Fail;
			}

			// Check card data
			var cardData = AuraData.CharCardDb.Find(card.Type);
			if (cardData == null)
			{
				Log.Warning("TradeCard: User '{0}' tried to trade a card that's not in the database.", client.Account.Name);
				goto L_Fail;
			}

			// Check trading goods
			if (cardData.TradeItem == 0 && cardData.TradePoints == 0)
				goto L_Fail;

			// Add goods
			LoginServer.Instance.Database.TradeCard(character, cardData);

			// Success
			Send.TradeCardR(client, cardId);

		L_Fail:
			Send.TradeCardR(client, 0);
		}
	}
}
