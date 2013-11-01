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
using Aura.Data.Database;
using Aura.Data;

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
					var character = new Character();
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

		/// <summary>
		/// Creates creature:character combination for the account.
		/// Returns false if either failed, true on success.
		/// </summary>
		/// <param name="accountId"></param>
		/// <param name="character"></param>
		/// <returns></returns>
		public bool CreateCharacter(string accountId, Character character, List<CharCardSetInfo> items, List<ushort> keywords, List<Skill> skills)
		{
			using (var conn = AuraDb.Instance.Connection)
			{
				MySqlTransaction transaction = null;
				try
				{
					transaction = conn.BeginTransaction();

					// Creature
					character.CreatureId = this.CreateCreature(character, conn, transaction);

					// Character
					{
						var mc = new MySqlCommand(
							"INSERT INTO `characters` " +
							"(`accountId`, `creatureId`) " +
							"VALUES (@accountId, @creatureId)"
						, conn, transaction);

						mc.Parameters.AddWithValue("@accountId", accountId);
						mc.Parameters.AddWithValue("@creatureId", character.CreatureId);

						mc.ExecuteNonQuery();

						character.Id = mc.LastInsertedId;
					}

					// Items
					this.AddItems(character.CreatureId, items, conn, transaction);

					// Keywords
					this.AddKeywords(character.CreatureId, keywords, conn, transaction);

					// Skills
					this.AddSkills(character.CreatureId, skills, conn, transaction);

					transaction.Commit();

					return true;
				}
				catch (Exception ex)
				{
					character.Id = character.CreatureId = 0;

					transaction.Rollback();

					Log.Exception(ex);

					return false;
				}
			}
		}

		public bool CreatePet(string accountId, Character pet, List<Skill> skills)
		{
			using (var conn = AuraDb.Instance.Connection)
			{
				MySqlTransaction transaction = null;
				try
				{
					transaction = conn.BeginTransaction();

					// Creature
					pet.CreatureId = this.CreateCreature(pet, conn, transaction);

					// Pet
					{
						var mc = new MySqlCommand(
							"INSERT INTO `pets` " +
							"(`accountId`, `creatureId`) " +
							"VALUES (@accountId, @creatureId)"
						, conn, transaction);

						mc.Parameters.AddWithValue("@accountId", accountId);
						mc.Parameters.AddWithValue("@creatureId", pet.CreatureId);

						mc.ExecuteNonQuery();

						pet.Id = mc.LastInsertedId;
					}

					// Skills
					this.AddSkills(pet.CreatureId, skills, conn, transaction);

					transaction.Commit();

					return true;
				}
				catch (Exception ex)
				{
					pet.Id = pet.CreatureId = 0;

					transaction.Rollback();

					Log.Exception(ex);

					return false;
				}
			}
		}

		/// <summary>
		/// Creatures creature based on character and returns its id.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="conn"></param>
		/// <param name="transaction"></param>
		/// <returns></returns>
		private long CreateCreature(Character creature, MySqlConnection conn, MySqlTransaction transaction)
		{
			var mc = new MySqlCommand(
				"INSERT INTO `creatures` " +
				"(`server`, `name`, `race`, `skinColor`, `eyeType`, `eyeColor`, `mouthType`, `height`, `weight`, `upper`, `lower`, `color1`, `color2`, `color3`, " +
				" `lifeMax`, `lifeDelta`, `manaMax`, `manaDelta`, `staminaMax`, `staminaDelta`, `str`, `int`, `dex`, `will`, `luck`, `defense`, `protection`, `ap`) " +
				"VALUES (@server, @name, @race, @skinColor, @eyeType, @eyeColor, @mouthType, @height, @weight, @upper, @lower, @color1, @color2, @color3, " +
				" @lifeMax, @lifeDelta, @manaMax, @manaDelta, @staminaMax, @staminaDelta, @str, @int, @dex, @will, @luck, @defense, @protection, @ap)"
			, conn, transaction);

			mc.Parameters.AddWithValue("@server", creature.Server);
			mc.Parameters.AddWithValue("@name", creature.Name);
			mc.Parameters.AddWithValue("@race", creature.Race);
			mc.Parameters.AddWithValue("@skinColor", creature.SkinColor);
			mc.Parameters.AddWithValue("@eyeType", creature.EyeType);
			mc.Parameters.AddWithValue("@eyeColor", creature.EyeColor);
			mc.Parameters.AddWithValue("@mouthType", creature.MouthType);
			mc.Parameters.AddWithValue("@height", creature.Height);
			mc.Parameters.AddWithValue("@weight", creature.Weight);
			mc.Parameters.AddWithValue("@upper", creature.Upper);
			mc.Parameters.AddWithValue("@lower", creature.Lower);
			mc.Parameters.AddWithValue("@color1", creature.Color1);
			mc.Parameters.AddWithValue("@color2", creature.Color2);
			mc.Parameters.AddWithValue("@color3", creature.Color3);
			mc.Parameters.AddWithValue("@lifeMax", creature.Life);
			mc.Parameters.AddWithValue("@lifeDelta", creature.Life);
			mc.Parameters.AddWithValue("@manaMax", creature.Mana);
			mc.Parameters.AddWithValue("@manaDelta", creature.Mana);
			mc.Parameters.AddWithValue("@staminaMax", creature.Stamina);
			mc.Parameters.AddWithValue("@staminaDelta", creature.Stamina);
			mc.Parameters.AddWithValue("@str", creature.Str);
			mc.Parameters.AddWithValue("@int", creature.Int);
			mc.Parameters.AddWithValue("@dex", creature.Dex);
			mc.Parameters.AddWithValue("@will", creature.Will);
			mc.Parameters.AddWithValue("@luck", creature.Luck);
			mc.Parameters.AddWithValue("@defense", creature.Defense);
			mc.Parameters.AddWithValue("@protection", creature.Protection);
			mc.Parameters.AddWithValue("@ap", creature.AP);

			mc.ExecuteNonQuery();

			return mc.LastInsertedId;
		}

		/// <summary>
		/// Adds items for creature.
		/// </summary>
		/// <param name="creatureId"></param>
		/// <param name="cardItems"></param>
		private void AddItems(long creatureId, List<CharCardSetInfo> cardItems, MySqlConnection conn, MySqlTransaction transaction)
		{
			var mc = new MySqlCommand(
				"INSERT INTO `items` " +
				"(`creatureId`, `class`, `pocket`, `color1`, `color2`, `color3`, `price`, `durability`, `durabilityMax`, " +
				" `durabilityNew`, `attackMin`, `attackMax`, `balance`, `critical`, `defense`, `protection`, `attackSpeed`, `sellPrice`)" +
				"VALUES (@creatureId, @class, @pocket, @color1, @color2, @color3, @price, @durability, @durabilityMax, " +
				" @durabilityNew, @attackMin, @attackMax, @balance, @critical, @defense, @protection, @attackSpeed, @sellPrice)"
			, conn, transaction);

			foreach (var item in cardItems)
			{
				var dataInfo = AuraData.ItemDb.Find(item.Class);
				if (dataInfo == null)
				{
					Log.Warning("Item '{0}' couldn't be found in the database.", item.Class);
					continue;
				}

				mc.Parameters.Clear();
				mc.Parameters.AddWithValue("@creatureId", creatureId);
				mc.Parameters.AddWithValue("@class", item.Class);
				mc.Parameters.AddWithValue("@pocket", item.Pocket);
				mc.Parameters.AddWithValue("@color1", item.Color1);
				mc.Parameters.AddWithValue("@color2", item.Color2);
				mc.Parameters.AddWithValue("@color3", item.Color3);
				mc.Parameters.AddWithValue("@price", dataInfo.Price);
				mc.Parameters.AddWithValue("@durability", dataInfo.Durability);
				mc.Parameters.AddWithValue("@durabilityMax", dataInfo.Durability);
				mc.Parameters.AddWithValue("@durabilityNew", dataInfo.Durability);
				mc.Parameters.AddWithValue("@attackMin", dataInfo.AttackMin);
				mc.Parameters.AddWithValue("@attackMax", dataInfo.AttackMax);
				mc.Parameters.AddWithValue("@balance", dataInfo.Balance);
				mc.Parameters.AddWithValue("@critical", dataInfo.Critical);
				mc.Parameters.AddWithValue("@defense", dataInfo.Defense);
				mc.Parameters.AddWithValue("@protection", dataInfo.Protection);
				mc.Parameters.AddWithValue("@attackSpeed", dataInfo.AttackSpeed);
				mc.Parameters.AddWithValue("@sellPrice", dataInfo.SellingPrice);
				mc.ExecuteNonQuery();
			}
		}

		/// <summary>
		/// Adds keywords for creature.
		/// </summary>
		/// <param name="creatureId"></param>
		/// <param name="keywordIds"></param>
		private void AddKeywords(long creatureId, List<ushort> keywordIds, MySqlConnection conn, MySqlTransaction transaction)
		{
			var mc = new MySqlCommand("INSERT INTO `keywords` (`keywordId`, `creatureId`) VALUES (@keywordId, @creatureId)", conn, transaction);

			foreach (var keywordId in keywordIds)
			{
				mc.Parameters.Clear();
				mc.Parameters.AddWithValue("@creatureId", creatureId);
				mc.Parameters.AddWithValue("@keywordId", keywordId);
				mc.ExecuteNonQuery();
			}
		}

		/// <summary>
		/// Adds skills for creature.
		/// </summary>
		/// <param name="creatureId"></param>
		/// <param name="skills"></param>
		private void AddSkills(long creatureId, List<Skill> skills, MySqlConnection conn, MySqlTransaction transaction)
		{
			var mc = new MySqlCommand("INSERT INTO `skills` VALUES (@skillId, @creatureId, @rank, 0)", conn, transaction);

			foreach (var skill in skills)
			{
				mc.Parameters.Clear();
				mc.Parameters.AddWithValue("@creatureId", creatureId);
				mc.Parameters.AddWithValue("@skillId", (ushort)skill.Id);
				mc.Parameters.AddWithValue("@rank", (byte)skill.Rank);
				mc.ExecuteNonQuery();
			}
		}
	}
}
