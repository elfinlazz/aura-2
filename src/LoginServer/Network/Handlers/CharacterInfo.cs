// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Shared.Network;
using Aura.Login.Database;

namespace Aura.Login.Network.Handlers
{
	public partial class LoginServerHandlers : PacketHandlerManager<LoginClient>
	{
		/// <summary>
		/// Character information request, if client doesn't have it cached.
		/// </summary>
		/// <example>
		/// 0001 [................] String : mabius1
		/// 0002 [0010000000000000] Long   : ...
		/// </example>
		[PacketHandler(Op.CharacterInfoRequest)]
		public void CharacterInfoRequest(LoginClient client, MabiPacket packet)
		{
			var server = packet.GetString();
			var characterId = packet.GetLong();

			var character = client.Account.GetCharacter(characterId);
			if (character == null)
			{
				Send.CharacterInfo_Fail(client);
				return;
			}

			var items = LoginDb.Instance.GetEquipment(character.CreatureId);

			Send.CharacterInfo(client, character, items);
		}

		/// <summary>
		/// Pet/Partner information request, if client doesn't have it cached.
		/// </summary>
		/// <example>
		/// 0001 [................] String : mabius1
		/// 0002 [0010010000000000] Long   : ...
		/// </example>
		[PacketHandler(Op.PetInfoRequest)]
		public void PetInfoRequest(LoginClient client, MabiPacket packet)
		{
			var server = packet.GetString();
			var petId = packet.GetLong();

			var character = client.Account.GetPet(petId);
			if (character == null)
			{
				Send.CharacterInfo_Fail(client);
				return;
			}

			var items = LoginDb.Instance.GetEquipment(character.CreatureId);

			Send.CharacterInfo(client, character, items);
		}
	}
}
