// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Shared.Database;
using MySql.Data.MySqlClient;
using Aura.Shared.Util;
using Aura.Shared.Mabi;
using Aura.Shared.Mabi.Const;

namespace Aura.Login.Database
{
	public class LoginDb
	{
		public static readonly LoginDb Instance = new LoginDb();

		private LoginDb()
		{
		}

		/// <summary>
		/// Adds new account to the database.
		/// </summary>
		/// <param name="accountId"></param>
		/// <param name="password"></param>
		public void CreateAccount(string accountId, string password)
		{
			using (var conn = AuraDb.Instance.Connection)
			{
				password = Password.Hash(password);

				var mc = new MySqlCommand("INSERT INTO `accounts` (`accountId`, `password`, `creation`) VALUES (@accountId, @password, @creation)", conn);
				mc.Parameters.AddWithValue("@accountId", accountId);
				mc.Parameters.AddWithValue("@password", password);
				mc.Parameters.AddWithValue("@creation", MabiTime.Now.DateTime);

				mc.ExecuteNonQuery();
			}
		}

		/// <summary>
		/// Returns account or null if account doesn't exist.
		/// </summary>
		/// <param name="accountId"></param>
		/// <returns></returns>
		public Account GetAccount(string accountId)
		{
			using (var conn = AuraDb.Instance.Connection)
			{
				var mc = new MySqlCommand("SELECT * FROM `accounts` WHERE `accountId` = @accountId", conn);
				mc.Parameters.AddWithValue("@accountId", accountId);

				using (var reader = mc.ExecuteReader())
				{
					if (!reader.Read())
						return null;

					var account = new Account();
					account.Name = reader.GetStringSafe("accountId");
					account.Password = reader.GetStringSafe("password");
					account.SecondaryPassword = reader.GetStringSafe("secondaryPassword");
					account.SessionKey = reader.GetInt64("sessionKey");
					account.BannedExpiration = reader.GetDateTimeSafe("banExpiration");
					account.BannedReason = reader.GetStringSafe("banReason");

					return account;
				}
			}
		}

		/// <summary>
		/// Updates secondary password.
		/// </summary>
		/// <param name="account"></param>
		public void UpdateAccountSecondaryPassword(Account account)
		{
			using (var conn = AuraDb.Instance.Connection)
			{
				var mc = new MySqlCommand("UPDATE `accounts` SET `secondaryPassword` = @secondaryPassword WHERE `accountId` = @accountId", conn);
				mc.Parameters.AddWithValue("@accountId", account.Name);
				mc.Parameters.AddWithValue("@secondaryPassword", account.SecondaryPassword);

				mc.ExecuteNonQuery();
			}
		}

		/// <summary>
		/// Sets new randomized session key for the account and returns it.
		/// </summary>
		/// <param name="accountId"></param>
		/// <returns></returns>
		public long CreateSession(string accountId)
		{
			using (var conn = AuraDb.Instance.Connection)
			{
				var sessionKey = RandomProvider.Get().NextInt64();

				var mc = new MySqlCommand("UPDATE `accounts` SET `sessionKey` = @sessionKey WHERE `accountId` = @accountId", conn);
				mc.Parameters.AddWithValue("@accountId", accountId);
				mc.Parameters.AddWithValue("@sessionKey", sessionKey);

				mc.ExecuteNonQuery();

				return sessionKey;
			}
		}

		/// <summary>
		/// Updates lastLogin and loggedIn from the account.
		/// </summary>
		/// <param name="account"></param>
		public void UpdateAccount(Account account)
		{
			using (var conn = AuraDb.Instance.Connection)
			{
				var mc = new MySqlCommand("UPDATE `accounts` SET `lastLogin` = @lastLogin, `loggedIn` = @loggedIn WHERE `accountId` = @accountId", conn);
				mc.Parameters.AddWithValue("@accountId", account.Name);
				mc.Parameters.AddWithValue("@lastLogin", account.LastLogin);
				mc.Parameters.AddWithValue("@loggedIn", account.LoggedIn);

				mc.ExecuteNonQuery();
			}
		}
		/// <summary>
		/// Returns all character cards present for this account.
		/// </summary>
		/// <param name="accountId"></param>
		/// <returns></returns>
		public List<Card> GetCharacterCards(string accountId)
		{
			using (var conn = AuraDb.Instance.Connection)
			{
				var mc = new MySqlCommand("SELECT `cardId`, `type` FROM `cards` WHERE `accountId` = @accountId AND race = 0 AND !`isGift`", conn);
				mc.Parameters.AddWithValue("@accountId", accountId);

				var result = new List<Card>();
				using (var reader = mc.ExecuteReader())
				{
					while (reader.Read())
					{
						var card = new Card();
						card.Id = reader.GetInt64("cardId");
						card.Type = reader.GetInt32("type");

						result.Add(card);
					}
				}

				return result;
			}
		}

		/// <summary>
		/// Returns all pet and partner cards present for this account.
		/// </summary>
		/// <param name="accountId"></param>
		/// <returns></returns>
		public List<Card> GetPetCards(string accountId)
		{
			using (var conn = AuraDb.Instance.Connection)
			{
				var mc = new MySqlCommand("SELECT `cardId`, `type`, `race` FROM `cards` WHERE `accountId` = @accountId AND race > 0 AND !`isGift`", conn);
				mc.Parameters.AddWithValue("@accountId", accountId);

				var result = new List<Card>();
				using (var reader = mc.ExecuteReader())
				{
					while (reader.Read())
					{
						var card = new Card();
						card.Id = reader.GetInt64("cardId");
						card.Type = reader.GetInt32("type");
						card.Race = reader.GetInt32("race");

						result.Add(card);
					}
				}

				return result;
			}
		}

