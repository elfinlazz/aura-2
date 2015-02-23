// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using Aura.Login.Database;
using Aura.Shared.Database;
using Aura.Shared.Mabi;
using Aura.Shared.Mabi.Const;
using Aura.Shared.Network;
using Aura.Shared.Network.Sending.Helpers;

namespace Aura.Login.Network
{
	public static partial class Send
	{
		/// <summary>
		/// Sends ClientIdentR to client.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="success"></param>
		public static void CheckIdentR(LoginClient client, bool success)
		{
			var packet = new Packet(Op.ClientIdentR, MabiId.Login);
			packet.PutByte(success);
			packet.PutLong(DateTime.Now);

			client.Send(packet);
		}

		/// <summary>
		/// Sends message as response to login (LoginR).
		/// </summary>
		/// <param name="client"></param>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public static void LoginR_Msg(LoginClient client, string format, params object[] args)
		{
			var packet = new Packet(Op.LoginR, MabiId.Login);
			packet.PutByte((byte)LoginResult.Message);
			packet.PutInt(14);
			packet.PutInt(1);
			packet.PutString(format, args);

			client.Send(packet);
		}

		/// <summary>
		/// Sends (negative) LoginR to client.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="result"></param>
		public static void LoginR_Fail(LoginClient client, LoginResult result)
		{
			var packet = new Packet(Op.LoginR, MabiId.Login);
			packet.PutByte((byte)result);
			if (result == LoginResult.SecondaryFail)
			{
				packet.PutInt(12);
				packet.PutByte(1);
			}

			client.Send(packet);
		}

		/// <summary>
		/// Sends positive response to login.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="account"></param>
		/// <param name="sessionKey"></param>
		/// <param name="serverList"></param>
		public static void LoginR(LoginClient client, Account account, long sessionKey, ICollection<ServerInfo> serverList)
		{
			var packet = new Packet(Op.LoginR, MabiId.Login);
			packet.PutByte((byte)LoginResult.Success);
			packet.PutString(account.Name);
			// [160XXX] Double account name
			{
				packet.PutString(account.Name);
			}
			packet.PutLong(sessionKey);
			packet.PutByte(0);

			// Servers
			// --------------------------------------------------------------
			packet.AddServerList(serverList, ServerInfoType.Client);

			// Account Info
			// --------------------------------------------------------------
			packet.Add(account);

			client.Send(packet);
		}

		/// <summary>
		/// Sends LoginR with request for secondary password to client.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="account"></param>
		/// <param name="sessionKey"></param>
		public static void LoginR_Secondary(LoginClient client, Account account, long sessionKey)
		{
			var packet = new Packet(Op.LoginR, MabiId.Login);
			packet.PutByte((byte)LoginResult.SecondaryReq);
			packet.PutString(account.Name); // Official seems to send this
			packet.PutString(account.Name); // back hashed.
			packet.PutLong(sessionKey);
			if (account.SecondaryPassword == null)
				packet.PutString("FIRST");
			else
				packet.PutString("NOT_FIRST");

			client.Send(packet);
		}

		/// <summary>
		/// Sends negative xInfoRequestR to client.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="op"></param>
		public static void CharacterInfoRequestR_Fail(LoginClient client, int op)
		{
			Send.CharacterInfoRequestR(client, op, null, null);
		}

		/// <summary>
		/// Sends xInfoRequestR to client.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="op"></param>
		/// <param name="character"></param>
		/// <param name="items"></param>
		public static void CharacterInfoRequestR(LoginClient client, int op, Character character, List<Item> items)
		{
			var packet = new Packet(op, MabiId.Login);
			packet.PutByte(character != null);

			if (character != null)
			{
				packet.PutString(character.Server);
				packet.PutLong(character.EntityId);
				packet.PutByte(1);
				packet.PutString(character.Name);
				packet.PutString("");
				packet.PutString("");
				packet.PutInt(character.Race);
				packet.PutByte(character.SkinColor);
				packet.PutShort(character.EyeType);
				packet.PutByte(character.EyeColor);
				packet.PutByte(character.MouthType);
				packet.PutUInt((uint)character.State);
				packet.PutFloat(character.Height);
				packet.PutFloat(character.Weight);
				packet.PutFloat(character.Upper);
				packet.PutFloat(character.Lower);
				packet.PutInt(0);
				packet.PutInt(0);
				packet.PutInt(0);
				packet.PutByte(0);
				packet.PutInt(0);
				packet.PutByte(0);
				packet.PutInt((int)character.Color1);
				packet.PutInt((int)character.Color2);
				packet.PutInt((int)character.Color3);
				packet.PutFloat(0.0f);
				packet.PutString("");
				packet.PutFloat(49.0f);
				packet.PutFloat(49.0f);
				packet.PutFloat(0.0f);
				packet.PutFloat(49.0f);
				// [180800, NA196 (14.10.2014)] ?
				{
					packet.PutShort(0);
				}
				packet.PutInt(0);
				packet.PutInt(0);
				packet.PutShort(0);
				packet.PutLong(0);
				packet.PutString("");
				packet.PutByte(0);

				packet.PutInt(items.Count);
				foreach (var item in items)
				{
					packet.PutLong(item.Id);
					packet.PutBin(item.Info);
				}

				packet.PutInt(0);  // PetRemainingTime
				packet.PutLong(0); // PetLastTime
				packet.PutLong(0); // PetExpireTime
			}

			client.Send(packet);
		}

