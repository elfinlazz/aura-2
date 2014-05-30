// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

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
		public Element Element { get; set; }

		public float Size { get; set; }
		public uint Color1 { get; set; }
		public uint Color2 { get; set; }
		public uint Color3 { get; set; }

		public float RunningSpeed { get; set; }
		public float WalkingSpeed { get; set; }

		public int InventoryWidth { get; set; }
		public int InventoryHeight { get; set; }

		public short AttackSkill { get; set; }
		public int AttackRange { get; set; }
		public int AttackMin { get; set; }
		public int AttackMax { get; set; }
		public int AttackSpeed { get; set; }
		public int KnockCount { get; set; }
		public int Critical { get; set; }
		public int CriticalRate { get; set; }
		public int SplashRadius { get; set; }
		public int SplashAngle { get; set; }
		public float SplashDamage { get; set; }
		public RaceStands Stand { get; set; }

		public string AI { get; set; }
		public float CombatPower { get; set; }
		public float Life { get; set; }
		public int Defense { get; set; }
		public int Protection { get; set; }
		public int Exp { get; set; }

		public int GoldMin { get; set; }
		public int GoldMax { get; set; }
		public List<DropData> Drops { get; set; }

		public List<RaceSkillData> Skills { get; set; }

		public RaceData()
		{
			this.Drops = new List<DropData>();
			this.Skills = new List<RaceSkillData>();
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

		public DropData()
		{
		}

		public DropData(int itemId, float chance)
		{
			this.ItemId = itemId;
			this.Chance = chance;
		}
	}

	[Serializable]
	public class RaceSkillData
	{
		public int RaceId { get; set; }
		public ushort SkillId { get; set; }
		public byte Rank { get; set; }
	}

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

		[Mandatory("id", "name", "group", "tags", "gender", "vehicleType", "runSpeedFactor", "state", "invWidth", "invHeight", "attackMin", "attackMax", "range", "attackSpeed", "knockCount", "critical", "criticalRate", "splashRadius", "splashAngle", "splashDamage", "stand", "ai", "color1", "color2", "color3", "size", "cp", "life", "defense", "protection", "element")]
		protected override void ReadEntry(JObject entry)
		{
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
			raceData.InventoryHeight = entry.ReadInt("invHheight");
			raceData.AttackSkill = 23002; // Combat Mastery, they all use this anyway.
			raceData.AttackMin = entry.ReadInt("attackMin");
			raceData.AttackMax = entry.ReadInt("attackMax");
			raceData.AttackRange = entry.ReadInt("range");
			raceData.AttackSpeed = entry.ReadInt("attackSpeed");
			raceData.KnockCount = entry.ReadInt("knockCount");
			raceData.Critical = entry.ReadInt("critical");
			raceData.CriticalRate = entry.ReadInt("criticalRate");
			raceData.SplashRadius = entry.ReadInt("splashRadius");
			raceData.SplashAngle = entry.ReadInt("splashAngle");
			raceData.SplashDamage = entry.ReadFloat("splashDamage");
			raceData.Stand = (RaceStands)entry.ReadInt("stand");

			// Stat Info
			raceData.AI = entry.ReadString("ai");
			raceData.Color1 = entry.ReadUInt("color1");
			raceData.Color2 = entry.ReadUInt("color2");
			raceData.Color3 = entry.ReadUInt("color3");
			raceData.Size = entry.ReadFloat("size");
			raceData.CombatPower = entry.ReadFloat("cp");
			raceData.Life = entry.ReadFloat("life");
			raceData.Defense = entry.ReadInt("defense");
			raceData.Protection = (int)entry.ReadFloat("protection");
			raceData.Element = (Element)entry.ReadByte("element");
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

					dropData.Chance /= 100;
					if (dropData.Chance > 1)
						dropData.Chance = 1;
					else if (dropData.Chance < 0)
						dropData.Chance = 0;

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

	public enum Gender : byte { None, Female, Male, Universal }
	public enum Element : byte { None, Ice, Fire, Lightning }
}
