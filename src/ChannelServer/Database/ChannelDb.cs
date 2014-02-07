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
using Aura.Channel.Skills;
using Aura.Channel.Scripting;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

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

				account.Vars.Perm = this.LoadVars(account.Id, 0);

				// Characters
				// ----------------------------------------------------------
				using (var mc = new MySqlCommand("SELECT * FROM `characters` WHERE `accountId` = @accountId", conn))
				{
					mc.Parameters.AddWithValue("@accountId", accountId);

					using (var reader = mc.ExecuteReader())
					{
						while (reader.Read())
						{
							var character = this.GetCharacter<Character>(account, reader.GetInt64("entityId"), "characters");
							if (character == null)
								continue;

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
							var character = this.GetCharacter<Pet>(account, reader.GetInt64("entityId"), "pets");
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
							var character = this.GetCharacter<Pet>(account, reader.GetInt64("entityId"), "partners");
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
		private TCreature GetCharacter<TCreature>(Account account, long entityId, string table) where TCreature : PlayerCreature, new()
		{
			var character = new TCreature();
			ushort title = 0, optionTitle = 0;
			float lifeDelta = 0, manaDelta = 0, staminaDelta = 0;

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
					var r = reader.GetInt32("region");
					var x = reader.GetInt32("x");
					var y = reader.GetInt32("y");
					character.SetLocation(r, x, y);
					character.Direction = reader.GetByte("direction");
					character.Inventory.WeaponSet = (WeaponSet)reader.GetByte("weaponSet");
					character.Level = reader.GetInt16("level");
					character.LevelTotal = reader.GetInt32("levelTotal");
					character.Exp = reader.GetInt64("exp");
					character.AbilityPoints = reader.GetInt16("ap");
					character.Age = reader.GetInt16("age");

					character.LifeFoodMod = reader.GetFloat("lifeFood");
					character.ManaFoodMod = reader.GetFloat("manaFood");
					character.StaminaFoodMod = reader.GetFloat("staminaFood");
					character.LifeMaxBase = reader.GetFloat("lifeMax");
					character.ManaMaxBase = reader.GetFloat("manaMax");
					character.StaminaMaxBase = reader.GetFloat("staminaMax");
					character.Injuries = reader.GetFloat("injuries");
					character.Hunger = reader.GetFloat("hunger");

					lifeDelta = reader.GetFloat("lifeDelta");
					manaDelta = reader.GetFloat("manaDelta");
					staminaDelta = reader.GetFloat("staminaDelta");

					character.StrBase = reader.GetFloat("str");
					character.DexBase = reader.GetFloat("dex");
					character.IntBase = reader.GetFloat("int");
					character.WillBase = reader.GetFloat("will");
					character.LuckBase = reader.GetFloat("luck");
					character.StrFoodMod = reader.GetFloat("strFood");
					character.IntFoodMod = reader.GetFloat("intFood");
					character.DexFoodMod = reader.GetFloat("dexFood");
					character.WillFoodMod = reader.GetFloat("willFood");
					character.LuckFoodMod = reader.GetFloat("luckFood");

					title = reader.GetUInt16("title");
					optionTitle = reader.GetUInt16("optionTitle");
				}

				character.LoadDefault();
			}

			this.GetCharacterItems(character);
			this.GetCharacterKeywords(character);
			this.GetCharacterTitles(character);
			this.GetCharacterSkills(character);

			// Add GM titles for the characters of authority 50+ accounts
			if (account != null)
			{
				if (account.Authority >= 50) character.Titles.Add(60000, TitleState.Usable); // GM
				if (account.Authority >= 99) character.Titles.Add(60001, TitleState.Usable); // devCAT
				if (account.Authority >= 99) character.Titles.Add(60002, TitleState.Usable); // devDOG
			}

			// Init titles
			if (title != 0) character.Titles.ChangeTitle(title, false);
			if (optionTitle != 0) character.Titles.ChangeTitle(optionTitle, true);

			// Calculate stats, not that we have modded the maxes
			character.Life = (character.LifeMax - lifeDelta);
			character.Mana = (character.ManaMax - manaDelta);
			character.Stamina = (character.StaminaMax - staminaDelta);

			character.Vars.Perm = this.LoadVars(account.Id, character.CreatureId);

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
			{
				if (!character.Inventory.InitAdd(item))
				{
					Log.Error("GetCharacterItems: Unable to add item '{0}' ({1}) to inventory.", item.Info.Id, item.EntityId);
				}
			}
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
						item.Info.Color2 = reader.GetUInt32("color2");
						item.Info.Color3 = reader.GetUInt32("color3");
						item.Info.Amount = reader.GetUInt16("amount");
						item.Info.State = reader.GetByte("state");
						item.OptionInfo.Price = reader.GetInt32("price");
						item.OptionInfo.SellingPrice = reader.GetInt32("sellPrice");
						item.OptionInfo.Durability = reader.GetInt32("durability");
						item.OptionInfo.DurabilityMax = reader.GetInt32("durabilityMax");
						item.OptionInfo.DurabilityOriginal = reader.GetInt32("durabilityOriginal");
						item.OptionInfo.AttackMin = reader.GetUInt16("attackMin");
						item.OptionInfo.AttackMax = reader.GetUInt16("attackMax");
						item.OptionInfo.Balance = reader.GetByte("balance");
						item.OptionInfo.Critical = reader.GetByte("critical");
						item.OptionInfo.Defense = reader.GetInt32("defense");
						item.OptionInfo.Protection = reader.GetInt16("protection");
						item.OptionInfo.EffectiveRange = reader.GetInt16("range");
						item.OptionInfo.AttackSpeed = (AttackSpeed)reader.GetByte("attackSpeed");
						item.OptionInfo.Experience = reader.GetInt16("experience");
						item.MetaData1.Parse(reader.GetStringSafe("meta1"));
						item.MetaData2.Parse(reader.GetStringSafe("meta2"));

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

			if (character.Is(EntityType.Character))
			{
				// Default
				character.Keywords.Add("personal_info");
				character.Keywords.Add("rumor");
				character.Keywords.Add("about_skill");
				character.Keywords.Add("about_arbeit");
				character.Keywords.Add("about_study");

				// Continent Warp
				character.Keywords.Add("portal_qilla_base_camp");
				character.Keywords.Add("portal_belfast");
				character.Keywords.Add("portal_dunbarton");
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
		/// Reads skills from database and adds them to character.
		/// </summary>
		/// <param name="character"></param>
		private void GetCharacterSkills(PlayerCreature character)
		{
			using (var conn = AuraDb.Instance.Connection)
			using (var mc = new MySqlCommand("SELECT * FROM `skills` WHERE `creatureId` = @creatureId", conn))
			{
				mc.Parameters.AddWithValue("@creatureId", character.CreatureId);

				using (var reader = mc.ExecuteReader())
				{
					while (reader.Read())
					{
						var skillId = (SkillId)reader.GetInt32("skillId");
						var rank = (SkillRank)reader.GetByte("rank");
						var exp = reader.GetInt32("exp");

						character.Skills.Add(new Skill(skillId, rank, character.Race, exp));
					}
				}
			}

			// Add skills if they don't exist yet. Everybody gets
			// Combat Mastery, only normal characters get all the
			// hidden ones for now
			// TODO: Move to race skill db.
			character.Skills.Add(new Skill(SkillId.CombatMastery, SkillRank.RF, character.Race));
			if (character.Is(EntityType.Character))
			{
				character.Skills.Add(new Skill(SkillId.HiddenEnchant, SkillRank.Novice, character.Race));
				character.Skills.Add(new Skill(SkillId.HiddenResurrection, SkillRank.Novice, character.Race));
				character.Skills.Add(new Skill(SkillId.HiddenTownBack, SkillRank.Novice, character.Race));
				character.Skills.Add(new Skill(SkillId.HiddenGuildStoneSetting, SkillRank.Novice, character.Race));
				character.Skills.Add(new Skill(SkillId.HiddenBlessing, SkillRank.Novice, character.Race));
				character.Skills.Add(new Skill(SkillId.CampfireKit, SkillRank.Novice, character.Race));
				character.Skills.Add(new Skill(SkillId.SkillUntrainKit, SkillRank.Novice, character.Race));
				character.Skills.Add(new Skill(SkillId.BigBlessingWaterKit, SkillRank.Novice, character.Race));
				character.Skills.Add(new Skill(SkillId.Dye, SkillRank.Novice, character.Race));
				character.Skills.Add(new Skill(SkillId.EnchantElementalAllSlot, SkillRank.Novice, character.Race));
				character.Skills.Add(new Skill(SkillId.HiddenPoison, SkillRank.Novice, character.Race));
				character.Skills.Add(new Skill(SkillId.HiddenBomb, SkillRank.Novice, character.Race));
				character.Skills.Add(new Skill(SkillId.FossilRestoration, SkillRank.Novice, character.Race));
				character.Skills.Add(new Skill(SkillId.SeesawJump, SkillRank.Novice, character.Race));
				character.Skills.Add(new Skill(SkillId.SeesawCreate, SkillRank.Novice, character.Race));
				character.Skills.Add(new Skill(SkillId.DragonSupport, SkillRank.Novice, character.Race));
				character.Skills.Add(new Skill(SkillId.IceMine, SkillRank.Novice, character.Race));
				character.Skills.Add(new Skill(SkillId.Scan, SkillRank.Novice, character.Race));
				character.Skills.Add(new Skill(SkillId.UseSupportItem, SkillRank.Novice, character.Race));
				character.Skills.Add(new Skill(SkillId.TickingQuizBomb, SkillRank.Novice, character.Race));
				character.Skills.Add(new Skill(SkillId.ItemSeal, SkillRank.Novice, character.Race));
				character.Skills.Add(new Skill(SkillId.ItemUnseal, SkillRank.Novice, character.Race));
				character.Skills.Add(new Skill(SkillId.ItemDungeonPass, SkillRank.Novice, character.Race));
				character.Skills.Add(new Skill(SkillId.UseElathaItem, SkillRank.Novice, character.Race));
				character.Skills.Add(new Skill(SkillId.UseMorrighansFeather, SkillRank.Novice, character.Race));
				character.Skills.Add(new Skill(SkillId.PetBuffing, SkillRank.Novice, character.Race));
				character.Skills.Add(new Skill(SkillId.CherryTreeKit, SkillRank.Novice, character.Race));
				character.Skills.Add(new Skill(SkillId.ThrowConfetti, SkillRank.Novice, character.Race));
				character.Skills.Add(new Skill(SkillId.UsePartyPopper, SkillRank.Novice, character.Race));
				character.Skills.Add(new Skill(SkillId.HammerGame, SkillRank.Novice, character.Race));
				character.Skills.Add(new Skill(SkillId.SpiritShift, SkillRank.Novice, character.Race));
				character.Skills.Add(new Skill(SkillId.EmergencyEscapeBomb, SkillRank.Novice, character.Race));
				character.Skills.Add(new Skill(SkillId.EmergencyIceBomb, SkillRank.Novice, character.Race));
				character.Skills.Add(new Skill(SkillId.NameColorChange, SkillRank.Novice, character.Race));
				character.Skills.Add(new Skill(SkillId.HolyFlame, SkillRank.Novice, character.Race));
				character.Skills.Add(new Skill(SkillId.CreateFaliasPortal, SkillRank.Novice, character.Race));
				character.Skills.Add(new Skill(SkillId.UseItemChattingColorChange, SkillRank.Novice, character.Race));
				character.Skills.Add(new Skill(SkillId.InstallPrivateFarmFacility, SkillRank.Novice, character.Race));
				character.Skills.Add(new Skill(SkillId.ReorientHomesteadbuilding, SkillRank.Novice, character.Race));
				character.Skills.Add(new Skill(SkillId.GachaponSynthesis, SkillRank.Novice, character.Race));
				character.Skills.Add(new Skill(SkillId.MakeChocoStatue, SkillRank.Novice, character.Race));
				character.Skills.Add(new Skill(SkillId.Paint, SkillRank.Novice, character.Race));
				character.Skills.Add(new Skill(SkillId.MixPaint, SkillRank.Novice, character.Race));
				character.Skills.Add(new Skill(SkillId.PetSealToItem, SkillRank.Novice, character.Race));
				character.Skills.Add(new Skill(SkillId.FlownHotAirBalloon, SkillRank.Novice, character.Race));
				character.Skills.Add(new Skill(SkillId.ItemSeal2, SkillRank.Novice, character.Race));
				character.Skills.Add(new Skill(SkillId.CureZombie, SkillRank.Novice, character.Race));
				character.Skills.Add(new Skill(SkillId.ContinentWarp, SkillRank.Novice, character.Race));
				character.Skills.Add(new Skill(SkillId.AddSeasoning, SkillRank.Novice, character.Race));
				character.Skills.Add(new Skill(SkillId.Gathering, SkillRank.Novice, character.Race));
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

			this.SaveVars(account.Id, 0, account.Vars.Perm);

			// Save characters
			foreach (var character in account.Characters.Where(a => a.Save))
				this.SaveCharacter(character, account);
			foreach (var pet in account.Pets.Where(a => a.Save))
				this.SaveCharacter(pet, account);
		}

		/// <summary>
		/// Saves creature and all its data.
		/// </summary>
		/// <param name="creature"></param>
		public void SaveCharacter(PlayerCreature creature, Account account)
		{
			using (var conn = AuraDb.Instance.Connection)
			using (var cmd = new UpdateCommand("UPDATE `creatures` SET {0} WHERE `creatureId` = @creatureId", conn))
			{
				var characterLocation = creature.GetPosition();

				cmd.AddParameter("@creatureId", creature.CreatureId);
				cmd.Set("height", creature.Height);
				cmd.Set("weight", creature.Weight);
				cmd.Set("upper", creature.Upper);
				cmd.Set("lower", creature.Lower);
				cmd.Set("region", creature.RegionId);
				cmd.Set("x", characterLocation.X);
				cmd.Set("y", characterLocation.Y);
				cmd.Set("direction", creature.Direction);
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
				cmd.Set("weaponSet", (byte)creature.Inventory.WeaponSet);
				cmd.Set("lifeFood", creature.LifeFoodMod);
				cmd.Set("manaFood", creature.ManaFoodMod);
				cmd.Set("staminaFood", creature.StaminaFoodMod);
				cmd.Set("strFood", creature.StrFoodMod);
				cmd.Set("intFood", creature.IntFoodMod);
				cmd.Set("dexFood", creature.DexFoodMod);
				cmd.Set("willFood", creature.WillFoodMod);
				cmd.Set("luckFood", creature.LuckFoodMod);
				cmd.Set("title", creature.Titles.SelectedTitle);
				cmd.Set("optionTitle", creature.Titles.SelectedOptionTitle);

				cmd.Execute();
			}

			this.SaveCharacterItems(creature);
			this.SaveCharacterKeywords(creature);
			this.SaveCharacterTitles(creature);
			this.SaveCharacterSkills(creature);
			//this.SaveCharacterQuests(creature);
			//this.SaveCharacterCooldowns(creature);

			this.SaveVars(account.Id, creature.CreatureId, creature.Vars.Perm);
		}

		/// <summary>
		/// Writes all of creature's keywords to the database.
		/// </summary>
		/// <param name="creature"></param>
		private void SaveCharacterKeywords(PlayerCreature creature)
		{
			using (var conn = AuraDb.Instance.Connection)
			using (var transaction = conn.BeginTransaction())
			{
				using (var mc = new MySqlCommand("DELETE FROM `keywords` WHERE `creatureId` = @creatureId", conn, transaction))
				{
					mc.Parameters.AddWithValue("@creatureId", creature.CreatureId);
					mc.ExecuteNonQuery();
				}

				foreach (var keywordId in creature.Keywords.GetList())
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
		private void SaveCharacterTitles(PlayerCreature creature)
		{
			using (var conn = AuraDb.Instance.Connection)
			using (var transaction = conn.BeginTransaction())
			{
				using (var mc = new MySqlCommand("DELETE FROM `titles` WHERE `creatureId` = @creatureId", conn, transaction))
				{
					mc.Parameters.AddWithValue("@creatureId", creature.CreatureId);
					mc.ExecuteNonQuery();
				}

				foreach (var title in creature.Titles.GetList())
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

		/// <summary>
		/// Saves all of creature's items.
		/// </summary>
		/// <param name="creature"></param>
		private void SaveCharacterItems(PlayerCreature creature)
		{
			using (var conn = AuraDb.Instance.Connection)
			using (var transaction = conn.BeginTransaction())
			{
				using (var mc = new MySqlCommand("DELETE FROM `items` WHERE `creatureId` = @creatureId", conn, transaction))
				{
					mc.Parameters.AddWithValue("@creatureId", creature.CreatureId);
					mc.ExecuteNonQuery();
				}

				foreach (var item in creature.Inventory.Items)
				{
					using (var cmd = new InsertCommand("INSERT INTO `items` {0}", conn, transaction))
					{
						cmd.Set("creatureId", creature.CreatureId);
						if (item.EntityId < MabiId.TmpItems)
							cmd.Set("entityId", item.EntityId);
						cmd.Set("itemId", item.Info.Id);
						cmd.Set("pocket", (byte)item.Info.Pocket);
						cmd.Set("x", item.Info.X);
						cmd.Set("y", item.Info.Y);
						cmd.Set("color1", item.Info.Color1);
						cmd.Set("color2", item.Info.Color2);
						cmd.Set("color3", item.Info.Color3);
						cmd.Set("price", item.OptionInfo.Price);
						cmd.Set("sellPrice", item.OptionInfo.SellingPrice);
						cmd.Set("amount", item.Info.Amount);
						cmd.Set("linkedPocket", item.OptionInfo.LinkedPocketId);
						cmd.Set("state", item.Info.State);
						cmd.Set("durability", item.OptionInfo.Durability);
						cmd.Set("durabilityMax", item.OptionInfo.DurabilityMax);
						cmd.Set("durabilityOriginal", item.OptionInfo.DurabilityOriginal);
						cmd.Set("attackMin", item.OptionInfo.AttackMin);
						cmd.Set("attackMax", item.OptionInfo.AttackMax);
						cmd.Set("balance", item.OptionInfo.Balance);
						cmd.Set("critical", item.OptionInfo.Critical);
						cmd.Set("defense", item.OptionInfo.Defense);
						cmd.Set("protection", item.OptionInfo.Protection);
						cmd.Set("range", item.OptionInfo.EffectiveRange);
						cmd.Set("attackSpeed", (byte)item.OptionInfo.AttackSpeed);
						cmd.Set("experience", item.OptionInfo.Experience);
						cmd.Set("meta1", item.MetaData1.ToString());
						cmd.Set("meta2", item.MetaData2.ToString());

						cmd.Execute();
					}
				}

				transaction.Commit();
			}
		}
		/// <summary>
		/// Writes all of creature's skills to the database.
		/// </summary>
		/// <param name="creature"></param>
		private void SaveCharacterSkills(PlayerCreature creature)
		{
			using (var conn = AuraDb.Instance.Connection)
			using (var transaction = conn.BeginTransaction())
			{
				using (var mc = new MySqlCommand("DELETE FROM `skills` WHERE `creatureId` = @creatureId", conn, transaction))
				{
					mc.Parameters.AddWithValue("@creatureId", creature.CreatureId);
					mc.ExecuteNonQuery();
				}

				foreach (var skill in creature.Skills.GetList())
				{
					using (var cmd = new InsertCommand("INSERT INTO `skills` {0}", conn, transaction))
					{
						cmd.Set("skillId", (ushort)skill.Info.Id);
						cmd.Set("creatureId", creature.CreatureId);
						cmd.Set("rank", (byte)skill.Info.Rank);
						cmd.Set("exp", skill.Info.Experience);

						cmd.Execute();
					}
				}

				transaction.Commit();
			}
		}

		/// <summary>
		/// Returns manager with all variables for the account (if creature
		/// id is 0) or creature.
		/// </summary>
		/// <param name="accountId"></param>
		/// <param name="creatureId">Use 0 to only get all account variables.</param>
		/// <returns></returns>
		public VariableManager LoadVars(string accountId, long creatureId)
		{
			using (var conn = AuraDb.Instance.Connection)
			using (var mc = new MySqlCommand("SELECT * FROM vars WHERE accountId = @accountId AND creatureId = @creatureId", conn))
			{
				mc.Parameters.AddWithValue("@accountId", accountId);
				mc.Parameters.AddWithValue("@creatureId", creatureId);

				var vars = new VariableManager();

				using (var reader = mc.ExecuteReader())
				{
					while (reader.Read())
					{
						var name = reader.GetString("name");
						var type = reader.GetString("type");
						var val = reader.GetStringSafe("value");

						if (val == null)
							continue;

						switch (type)
						{
							case "1u": vars[name] = byte.Parse(val); break;
							case "2u": vars[name] = ushort.Parse(val); break;
							case "4u": vars[name] = uint.Parse(val); break;
							case "8u": vars[name] = ulong.Parse(val); break;
							case "1": vars[name] = sbyte.Parse(val); break;
							case "2": vars[name] = short.Parse(val); break;
							case "4": vars[name] = int.Parse(val); break;
							case "8": vars[name] = long.Parse(val); break;
							case "f": vars[name] = float.Parse(val); break;
							case "d": vars[name] = double.Parse(val); break;
							case "b": vars[name] = bool.Parse(val); break;
							case "s": vars[name] = val; break;
							case "o":
								var buffer = Convert.FromBase64String(val);
								var bf = new BinaryFormatter();
								using (var ms = new MemoryStream(buffer))
								{
									vars[name] = bf.Deserialize(ms);
								}

								break;
							default:
								Log.Warning("LoadVars: Unknown variable type '{0}'.", type);
								continue;
						}
					}
				}

				return vars;
			}
		}

		/// <summary>
		/// Saves all variables in manager.
		/// </summary>
		/// <param name="accountId"></param>
		/// <param name="creatureId">Use 0 to save as account variables.</param>
		/// <param name="vars"></param>
		public void SaveVars(string accountId, long creatureId, VariableManager vars)
		{
			using (var conn = AuraDb.Instance.Connection)
			using (var transaction = conn.BeginTransaction())
			{
				using (var deleteMc = new MySqlCommand("DELETE FROM vars WHERE accountId = @accountId AND creatureId = @creatureId", conn, transaction))
				{
					deleteMc.Parameters.AddWithValue("@accountId", accountId);
					deleteMc.Parameters.AddWithValue("@creatureId", creatureId);
					deleteMc.ExecuteNonQuery();
				}

				var bf = new BinaryFormatter();

				foreach (var var in vars.GetList())
				{
					if (var.Value == null)
						continue;

					// Get type
					string type;
					if (var.Value is byte) type = "1u";
					else if (var.Value is ushort) type = "2u";
					else if (var.Value is uint) type = "4u";
					else if (var.Value is ulong) type = "8u";
					else if (var.Value is sbyte) type = "1";
					else if (var.Value is short) type = "2";
					else if (var.Value is int) type = "4";
					else if (var.Value is long) type = "8";
					else if (var.Value is float) type = "f";
					else if (var.Value is double) type = "d";
					else if (var.Value is bool) type = "b";
					else if (var.Value is string) type = "s";
					else type = "o";

					// Get value
					var val = string.Empty;
					if (type != "o")
					{
						val = var.Value.ToString();
					}
					else
					{
						// Objects are serialized to a Base64 string,
						// because we're storing as string for easier
						// inter-language access.
						using (var ms = new MemoryStream())
						{
							bf.Serialize(ms, var.Value);
							val = Convert.ToBase64String(ms.ToArray());
						}
					}

					// Make sure value isn't too big for the mediumtext field
					// (unlikely as it may be).
					if (val.Length > ushort.MaxValue)
					{
						Log.Warning("SaveVars: Skipping variable '{0}', it's too big.", var.Key);
						continue;
					}

					// Save
					using (var cmd = new InsertCommand("INSERT INTO `vars` {0}", conn, transaction))
					{
						cmd.Set("accountId", accountId);
						cmd.Set("creatureId", creatureId);
						cmd.Set("name", var.Key);
						cmd.Set("type", type);
						cmd.Set("value", val);

						cmd.Execute();
					}
				}

				transaction.Commit();
			}
		}
	}
}