		/// <summary>
		/// Sends NameCheckR to client.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="result"></param>
		public static void NameCheckR(LoginClient client, NameCheckResult result)
		{
			var response = new Packet(Op.NameCheckR, MabiId.Login);
			response.PutByte(result == NameCheckResult.Okay);
			response.PutByte((byte)result);

			client.Send(response);
		}

		/// <summary>
		/// Sends negative CreateCharacterR to client.
		/// </summary>
		/// <param name="client"></param>
		public static void CreateCharacterR_Fail(LoginClient client)
		{
			CreateCharacterR(client, null, 0);
		}

		/// <summary>
		/// Semds CreateCharacterR to client.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="serverName">Response is negative if null.</param>
		/// <param name="id"></param>
		public static void CreateCharacterR(LoginClient client, string serverName, long id)
		{
			CreateR(client, Op.CreateCharacterR, serverName, id);
		}

		/// <summary>
		/// Sends negative CreatePetR to client.
		/// </summary>
		/// <param name="client"></param>
		public static void CreatePetR_Fail(LoginClient client)
		{
			CreatePetR(client, null, 0);
		}

		/// <summary>
		/// Semds CreatePetR to client.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="serverName">Response is negative if null.</param>
		/// <param name="id"></param>
		public static void CreatePetR(LoginClient client, string serverName, long id)
		{
			CreateR(client, Op.CreatePetR, serverName, id);
		}

		/// <summary>
		/// One response to respond them all.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="op"></param>
		/// <param name="serverName"></param>
		/// <param name="id"></param>
		private static void CreateR(LoginClient client, int op, string serverName, long id)
		{
			var response = new Packet(op, MabiId.Login);
			response.PutByte(serverName != null);

			if (serverName != null)
			{
				response.PutString(serverName);
				response.PutLong(id);
			}

			client.Send(response);
		}

		/// <summary>
		/// Sends PetCreationOptionsRequestR to client.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="type"></param>
		/// <param name="list">No overwrite if list is null.</param>
		public static void PetCreationOptionsRequestR(LoginClient client, PetCreationOptionsListType type, List<int> list)
		{
			var packet = new Packet(Op.PetCreationOptionsRequestR, MabiId.Login);
			packet.PutByte(list != null);

			if (list != null)
			{
				packet.PutByte((byte)type);
				foreach (var race in list)
					packet.PutInt(race);
				packet.PutInt(-1); // End of list
			}

			client.Send(packet);
		}

		/// <summary>
		/// Sends AcceptGiftR to client.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="gift">Negative response if null</param>
		public static void AcceptGiftR(LoginClient client, Gift gift)
		{
			var packet = new Packet(Op.AcceptGiftR, MabiId.Login);
			packet.PutByte(gift != null);

			if (gift != null)
			{
				packet.PutByte(gift.IsCharacter);
				packet.PutInt(0); // ?
				packet.PutInt(0); // ?
				packet.PutInt(gift.Type);
				// ?
			}

			client.Send(packet);
		}

		/// <summary>
		/// Sends RefuseGiftR to client.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="success"></param>
		public static void RefuseGiftR(LoginClient client, bool success)
		{
			var packet = new Packet(Op.RefuseGiftR, MabiId.Login);
			packet.PutByte(success);
			// ?

			client.Send(packet);
		}

		/// <summary>
		/// Sends AccountInfoRequestR to client, with client's account's data.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="success"></param>
		public static void AccountInfoRequestR(LoginClient client, bool success)
		{
			var packet = new Packet(Op.AccountInfoRequestR, MabiId.Login);
			packet.PutByte(success);

			if (success)
				packet.Add(client.Account);

			client.Send(packet);
		}

		/// <summary>
		/// Sends negative (DeleteXRequestR|RecoverXR|DeleteXR) to client.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="op">(DeleteXRequestR|RecoverXR|DeleteXR)</param>
		public static void DeleteR_Fail(LoginClient client, int op)
		{
			DeleteR(client, op, null, 0);
		}

