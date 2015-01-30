// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using Aura.Data;
using Aura.Data.Database;
using Aura.Login.Database;
using Aura.Login.Util;
using Aura.Shared.Database;
using Aura.Shared.Network;
using Aura.Shared.Util;
using Aura.Shared.Mabi.Const;

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
		/// 0002 [................] String : name
		/// </example>
		[PacketHandler(Op.NameCheck)]
		public void NameCheck(LoginClient client, Packet packet)
		{
			var server = packet.GetString();
			var name = packet.GetString();

			var result = LoginServer.Instance.Database.NameOkay(name, server);

			Send.NameCheckR(client, result);
		}

		/// <summary>
		/// Character creation request.
		/// </summary>
		/// <example>
		/// Giant
		// 001 [................] String : Aura
		// 002 [........00000019] Long   : 25
		// 003 [................] String : dfaff
		// 004 [........00001F41] Int    : 8001
		// 005 [..............13] Byte   : 19
		// 006 [........00001B6E] Int    : 7022
		// 007 [..............19] Byte   : 25
		// 008 [..............0A] Byte   : 10
		// 009 [..............41] Byte   : 65
		// 010 [..............1F] Byte   : 31
		// 011 [..............1F] Byte   : 31
		// 012 [........00001EDC] Int    : 7900
		/// </example>
		[PacketHandler(Op.CreateCharacter)]
		public void CreateCharacter(LoginClient client, Packet packet)
		{
			var serverName = packet.GetString();
			var cardId = packet.GetLong();
			var name = packet.GetString();
			var race = packet.GetInt();
			var skinColor = packet.GetByte();
			var hair = packet.GetInt();
			var hairColor = packet.GetByte();
			var age = packet.GetByte();

			// [180600, NA187 (25.06.2014)] Changed from byte to short
			var eye = packet.GetShort();

			var eyeColor = packet.GetByte();
			var mouth = packet.GetByte();
			var face = packet.GetInt();

			// Check age
			age = Math.Max((byte)10, Math.Min((byte)17, age));

			// Get stuff
			var card = client.Account.GetCharacterCard(cardId);
			var faceItem = AuraData.ItemDb.Find(face);
			var hairItem = AuraData.ItemDb.Find(hair);

			// Check card and server
			if (card == null || !LoginServer.Instance.ServerList.Has(serverName))
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
			var nameCheck = LoginServer.Instance.Database.NameOkay(name, serverName);
			if (nameCheck != NameCheckResult.Okay)
			{
				Log.Error("Character creation: Invalid name ({0}).", nameCheck);
				goto L_Fail;
			}

			// Create character
			var character = new Character();
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

			// Try to create character
			if (!client.Account.CreateCharacter(character, cardInfo))
			{
				Log.Error("Character creation: Failed for unknown reasons.");
				goto L_Fail;
			}

			// Delete card
			if (LoginServer.Instance.Conf.Login.ConsumeCharacterCards)
			{
				if (!client.Account.DeleteCharacterCard(card))
					goto L_Fail;
			}

			// Success~
			Send.CreateCharacterR(client, serverName, character.EntityId);

			Log.Info("New character '{0}' created in account '{1}'.", name, client.Account.Name);

			return;

		L_Fail:
			Send.CreateCharacterR_Fail(client);
		}

		/// <summary>
		/// Pet creation request.
		/// </summary>
		/// <example>
		/// 0001 [................] String : mabius1
		/// 0002 [0000000000104054] Long   : 1065044
		/// 0003 [................] String : name
		/// 0004 [........00000000] Int    : 0
		/// 0005 [........00000000] Int    : 0
		/// 0006 [........00000000] Int    : 0
		/// </example>
		[PacketHandler(Op.CreatePet)]
		public void CreatePet(LoginClient client, Packet packet)
		{
			var serverName = packet.GetString();
			var cardId = packet.GetLong();
			var name = packet.GetString();
			var color1 = packet.GetUInt();
			var color2 = packet.GetUInt();
			var color3 = packet.GetUInt();

			// Check card and server
			var card = client.Account.GetPetCard(cardId);
			if (card == null || !LoginServer.Instance.ServerList.Has(serverName))
			{
				Log.Error("Pet creation: Missing card or server ({0}).", serverName);
				goto L_Fail;
			}

			// Check pet info
			var petInfo = AuraData.PetDb.Find(card.Race);
			if (petInfo == null)
			{
				Log.Error("Pet creation: Missing pet info ({0}).", card.Race);
				goto L_Fail;
			}

			// Check name
			var nameCheck = LoginServer.Instance.Database.NameOkay(name, serverName);
			if (nameCheck != NameCheckResult.Okay)
			{
				Log.Error("Pet creation: Invalid name ({0}).", nameCheck);
				goto L_Fail;
			}

			// Create pet
			var pet = new Character();
			pet.Name = name;
			pet.Race = card.Race;
			pet.Age = 1;
			pet.Server = serverName;
			pet.State |= CreatureStates.Initialized;
			pet.Height = petInfo.Height;
			pet.Upper = petInfo.Upper;
			pet.Lower = petInfo.Lower;

			pet.Life = petInfo.Life;
			pet.Mana = petInfo.Mana;
			pet.Stamina = petInfo.Stamina;
			pet.Str = petInfo.Str;
			pet.Int = petInfo.Int;
			pet.Dex = petInfo.Dex;
			pet.Will = petInfo.Will;
			pet.Luck = petInfo.Luck;
			pet.Defense = petInfo.Defense;
			pet.Protection = petInfo.Protection;

			if (color1 > 0 || color2 > 0 || color3 > 0)
			{
				pet.Color1 = color1;
				pet.Color2 = color2;
				pet.Color3 = color3;
			}
			else
			{
				pet.Color1 = petInfo.Color1;
				pet.Color2 = petInfo.Color2;
				pet.Color3 = petInfo.Color3;
			}

			// Try to create pet
			if (!client.Account.CreatePet(pet))
			{
				Log.Error("Pet creation: Failed for unknown reasons.");
				goto L_Fail;
			}

			// Delete card
			if (LoginServer.Instance.Conf.Login.ConsumePetCards)
			{
				if (!client.Account.DeletePetCard(card))
					goto L_Fail;
			}

			// Success~
			Send.CreatePetR(client, serverName, pet.EntityId);

			Log.Info("New pet '{0}' created in account '{1}'.", name, client.Account.Name);

			return;

		L_Fail:
			Send.CreatePetR_Fail(client);
		}

		/// <summary>
		/// Sent when entering pet creation, request for
		/// potential option changes.
		/// </summary>
		/// <remarks>
		/// Allows to modify the client-side pet list. A list of races can
		/// be sent to the client, to be either included (white-list) or
		/// excluded (black-list). White-list removes all races except for those
		/// in the list, black-list removes all races in the list.
		/// 
		/// Client requires a re-start to accept a new list.
		/// </remarks>
		/// <example>
		/// No parameters.
		/// </example>
		[PacketHandler(Op.PetCreationOptionsRequest)]
		public void PetCreationOptionsRequest(LoginClient client, Packet packet)
		{
			//var availableRaces = new List<int>();
			//availableRaces.Add(430505);

			// TODO: Make this configurable.

			Send.PetCreationOptionsRequestR(client, PetCreationOptionsListType.WhiteList, null);
		}

		/// <summary>
		/// Partner creation request.
		/// </summary>
		/// <remarks>
		/// Uses pet creation response.
		/// </remarks>
		/// <example>
		/// ...
		/// </example>
		[PacketHandler(Op.CreatePartner)]
		public void CreatePartner(LoginClient client, Packet packet)
		{
			var serverName = packet.GetString();
			var cardId = packet.GetLong();
			var name = packet.GetString();
			var race = packet.GetInt();
			var skinColor = packet.GetByte();
			var hair = packet.GetInt();
			var hairColor = packet.GetByte();
			var eyeType = packet.GetShort();
			var eyeColor = packet.GetByte();
			var mouthType = packet.GetByte();
			var face = packet.GetInt();
			var height = packet.GetFloat();
			var weight = packet.GetFloat();
			var upper = packet.GetFloat();
			var lower = packet.GetFloat();
			var personality = packet.GetInt();

			// Check proprtions
			height = Math.Max(0f, Math.Min(1f, height));
			weight = Math.Max(0f, Math.Min(2f, weight));
			upper = Math.Max(0f, Math.Min(2f, upper));
			lower = Math.Max(0f, Math.Min(2f, lower));

			// Get stuff
			var card = client.Account.GetPetCard(cardId);
			var faceItem = AuraData.ItemDb.Find(face);
			var hairItem = AuraData.ItemDb.Find(hair);

			// Check card and server
			if (card == null || !LoginServer.Instance.ServerList.Has(serverName))
			{
				Log.Error("Partner creation: Missing card or server ({0}).", serverName);
				goto L_Fail;
			}

			// Check face/hair
			if (faceItem == null || hairItem == null || (faceItem.Type != ItemType.Hair && faceItem.Type != ItemType.Face) || (hairItem.Type != ItemType.Hair && hairItem.Type != ItemType.Face))
			{
				Log.Error("Partner creation: Invalid face ({0}) or hair ({1}).", face, hair);
				goto L_Fail;
			}

			// Check pet info
			var petInfo = AuraData.PetDb.Find(card.Race);
			if (petInfo == null)
			{
				Log.Error("Partner creation: Missing pet info ({0}).", card.Race);
				goto L_Fail;
			}

			// Check name
			var nameCheck = LoginServer.Instance.Database.NameOkay(name, serverName);
			if (nameCheck != NameCheckResult.Okay)
			{
				Log.Error("Partner creation: Invalid name ({0}).", nameCheck);
				goto L_Fail;
			}

			// Create partner
			var partner = new Character();
			partner.Name = name;
			partner.Race = card.Race;
			partner.Face = face;
			partner.SkinColor = skinColor;
			partner.Hair = hair;
			partner.HairColor = hairColor;
			partner.EyeType = eyeType;
			partner.EyeColor = eyeColor;
			partner.MouthType = mouthType;
			partner.Server = serverName;
			partner.State |= CreatureStates.Initialized;

			partner.Height = height;
			partner.Weight = weight;
			partner.Upper = upper;
			partner.Lower = lower;

			partner.Life = petInfo.Life;
			partner.Mana = petInfo.Mana;
			partner.Stamina = petInfo.Stamina;
			partner.Str = petInfo.Str;
			partner.Int = petInfo.Int;
			partner.Dex = petInfo.Dex;
			partner.Will = petInfo.Will;
			partner.Luck = petInfo.Luck;
			partner.Defense = petInfo.Defense;
			partner.Protection = petInfo.Protection;

			// Try to create character
			if (!client.Account.CreatePartner(partner))
			{
				Log.Error("Partner creation: Failed for unknown reasons.");
				goto L_Fail;
			}

			// Delete card
			if (LoginServer.Instance.Conf.Login.ConsumePartnerCards)
			{
				if (!client.Account.DeletePetCard(card))
					goto L_Fail;
			}

			// Success~
			Send.CreatePetR(client, serverName, partner.EntityId);

			Log.Info("New partner '{0}' created in account '{1}'.", name, client.Account.Name);

			return;

		L_Fail:
			Send.CreatePetR_Fail(client);
		}

		/// <summary>
		/// Sent when entering partner creation
		/// </summary>
		/// <remarks>
		/// Request for potential option changes?
		/// </remarks>
		/// <example>
		/// No parameters.
		/// </example>
		[PacketHandler(Op.PartnerCreationOptionsRequest)]
		public void PartnerCreationOptionsRequest(LoginClient client, Packet packet)
		{

		}
	}
}
