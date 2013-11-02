// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence file in the main folder

using System;
using Aura.Login.Database;
using Aura.Login.Util;
using Aura.Shared.Mabi;
using Aura.Shared.Network;

namespace Aura.Login.Network.Handlers
{
	public partial class LoginServerHandlers : PacketHandlerManager<LoginClient>
	{
		/// <summary>
		/// Character/Pet/Partner deletion (request) / restoring
		/// </summary>
		/// <remarks>
		/// Deletion is the same for all creatures, only difference is the ops.
		/// We also handle deletion, requesting deletion, and restoring at once.
		/// </remarks>
		/// <example>
		/// Character
		/// 001 [................] String : Aura
		/// 002 [..10000000000012] Long   : 4503599627370514
		/// 003 [................] String : name
		/// </example>
		[PacketHandler(Op.DeleteCharacterRequest, Op.DeletePetRequest, Op.RecoverCharacter, Op.RecoverPet, Op.DeleteCharacter, Op.DeletePet)]
		public void Delete(LoginClient client, MabiPacket packet)
		{
			var serverName = packet.GetString();
			var id = packet.GetLong();

			var isPet = (packet.Op >= Op.DeletePetRequest);
			var character = (isPet ? client.Account.GetPet(id) : client.Account.GetCharacter(id));
			var now = ErinnTime.Now.DateTime;

			// The response op is always +1.
			var op = packet.Op + 1;

			// Check creature and whether it can be deleted already,
			// if we got a delete request.
			if (character == null || ((op == Op.DeleteCharacterR || op == Op.DeletePetR) && character.DeletionTime > now))
			{
				Send.DeleteR_Fail(client, op);
				return;
			}

			// If character is supposed to be deleted, or it's a request, and there is no delay configured
			if (
				(packet.Op == Op.DeleteCharacter || packet.Op == Op.DeletePet) ||
				((packet.Op == Op.DeleteCharacterRequest || packet.Op == Op.DeletePetRequest) && LoginConf.Instance.DeletionWait == 0)
			)
			{
				// Mark for deletion, will be done on UpdateDeletionTime.
				character.DeletionTime = DateTime.MaxValue;
				if (!isPet)
					client.Account.Characters.Remove(character);
				else
					client.Account.Pets.Remove(character);

				// Send account info, to remove char on client side
				Send.AccountInfoRequestR(client, true);
			}
			// If character is being recovered
			else if (packet.Op == Op.RecoverCharacter || packet.Op == Op.RecoverPet)
			{
				// Reset time
				character.DeletionTime = DateTime.MinValue;
			}
			// Op.DeleteCharRequest || Op.DeletePetRequest || Error?
			else
			{
				// Set time at which the character can be deleted for good.
				// Below 100 means x hours, above 100 tomorrow at x.
				if (LoginConf.Instance.DeletionWait > 100)
					character.DeletionTime = (now.AddDays(1).Date + new TimeSpan(LoginConf.Instance.DeletionWait - 100, 0, 0));
				else
					character.DeletionTime = now.AddHours(LoginConf.Instance.DeletionWait);
			}

			LoginDb.Instance.UpdateDeletionTime(character);

			// Successful response
			Send.DeleteR(client, op, serverName, id);
		}
	}
}