		/// <summary>
		/// Sends (DeleteXRequestR|RecoverXR|DeleteXR) to client.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="op">(DeleteXRequestR|RecoverXR|DeleteXR)</param>
		/// <param name="serverName">Negative response if null</param>
		/// <param name="id"></param>
		public static void DeleteR(LoginClient client, int op, string serverName, long id)
		{
			var packet = new Packet(op, MabiId.Login);
			packet.PutByte(serverName != null);

			if (serverName != null)
			{
				packet.PutString(serverName);
				packet.PutLong(id);
				packet.PutLong(0);
			}

			client.Send(packet);
		}

		/// <summary>
		/// Sends negative ChannelInfoRequestR to client.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="info"></param>
		public static void ChannelInfoRequestR_Fail(LoginClient client)
		{
			ChannelInfoRequestR(client, null, 0);
		}

		/// <summary>
		/// Sends ChannelInfoRequestR to client.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="info">Negative response if null.</param>
		/// <param name="characterId"></param>
		public static void ChannelInfoRequestR(LoginClient client, ChannelInfo info, long characterId)
		{
			var packet = new Packet(Op.ChannelInfoRequestR, MabiId.Channel);
			packet.PutByte(info != null);

			if (info != null)
			{
				packet.PutString(info.ServerName);
				packet.PutString(info.Name);
				packet.PutShort(6); // Channel "Id"? (seems to be equal to channel nr)
				packet.PutString(info.Host);
				packet.PutString(info.Host);
				packet.PutShort((short)info.Port);
				packet.PutShort((short)(info.Port + 2));
				packet.PutInt(1);
				packet.PutLong(characterId);
			}

			client.Send(packet);
		}

		/// <summary>
		/// Sends Internal.ServerIdentifyR  to channel client.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="success"></param>
		public static void Internal_ServerIdentifyR(LoginClient client, bool success)
		{
			var packet = new Packet(Op.Internal.ServerIdentifyR, 0);
			packet.PutByte(success);

			client.Send(packet);
		}

		/// <summary>
		/// Sends server/channel status update to all connected players.
		/// </summary>
		public static void ChannelStatus(ICollection<ServerInfo> serverList)
		{
			var packet = new Packet(Op.ChannelStatus, MabiId.Login);
			packet.AddServerList(serverList, ServerInfoType.Client);

			LoginServer.Instance.BroadcastPlayers(packet);
		}

		/// <summary>
		/// Sends server/channel status update to all connected channels.
		/// </summary>
		public static void Internal_ChannelStatus(ICollection<ServerInfo> serverList)
		{
			var packet = new Packet(Op.Internal.ChannelStatus, MabiId.Login);
			packet.AddServerList(serverList, ServerInfoType.Internal);

			LoginServer.Instance.BroadcastChannels(packet);
		}

		/// <summary>
		/// Sends LoginUnkR to client.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="unkByte"></param>
		public static void LoginUnkR(LoginClient client, byte unkByte)
		{
			var packet = new Packet(Op.LoginUnkR, MabiId.Login);
			packet.PutByte(unkByte);

			client.Send(packet);
		}

		/// <summary>
		/// Sends Internal.Broadcast to all channel servers.
		/// </summary>
		public static void Internal_Broadcast(string message)
		{
			var packet = new Packet(Op.Internal.BroadcastNotice, 0);
			packet.PutString(message);

			LoginServer.Instance.BroadcastChannels(packet);
		}

		/// <summary>
		/// Sends negative TradeCardR to client (temp).
		/// </summary>
		/// <param name="client"></param>
		/// <param name="cardId">Negative response if 0.</param>
		public static void TradeCardR(LoginClient client, long cardId)
		{
			var packet = new Packet(Op.TradeCardR, MabiId.Login);
			packet.PutByte(cardId != 0);
			if (cardId != 0)
				packet.PutLong(cardId);

			client.Send(packet);
		}

