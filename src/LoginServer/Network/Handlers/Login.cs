// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Text;
using Aura.Login.Database;
using Aura.Login.Util;
using Aura.Shared.Database;
using Aura.Shared.Mabi;
using Aura.Shared.Mabi.Const;
using Aura.Shared.Network;
using Aura.Shared.Util;
using System.Collections.Generic;

namespace Aura.Login.Network.Handlers
{
	public partial class LoginServerHandlers : PacketHandlerManager<LoginClient>
	{
		/// <summary>
		/// First actual packet from the client, includes
		/// identification hash from "data\vf.dat".
		/// </summary>
		/// <example>
		/// NA166
		/// 001 [..............01] Byte   : 1
		/// 002 [................] String : USA_Regular-A2C30B-BDA-F8C
		/// 003 [........00000000] Int    : 0
		/// 004 [................] String : admin
		/// 005 [................] String : admin
		/// </example>
		[PacketHandler(Op.ClientIdent)]
		public void ClientIdent(LoginClient client, Packet packet)
		{
			var unkByte = packet.GetByte();
			var ident = packet.GetString();
			// [180x00] Added some time in G18
			{
				var unkInt = packet.GetInt();
				var accountName1 = packet.GetString(); // sometimes empty?
				var accountName2 = packet.GetString();
			}

			//if (ident != "WHO_Gives-A10211-799-107")
			//{
			//    Send.CheckIdentR(client, false);
			//    return;
			//}

			Send.CheckIdentR(client, true);
		}

