// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network;
using Aura.Channel.Scripting;
using Aura.Channel.Skills;
using Aura.Channel.Util;
using Aura.Channel.World.Entities;
using Aura.Channel.World.Entities.Creatures;
using Aura.Channel.World.Inventory;
using Aura.Channel.World.Quests;
using Aura.Shared.Database;
using Aura.Shared.Mabi.Const;
using Aura.Shared.Util;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace Aura.Channel.Database
{
	public class ChannelDb : AuraDb
	{
		/// <summary>
		/// Returns account incl all characters or null, if it doesn't exist.
		/// </summary>
		/// <param name="accountId"></param>
		/// <returns></returns>
		public Account GetAccount(string accountId)
		{
			var account = new Account();

			using (var conn = this.Connection)
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
						account.AutobanCount = reader.GetInt32("autobanCount");
						account.AutobanScore = reader.GetInt32("autobanScore");
						account.LastAutobanReduction = reader.GetDateTimeSafe("lastAutobanReduction");
						account.Bank.Gold = reader.GetInt32("bankGold");

						// We don't need to decrease their score if it's already zero!
						if (account.AutobanScore > 0 && ChannelServer.Instance.Conf.Autoban.ReductionTime.Ticks != 0)
						{
							var elapsed = DateTime.Now - account.LastAutobanReduction;
							var delta = (int)(elapsed.TotalMinutes / ChannelServer.Instance.Conf.Autoban.ReductionTime.TotalMinutes);

							// Adding a -delta means they're a time traveller! =*O*=
							// It would also increase their score.
							if (delta < 0)
							{
								account.AutobanScore -= delta;
								// We add the delta to prevent rapid logins/outs from affecting the score
								account.LastAutobanReduction = account.LastAutobanReduction.Add(
									TimeSpan.FromMinutes((long)(ChannelServer.Instance.Conf.Autoban.ReductionTime.TotalMinutes * delta)));
							}
						}
					}
				}

				// Read account variables
				account.Vars.Perm = this.LoadVars(account.Id, 0);

				// Characters
				// ----------------------------------------------------------
				var creatureId = 0L;
				try
				{
					using (var mc = new MySqlCommand("SELECT * FROM `characters` WHERE `accountId` = @accountId", conn))
					{
						mc.Parameters.AddWithValue("@accountId", accountId);

						using (var reader = mc.ExecuteReader())
						{
							while (reader.Read())
							{
								creatureId = reader.GetInt64("entityId");
								var character = this.GetCharacter<Character>(account, creatureId, "characters");
								if (character == null)
									continue;

								account.Characters.Add(character);
							}
						}
					}

				}
				catch (Exception ex)
				{
					Log.Exception(ex, "Problem while loading character '{0}'.", creatureId);
				}

				// Pets
				// ----------------------------------------------------------
				creatureId = 0L;
				try
				{
					using (var mc = new MySqlCommand("SELECT * FROM `pets` WHERE `accountId` = @accountId", conn))
					{
						mc.Parameters.AddWithValue("@accountId", accountId);

						using (var reader = mc.ExecuteReader())
						{
							while (reader.Read())
							{
								creatureId = reader.GetInt64("entityId");
								var character = this.GetCharacter<Pet>(account, creatureId, "pets");
								if (character == null)
									continue;

								account.Pets.Add(character);
							}
						}
					}
				}
				catch (Exception ex)
				{
					Log.Exception(ex, "Problem while loading pet '{0}'.", creatureId);
				}

				// Partners
				// ----------------------------------------------------------
				creatureId = 0L;
				try
				{
					using (var mc = new MySqlCommand("SELECT * FROM `partners` WHERE `accountId` = @accountId", conn))
					{
						mc.Parameters.AddWithValue("@accountId", accountId);

						using (var reader = mc.ExecuteReader())
						{
							while (reader.Read())
							{
								creatureId = reader.GetInt64("entityId");
								var character = this.GetCharacter<Pet>(account, creatureId, "partners");
								if (character == null)
									continue;

								account.Pets.Add(character);
							}
						}
					}
				}
				catch (Exception ex)
				{
					Log.Exception(ex, "Problem while loading partner '{0}'.", creatureId);
				}
			}

			return account;
		}

		/// <summary>
		/// Returns creature by entityId from table.
		/// </summary>
		/// <typeparam name="TCreature"></typeparam>
		/// <param name="account"></param>
		/// <param name="entityId"></param>
		/// <param name="table"></param>
		/// <returns></returns>
		private TCreature GetCharacter<TCreature>(Account account, long entityId, string table) where TCreature : PlayerCreature, new()
		{
			var character = new TCreature();
			ushort title = 0, optionTitle = 0;
			float lifeDelta = 0, manaDelta = 0, staminaDelta = 0;
			int bankWidth = 12, bankHeight = 8;

			using (var conn = this.Connection)
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
					character.EyeType = reader.GetInt16("eyeType");
					character.EyeColor = reader.GetByte("eyeColor");
					character.MouthType = reader.GetByte("mouthType");
					character.Height = reader.GetFloat("height");
					character.Weight = reader.GetFloat("weight");
					character.Upper = reader.GetFloat("upper");
					character.Lower = reader.GetFloat("lower");
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
					character.TotalLevel = reader.GetInt32("levelTotal");
					character.Exp = reader.GetInt64("exp");
					character.AbilityPoints = reader.GetInt16("ap");
					character.Age = reader.GetInt16("age");
					character.State = (CreatureStates)reader.GetUInt32("state");

					character.CreationTime = reader.GetDateTimeSafe("creationTime");
					character.LastRebirth = reader.GetDateTimeSafe("lastRebirth");
					character.LastLogin = reader.GetDateTimeSafe("lastLogin");
					character.LastAging = reader.GetDateTimeSafe("lastAging");
					character.RebirthCount = reader.GetInt32("rebirthCount");

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

					bankWidth = reader.GetInt32("bankWidth");
					bankHeight = reader.GetInt32("bankHeight");
				}

				character.LoadDefault();
			}

			// Load items before quests, so we can check for the quest item
			this.GetCharacterItems(character);
			this.GetCharacterKeywords(character);
			this.GetCharacterTitles(character);
			this.GetCharacterSkills(character);
			this.GetCharacterQuests(character);

			// Add bank tab for characters
			// TODO: Bank tabs for pets?
			if (character.IsCharacter)
				this.AddBankTabForCharacter(account, character, bankWidth, bankHeight);

			// Add GM titles for the characters of authority 50+ accounts
			if (account != null)
			{
				if (account.Authority >= 50) character.Titles.Add(60000, TitleState.Usable); // GM
				if (account.Authority >= 99) character.Titles.Add(60001, TitleState.Usable); // devCAT
				//if (account.Authority >= 99) character.Titles.Add(60002, TitleState.Usable); // devDOG
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

			// Create bag pockets
			foreach (var item in items.Where(a => a.OptionInfo.LinkedPocketId != Pocket.None))
				character.Inventory.Add(new InventoryPocketNormal(item.OptionInfo.LinkedPocketId, item.Data.BagWidth, item.Data.BagHeight));

			// Add items
			foreach (var item in items)
			{
				// Ignore items that were in bags that don't exist anymore.
				if (item.Info.Pocket >= Pocket.ItemBags && item.Info.Pocket <= Pocket.ItemBagsMax && !character.Inventory.Has(item.Info.Pocket))
				{
					Log.Debug("GetCharacterItems: Item '{0}' ({1}) is inside a bag that hasn't been loaded yet.", item.Info.Id, item.EntityIdHex);
					continue;
				}

				// Try to add item
				if (!character.Inventory.InitAdd(item))
					Log.Error("GetCharacterItems: Unable to add item '{0}' ({1}) to inventory.", item.Info.Id, item.EntityId);
			}
		}

		/// <summary>
		/// Reads bank tab for character from db and adds it to account.
		/// </summary>
		/// <param name="account"></param>
		/// <param name="character"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		private void AddBankTabForCharacter(Account account, PlayerCreature character, int width, int height)
		{
			// Add tab
			var race = character.IsHuman ? BankTabRace.Human : character.IsElf ? BankTabRace.Elf : BankTabRace.Giant;
			account.Bank.AddTab(character.Name, race, width, height);

			// Read bank items
			var items = this.GetItems(character.CreatureId, true);
			foreach (var item in items)
			{
				if (!account.Bank.InitAdd(character.Name, item))
					Log.Error("AddBankTabFromCharacter: Unable to add item '{0}' ({1}) to bank tab '{2}'.", item.Info.Id, item.EntityId, character.Name);
			}
		}

		/// <summary>
		/// Returns list of items for creature with the given id.
		/// </summary>
		/// <remarks>
		/// TODO: This should be refactored to read all items in one go,
		///   instead of reading two or more separate lists.
		///   1) Make it "ReadItem"
		///   2) Handle the items in the actual item loading methods
		/// </remarks>
		/// <param name="creatureId"></param>
		/// <returns></returns>
		private List<Item> GetItems(long creatureId, bool bank = false)
		{
			var result = new List<Item>();

			using (var conn = this.Connection)
			{
				var query = "SELECT * FROM `items` WHERE `creatureId` = @creatureId";

				// Filter bank items
				if (bank)
					query += " AND `pocket` = 0 AND `bank` IS NOT NULL";
				else
					query += " AND `pocket` != 0 AND `bank` IS NULL";

				// Sort descending by linkedPocket to get bags first, they have
				// to be created before the items can be added.
				query += " ORDER BY `linkedPocket` DESC";

				using (var mc = new MySqlCommand(query, conn))
				{
					mc.Parameters.AddWithValue("@creatureId", creatureId);

					using (var reader = mc.ExecuteReader())
					{
						while (reader.Read())
						{
							var itemId = reader.GetInt32("itemId");
							var entityId = reader.GetInt64("entityId");

							var item = new Item(itemId, entityId);
							item.Bank = reader.GetStringSafe("bank");
							item.Info.Pocket = (Pocket)reader.GetInt32("pocket");
							item.Info.X = reader.GetInt32("x");
							item.Info.Y = reader.GetInt32("y");
							item.Info.Color1 = reader.GetUInt32("color1");
							item.Info.Color2 = reader.GetUInt32("color2");
							item.Info.Color3 = reader.GetUInt32("color3");
							item.Info.Amount = reader.GetUInt16("amount");
							item.Info.State = reader.GetByte("state");
							item.Info.FigureB = reader.GetByte("figureB");
							item.OptionInfo.Price = reader.GetInt32("price");
							item.OptionInfo.SellingPrice = reader.GetInt32("sellPrice");
							item.OptionInfo.Durability = reader.GetInt32("durability");
							item.OptionInfo.DurabilityMax = reader.GetInt32("durabilityMax");
							item.OptionInfo.DurabilityOriginal = reader.GetInt32("durabilityOriginal");
							item.OptionInfo.AttackMin = reader.GetUInt16("attackMin");
							item.OptionInfo.AttackMax = reader.GetUInt16("attackMax");
							item.OptionInfo.InjuryMin = reader.GetUInt16("injuryMin");
							item.OptionInfo.InjuryMax = reader.GetUInt16("injuryMax");
							item.OptionInfo.Balance = reader.GetByte("balance");
							item.OptionInfo.Critical = reader.GetSByte("critical");
							item.OptionInfo.Defense = reader.GetInt32("defense");
							item.OptionInfo.Protection = reader.GetInt16("protection");
							item.OptionInfo.EffectiveRange = reader.GetInt16("range");
							item.OptionInfo.AttackSpeed = (AttackSpeed)reader.GetByte("attackSpeed");
							item.Proficiency = reader.GetInt32("experience");
							item.OptionInfo.Upgraded = reader.GetByte("upgrades");
							item.MetaData1.Parse(reader.GetStringSafe("meta1"));
							item.MetaData2.Parse(reader.GetStringSafe("meta2"));
							item.OptionInfo.LinkedPocketId = (Pocket)reader.GetByte("linkedPocket");
							item.OptionInfo.Flags = (ItemFlags)reader.GetByte("flags");
							item.OptionInfo.Prefix = reader.GetUInt16("prefix");
							item.OptionInfo.Suffix = reader.GetUInt16("suffix");

							result.Add(item);
						}
					}
				}

				// Load ego data
				using (var mc = new MySqlCommand("SELECT * FROM `egos` WHERE itemEntityId = @itemEntityId", conn))
				{
					foreach (var item in result.Where(item => item.Data.HasTag("/ego_weapon/")))
					{
						mc.Parameters.AddWithValue("@itemEntityId", item.EntityId);

						using (var reader = mc.ExecuteReader())
						{
							if (!reader.Read())
							{
								Log.Warning("ChannelDb.GetItems: No ego data for '{0}'.", item.EntityIdHex);
								continue;
							}

							item.EgoInfo.Race = (EgoRace)reader.GetByte("egoRace");
							item.EgoInfo.Name = reader.GetStringSafe("name");
							item.EgoInfo.StrLevel = reader.GetByte("strLevel");
							item.EgoInfo.StrExp = reader.GetInt32("strExp");
							item.EgoInfo.IntLevel = reader.GetByte("intLevel");
							item.EgoInfo.IntExp = reader.GetInt32("intExp");
							item.EgoInfo.DexLevel = reader.GetByte("dexLevel");
							item.EgoInfo.DexExp = reader.GetInt32("dexExp");
							item.EgoInfo.WillLevel = reader.GetByte("willLevel");
							item.EgoInfo.WillExp = reader.GetInt32("willExp");
							item.EgoInfo.LuckLevel = reader.GetByte("luckLevel");
							item.EgoInfo.LuckExp = reader.GetInt32("luckExp");
							item.EgoInfo.SocialLevel = reader.GetByte("socialLevel");
							item.EgoInfo.SocialExp = reader.GetInt32("socialExp");
							item.EgoInfo.AwakeningEnergy = reader.GetByte("awakeningEnergy");
							item.EgoInfo.AwakeningExp = reader.GetInt32("awakeningExp");
							item.EgoInfo.LastFeeding = reader.GetDateTimeSafe("lastFeeding");
						}

						mc.Parameters.Clear();
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
			using (var conn = this.Connection)
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

			if (character is Character)
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
			using (var conn = this.Connection)
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
			using (var conn = this.Connection)
			using (var mc = new MySqlCommand("SELECT * FROM `skills` WHERE `creatureId` = @creatureId", conn))
			{
				mc.Parameters.AddWithValue("@creatureId", character.CreatureId);

				using (var reader = mc.ExecuteReader())
				{
					while (reader.Read())
					{
						var skillId = (SkillId)reader.GetInt32("skillId");
						var rank = (SkillRank)reader.GetByte("rank");

						var skill = new Skill(character, skillId, rank, character.Race);
						skill.Info.ConditionCount1 = reader.GetInt16("condition1");
						skill.Info.ConditionCount2 = reader.GetInt16("condition2");
						skill.Info.ConditionCount3 = reader.GetInt16("condition3");
						skill.Info.ConditionCount4 = reader.GetInt16("condition4");
						skill.Info.ConditionCount5 = reader.GetInt16("condition5");
						skill.Info.ConditionCount6 = reader.GetInt16("condition6");
						skill.Info.ConditionCount7 = reader.GetInt16("condition7");
						skill.Info.ConditionCount8 = reader.GetInt16("condition8");
						skill.Info.ConditionCount9 = reader.GetInt16("condition9");
						skill.UpdateExperience();

						character.Skills.Add(skill);
					}
				}
			}

			// Add skills if they don't exist yet. Everybody gets
			// Combat Mastery, only normal characters get all the
			// hidden ones for now
			// TODO: Move to race skill db.
			character.Skills.Add(SkillId.CombatMastery, SkillRank.RF, character.Race);
			// According to the Wiki you get Crit upon advancing CM to RF, should CM be Novice?
			character.Skills.Add(SkillId.CriticalHit, SkillRank.Novice, character.Race);
			if (character is Character)
			{
				character.Skills.Add(SkillId.HiddenEnchant, SkillRank.Novice, character.Race);
				character.Skills.Add(SkillId.HiddenResurrection, SkillRank.Novice, character.Race);
				character.Skills.Add(SkillId.HiddenTownBack, SkillRank.Novice, character.Race);
				character.Skills.Add(SkillId.HiddenGuildStoneSetting, SkillRank.Novice, character.Race);
				character.Skills.Add(SkillId.HiddenBlessing, SkillRank.Novice, character.Race);
				character.Skills.Add(SkillId.CampfireKit, SkillRank.Novice, character.Race);
				character.Skills.Add(SkillId.SkillUntrainKit, SkillRank.Novice, character.Race);
				character.Skills.Add(SkillId.BigBlessingWaterKit, SkillRank.Novice, character.Race);
				character.Skills.Add(SkillId.Dye, SkillRank.Novice, character.Race);
				character.Skills.Add(SkillId.EnchantElementalAllSlot, SkillRank.Novice, character.Race);
				character.Skills.Add(SkillId.HiddenPoison, SkillRank.Novice, character.Race);
				character.Skills.Add(SkillId.HiddenBomb, SkillRank.Novice, character.Race);
				character.Skills.Add(SkillId.FossilRestoration, SkillRank.Novice, character.Race);
				character.Skills.Add(SkillId.SeesawJump, SkillRank.Novice, character.Race);
				character.Skills.Add(SkillId.SeesawCreate, SkillRank.Novice, character.Race);
				character.Skills.Add(SkillId.DragonSupport, SkillRank.Novice, character.Race);
				character.Skills.Add(SkillId.IceMine, SkillRank.Novice, character.Race);
				character.Skills.Add(SkillId.Scan, SkillRank.Novice, character.Race);
				character.Skills.Add(SkillId.UseSupportItem, SkillRank.Novice, character.Race);
				character.Skills.Add(SkillId.TickingQuizBomb, SkillRank.Novice, character.Race);
				character.Skills.Add(SkillId.ItemSeal, SkillRank.Novice, character.Race);
				character.Skills.Add(SkillId.ItemUnseal, SkillRank.Novice, character.Race);
				character.Skills.Add(SkillId.ItemDungeonPass, SkillRank.Novice, character.Race);
				character.Skills.Add(SkillId.UseElathaItem, SkillRank.Novice, character.Race);
				character.Skills.Add(SkillId.UseMorrighansFeather, SkillRank.Novice, character.Race);
				character.Skills.Add(SkillId.PetBuffing, SkillRank.Novice, character.Race);
				character.Skills.Add(SkillId.CherryTreeKit, SkillRank.Novice, character.Race);
				character.Skills.Add(SkillId.ThrowConfetti, SkillRank.Novice, character.Race);
				character.Skills.Add(SkillId.UsePartyPopper, SkillRank.Novice, character.Race);
				character.Skills.Add(SkillId.HammerGame, SkillRank.Novice, character.Race);
				character.Skills.Add(SkillId.SpiritShift, SkillRank.Novice, character.Race);
				character.Skills.Add(SkillId.EmergencyEscapeBomb, SkillRank.Novice, character.Race);
				character.Skills.Add(SkillId.EmergencyIceBomb, SkillRank.Novice, character.Race);
				character.Skills.Add(SkillId.NameColorChange, SkillRank.Novice, character.Race);
				character.Skills.Add(SkillId.HolyFlame, SkillRank.Novice, character.Race);
				character.Skills.Add(SkillId.CreateFaliasPortal, SkillRank.Novice, character.Race);
				character.Skills.Add(SkillId.UseItemChattingColorChange, SkillRank.Novice, character.Race);
				character.Skills.Add(SkillId.InstallPrivateFarmFacility, SkillRank.Novice, character.Race);
				character.Skills.Add(SkillId.ReorientHomesteadbuilding, SkillRank.Novice, character.Race);
				character.Skills.Add(SkillId.GachaponSynthesis, SkillRank.Novice, character.Race);
				character.Skills.Add(SkillId.MakeChocoStatue, SkillRank.Novice, character.Race);
				character.Skills.Add(SkillId.Paint, SkillRank.Novice, character.Race);
				character.Skills.Add(SkillId.MixPaint, SkillRank.Novice, character.Race);
				character.Skills.Add(SkillId.PetSealToItem, SkillRank.Novice, character.Race);
				character.Skills.Add(SkillId.FlownHotAirBalloon, SkillRank.Novice, character.Race);
				character.Skills.Add(SkillId.ItemSeal2, SkillRank.Novice, character.Race);
				character.Skills.Add(SkillId.CureZombie, SkillRank.Novice, character.Race);
				character.Skills.Add(SkillId.ContinentWarp, SkillRank.Novice, character.Race);
				character.Skills.Add(SkillId.AddSeasoning, SkillRank.Novice, character.Race);
				character.Skills.Add(SkillId.Gathering, SkillRank.Novice, character.Race);
			}
		}

		/// <summary>
		/// Reads all quests of character from db.
		/// </summary>
		/// <param name="character"></param>
		public void GetCharacterQuests(PlayerCreature character)
		{
			using (var conn = this.Connection)
			{
				using (var mc = new MySqlCommand("SELECT * FROM `quests` WHERE `creatureId` = @creatureId", conn))
				{
					mc.Parameters.AddWithValue("@creatureId", character.CreatureId);

					using (var reader = mc.ExecuteReader())
					{
						while (reader.Read())
						{
							try
							{
								var uniqueId = reader.GetInt64("questIdUnique");
								var id = reader.GetInt32("questId");
								var state = (QuestState)reader.GetInt32("state");
								var itemEntityId = reader.GetInt64("itemEntityId");
								var metaData = reader.GetStringSafe("metaData");

								var quest = new Quest(id, uniqueId, state, metaData);

								if (quest.State == QuestState.InProgress)
								{
									// Don't add quest if quest item is missing
									quest.QuestItem = character.Inventory.GetItem(itemEntityId);
									if (quest.QuestItem == null)
									{
										Log.Error("Db.GetCharacterQuests: Unable to find quest item for '{0}'.", quest.Id);
										continue;
									}

									quest.QuestItem.QuestId = quest.UniqueId;
								}

								character.Quests.Add(quest);
							}
							catch (Exception ex)
							{
								Log.Warning(ex.Message);
							}
						}
					}
				}
				using (var mc = new MySqlCommand("SELECT * FROM `quest_progress` WHERE `creatureId` = @creatureId", conn))
				{
					mc.Parameters.AddWithValue("@creatureId", character.CreatureId);

					using (var reader = mc.ExecuteReader())
					{
						while (reader.Read())
						{
							var uniqueId = reader.GetInt64("questIdUnique");
							var objective = reader.GetStringSafe("objective");

							var quest = character.Quests.Get(uniqueId);
							if (quest == null)
							{
								Log.Error("Db.GetCharacterQuests: Unable to find quest for objective '{0}'.", objective);
								continue;
							}

							var progress = quest.GetProgress(objective);
							if (progress == null)
							{
								Log.Error("Db.GetCharacterQuests: Unable to find objective '{0}' for quest '{1}'.", objective, quest.Id);
								continue;
							}

							progress.Count = reader.GetInt32("count");
							progress.Done = reader.GetBoolean("done");
							progress.Unlocked = reader.GetBoolean("unlocked");
						}
					}
				}
				using (var mc = new MySqlCommand("SELECT * FROM `ptj` WHERE `creatureId` = @creatureId", conn))
				{
					mc.Parameters.AddWithValue("@creatureId", character.CreatureId);

					using (var reader = mc.ExecuteReader())
					{
						while (reader.Read())
						{
							var type = (PtjType)reader.GetInt32("type");
							var done = reader.GetInt32("done");
							var success = reader.GetInt32("success");
							var lastChange = reader.GetDateTimeSafe("lastChange");

							// Reduce done by 1 for each day after the third
							var daysSinceChange = (int)(DateTime.Now - lastChange).TotalDays;
							var forgetDays = Math.Max(0, daysSinceChange - 3);

							// Make NPCs "forget", cap at 1 job done
							if (forgetDays > 0)
							{
								done = Math.Max(1, done - forgetDays);

								// Set last change to 3 days ago, to keep forgetting,
								// starting tomorrow.
								lastChange = DateTime.Now.AddDays(-3);
							}

							// Adjust success
							if (done < success)
								success = done;

							// Add record
							var record = character.Quests.GetPtjTrackRecord(type);
							record.Done = done;
							record.Success = success;
							record.LastChange = lastChange;
						}
					}
				}
			}
		}

		public void LogSecurityIncident(ChannelClient client, IncidentSeverityLevel level, string report, string stacktrace)
		{
			if (client == null || client.Account == null)
				return; // TODO: log?

			using (var conn = this.Connection)
			using (var cmd = new InsertCommand("INSERT INTO `log_security` {0}", conn))
			{
				cmd.Set("accountId", client.Account.Id);
				cmd.Set("characterId", client.Controlling == null ? null : (long?)(client.Controlling.EntityId));
				cmd.Set("ipAddress", client.Address);
				cmd.Set("date", DateTime.Now);
				cmd.Set("level", (int)level);
				cmd.Set("report", report);
				cmd.Set("stacktrace", stacktrace);

				cmd.Execute();
			}
		}

		/// <summary>
		/// Saves all quests of character.
		/// </summary>
		/// <param name="character"></param>
		public void SaveQuests(PlayerCreature character)
		{
			using (var conn = this.Connection)
			using (var transaction = conn.BeginTransaction())
			{
				// Delete quests
				using (var mc = new MySqlCommand("DELETE FROM `quests` WHERE `creatureId` = @creatureId", conn, transaction))
				{
					mc.Parameters.AddWithValue("@creatureId", character.CreatureId);
					mc.ExecuteNonQuery();
				}

				// Delete progress
				using (var mc = new MySqlCommand("DELETE FROM `quest_progress` WHERE `creatureId` = @creatureId", conn, transaction))
				{
					mc.Parameters.AddWithValue("@creatureId", character.CreatureId);
					mc.ExecuteNonQuery();
				}

				// Delete PTJ records
				using (var mc = new MySqlCommand("DELETE FROM `ptj` WHERE `creatureId` = @creatureId", conn, transaction))
				{
					mc.Parameters.AddWithValue("@creatureId", character.CreatureId);
					mc.ExecuteNonQuery();
				}

				// Add everything
				foreach (var quest in character.Quests.GetList())
				{
					if (quest.State == QuestState.InProgress && !character.Inventory.Has(quest.QuestItem))
					{
						Log.Warning("Db.SaveQuests: Missing '{0}'s quest item for '{1}'.", character.Name, quest.Id);
						continue;
					}

					// Don't save PTJs, fail them
					if (quest.Data.Type == QuestType.Deliver)
					{
						if (quest.State != QuestState.Complete)
							character.Quests.ModifyPtjTrackRecord(quest.Data.PtjType, +1, 0);
						continue;
					}

					using (var cmd = new InsertCommand("INSERT INTO `quests` {0}", conn, transaction))
					{
						if (quest.UniqueId < MabiId.QuestsTmp)
							cmd.Set("questIdUnique", quest.UniqueId);
						cmd.Set("creatureId", character.CreatureId);
						cmd.Set("questId", quest.Id);
						cmd.Set("state", (int)quest.State);
						cmd.Set("itemEntityId", (quest.State == QuestState.InProgress ? quest.QuestItem.EntityId : 0));
						cmd.Set("metaData", quest.MetaData);

						cmd.Execute();

						if (quest.UniqueId >= MabiId.QuestsTmp)
							quest.UniqueId = cmd.LastId;
					}

					foreach (var objective in quest.GetList())
					{
						using (var cmd = new InsertCommand("INSERT INTO `quest_progress` {0}", conn, transaction))
						{
							cmd.Set("creatureId", character.CreatureId);
							cmd.Set("questIdUnique", quest.UniqueId);
							cmd.Set("objective", objective.Ident);
							cmd.Set("count", objective.Count);
							cmd.Set("done", objective.Done);
							cmd.Set("unlocked", objective.Unlocked);
							cmd.Execute();
						}
					}
				}

				foreach (var record in character.Quests.GetPtjTrackRecords())
				{
					if (record.Done == 0)
						continue;

					using (var cmd = new InsertCommand("INSERT INTO `ptj` {0}", conn, transaction))
					{
						cmd.Set("creatureId", character.CreatureId);
						cmd.Set("type", (int)record.Type);
						cmd.Set("done", record.Done);
						cmd.Set("success", record.Success);
						cmd.Set("lastChange", record.LastChange);
						cmd.Execute();
					}
				}

				transaction.Commit();
			}
		}

		/// <summary>
		/// Saves account, incl. all character data.
		/// </summary>
		/// <param name="account"></param>
		public void SaveAccount(Account account)
		{
			using (var conn = this.Connection)
			using (var cmd = new UpdateCommand("UPDATE `accounts` SET {0} WHERE `accountId` = @accountId", conn))
			{
				cmd.AddParameter("@accountId", account.Id);
				cmd.Set("authority", (byte)account.Authority);
				cmd.Set("lastlogin", account.LastLogin);
				cmd.Set("banReason", account.BanReason);
				cmd.Set("banExpiration", account.BanExpiration);
				cmd.Set("autobanCount", account.AutobanCount);
				cmd.Set("autobanScore", account.AutobanScore);
				cmd.Set("lastAutobanReduction", account.LastAutobanReduction);
				cmd.Set("bankGold", account.Bank.Gold);

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
		/// <param name="account"></param>
		public void SaveCharacter(PlayerCreature creature, Account account)
		{
			using (var conn = this.Connection)
			using (var cmd = new UpdateCommand("UPDATE `creatures` SET {0} WHERE `creatureId` = @creatureId", conn))
			{
				var characterLocation = creature.GetPosition();

				cmd.AddParameter("@creatureId", creature.CreatureId);
				cmd.Set("skinColor", creature.SkinColor);
				cmd.Set("eyeType", creature.EyeType);
				cmd.Set("eyeColor", creature.EyeColor);
				cmd.Set("mouthType", creature.MouthType);
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
				cmd.Set("levelTotal", creature.TotalLevel);
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
				cmd.Set("state", (uint)creature.State);
				cmd.Set("age", creature.Age);
				cmd.Set("rebirthCount", creature.RebirthCount);

				cmd.Set("lastAging", creature.LastAging);
				if (creature.LastRebirth != DateTime.MinValue)
					cmd.Set("lastRebirth", creature.LastRebirth);
				if (creature.LastLogin != DateTime.MinValue)
					cmd.Set("lastLogin", creature.LastLogin);

				cmd.Execute();
			}

			this.SaveCharacterItems(creature);
			this.SaveQuests(creature);
			this.SaveCharacterKeywords(creature);
			this.SaveCharacterTitles(creature);
			this.SaveCharacterSkills(creature);
			//this.SaveCharacterCooldowns(creature);

			this.SaveVars(account.Id, creature.CreatureId, creature.Vars.Perm);
		}

		/// <summary>
		/// Writes all of creature's keywords to the database.
		/// </summary>
		/// <param name="creature"></param>
		private void SaveCharacterKeywords(PlayerCreature creature)
		{
			using (var conn = this.Connection)
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
			using (var conn = this.Connection)
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
					// TODO: Are enough titles affected to justify a column
					//   in the db for this?
					if (title.Key == 60000 || title.Key == 60001 || title.Key == 60002 || title.Key == 50000) // GM, devCAT, Guild
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
			using (var conn = this.Connection)
			using (var transaction = conn.BeginTransaction())
			{
				using (var mc = new MySqlCommand("DELETE FROM `items` WHERE `creatureId` = @creatureId", conn, transaction))
				{
					mc.Parameters.AddWithValue("@creatureId", creature.CreatureId);
					mc.ExecuteNonQuery();
				}

				var items = creature.Inventory.Items.Union(creature.Client.Account.Bank.GetTabItems(creature.Name));
				foreach (var item in items)
				{
					using (var cmd = new InsertCommand("INSERT INTO `items` {0}", conn, transaction))
					{
						cmd.Set("creatureId", creature.CreatureId);
						if (item.EntityId < MabiId.TmpItems)
							cmd.Set("entityId", item.EntityId);
						cmd.Set("itemId", item.Info.Id);
						cmd.Set("bank", item.Bank);
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
						cmd.Set("figureB", item.Info.FigureB);
						cmd.Set("durability", item.OptionInfo.Durability);
						cmd.Set("durabilityMax", item.OptionInfo.DurabilityMax);
						cmd.Set("durabilityOriginal", item.OptionInfo.DurabilityOriginal);
						cmd.Set("attackMin", item.OptionInfo.AttackMin);
						cmd.Set("attackMax", item.OptionInfo.AttackMax);
						cmd.Set("injuryMin", item.OptionInfo.InjuryMin);
						cmd.Set("injuryMax", item.OptionInfo.InjuryMax);
						cmd.Set("balance", item.OptionInfo.Balance);
						cmd.Set("critical", item.OptionInfo.Critical);
						cmd.Set("defense", item.OptionInfo.Defense);
						cmd.Set("protection", item.OptionInfo.Protection);
						cmd.Set("range", item.OptionInfo.EffectiveRange);
						cmd.Set("attackSpeed", (byte)item.OptionInfo.AttackSpeed);
						cmd.Set("experience", item.Proficiency);
						cmd.Set("upgrades", item.OptionInfo.Upgraded);
						cmd.Set("meta1", item.MetaData1.ToString());
						cmd.Set("meta2", item.MetaData2.ToString());
						cmd.Set("flags", (byte)item.OptionInfo.Flags);
						cmd.Set("prefix", item.OptionInfo.Prefix);
						cmd.Set("suffix", item.OptionInfo.Suffix);

						cmd.Execute();

						if (item.EntityId >= MabiId.TmpItems)
							item.EntityId = cmd.LastId;
					}

					// Save ego data
					if (item.Data.HasTag("/ego_weapon/"))
					{
						using (var cmd = new InsertCommand("INSERT INTO `egos` {0}", conn, transaction))
						{
							cmd.Set("itemEntityId", item.EntityId);
							cmd.Set("egoRace", (byte)item.EgoInfo.Race);
							cmd.Set("name", item.EgoInfo.Name);
							cmd.Set("strLevel", item.EgoInfo.StrLevel);
							cmd.Set("strExp", item.EgoInfo.StrExp);
							cmd.Set("intLevel", item.EgoInfo.IntLevel);
							cmd.Set("intExp", item.EgoInfo.IntExp);
							cmd.Set("dexLevel", item.EgoInfo.DexLevel);
							cmd.Set("dexExp", item.EgoInfo.DexExp);
							cmd.Set("willLevel", item.EgoInfo.WillLevel);
							cmd.Set("willExp", item.EgoInfo.WillExp);
							cmd.Set("luckLevel", item.EgoInfo.LuckLevel);
							cmd.Set("luckExp", item.EgoInfo.LuckExp);
							cmd.Set("socialLevel", item.EgoInfo.SocialLevel);
							cmd.Set("socialExp", item.EgoInfo.SocialExp);
							cmd.Set("awakeningEnergy", item.EgoInfo.AwakeningEnergy);
							cmd.Set("awakeningExp", item.EgoInfo.AwakeningExp);
							cmd.Set("lastFeeding", item.EgoInfo.LastFeeding);

							cmd.Execute();
						}
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
			using (var conn = this.Connection)
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
						cmd.Set("condition1", skill.Info.ConditionCount1);
						cmd.Set("condition2", skill.Info.ConditionCount2);
						cmd.Set("condition3", skill.Info.ConditionCount3);
						cmd.Set("condition4", skill.Info.ConditionCount4);
						cmd.Set("condition5", skill.Info.ConditionCount5);
						cmd.Set("condition6", skill.Info.ConditionCount6);
						cmd.Set("condition7", skill.Info.ConditionCount7);
						cmd.Set("condition8", skill.Info.ConditionCount8);
						cmd.Set("condition9", skill.Info.ConditionCount9);

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
			using (var conn = this.Connection)
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
							case "dt": vars[name] = DateTime.Parse(val); break;
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
			using (var conn = this.Connection)
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
					else if (var.Value is DateTime) type = "dt";
					else type = "o";

					// Get value
					var val = string.Empty;
					if (type == "o")
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
					else if (type == "dt")
					{
						val = var.Value.ToString();
					}
					else
					{
						val = var.Value.ToString();
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

		/// <summary>
		/// Returns true if items with temp ids are found in the db.
		/// </summary>
		/// <returns></returns>
		public bool TmpItemsExist()
		{
			using (var conn = this.Connection)
			using (var mc = new MySqlCommand("SELECT itemId FROM `items` WHERE `entityId` >= @entityId", conn))
			{
				mc.Parameters.AddWithValue("@entityId", MabiId.TmpItems);
				using (var reader = mc.ExecuteReader())
					return reader.HasRows;
			}
		}

		/// <summary>
		/// Returns coupon or null. 
		/// </summary>
		/// <param name="code"></param>
		/// <returns></returns>
		public string GetCouponScript(string code)
		{
			using (var conn = this.Connection)
			using (var mc = new MySqlCommand("SELECT * FROM `coupons` WHERE `code` = @code AND (`expiration` IS NULL OR `expiration` > NOW()) AND NOT `used`", conn))
			{
				mc.Parameters.AddWithValue("@code", code);

				using (var reader = mc.ExecuteReader())
				{
					if (reader.HasRows)
					{
						reader.Read();
						return reader.GetStringSafe("script");
					}
					else
					{
						return null;
					}
				}
			}
		}

		/// <summary>
		/// Sets coupon to used, so it can't be redeemed anymore.
		/// </summary>
		/// <param name="code"></param>
		/// <returns></returns>
		public bool UseCoupon(string code)
		{
			using (var conn = this.Connection)
			using (var cmd = new UpdateCommand("UPDATE `coupons` SET {0} WHERE `code` = @code", conn))
			{
				cmd.AddParameter("@code", code);
				cmd.Set("used", true);

				return (cmd.Execute() > 0);
			}
		}
	}
}