		/// <summary>
		/// Adds account information to packet.
		/// </summary>
		/// <param name="packet"></param>
		/// <param name="account"></param>
		private static void Add(this Packet packet, Account account)
		{
			packet.PutLong(DateTime.Now);	// Last Login
			packet.PutLong(DateTime.Now);	// Last Logout
			packet.PutInt(0);
			packet.PutByte(1);
			packet.PutByte(34);
			packet.PutInt(0); // 0x800200FF
			packet.PutByte(1);

			// Premium services, listed in char selection
			// --------------------------------------------------------------
			// All 3 are visible, if one is set.
			packet.PutByte(false);			// Nao's Support
			packet.PutLong(0);
			packet.PutByte(false);			// Extra Storage
			packet.PutLong(0);
			packet.PutByte(false);			// Advanced Play
			packet.PutLong(0);

			packet.PutByte(0);
			packet.PutByte(1);

			// Always visible?
			packet.PutByte(false);          // Inventory Plus Kit
			packet.PutLong(0);              // DateTime
			packet.PutByte(false);          // Mabinogi Premium Pack
			packet.PutLong(0);
			packet.PutByte(false);          // Mabinogi VIP
			packet.PutLong(0);

			// [170402, TW170300] New premium thing
			{
				// Invisible?
				packet.PutByte(0);
				packet.PutLong(0);
			}

			// [180800, NA196 (14.10.2014)] ?
			{
				packet.PutByte(0);
				packet.PutLong(0);
			}

			packet.PutByte(0);
			packet.PutByte(0);				// 1: 프리미엄 PC방 서비스 사용중, 16: Free Play Event
			packet.PutByte(false);			// Free Beginner Service

			// Characters
			// --------------------------------------------------------------
			packet.PutShort((short)account.Characters.Count);
			foreach (var character in account.Characters)
			{
				packet.PutString(character.Server);
				packet.PutLong(character.EntityId);
				packet.PutString(character.Name);
				packet.PutByte((byte)character.DeletionFlag);
				packet.PutLong(0);
				packet.PutInt(0);
				packet.PutByte(0); // 0: Human, 1: Elf, 2: Giant
				packet.PutByte(0); // Assist character ?
				packet.PutByte(0); // >0 hides all characters?
			}

			// Pets
			// --------------------------------------------------------------
			packet.PutShort((short)account.Pets.Count);
			foreach (var pet in account.Pets)
			{
				packet.PutString(pet.Server);
				packet.PutLong(pet.EntityId);
				packet.PutString(pet.Name);
				packet.PutByte((byte)pet.DeletionFlag);
				packet.PutLong(0);
				packet.PutInt(pet.Race);
				packet.PutLong(0);
				packet.PutLong(0);
				packet.PutInt(0);
				packet.PutByte(0);
			}

			// Character cards
			// --------------------------------------------------------------
			packet.PutShort((short)account.CharacterCards.Count);
			foreach (var card in account.CharacterCards)
			{
				packet.PutByte(1);
				packet.PutLong(card.Id);
				packet.PutInt(card.Type);
				packet.PutLong(0);
				packet.PutLong(0);
				packet.PutInt(0);
			}

			// Pet cards
			// --------------------------------------------------------------
			packet.PutShort((short)account.PetCards.Count);
			foreach (var card in account.PetCards)
			{
				packet.PutByte(1);
				packet.PutLong(card.Id);
				packet.PutInt(card.Type);
				packet.PutInt(card.Race);
				packet.PutLong(0);
				packet.PutLong(0);
				packet.PutInt(0);
			}

			// Gifts
			// --------------------------------------------------------------
			packet.PutShort((short)account.Gifts.Count);
			foreach (var gift in account.Gifts)
			{
				packet.PutLong(gift.Id);
				packet.PutByte(gift.IsCharacter);
				packet.PutInt(gift.Type);
				packet.PutInt(gift.Race);
				packet.PutString(gift.Sender);
				packet.PutString(gift.SenderServer);
				packet.PutString(gift.Message);
				packet.PutString(gift.Receiver);
				packet.PutString(gift.ReceiverServer);
				packet.PutLong(gift.Added);
			}

			packet.PutByte(0);
		}
	}

	public enum LoginType
	{
		/// <summary>
		/// Only seen in KR
		/// </summary>
		KR = 0,

		/// <summary>
		/// Used to request disconnect when you're already logged in.
		/// </summary>
		RequestDisconnect = 1,

		/// <summary>
		/// Coming from channel (session key)
		/// </summary>
		FromChannel = 2,

		/// <summary>
		/// NX auth hash
		/// </summary>
		NewHash = 5,

		/// <summary>
		/// Default, hashed password
		/// </summary>
		Normal = 12,

		/// <summary>
		/// ? o.o
		/// </summary>
		CmdLogin = 16,

		/// <summary>
		/// Last seen in EU (no hashed password)
		/// </summary>
		EU = 18,

		/// <summary>
		/// Password + Secondary password
		/// </summary>
		SecondaryPassword = 20,

		/// <summary>
		/// RSA password, used by CH
		/// </summary>
		CH = 23,
	}

	public enum LoginResult
	{
		Fail = 0,
		Success = 1,
		Empty = 2,
		IdOrPassIncorrect = 3,
		/* IdOrPassIncorrect = 4, */
		TooManyConnections = 6,
		AlreadyLoggedIn = 7,
		UnderAge = 33,
		Message = 51,
		SecondaryReq = 90,
		SecondaryFail = 91,
		Banned = 101,
	}

	public enum PetCreationOptionsListType : byte
	{
		BlackList = 0, WhiteList = 1
	}
}