		/// <summary>
		/// Login packet
		/// </summary>
		/// <example>
		/// NA166, NX hash
		/// 001 [..............05] Byte   : 5
		/// 002 [................] String : admin
		/// 003 [................] String : hash
		/// 004 [................] Bin    : mac
		/// 005 [........00000000] Int    : 0
		/// 006 [........00000000] Int    : 0
		/// 007 [................] String : local ip
		/// 
		/// NA166
		/// 001 [..............14] Byte   : 20
		/// 002 [................] String : admin
		/// 003 [................] String : admin
		/// 004 [0000000000000000] Long   : session key
		/// 003 [................] String : hash
		/// 004 [................] Bin    : mac
		/// 005 [........00000000] Int    : 0
		/// 006 [........00000000] Int    : 0
		/// 007 [................] String : local ip
		/// </example>
		[PacketHandler(Op.Login)]
		public void Login(LoginClient client, Packet packet)
		{
			var loginType = (LoginType)packet.GetByte();
			var accountId = packet.GetString();
			var password = "";
			var secondaryPassword = "";
			var sessionKey = 0L;

			switch (loginType)
			{
				// Normal login, password
				case LoginType.Normal:
				case LoginType.EU:
				case LoginType.KR:
				case LoginType.CmdLogin:

					// [150100] From raw to MD5
					// [KR180XYY] From MD5 to SHA1
					var passbin = packet.GetBin();
					password = Encoding.UTF8.GetString(passbin);

					// Upgrade raw to MD5
					if (loginType == LoginType.EU)
						password = Password.RawToMD5(passbin);

					// Upgrade MD5 to SHA1 (used by newer clients)
					if (password.Length == 32) // MD5
						password = Password.MD5ToSHA256(password);

					// Create new account
					if (LoginServer.Instance.Conf.Login.NewAccounts && (accountId.StartsWith("new//") || accountId.StartsWith("new__")))
					{
						accountId = accountId.Remove(0, 5);

						if (!LoginServer.Instance.Database.AccountExists(accountId) && password != "")
						{
							LoginServer.Instance.Database.CreateAccount(accountId, password);
							Log.Info("New account '{0}' was created.", accountId);
						}
					}

					// Set login type to normal if it's not secondary,
					// we have all information and don't care anymore.
					if (loginType != LoginType.SecondaryPassword)
						loginType = LoginType.Normal;

					break;

				// Logging in, coming from a channel
				case LoginType.FromChannel:

					// [160XXX] Double account name
					{
						packet.GetString();
					}
					sessionKey = packet.GetLong();

					break;

				// Second password
				case LoginType.SecondaryPassword:

					// [XXXXXX] Double account name
					{
						packet.GetString();
					}
					sessionKey = packet.GetLong();
					secondaryPassword = packet.GetString(); // SSH1

					break;

				// Unsupported NX hash
				case LoginType.NewHash:

					// Triggered by people using their official accounts?
					// Are those information cached somewhere?
					// TODO: Rephrase? Sounds weird, as if we *know* their data.
					Send.LoginR_Msg(client, Localization.Get("Please don't use your official login information."));
					return;

				// Unsupported/unknown type
				case LoginType.CH:
				default:
					Send.LoginR_Msg(client, Localization.Get("Unsupported password encryption, please use the latest NA client."));
					return;
			}

			var machineId = packet.GetBin();
			var unkInt1 = packet.GetInt();
			var unkInt2 = packet.GetInt();
			var localClientIP = packet.GetString();

			// Get account
			var account = LoginServer.Instance.Database.GetAccount(accountId);
			if (account == null)
			{
				Send.LoginR_Fail(client, LoginResult.IdOrPassIncorrect);
				return;
			}

			// Update account's secondary password
			if (loginType == LoginType.SecondaryPassword && account.SecondaryPassword == null)
			{
				account.SecondaryPassword = secondaryPassword;
				LoginServer.Instance.Database.UpdateAccountSecondaryPassword(account);
			}

			// Check bans
			if (account.BannedExpiration > DateTime.Now)
			{
				Send.LoginR_Msg(client, Localization.Get("You've been banned till {0}.\r\nReason: {1}"), account.BannedExpiration, account.BannedReason);
				return;
			}

			// Check password/session
			if (!Password.Check(password, account.Password) && account.SessionKey != sessionKey)
			{
				Send.LoginR_Fail(client, LoginResult.IdOrPassIncorrect);
				return;
			}

			// Check secondary password
			if (loginType == LoginType.SecondaryPassword)
			{
				// Set new secondary password
				if (account.SecondaryPassword == null)
				{
					account.SecondaryPassword = secondaryPassword;
					LoginServer.Instance.Database.UpdateAccountSecondaryPassword(account);
				}
				// Check secondary
				else if (account.SecondaryPassword != secondaryPassword)
				{
					Send.LoginR_Fail(client, LoginResult.SecondaryFail);
					return;
				}
			}

			// Check logged in already
			if (account.LoggedIn)
			{
				Send.LoginR_Fail(client, LoginResult.AlreadyLoggedIn);
				return;
			}

			account.SessionKey = LoginServer.Instance.Database.CreateSession(account.Name);

			// Second password, please!
			if (LoginServer.Instance.Conf.Login.EnableSecondaryPassword && loginType == LoginType.Normal)
			{
				Send.LoginR_Secondary(client, account, account.SessionKey);
				return;
			}

			// Update account
			account.LastLogin = DateTime.Now;
			account.LoggedIn = true;
			LoginServer.Instance.Database.UpdateAccount(account);

			// Req. Info
			account.CharacterCards = LoginServer.Instance.Database.GetCharacterCards(account.Name);
			account.PetCards = LoginServer.Instance.Database.GetPetCards(account.Name);
			account.Characters = LoginServer.Instance.Database.GetCharacters(account.Name);
			account.Pets = LoginServer.Instance.Database.GetPetsAndPartners(account.Name);
			account.Gifts = LoginServer.Instance.Database.GetGifts(account.Name);

			// Add free cards if there are none.
			// If you don't have chars and char cards, you get a new free card,
			// if you don't have pets or pet cards either, you'll also get a 7-day horse.
			if (account.CharacterCards.Count < 1 && account.Characters.Count < 1)
			{
				// Free card
				var card = LoginServer.Instance.Database.AddCard(account.Name, 147, 0);
				account.CharacterCards.Add(card);

				if (account.PetCards.Count < 1 && account.Pets.Count < 1)
				{
					// 7-day Horse
					card = LoginServer.Instance.Database.AddCard(account.Name, MabiId.PetCardType, 260016);
					account.PetCards.Add(card);
				}
			}

			// Success
			Send.LoginR(client, account, account.SessionKey, LoginServer.Instance.ServerList.List);

			client.Account = account;
			client.State = ClientState.LoggedIn;

			Log.Info("User '{0}' logged in.", account.Name);
		}
	}
}
