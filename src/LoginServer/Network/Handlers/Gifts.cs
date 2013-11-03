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
		/// Accepts a gift.
		/// </summary>
		/// <remarks>
		/// Turns the gift (card) into a character or pet card.
		/// </remarks>
		/// <example>
		/// ...
		/// </example>
		[PacketHandler(Op.AcceptGift)]
		public void AcceptGift(LoginClient client, Packet packet)
		{
			var giftId = packet.GetLong();

			var gift = client.Account.GetGift(giftId);
			if (gift != null)
				client.Account.ChangeGiftToCard(gift);

			Send.AcceptGiftR(client, gift);
		}

		/// <summary>
		/// Refuses and deletes gift.
		/// </summary>
		/// <example>
		/// ...
		/// </example>
		[PacketHandler(Op.RefuseGift)]
		public void RefuseGift(LoginClient client, Packet packet)
		{
			var giftId = packet.GetLong();

			var gift = client.Account.GetGift(giftId);
			if (gift != null)
				client.Account.DeleteGift(gift);

			// XXX: The gift should probably be returned to the donator,
			//  or he'd lose "money".

			Send.RefuseGiftR(client, (gift != null));
		}
	}
}
