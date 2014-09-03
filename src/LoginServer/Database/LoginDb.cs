// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Data;
using Aura.Data.Database;
using Aura.Shared.Database;
using Aura.Shared.Mabi.Const;
using Aura.Shared.Util;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;

namespace Aura.Login.Database
{
	public class LoginDb : AuraDb
	{
		/// <summary>
		/// Checks whether the SQL update file has already been applied.
		/// </summary>
		/// <param name="updateFile"></param>
		/// <returns></returns>
		public bool CheckUpdate(string updateFile)
		{
			using (var conn = this.Connection)
			using (var mc = new MySqlCommand("SELECT * FROM `updates` WHERE `path` = @path", conn))
			{
				mc.Parameters.AddWithValue("@path", updateFile);

				using (var reader = mc.ExecuteReader())
					return reader.Read();
			}
		}

		/// <summary>
		/// Executes SQL update file.
		/// </summary>
		/// <param name="updateFile"></param>
		public void RunUpdate(string updateFile)
		{
			try
			{
				using (var conn = this.Connection)
				{
					// Run update
					using (var cmd = new MySqlCommand(File.ReadAllText(Path.Combine("sql", updateFile)), conn))
						cmd.ExecuteNonQuery();

					// Log update
					using (var cmd = new InsertCommand("INSERT INTO `updates` {0}", conn))
					{
						cmd.Set("path", updateFile);
						cmd.Execute();
					}

					Log.Info("Successfully applied '{0}'.", updateFile);
				}
			}
			catch (Exception ex)
			{
				Log.Error("RunUpdate: Failed to run '{0}': {1}", updateFile, ex.Message);
				CliUtil.Exit(1);
			}
		}

