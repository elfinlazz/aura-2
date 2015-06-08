// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Aura.Mabi.Const;

namespace Aura.Data.Database
{
	[Serializable]
	public class RaceData : TaggableData
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Group { get; set; }
		public Gender Gender { get; set; }

		public uint DefaultState { get; set; }
		public int VehicleType { get; set; }
		public float RunSpeedFactor { get; set; }

		public int ElementPhysical { get; set; }
		public int ElementLightning { get; set; }
		public int ElementFire { get; set; }
		public int ElementIce { get; set; }

		public float SizeMin { get; set; }
		public float SizeMax { get; set; }
		public uint Color1 { get; set; }
		public uint Color2 { get; set; }
		public uint Color3 { get; set; }
		public RaceFaceData Face { get; set; }

		public float RunningSpeed { get; set; }
		public float WalkingSpeed { get; set; }

		public int InventoryWidth { get; set; }
		public int InventoryHeight { get; set; }

		public short AttackSkill { get; set; }
		public int AttackRange { get; set; }
		public int AttackMinBase { get; set; } // Monster
		public int AttackMaxBase { get; set; } // Monster
		public int AttackMinBaseMod { get; set; } // Race
		public int AttackMaxBaseMod { get; set; } // Race
		public int InjuryMinBase { get; set; } // Monster
		public int InjuryMaxBase { get; set; } // Monster
		public int InjuryMinBaseMod { get; set; } // Race
		public int InjuryMaxBaseMod { get; set; } // Race
		public int AttackSpeed { get; set; }
		public int KnockCount { get; set; }
		public int CriticalBase { get; set; } // Monster
		public int CriticalBaseMod { get; set; } // Race
		public int BalanceBase { get; set; } // Monster
		public int BalanceBaseMod { get; set; } // Race
		public int SplashRadius { get; set; }
		public int SplashAngle { get; set; }
		public float SplashDamage { get; set; }
		public RaceStands Stand { get; set; }

		public int Level { get; set; }
		public float Str { get; set; }
		public float Int { get; set; }
		public float Dex { get; set; }
		public float Will { get; set; }
		public float Luck { get; set; }

		public string AI { get; set; }
		public float CombatPower { get; set; }
		public float Life { get; set; }
		public float Mana { get; set; }
		public float Stamina { get; set; }
		public int Defense { get; set; }
		public int Protection { get; set; }
		public int Exp { get; set; }

		public int GoldMin { get; set; }
		public int GoldMax { get; set; }

		public List<DropData> Drops { get; set; }
		public List<RaceSkillData> Skills { get; set; }
		public List<RaceItemData> Equip { get; set; }

		public RaceData()
		{
			this.Drops = new List<DropData>();
			this.Skills = new List<RaceSkillData>();
			this.Equip = new List<RaceItemData>();
			this.Face = new RaceFaceData();
		}

