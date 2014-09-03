// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Login.Database;
using Aura.Shared.Network;

namespace Aura.Login.Network.Handlers
{
	public partial class LoginServerHandlers : PacketHandlerManager<LoginClient>
	{
		/// <summary>
		/// Character information request, if client doesn't have it cached.
		/// </summary>
		/// <remarks>
		/// Handles characters, pets, and partners. The results are pretty
		/// much the same, aside from the op.
		/// </remarks>
		/// <example>
		/// 0001 [................] String : mabius1
		/// 0002 [0010000000000000] Long   : id
		/// </example>
		[PacketHandler(Op.CharacterInfoRequest, Op.PetInfoRequest)]
		public void CharacterInfoRequest(LoginClient client, Packet packet)
		{
			var server = packet.GetString();
			var characterId = packet.GetLong();

			var op = packet.Op + 1;

			var character = (packet.Op == Op.CharacterInfoRequest ? client.Account.GetCharacter(characterId) : client.Account.GetPet(characterId));
			if (character == null)
			{
				Send.CharacterInfoRequestR_Fail(client, op);
				return;
			}

			var items = LoginServer.Instance.Database.GetEquipment(character.CreatureId);

			Send.CharacterInfoRequestR(client, op, character, items);
		}

		/// <summary>
		/// Requests update of the cards/chars
		/// </summary>
		/// <remarks>
		/// Apperantly only happens after gift handling.
		/// </remarks>
		/// <example>
		/// ...
		/// </example>
		[PacketHandler(Op.AccountInfoRequest)]
		public void AccountInfoRequest(LoginClient client, Packet packet)
		{
			if (client.Account != null)
				Send.AccountInfoRequestR(client, true);
			else
				Send.AccountInfoRequestR(client, false);
		}
	}
}