		/// <summary>
		/// Returns account or null if account doesn't exist.
		/// </summary>
		/// <param name="accountId"></param>
		/// <returns></returns>
		public Account GetAccount(string accountId)
		{
			using (var conn = this.Connection)
			using (var mc = new MySqlCommand("SELECT * FROM `accounts` WHERE `accountId` = @accountId", conn))
			{
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
			using (var conn = this.Connection)
			using (var mc = new MySqlCommand("UPDATE `accounts` SET `secondaryPassword` = @secondaryPassword WHERE `accountId` = @accountId", conn))
			{
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
			using (var conn = this.Connection)
			using (var mc = new MySqlCommand("UPDATE `accounts` SET `sessionKey` = @sessionKey WHERE `accountId` = @accountId", conn))
			{
				var sessionKey = RandomProvider.Get().NextInt64();

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
			using (var conn = this.Connection)
			using (var cmd = new UpdateCommand("UPDATE `accounts` SET `lastLogin` = @lastLogin, `loggedIn` = @loggedIn WHERE `accountId` = @accountId", conn))
			{
				cmd.Set("accountId", account.Name);
				cmd.Set("lastLogin", account.LastLogin);
				cmd.Set("loggedIn", account.LoggedIn);

				cmd.Execute();
			}
		}
		/// <summary>
		/// Returns all character cards present for this account.
		/// </summary>
		/// <param name="accountId"></param>
		/// <returns></returns>
		public List<Card> GetCharacterCards(string accountId)
		{
			using (var conn = this.Connection)
			using (var mc = new MySqlCommand("SELECT `cardId`, `type` FROM `cards` WHERE `accountId` = @accountId AND race = 0 AND !`isGift`", conn))
			{
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
			using (var conn = this.Connection)
			using (var mc = new MySqlCommand("SELECT `cardId`, `type`, `race` FROM `cards` WHERE `accountId` = @accountId AND race > 0 AND !`isGift`", conn))
			{
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
			using (var conn = this.Connection)
			using (var mc = new MySqlCommand("SELECT * FROM `cards` WHERE `accountId` = @accountId AND `isGift`", conn))
			{
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
			using (var conn = this.Connection)
			{
				var result = new List<Character>();
				this.GetCharacters(accountId, "characters", CharacterType.Character, ref result, conn);

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
			using (var conn = this.Connection)
			{
				var result = new List<Character>();
				this.GetCharacters(accountId, "pets", CharacterType.Pet, ref result, conn);
				this.GetCharacters(accountId, "partners", CharacterType.Partner, ref result, conn);

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
		private void GetCharacters(string accountId, string table, CharacterType type, ref List<Character> result, MySqlConnection conn)
		{
			using (var mc = new MySqlCommand(
				"SELECT * " +
				"FROM `" + table + "` AS c " +
				"INNER JOIN `creatures` AS cr ON c.creatureId = cr.creatureId " +
				"WHERE `accountId` = @accountId "
			, conn))
			{
				mc.Parameters.AddWithValue("@accountId", accountId);

				using (var reader = mc.ExecuteReader())
				{
					while (reader.Read())
					{
						var character = new Character();
						character.EntityId = reader.GetInt64("entityId");
						character.CreatureId = reader.GetInt64("creatureId");
						character.Name = reader.GetStringSafe("name");
						character.Server = reader.GetStringSafe("server");
						character.Race = reader.GetInt32("race");
						character.DeletionTime = reader.GetDateTimeSafe("deletionTime");
						character.SkinColor = reader.GetByte("skinColor");
						character.EyeType = reader.GetInt16("eyeType");
						character.EyeColor = reader.GetByte("eyeColor");
						character.MouthType = reader.GetByte("mouthType");
						character.State = (CreatureStates)reader.GetUInt32("state");
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
		}

		/// <summary>
		/// Returns list of all visible items on creature.
		/// </summary>
		/// <param name="creatureId"></param>
		/// <returns></returns>
		public List<Item> GetEquipment(long creatureId)
		{
			using (var conn = this.Connection)
			using (var mc = new MySqlCommand("SELECT * FROM `items` WHERE `creatureId` = @creatureId", conn))
			{
				mc.Parameters.AddWithValue("@creatureId", creatureId);

				var result = new List<Item>();
				using (var reader = mc.ExecuteReader())
				{
					while (reader.Read())
					{
						var item = new Item();
						item.Id = reader.GetInt64("entityId");
						item.Info.Id = reader.GetInt32("itemId");
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
		/// character's ids are set to the new ids.
		/// </summary>
		/// <param name="accountId"></param>
		/// <param name="character"></param>
		/// <param name="items"></param>
		/// <returns></returns>
		public bool CreateCharacter(string accountId, Character character, List<Item> items)
		{
			return this.Create("characters", accountId, character, items);
		}

		/// <summary>
		/// Creates creature:pet combination for the account.
		/// Returns false if either failed, true on success.
		/// pet's ids are set to the new ids.
		/// </summary>
		/// <param name="accountId"></param>
		/// <param name="pet"></param>
		/// <returns></returns>
		public bool CreatePet(string accountId, Character pet)
		{
			return this.Create("pets", accountId, pet, null);
		}

		/// <summary>
		/// Creates creature:partner combination for the account.
		/// Returns false if either failed, true on success.
		/// partner's ids are set to the new ids.
		/// </summary>
		/// <param name="accountId"></param>
		/// <param name="partner"></param>
		/// <param name="items"></param>
		/// <returns></returns>
		public bool CreatePartner(string accountId, Character partner, List<Item> items)
		{
			return this.Create("partners", accountId, partner, items);
		}

		/// <summary>
		/// Creates creature:x combination for the account.
		/// Returns false if either failed, true on success.
		/// character's ids are set to the new ids.
		/// </summary>
		/// <param name="table"></param>
		/// <param name="accountId"></param>
		/// <param name="character"></param>
		/// <param name="items"></param>
		/// <returns></returns>
		private bool Create(string table, string accountId, Character character, List<Item> items)
		{
			using (var conn = this.Connection)
			using (var transaction = conn.BeginTransaction())
			{
				try
				{
					// Creature
					character.CreatureId = this.CreateCreature(character, conn, transaction);

					// Character
					using (var cmd = new InsertCommand("INSERT INTO `" + table + "` {0}", conn, transaction))
					{
						cmd.Set("accountId", accountId);
						cmd.Set("creatureId", character.CreatureId);

						cmd.Execute();

						character.EntityId = cmd.LastId;
					}

					// Items
					if (items != null)
						this.AddItems(character.CreatureId, items, conn, transaction);

					transaction.Commit();

					return true;
				}
				catch (Exception ex)
				{
					character.EntityId = character.CreatureId = 0;

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
			using (var cmd = new InsertCommand("INSERT INTO `creatures` {0}", conn, transaction))
			{
				cmd.Set("server", creature.Server);
				cmd.Set("name", creature.Name);
				cmd.Set("age", creature.Age);
				cmd.Set("race", creature.Race);
				cmd.Set("skinColor", creature.SkinColor);
				cmd.Set("eyeType", creature.EyeType);
				cmd.Set("eyeColor", creature.EyeColor);
				cmd.Set("mouthType", creature.MouthType);
				cmd.Set("state", (uint)creature.State);
				cmd.Set("height", creature.Height);
				cmd.Set("weight", creature.Weight);
				cmd.Set("upper", creature.Upper);
				cmd.Set("lower", creature.Lower);
				cmd.Set("color1", creature.Color1);
				cmd.Set("color2", creature.Color2);
				cmd.Set("color3", creature.Color3);
				cmd.Set("lifeMax", creature.Life);
				cmd.Set("manaMax", creature.Mana);
				cmd.Set("staminaMax", creature.Stamina);
				cmd.Set("str", creature.Str);
				cmd.Set("int", creature.Int);
				cmd.Set("dex", creature.Dex);
				cmd.Set("will", creature.Will);
				cmd.Set("luck", creature.Luck);
				cmd.Set("defense", creature.Defense);
				cmd.Set("protection", creature.Protection);
				cmd.Set("ap", creature.AP);
				cmd.Set("creationTime", DateTime.Now);
				cmd.Set("lastAging", DateTime.Now);

				cmd.Execute();

				return cmd.LastId;
			}
		}

		/// <summary>
		/// Adds items for creature.
		/// </summary>
		/// <param name="creatureId"></param>
		/// <param name="items"></param>
		/// <param name="conn"></param>
		/// <param name="transaction"></param>
		private void AddItems(long creatureId, List<Item> items, MySqlConnection conn, MySqlTransaction transaction)
		{
			foreach (var item in items)
			{
				var dataInfo = AuraData.ItemDb.Find(item.Info.Id);
				if (dataInfo == null)
				{
					Log.Warning("Item '{0}' couldn't be found in the database.", item.Info.Id);
					continue;
				}

				using (var cmd = new InsertCommand("INSERT INTO `items` {0}", conn, transaction))
				{
					cmd.Set("creatureId", creatureId);
					cmd.Set("itemId", item.Info.Id);
					cmd.Set("pocket", (byte)item.Info.Pocket);
					cmd.Set("color1", item.Info.Color1);
					cmd.Set("color2", item.Info.Color2);
					cmd.Set("color3", item.Info.Color3);
					cmd.Set("price", dataInfo.Price);
					cmd.Set("durability", dataInfo.Durability);
					cmd.Set("durabilityMax", dataInfo.Durability);
					cmd.Set("durabilityOriginal", dataInfo.Durability);
					cmd.Set("attackMin", dataInfo.AttackMin);
					cmd.Set("attackMax", dataInfo.AttackMax);
					cmd.Set("balance", dataInfo.Balance);
					cmd.Set("critical", dataInfo.Critical);
					cmd.Set("defense", dataInfo.Defense);
					cmd.Set("protection", dataInfo.Protection);
					cmd.Set("attackSpeed", dataInfo.AttackSpeed);
					cmd.Set("sellPrice", dataInfo.SellingPrice);

					cmd.Execute();
				}
			}
		}

		/// <summary>
		/// Removes the card from the db, returns true if successful.
		/// </summary>
		/// <param name="card"></param>
		/// <returns></returns>
		public bool DeleteCard(Card card)
		{
			using (var conn = this.Connection)
			using (var mc = new MySqlCommand("DELETE FROM `cards` WHERE `cardId` = @cardId", conn))
			{
				mc.Parameters.AddWithValue("@cardId", card.Id);

				return (mc.ExecuteNonQuery() > 0);
			}
		}

		/// <summary>
		/// Changes gift card with the given id to a normal card.
		/// </summary>
		/// <param name="giftId"></param>
		public void ChangeGiftToCard(long giftId)
		{
			using (var conn = this.Connection)
			using (var mc = new MySqlCommand("UPDATE `cards` SET `isGift` = FALSE WHERE `cardId` = @cardId", conn))
			{
				mc.Parameters.AddWithValue("@cardId", giftId);

				mc.ExecuteNonQuery();
			}
		}

		/// <summary>
		/// Updates deletion time for character, or deletes it.
		/// </summary>
		/// <param name="character"></param>
		public void UpdateDeletionTime(Character character)
		{
			using (var conn = this.Connection)
			{
				if (character.DeletionFlag == DeletionFlag.Delete)
				{
					using (var mc = new MySqlCommand("DELETE FROM `creatures` WHERE `creatureId` = @creatureId", conn))
					{
						mc.Parameters.AddWithValue("@creatureId", character.CreatureId);
						mc.ExecuteNonQuery();
					}
				}
				else
				{
					using (var cmd = new UpdateCommand("UPDATE `creatures` SET {0} WHERE `creatureId` = @creatureId", conn))
					{
						cmd.AddParameter("@creatureId", character.CreatureId);
						cmd.Set("deletionTime", character.DeletionTime);

						cmd.Execute();
					}
				}
			}
		}

		/// <summary>
		/// Changes auth level of account.
		/// </summary>
		/// <param name="accountId"></param>
		/// <param name="level"></param>
		/// <returns></returns>
		public bool ChangeAuth(string accountId, int level)
		{
			using (var conn = this.Connection)
			using (var cmd = new UpdateCommand("UPDATE `accounts` SET {0} WHERE `accountId` = @accountId", conn))
			{
				cmd.AddParameter("@accountId", accountId);
				cmd.Set("authority", level);

				return (cmd.Execute() > 0);
			}
		}

		/// <summary>
		/// Adds trade item and points of card to character.
		/// </summary>
		/// <param name="targetCharacter"></param>
		/// <param name="charCard"></param>
		public void TradeCard(Character targetCharacter, CharCardData charCard)
		{
			using (var conn = this.Connection)
			using (var cmd = new InsertCommand("INSERT INTO `items` {0}", conn))
			{
				cmd.Set("creatureId", targetCharacter.CreatureId);
				cmd.Set("itemId", charCard.TradeItem);
				cmd.Set("pocket", Pocket.Temporary);
				cmd.Set("color1", 0x808080);
				cmd.Set("color2", 0x808080);
				cmd.Set("color3", 0x808080);

				cmd.Execute();
			}

			// TODO: Add points (pons)...
		}

		/// <summary>
		/// Unsets creature's Initialized creature state flag.
		/// </summary>
		/// <param name="creatureId"></param>
		public void UninitializeCreature(long creatureId)
		{
			using (var conn = this.Connection)
			using (var mc = new MySqlCommand("UPDATE `creatures` SET `state` = `state` & ~1 WHERE `creatureId` = @creatureId", conn))
			{
				mc.Parameters.AddWithValue("@creatureId", creatureId);
				mc.ExecuteNonQuery();
			}
		}
	}
}
