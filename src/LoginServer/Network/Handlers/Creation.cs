// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Data;
using Aura.Data.Database;
using Aura.Login.Database;
using Aura.Shared.Network;
using Aura.Shared.Util;
using Aura.Shared.Database;
using System.Text.RegularExpressions;

namespace Aura.Login.Network.Handlers
{
	public partial class LoginServerHandlers : PacketHandlerManager<LoginClient>
	{
		/// <summary>
		/// Used while creating new chars/pets/partners,
		/// to see if a name can be used.
		/// </summary>
		/// <example>
		/// 0001 [................] String : mabius1
		/// 0002 [................] String : ...
		/// </example>
		[PacketHandler(Op.NameCheck)]
		public void NameCheck(LoginClient client, MabiPacket packet)
		{
			var server = packet.GetString();
			var name = packet.GetString();

			var result = AuraDb.Instance.NameOkay(name, server);

			Send.NameCheckR(client, result);
		}

		/// <summary>
		/// Character creation request.
		/// </summary>
		/// <example>
		/// ...
		/// </example>
		[PacketHandler(Op.CreateCharacter)]
		public void CreateCharacter(LoginClient client, MabiPacket packet)
		{
			var serverName = packet.GetString();
			var cardId = packet.GetLong();
			var name = packet.GetString();
			var race = packet.GetInt();
			var skinColor = packet.GetByte();
			var hair = packet.GetInt();
			var hairColor = packet.GetByte();
			var age = packet.GetByte();
			var eye = packet.GetByte();
			var eyeColor = packet.GetByte();
			var mouth = packet.GetByte();
			var face = packet.GetInt();

			// Check age
			if (age < 10)
				age = 10;
			else if (age > 17)
				age = 17;

			// Get stuff
			var card = client.Account.GetCharacterCard(cardId);
			var faceItem = AuraData.ItemDb.Find(face);
			var hairItem = AuraData.ItemDb.Find(hair);

			// Check card and server
			if (card == null || !LoginServer.Instance.Servers.Has(serverName))
			{
				Log.Error("Character creation: Missing card or server ({0}).", serverName);
				goto L_Fail;
			}

			// Check face/hair
			if (faceItem == null || hairItem == null || (faceItem.Type != ItemType.Hair && faceItem.Type != ItemType.Face) || (hairItem.Type != ItemType.Hair && hairItem.Type != ItemType.Face))
			{
				Log.Error("Character creation: Invalid face ({0}) or hair ({1}).", face, hair);
				goto L_Fail;
			}

			// Check card type and if the race can use it.
			var cardInfo = AuraData.CharCardDb.Find(card.Type);
			if (cardInfo == null || !cardInfo.Enabled(race))
			{
				Log.Error("Character creation: Missing card ({0}) or race not allowed ({1}).", card.Type, race);
				goto L_Fail;
			}

			// Check age info
			var ageInfo = AuraData.StatsBaseDb.Find(race, age);
			if (ageInfo == null)
			{
				Log.Error("Character creation: Unable to find age info for race '{0}', age '{1}'.", race, age);
				goto L_Fail;
			}

			// Check name
			var nameCheck = AuraDb.Instance.NameOkay(name, serverName);
			if (nameCheck != NameCheckResult.Okay)
			{
				Log.Error("Character creation: Invalid name ({0}).", nameCheck);
				goto L_Fail;
			}

			// Create character
			var character = new Character(CharacterType.Character);
			character.Name = name;
			character.Race = race;
			character.Face = face;
			character.SkinColor = skinColor;
			character.Hair = hair;
			character.HairColor = hairColor;
			character.EyeType = eye;
			character.EyeColor = eyeColor;
			character.MouthType = mouth;
			character.Age = age;
			character.Server = serverName;
			character.Height = (1.0f / 7.0f * (age - 10.0f)); // 0 ~ 1.0

			character.Life = ageInfo.Life;
			character.Mana = ageInfo.Mana;
			character.Stamina = ageInfo.Stamina;
			character.Str = ageInfo.Str;
			character.Int = ageInfo.Int;
			character.Dex = ageInfo.Dex;
			character.Will = ageInfo.Will;
			character.Luck = ageInfo.Luck;
			character.AP = ageInfo.AP;

			character.Region = 1;
			character.X = 12800;
			character.Y = 38100;

			// Try to create character
			if (!client.Account.CreateCharacter(character, cardInfo))
			{
				Log.Error("Character creation: Failed for unknown reasons.");
				goto L_Fail;
			}

			// Success~
			Send.CreateCharacterR(client, serverName, character.Id);

			return;

		L_Fail:
			Send.CreateCharacterR_Fail(client);
		}
	}
}