		/// <summary>
		/// Returns all gifts present for this account.
		/// </summary>
		/// <param name="accountId"></param>
		/// <returns></returns>
		public List<Gift> GetGifts(string accountId)
		{
			using (var conn = AuraDb.Instance.Connection)
			{
				var mc = new MySqlCommand("SELECT * FROM `cards` WHERE `accountId` = @accountId AND `isGift`", conn);
				mc.Parameters.AddWithValue("@accountId", accountId);

				var result = new List<Gift>();
				using (var reader = mc.ExecuteReader())
				{
					while (reader.Read())
					{
						var gift = new Gift();
						gift.Id = reader.GetInt64("cardId");
						gift.Type = reader.GetInt32("type");
						gift.Race = reader.GetInt32("race");
						gift.Message = reader.GetStringSafe("message");
						gift.Sender = reader.GetStringSafe("sender");
						gift.SenderServer = reader.GetStringSafe("senderServer");
						gift.Receiver = reader.GetStringSafe("receiver");
						gift.ReceiverServer = reader.GetStringSafe("receiverServer");
						gift.Added = reader.GetDateTimeSafe("added");

						result.Add(gift);
					}
				}

				return result;
			}
		}

		/// <summary>
		/// Returns a list of all characters on this account.
		/// </summary>
		/// <param name="accountId"></param>
		/// <returns></returns>
		public List<Character> GetCharacters(string accountId)
		{
			using (var conn = AuraDb.Instance.Connection)
			{
				var result = new List<Character>();
				this.GetCharacters(accountId, "characters", "characterId", CharacterType.Character, ref result, conn);

				return result;
			}
		}

		/// <summary>
		/// Returns a list of all pets/partners on this account.
		/// </summary>
		/// <param name="accountId"></param>
		/// <returns></returns>
		public List<Character> GetPetsAndPartners(string accountId)
		{
			using (var conn = AuraDb.Instance.Connection)
			{
				var result = new List<Character>();
				this.GetCharacters(accountId, "pets", "petId", CharacterType.Pet, ref result, conn);
				this.GetCharacters(accountId, "partners", "partnerId", CharacterType.Partner, ref result, conn);

				return result;
			}
		}

		/// <summary>
		/// Queries characters/pets/partners and adds them to result.
		/// </summary>
		/// <param name="accountId"></param>
		/// <param name="table"></param>
		/// <param name="primary"></param>
		/// <param name="type"></param>
		/// <param name="result"></param>
		/// <param name="conn"></param>
		private void GetCharacters(string accountId, string table, string primary, CharacterType type, ref List<Character> result, MySqlConnection conn)
		{
			var mc = new MySqlCommand(
				"SELECT * " +
				"FROM `" + table + "` AS c " +
				"INNER JOIN `creatures` AS cr ON c.creatureId = cr.creatureId " +
				"WHERE `accountId` = @accountId "
			, conn);
			mc.Parameters.AddWithValue("@accountId", accountId);

			using (var reader = mc.ExecuteReader())
			{
				while (reader.Read())
				{
					var character = new Character(type);
					character.Id = reader.GetInt64(primary);
					character.CreatureId = reader.GetInt64("creatureId");
					character.Name = reader.GetStringSafe("name");
					character.Server = reader.GetStringSafe("server");
					character.Race = reader.GetInt32("race");
					character.DeletionTime = reader.GetDateTimeSafe("deletionTime");
					character.SkinColor = reader.GetByte("skinColor");
					character.EyeType = reader.GetByte("eyeType");
					character.EyeColor = reader.GetByte("eyeColor");
					character.MouthType = reader.GetByte("mouthType");
					character.Height = reader.GetFloat("height");
					character.Weight = reader.GetFloat("weight");
					character.Upper = reader.GetFloat("upper");
					character.Lower = reader.GetInt32("lower");
					character.Color1 = reader.GetUInt32("color1");
					character.Color2 = reader.GetUInt32("color2");
					character.Color3 = reader.GetUInt32("color3");

					result.Add(character);
				}
			}
		}

		/// <summary>
		/// Returns list of all visible items on creature.
		/// </summary>
		/// <param name="creatureId"></param>
		/// <returns></returns>
		public List<Item> GetEquipment(long creatureId)
		{
			using (var conn = AuraDb.Instance.Connection)
			{
				var mc = new MySqlCommand("SELECT * FROM `items` WHERE `creatureId` = @creatureId", conn);
				mc.Parameters.AddWithValue("@creatureId", creatureId);

				var result = new List<Item>();
				using (var reader = mc.ExecuteReader())
				{
					while (reader.Read())
					{
						var item = new Item();
						item.Id = reader.GetInt64("itemId");
						item.Info.Class = reader.GetInt32("class");
						item.Info.Pocket = (Pocket)reader.GetByte("pocket");
						item.Info.Color1 = reader.GetUInt32("color1");
						item.Info.Color2 = reader.GetUInt32("color2");
						item.Info.Color3 = reader.GetUInt32("color3");
						item.Info.State = reader.GetByte("state");

						if (item.IsVisible)
							result.Add(item);
					}
				}

				return result;
			}
		}
	}
}
