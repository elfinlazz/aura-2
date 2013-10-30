// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence file in the main folder

using System;
using System.Linq;
using System.Collections.Generic;
using Aura.Login.Database;
using Aura.Shared.Mabi;
using Aura.Shared.Mabi.Const;
using Aura.Shared.Network;

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
			var packet = new MabiPacket(Op.ClientIdentR, MabiId.Login);
			packet.PutByte(success);
			packet.PutLong(MabiTime.Now.TimeStamp);

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
			var packet = new MabiPacket(Op.LoginR, MabiId.Login);
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
			var packet = new MabiPacket(Op.LoginR, MabiId.Login);
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
		/// <param name="servers"></param>
		public static void LoginR(LoginClient client, Account account, long sessionKey, List<ServerInfo> servers)
		{
			var packet = new MabiPacket(Op.LoginR, MabiId.Login);
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
			packet.PutByte((byte)servers.Count);
			foreach (var server in servers)
				packet.Add(server);

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
			var packet = new MabiPacket(Op.LoginR, MabiId.Login);
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

		public static void CharacterInfo_Fail(LoginClient client)
		{
			Send.CharacterInfo(client, Op.CharacterInfo, null, null);
		}

		public static void CharacterInfo(LoginClient client, Character character, List<Item> items)
		{
			Send.CharacterInfo(client, Op.CharacterInfo, character, items);
		}

		private static void CharacterInfo(LoginClient client, int op, Character character, List<Item> items)
		{
			var packet = new MabiPacket(op, MabiId.Login);
			if (character == null)
			{
				packet.PutByte(false);
				client.Send(packet);
			}

			packet.PutByte(true);
			packet.PutString(character.Server);
			packet.PutLong(character.Id);
			packet.PutByte(1);
			packet.PutString(character.Name);
			packet.PutString("");
			packet.PutString("");
			packet.PutInt(character.Race);
			packet.PutByte(character.SkinColor);
			packet.PutByte(character.EyeType);
			packet.PutByte(character.EyeColor);
			packet.PutByte(character.MouthType);
			packet.PutInt(0);
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

			client.Send(packet);
		}

		/// <summary>
		/// Adds server and channel information to packet.
		/// </summary>
		/// <param name="packet"></param>
		/// <param name="server"></param>
		private static void Add(this MabiPacket packet, ServerInfo server)
		{
			packet.PutString(server.Name);
			packet.PutShort(0); // Server type?
			packet.PutShort(0);
			packet.PutByte(1);

			// Channels
			// ----------------------------------------------------------
			packet.PutInt((int)server.Channels.Count);
			foreach (var channel in server.Channels.Values)
			{
				var state = channel.State;
				if ((DateTime.Now - channel.LastUpdate).TotalSeconds > 90)
					state = ChannelState.Maintenance;

				packet.PutString(channel.Name);
				packet.PutInt((int)state);
				packet.PutInt((int)channel.Events);
				packet.PutInt(0); // 1 for Housing? Hidden?
				packet.PutShort(channel.Stress);
			}
		}

		/// <summary>
		/// Adds account information to packet.
		/// </summary>
		/// <param name="packet"></param>
		/// <param name="account"></param>
		private static void Add(this  MabiPacket packet, Account account)
		{
			packet.PutLong(MabiTime.Now.TimeStamp);	// Last Login
			packet.PutLong(MabiTime.Now.TimeStamp);	// Last Logout
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

			packet.PutByte(0);
			packet.PutByte(0);				// 1: 프리미엄 PC방 서비스 사용중, 16: Free Play Event
			packet.PutByte(false);			// Free Beginner Service

			// Characters
			// --------------------------------------------------------------
			packet.PutShort((short)account.Characters.Count);
			foreach (var character in account.Characters)
			{
				packet.PutString(character.Server);
				packet.PutLong(character.Id);
				packet.PutString(character.Name);
				packet.PutByte((byte)character.DeletionFlag);
				packet.PutLong(0);
				packet.PutInt(0);
				packet.PutByte(0); // 0: Human, 1: Elf, 2: Giant
				packet.PutByte(0);
				packet.PutByte(0);
			}

			// Pets
			// --------------------------------------------------------------
			packet.PutShort((short)account.Pets.Count);
			foreach (var pet in account.Pets)
			{
				packet.PutString(pet.Server);
				packet.PutLong(pet.Id);
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
				packet.PutByte(0x01);
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
}