		public bool Is(RaceStands stand)
		{
			return (this.Stand & stand) != 0;
		}
	}

	[Serializable]
	public class DropData
	{
		public int ItemId { get; set; }
		public float Chance { get; set; }
		public int AmountMin { get; set; }
		public int AmountMax { get; set; }
		public int Prefix { get; set; }
		public int Suffix { get; set; }
		public uint? Color1 { get; set; }
		public uint? Color2 { get; set; }
		public uint? Color3 { get; set; }
		public int Expires { get; set; }
		public int Durability { get; set; }

		public DropData()
		{
		}

		public DropData(int itemId, float chance, int amount = 0, int amountMin = 0, int amountMax = 0, uint? color1 = null, uint? color2 = null, uint? color3 = null, int prefix = 0, int suffix = 0, int expires = 0, int durability = -1)
		{
			if (amount != 0)
				amountMin = amountMax = amount;
			if (amountMax < amountMin)
				amountMax = amountMin;

			this.ItemId = itemId;
			this.Chance = chance;
			this.AmountMin = amountMin;
			this.AmountMax = amountMax;
			this.Prefix = prefix;
			this.Suffix = suffix;
			this.Color1 = color1;
			this.Color2 = color2;
			this.Color3 = color3;
			this.Expires = expires;
			this.Durability = durability;
		}

		public DropData Copy()
		{
			var result = new DropData();

			result.ItemId = this.ItemId;
			result.Chance = this.Chance;
			result.AmountMin = this.AmountMin;
			result.AmountMax = this.AmountMax;
			result.Prefix = this.Prefix;
			result.Suffix = this.Suffix;
			result.Color1 = this.Color1;
			result.Color2 = this.Color2;
			result.Color3 = this.Color3;
			result.Expires = this.Expires;
			result.Durability = this.Durability;

			return result;
		}
	}

	[Serializable]
	public class RaceSkillData
	{
		public int RaceId { get; set; }
		public ushort SkillId { get; set; }
		public byte Rank { get; set; }
	}

	[Serializable]
	public class RaceItemData
	{
		public List<int> ItemIds { get; set; }
		public int Pocket { get; set; }
		public List<uint> Color1s { get; set; }
		public List<uint> Color2s { get; set; }
		public List<uint> Color3s { get; set; }

		public RaceItemData()
		{
			this.ItemIds = new List<int>();
			this.Color1s = new List<uint>();
			this.Color2s = new List<uint>();
			this.Color3s = new List<uint>();
		}

		public int GetRandomId(Random rnd)
		{
			return this.ItemIds[rnd.Next(this.ItemIds.Count)];
		}

		public uint GetRandomColor1(Random rnd)
		{
			if (this.Color1s.Count == 0)
				return uint.MaxValue;

			return this.Color1s[rnd.Next(this.Color1s.Count)];
		}

		public uint GetRandomColor2(Random rnd)
		{
			if (this.Color2s.Count == 0)
				return uint.MaxValue;

			return this.Color2s[rnd.Next(this.Color2s.Count)];
		}

		public uint GetRandomColor3(Random rnd)
		{
			if (this.Color3s.Count == 0)
				return uint.MaxValue;

			return this.Color3s[rnd.Next(this.Color3s.Count)];
		}
	}

	[Serializable]
	public class RaceFaceData
	{
		public List<int> EyeColors { get; set; }
		public List<int> EyeTypes { get; set; }
		public List<int> MouthTypes { get; set; }
		public List<int> SkinColors { get; set; }

		public RaceFaceData()
		{
			this.EyeColors = new List<int>();
			this.EyeTypes = new List<int>();
			this.MouthTypes = new List<int>();
			this.SkinColors = new List<int>();
		}

		public int GetRandomEyeColor(Random rnd)
		{
			if (this.EyeColors.Count == 0)
				return 0;

			return this.EyeColors[rnd.Next(this.EyeColors.Count)];
		}

		public int GetRandomEyeType(Random rnd)
		{
			if (this.EyeTypes.Count == 0)
				return 0;

			return this.EyeTypes[rnd.Next(this.EyeTypes.Count)];
		}

		public int GetRandomMouthType(Random rnd)
		{
			if (this.MouthTypes.Count == 0)
				return 0;

			return this.MouthTypes[rnd.Next(this.MouthTypes.Count)];
		}

		public int GetRandomSkinColor(Random rnd)
		{
			if (this.SkinColors.Count == 0)
				return 0;

			return this.SkinColors[rnd.Next(this.SkinColors.Count)];
		}
	}

	[Flags]
	public enum RaceStands : int
	{
		KnockBackable = 0x01,
		KnockDownable = 0x02,
	}

	/// <summary>
	/// Indexed by race id.
	/// Depends on: SpeedDb, FlightDb, RaceSkillDb
	/// </summary>
	public class RaceDb : DatabaseJsonIndexed<int, RaceData>
	{
		public List<RaceData> FindAll(string name)
		{
			name = name.ToLower();
			return this.Entries.FindAll(a => a.Value.Name.ToLower().Contains(name));
		}

		protected override void ReadEntry(JObject entry)
		{
			entry.AssertNotMissing("id", "name", "group", "tags", "gender", "vehicleType", "runSpeedFactor", "state", "invWidth", "invHeight", "range", "attackSpeed", "knockCount", "splashRadius", "splashAngle", "splashDamage", "stand", "ai", "color1", "color2", "color3", "cp", "life");

			var raceData = new RaceData();
			raceData.Id = entry.ReadInt("id");
			raceData.Name = entry.ReadString("name");
			raceData.Group = entry.ReadString("group");
			raceData.Tags = entry.ReadString("tags");
			raceData.Gender = (Gender)entry.ReadByte("gender");
			raceData.VehicleType = entry.ReadInt("vehicleType");
			raceData.RunSpeedFactor = entry.ReadFloat("runSpeedFactor");
			raceData.DefaultState = entry.ReadUInt("state");
			raceData.InventoryWidth = entry.ReadInt("invWidth");
			raceData.InventoryHeight = entry.ReadInt("invHeight");
			raceData.AttackSkill = 23002; // Combat Mastery, they all use this anyway.
			raceData.AttackMinBase = entry.ReadInt("attackMinBase");
			raceData.AttackMaxBase = entry.ReadInt("attackMaxBase");
			raceData.AttackMinBaseMod = entry.ReadInt("attackMinBaseMod");
			raceData.AttackMaxBaseMod = entry.ReadInt("attackMaxBaseMod");
			raceData.InjuryMinBase = entry.ReadInt("injuryMinBase");
			raceData.InjuryMaxBase = entry.ReadInt("injuryMaxBase");
			raceData.InjuryMinBaseMod = entry.ReadInt("injuryMinBaseMod");
			raceData.InjuryMaxBaseMod = entry.ReadInt("injuryMaxBaseMod");
			raceData.AttackRange = entry.ReadInt("range");
			raceData.AttackSpeed = entry.ReadInt("attackSpeed");
			raceData.KnockCount = entry.ReadInt("knockCount");
			raceData.CriticalBase = entry.ReadInt("criticalBase");
			raceData.CriticalBaseMod = entry.ReadInt("criticalBaseMod");
			raceData.BalanceBase = entry.ReadInt("balanceBase");
			raceData.BalanceBaseMod = entry.ReadInt("balanceBaseMod");
			raceData.SplashRadius = entry.ReadInt("splashRadius");
			raceData.SplashAngle = entry.ReadInt("splashAngle");
			raceData.SplashDamage = entry.ReadFloat("splashDamage");
			raceData.Stand = (RaceStands)entry.ReadInt("stand");
			raceData.AI = entry.ReadString("ai");

			// Looks
			raceData.Color1 = entry.ReadUInt("color1");
			raceData.Color2 = entry.ReadUInt("color2");
			raceData.Color3 = entry.ReadUInt("color3");

			if (!entry.ContainsAnyKeys("size", "sizeMin", "sizeMax"))
				raceData.SizeMin = raceData.SizeMax = 1;
			else if (entry.ContainsKey("size"))
				raceData.SizeMin = raceData.SizeMax = entry.ReadFloat("size");
			else
			{
				raceData.SizeMin = entry.ReadFloat("sizeMin", 1);
				raceData.SizeMax = entry.ReadFloat("sizeMax", 1);
			}

			if (raceData.SizeMin > raceData.SizeMax)
				raceData.SizeMax = raceData.SizeMin;

			// Face
			Action<string, JObject, List<int>> readArrOrIntCol = (col, obj, list) =>
			{
				if (obj[col] != null)
				{
					if (obj[col].Type == JTokenType.Integer)
					{
						list.Add(obj.ReadInt(col));
					}
					else if (obj[col].Type == JTokenType.Array)
					{
						list.AddRange(obj[col].Select(id => (int)id));
					}
				}
			};

			readArrOrIntCol("eyeColor", entry, raceData.Face.EyeColors);
			readArrOrIntCol("eyeType", entry, raceData.Face.EyeTypes);
			readArrOrIntCol("mouthType", entry, raceData.Face.MouthTypes);
			readArrOrIntCol("skinColor", entry, raceData.Face.SkinColors);

			// Stat Info
			raceData.Level = entry.ReadInt("level");
			raceData.Str = entry.ReadFloat("str");
			raceData.Int = entry.ReadFloat("int");
			raceData.Dex = entry.ReadFloat("dex");
			raceData.Will = entry.ReadFloat("will");
			raceData.Luck = entry.ReadFloat("luck");

			raceData.CombatPower = entry.ReadFloat("cp");
			raceData.Life = entry.ReadFloat("life");
			raceData.Mana = entry.ReadFloat("mana");
			raceData.Stamina = entry.ReadFloat("stamina");
			raceData.Defense = entry.ReadInt("defense");
			raceData.Protection = (int)entry.ReadFloat("protection");
			raceData.ElementPhysical = entry.ReadInt("elementPhysical");
			raceData.ElementLightning = entry.ReadInt("elementLightning");
			raceData.ElementFire = entry.ReadInt("elementFire");
			raceData.ElementIce = entry.ReadInt("elementIce");
			raceData.Exp = entry.ReadInt("exp");
			raceData.GoldMin = entry.ReadInt("goldMin");
			raceData.GoldMax = entry.ReadInt("goldMax");

			// Drops
			if (entry.ContainsKeys("drops"))
			{
				foreach (JObject drop in entry["drops"].Where(a => a.Type == JTokenType.Object))
				{
					drop.AssertNotMissing("itemId", "chance");

					var dropData = new DropData();
					dropData.ItemId = drop.ReadInt("itemId");
					dropData.Chance = drop.ReadFloat("chance");
					var amount = drop.ReadInt("amount");
					dropData.AmountMin = drop.ReadInt("minAmount");
					dropData.AmountMax = drop.ReadInt("maxAmount");
					dropData.Prefix = drop.ReadInt("prefix");
					dropData.Suffix = drop.ReadInt("suffix");

					if (amount != 0)
						dropData.AmountMin = dropData.AmountMax = amount;
					if (dropData.AmountMin > dropData.AmountMax)
						dropData.AmountMax = dropData.AmountMin;

					if (dropData.Chance > 100)
						dropData.Chance = 100;
					else if (dropData.Chance < 0)
						dropData.Chance = 0;

					if (drop.ContainsKey("color1")) dropData.Color1 = drop.ReadUInt("color1");
					if (drop.ContainsKey("color2")) dropData.Color2 = drop.ReadUInt("color2");
					if (drop.ContainsKey("color3")) dropData.Color3 = drop.ReadUInt("color3");

					dropData.Durability = drop.ReadInt("durability", -1);

					raceData.Drops.Add(dropData);
				}
			}

			// Skills
			if (entry.ContainsKeys("skills"))
			{
				foreach (JObject skill in entry["skills"].Where(a => a.Type == JTokenType.Object))
				{
					skill.AssertNotMissing("skillId", "rank");

					var skillData = new RaceSkillData();
					skillData.SkillId = skill.ReadUShort("skillId");

					var rank = skill.ReadString("rank");
					if (rank == "N") skillData.Rank = 0;
					else skillData.Rank = (byte)(16 - int.Parse(rank, NumberStyles.HexNumber));

					raceData.Skills.Add(skillData);
				}
			}

			// Items
			if (entry.ContainsKeys("equip"))
			{
				foreach (JObject item in entry["equip"].Where(a => a.Type == JTokenType.Object))
				{
					item.AssertNotMissing("itemId", "pocket");

					var itemData = new RaceItemData();

					// Item ids
					if (item["itemId"].Type == JTokenType.Integer)
					{
						itemData.ItemIds.Add(item.ReadInt("itemId"));
					}
					else if (item["itemId"].Type == JTokenType.Array)
					{
						foreach (var id in item["itemId"])
							itemData.ItemIds.Add((int)id);
					}

					// Check that we got ids
					if (itemData.ItemIds.Count == 0)
						throw new MandatoryValueException(null, "itemId", item);

					// Color 1
					if (item["color1"] != null)
					{
						if (item["color1"].Type == JTokenType.Integer)
						{
							itemData.Color1s.Add(item.ReadUInt("color1"));
						}
						else if (item["color1"].Type == JTokenType.Array)
						{
							foreach (var id in item["color1"])
								itemData.Color1s.Add((uint)id);
						}
					}

					// Color 2
					if (item["color2"] != null)
					{
						if (item["color2"].Type == JTokenType.Integer)
						{
							itemData.Color2s.Add(item.ReadUInt("color2"));
						}
						else if (item["color2"].Type == JTokenType.Array)
						{
							foreach (var id in item["color2"])
								itemData.Color2s.Add((uint)id);
						}
					}

					// Color 3
					if (item["color3"] != null)
					{
						if (item["color3"].Type == JTokenType.Integer)
						{
							itemData.Color3s.Add(item.ReadUInt("color3"));
						}
						else if (item["color3"].Type == JTokenType.Array)
						{
							foreach (var id in item["color3"])
								itemData.Color3s.Add((uint)id);
						}
					}

					// Pocket
					itemData.Pocket = item.ReadInt("pocket");

					raceData.Equip.Add(itemData);
				}
			}

			// Speed
			SpeedData actionInfo;
			if ((actionInfo = AuraData.SpeedDb.Find(raceData.Group + "/walk")) != null)
				raceData.WalkingSpeed = actionInfo.Speed;
			else if ((actionInfo = AuraData.SpeedDb.Find(raceData.Group + "/*")) != null)
				raceData.WalkingSpeed = actionInfo.Speed;
			else if ((actionInfo = AuraData.SpeedDb.Find(Regex.Replace(raceData.Group, "/.*$", "") + "/*/walk")) != null)
				raceData.WalkingSpeed = actionInfo.Speed;
			else if ((actionInfo = AuraData.SpeedDb.Find(Regex.Replace(raceData.Group, "/.*$", "") + "/*/*")) != null)
				raceData.WalkingSpeed = actionInfo.Speed;
			else
				raceData.WalkingSpeed = 207.6892f;

			if ((actionInfo = AuraData.SpeedDb.Find(raceData.Group + "/run")) != null)
				raceData.RunningSpeed = actionInfo.Speed;
			else if ((actionInfo = AuraData.SpeedDb.Find(raceData.Group + "/*")) != null)
				raceData.RunningSpeed = actionInfo.Speed;
			else if ((actionInfo = AuraData.SpeedDb.Find(Regex.Replace(raceData.Group, "/.*$", "") + "/*/run")) != null)
				raceData.RunningSpeed = actionInfo.Speed;
			else if ((actionInfo = AuraData.SpeedDb.Find(Regex.Replace(raceData.Group, "/.*$", "") + "/*/*")) != null)
				raceData.RunningSpeed = actionInfo.Speed;
			else
				raceData.RunningSpeed = 373.850647f;

			this.Entries[raceData.Id] = raceData;
		}
	}
}
