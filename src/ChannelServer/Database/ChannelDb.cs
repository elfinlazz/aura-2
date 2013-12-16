// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Data;
using Aura.Data.Database;
using Aura.Shared.Database;
using Aura.Shared.Mabi;
using Aura.Shared.Mabi.Const;
using Aura.Shared.Util;
using MySql.Data.MySqlClient;
using Aura.Channel.World.Entities;
using Aura.Channel.World;
using Aura.Channel.World.Entities.Creatures;

namespace Aura.Channel.Database
{
	public class ChannelDb
	{
		public static readonly ChannelDb Instance = new ChannelDb();

		private ChannelDb()
		{
		}

		/// <summary>
		/// Returns account incl all characters or null, if it doesn't exist.
		/// </summary>
		/// <param name="accountId"></param>
		/// <returns></returns>
		public Account GetAccount(string accountId)
		{
			var account = new Account();

			using (var conn = AuraDb.Instance.Connection)
			{
				// Account
				// ----------------------------------------------------------
				using (var mc = new MySqlCommand("SELECT * FROM `accounts` WHERE `accountId` = @accountId", conn))
				{
					mc.Parameters.AddWithValue("@accountId", accountId);

					using (var reader = mc.ExecuteReader())
					{
						if (!reader.HasRows)
							return null;

						reader.Read();

						account.Id = reader.GetStringSafe("accountId");
						account.SessionKey = reader.GetInt64("sessionKey");
						account.Authority = reader.GetByte("authority");
					}
				}

				// Characters
				// ----------------------------------------------------------
				using (var mc = new MySqlCommand("SELECT * FROM `characters` WHERE `accountId` = @accountId", conn))
				{
					mc.Parameters.AddWithValue("@accountId", accountId);

					using (var reader = mc.ExecuteReader())
					{
						while (reader.Read())
						{
							var character = this.GetCharacter<Character>(reader.GetInt64("entityId"), "characters");
							if (character == null)
								continue;

							// Add GM titles for the characters of authority 50+ accounts
							if (account.Authority >= 50) character.Titles.Add(60000, TitleState.Usable); // GM
							if (account.Authority >= 99) character.Titles.Add(60001, TitleState.Usable); // devCat

							account.Characters.Add(character);
						}
					}
				}

				// Pets
				// ----------------------------------------------------------
				using (var mc = new MySqlCommand("SELECT * FROM `pets` WHERE `accountId` = @accountId", conn))
				{
					mc.Parameters.AddWithValue("@accountId", accountId);

					using (var reader = mc.ExecuteReader())
					{
						while (reader.Read())
						{
							var character = this.GetCharacter<Pet>(reader.GetInt64("entityId"), "pets");
							if (character == null)
								continue;

							account.Pets.Add(character);
						}
					}
				}

				// Partners
				// ----------------------------------------------------------
				using (var mc = new MySqlCommand("SELECT * FROM `partners` WHERE `accountId` = @accountId", conn))
				{
					mc.Parameters.AddWithValue("@accountId", accountId);

					using (var reader = mc.ExecuteReader())
					{
						while (reader.Read())
						{
							var character = this.GetCharacter<Pet>(reader.GetInt64("entityId"), "partners");
							if (character == null)
								continue;

							account.Pets.Add(character);
						}
					}
				}
			}

			return account;
		}

		/// <summary>
		/// Returns creature by entityId from table.
		/// </summary>
		/// <typeparam name="TCreature"></typeparam>
		/// <param name="entityId"></param>
		/// <returns></returns>
		private TCreature GetCharacter<TCreature>(long entityId, string table) where TCreature : PlayerCreature, new()
		{
			var character = new TCreature();

			using (var conn = AuraDb.Instance.Connection)
			using (var mc = new MySqlCommand("SELECT * FROM `" + table + "` AS c INNER JOIN `creatures` AS cr ON c.creatureId = cr.creatureId WHERE `entityId` = @entityId", conn))
			{
				mc.Parameters.AddWithValue("@entityId", entityId);

				using (var reader = mc.ExecuteReader())
				{
					if (!reader.Read())
						return null;

					character.EntityId = reader.GetInt64("entityId");
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
					character.RegionId = reader.GetInt32("region");
					var x = reader.GetInt32("x");
					var y = reader.GetInt32("y");
					character.SetPosition(x, y);
					character.Direction = reader.GetByte("direction");
					//character.BattleState = reader.GetByte("battleState");
					//character.Inventory.WeaponSet = (WeaponSet)reader.GetByte("weaponSet");
					character.Level = reader.GetInt16("level");
					character.LevelTotal = reader.GetInt32("levelTotal");
					character.Exp = reader.GetInt64("exp");
					character.AbilityPoints = reader.GetInt16("ap");
					character.Age = reader.GetInt16("age");
					character.Injuries = reader.GetFloat("injuries");
					character.Life = (character.LifeMaxBase = reader.GetFloat("lifeMax"));
					character.Life -= reader.GetFloat("lifeDelta");
					character.Mana = (character.ManaMaxBase = reader.GetFloat("manaMax"));
					character.Mana -= reader.GetFloat("manaDelta");
					character.Hunger = reader.GetFloat("hunger");
					character.Stamina = (character.StaminaMaxBase = reader.GetFloat("staminaMax"));
					character.Stamina -= reader.GetFloat("staminaDelta");
					character.StrBase = reader.GetFloat("str");
					character.DexBase = reader.GetFloat("dex");
					character.IntBase = reader.GetFloat("int");
					character.WillBase = reader.GetFloat("will");
					character.LuckBase = reader.GetFloat("luck");
				}
			}

			this.GetCharacterItems(character);
			this.GetCharacterKeywords(character);
			this.GetCharacterTitles(character);

			return character;
		}

		/// <summary>
		/// Reads items from database and adds them to character.
		/// </summary>
		/// <param name="character"></param>
		private void GetCharacterItems(PlayerCreature character)
		{
			var items = this.GetItems(character.CreatureId);
			foreach (var item in items)
				character.Inventory.ForceAdd(item, item.Info.Pocket);
		}

		/// <summary>
		/// Returns list of items for creature with the given id.
		/// </summary>
		/// <param name="creatureId"></param>
		/// <returns></returns>
		private List<Item> GetItems(long creatureId)
		{
			var result = new List<Item>();

			using (var conn = AuraDb.Instance.Connection)
			using (var mc = new MySqlCommand("SELECT * FROM `items` WHERE `creatureId` = @creatureId", conn))
			{
				mc.Parameters.AddWithValue("@creatureId", creatureId);

				using (var reader = mc.ExecuteReader())
				{
					while (reader.Read())
					{
						var itemId = reader.GetInt32("itemId");

						var item = new Item(itemId);
						item.EntityId = reader.GetInt64("entityId");
						item.Info.Pocket = (Pocket)reader.GetInt32("pocket");
						item.Info.X = reader.GetInt32("x");
						item.Info.Y = reader.GetInt32("y");
						item.Info.Color1 = reader.GetUInt32("color1");
						item.Info.Color3 = reader.GetUInt32("color2");
						item.Info.Color3 = reader.GetUInt32("color3");
						item.Info.Amount = reader.GetUInt16("amount");
						item.Info.State = reader.GetByte("state");
						item.OptionInfo.Price = reader.GetInt32("price");
						item.OptionInfo.SellingPrice = reader.GetInt32("sellPrice");
						item.OptionInfo.Durability = reader.GetInt32("durability");
						item.OptionInfo.DurabilityMax = reader.GetInt32("durabilityMax");
						item.OptionInfo.DurabilityNew = reader.GetInt32("durabilityNew");
						item.OptionInfo.AttackMin = reader.GetUInt16("attackMin");
						item.OptionInfo.AttackMax = reader.GetUInt16("attackMax");
						item.OptionInfo.Balance = reader.GetByte("balance");
						item.OptionInfo.Critical = reader.GetByte("critical");
						item.OptionInfo.Defense = reader.GetInt32("defense");
						item.OptionInfo.Protection = reader.GetInt16("protection");
						item.OptionInfo.EffectiveRange = reader.GetInt16("range");
						item.OptionInfo.AttackSpeed = (AttackSpeed)reader.GetByte("attackSpeed");
						item.OptionInfo.Experience = reader.GetInt16("experience");

						result.Add(item);
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Reads keywords from database and adds them to character.
		/// </summary>
		/// <param name="character"></param>
		private void GetCharacterKeywords(PlayerCreature character)
		{
			using (var conn = AuraDb.Instance.Connection)
			using (var mc = new MySqlCommand("SELECT * FROM `keywords` WHERE `creatureId` = @creatureId", conn))
			{
				mc.Parameters.AddWithValue("@creatureId", character.CreatureId);

				using (var reader = mc.ExecuteReader())
				{
					while (reader.Read())
					{
						var keywordId = reader.GetUInt16("keywordId");
						character.Keywords.Add(keywordId);
					}
				}
			}
		}

		/// <summary>
		/// Reads titles from database and adds them to character.
		/// </summary>
		/// <param name="character"></param>
		private void GetCharacterTitles(PlayerCreature character)
		{
			using (var conn = AuraDb.Instance.Connection)
			using (var mc = new MySqlCommand("SELECT * FROM `titles` WHERE `creatureId` = @creatureId", conn))
			{
				mc.Parameters.AddWithValue("@creatureId", character.CreatureId);

				using (var reader = mc.ExecuteReader())
				{
					while (reader.Read())
					{
						var id = reader.GetUInt16("titleId");
						var usable = (reader.GetBoolean("usable") ? TitleState.Usable : TitleState.Known);

						character.Titles.Add(id, usable);
					}
				}
			}
		}

		/// <summary>
		/// Saves account, incl. all character data.
		/// </summary>
		/// <param name="account"></param>
		public void SaveAccount(Account account)
		{
			using (var conn = AuraDb.Instance.Connection)
			using (var cmd = new UpdateCommand("UPDATE `accounts` SET {0} WHERE `accountId` = @accountId", conn))
			{
				cmd.AddParameter("@accountId", account.Id);
				cmd.Set("authority", (byte)account.Authority);
				cmd.Set("lastlogin", account.LastLogin);
				cmd.Set("banReason", account.BanReason);
				cmd.Set("banExpiration", account.BanExpiration);

				cmd.Execute();
			}

			// Save characters
			foreach (var character in account.Characters.Where(a => a.Save))
				this.SaveCreature(character);
			foreach (var pet in account.Pets.Where(a => a.Save))
				this.SaveCreature(pet);
		}

		/// <summary>
		/// Saves creature and all its data.
		/// </summary>
		/// <param name="creature"></param>
		public void SaveCreature(PlayerCreature creature)
		{
			using (var conn = AuraDb.Instance.Connection)
			using (var cmd = new UpdateCommand("UPDATE `creatures` SET {0} WHERE `creatureId` = @creatureId", conn))
			{
				var characterLocation = creature.GetPosition();

				cmd.AddParameter("@creatureId", creature.CreatureId);
				cmd.Set("region", creature.RegionId);
				cmd.Set("x", characterLocation.X);
				cmd.Set("y", characterLocation.Y);
				cmd.Set("direction", creature.Direction);
				//up.Set("battleState", creature.BattleState);
				//up.Set("weaponSet", (byte)creature.Inventory.WeaponSet);
				cmd.Set("lifeDelta", creature.LifeMax - creature.Life);
				cmd.Set("injuries", creature.Injuries);
				cmd.Set("lifeMax", creature.LifeMaxBase);
				cmd.Set("manaDelta", creature.ManaMax - creature.Mana);
				cmd.Set("manaMax", creature.ManaMaxBase);
				cmd.Set("staminaDelta", creature.StaminaMax - creature.Stamina);
				cmd.Set("staminaMax", creature.StaminaMaxBase);
				cmd.Set("hunger", creature.Hunger);
				cmd.Set("level", creature.Level);
				cmd.Set("levelTotal", creature.LevelTotal);
				cmd.Set("exp", creature.Exp);
				cmd.Set("str", creature.StrBase);
				cmd.Set("dex", creature.DexBase);
				cmd.Set("int", creature.IntBase);
				cmd.Set("will", creature.WillBase);
				cmd.Set("luck", creature.LuckBase);
				cmd.Set("ap", creature.AbilityPoints);

				cmd.Execute();
			}

			this.SaveCreatureKeywords(creature);
			this.SaveCreatureTitles(creature);
			//this.SaveQuests(creature);
			//this.SaveItems(creature);
			//this.SaveSkills(creature);
			//this.SaveCooldowns(creature);
		}

		/// <summary>
		/// Writes all of creature's keywords to the database.
		/// </summary>
		/// <param name="creature"></param>
		private void SaveCreatureKeywords(PlayerCreature creature)
		{
			using (var conn = AuraDb.Instance.Connection)
			using (var transaction = conn.BeginTransaction())
			{
				using (var mc = new MySqlCommand("DELETE FROM `keywords` WHERE `creatureId` = @creatureId", conn, transaction))
				{
					mc.Parameters.AddWithValue("@creatureId", creature.CreatureId);
					mc.ExecuteNonQuery();
				}

				foreach (var keywordId in creature.Keywords)
				{
					using (var cmd = new InsertCommand("INSERT INTO `keywords` {0}", conn, transaction))
					{
						cmd.Set("creatureId", creature.CreatureId);
						cmd.Set("keywordId", keywordId);

						cmd.Execute();
					}
				}

				transaction.Commit();
			}
		}

		/// <summary>
		/// Writes all of creature's titles to the database.
		/// </summary>
		/// <param name="creature"></param>
		private void SaveCreatureTitles(PlayerCreature creature)
		{
			using (var conn = AuraDb.Instance.Connection)
			using (var transaction = conn.BeginTransaction())
			{
				using (var mc = new MySqlCommand("DELETE FROM `titles` WHERE `creatureId` = @creatureId", conn, transaction))
				{
					mc.Parameters.AddWithValue("@creatureId", creature.CreatureId);
					mc.ExecuteNonQuery();
				}

				foreach (var title in creature.Titles)
				{
					// Dynamic titles shouldn't be saved
					// TODO: Title db that tells us this?
					if (title.Key == 60000 || title.Key == 60001 || title.Key == 50000) // GM, devCAT, Guild
						continue;

					using (var cmd = new InsertCommand("INSERT INTO `titles` {0}", conn, transaction))
					{
						cmd.Set("creatureId", creature.CreatureId);
						cmd.Set("titleId", title.Key);
						cmd.Set("usable", (title.Value == TitleState.Usable));

						cmd.Execute();
					}
				}

				transaction.Commit();
			}
		}
	}
}
