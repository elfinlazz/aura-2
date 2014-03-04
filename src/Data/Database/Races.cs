// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Aura.Data.Database
{
	public class RaceData : TaggableData
	{
		public int Id { get; internal set; }
		public string Name { get; internal set; }
		public string Group { get; internal set; }
		public Gender Gender { get; internal set; }

		public int DefaultState { get; internal set; }
		public int VehicleType { get; internal set; }
		public float RunSpeedFactor { get; internal set; }
		public Element Element { get; internal set; }

		public float Size { get; internal set; }
		public uint Color1 { get; internal set; }
		public uint Color2 { get; internal set; }
		public uint Color3 { get; internal set; }

		public float RunningSpeed { get; internal set; }
		public float WalkingSpeed { get; internal set; }

		public int InventoryWidth { get; internal set; }
		public int InventoryHeight { get; internal set; }

		public short AttackSkill { get; internal set; }
		public int AttackRange { get; internal set; }
		public int AttackMin { get; internal set; }
		public int AttackMax { get; internal set; }
		public int AttackSpeed { get; internal set; }
		public int KnockCount { get; internal set; }
		public int Critical { get; internal set; }
		public int SplashRadius { get; internal set; }
		public int SplashAngle { get; internal set; }
		public float SplashDamage { get; internal set; }
		public RaceStands Stand { get; internal set; }

		public string AI { get; internal set; }
		public float CombatPower { get; internal set; }
		public float Life { get; internal set; }
		public int Defense { get; internal set; }
		public int Protection { get; internal set; }
		public int Exp { get; internal set; }

		public int GoldMin { get; internal set; }
		public int GoldMax { get; internal set; }
		public List<DropData> Drops { get; internal set; }

		public List<RaceSkillData> Skills { get; internal set; }

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

	public class DropData
	{
		public int ItemId;
		public float Chance;

		public DropData()
		{
		}

		public DropData(int itemId, float chance)
		{
			this.ItemId = itemId;
			this.Chance = chance;
		}
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
	public class RaceDb : DatabaseCSVIndexed<int, RaceData>
	{
		public List<RaceData> FindAll(string name)
		{
			name = name.ToLower();
			return this.Entries.FindAll(a => a.Value.Name.ToLower() == name);
		}

		[MinFieldCount(33)]
		protected override void ReadEntry(CSVEntry entry)
		{
			var info = new RaceData();
			info.Id = entry.ReadInt();
			info.Name = entry.ReadString();
			info.Group = entry.ReadString();
			info.Tags = entry.ReadString();
			info.Gender = (Gender)entry.ReadByte();
			info.VehicleType = entry.ReadInt();
			info.RunSpeedFactor = entry.ReadFloat();
			info.DefaultState = entry.ReadIntHex();
			info.InventoryWidth = entry.ReadInt();
			info.InventoryHeight = entry.ReadInt();
			info.AttackSkill = 23002; // Combat Mastery, they all use this anyway.
			info.AttackMin = entry.ReadInt();
			info.AttackMax = entry.ReadInt();
			info.AttackRange = entry.ReadInt();
			info.AttackSpeed = entry.ReadInt();
			info.KnockCount = entry.ReadInt();
			info.Critical = entry.ReadInt();
			info.SplashRadius = entry.ReadInt();
			info.SplashAngle = entry.ReadInt();
			info.SplashDamage = entry.ReadFloat();
			info.Stand = (RaceStands)entry.ReadIntHex();

			// Stat Info
			info.AI = entry.ReadString();
			info.Color1 = entry.ReadUIntHex();
			info.Color2 = entry.ReadUIntHex();
			info.Color3 = entry.ReadUIntHex();
			info.Size = entry.ReadFloat();
			info.CombatPower = entry.ReadFloat();
			info.Life = entry.ReadFloat();
			info.Defense = entry.ReadInt();
			info.Protection = (int)entry.ReadFloat();
			info.Element = (Element)entry.ReadByte();
			info.Exp = entry.ReadInt();
			info.GoldMin = entry.ReadInt();
			info.GoldMax = entry.ReadInt();

			// Optional drop information
			while (!entry.End)
			{
				// Drop format: <itemId>:<chance>, skip this drop if incorrect.
				var drop = entry.ReadString().Split(':');
				if (drop.Length != 2)
					throw new DatabaseWarningException("Incomplete drop information.");

				var di = new DropData();
				di.ItemId = Convert.ToInt32(drop[0]);
				di.Chance = float.Parse(drop[1], NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"));

				di.Chance /= 100;
				if (di.Chance > 1)
					di.Chance = 1;
				else if (di.Chance < 0)
					di.Chance = 0;

				info.Drops.Add(di);
			}

			// External information from other dbs
			SpeedData actionInfo;
			if ((actionInfo = AuraData.SpeedDb.Find(info.Group + "/walk")) != null)
				info.WalkingSpeed = actionInfo.Speed;
			else if ((actionInfo = AuraData.SpeedDb.Find(info.Group + "/*")) != null)
				info.WalkingSpeed = actionInfo.Speed;
			else if ((actionInfo = AuraData.SpeedDb.Find(Regex.Replace(info.Group, "/.*$", "") + "/*/walk")) != null)
				info.WalkingSpeed = actionInfo.Speed;
			else if ((actionInfo = AuraData.SpeedDb.Find(Regex.Replace(info.Group, "/.*$", "") + "/*/*")) != null)
				info.WalkingSpeed = actionInfo.Speed;
			else
				info.WalkingSpeed = 207.6892f;

			if ((actionInfo = AuraData.SpeedDb.Find(info.Group + "/run")) != null)
				info.RunningSpeed = actionInfo.Speed;
			else if ((actionInfo = AuraData.SpeedDb.Find(info.Group + "/*")) != null)
				info.RunningSpeed = actionInfo.Speed;
			else if ((actionInfo = AuraData.SpeedDb.Find(Regex.Replace(info.Group, "/.*$", "") + "/*/run")) != null)
				info.RunningSpeed = actionInfo.Speed;
			else if ((actionInfo = AuraData.SpeedDb.Find(Regex.Replace(info.Group, "/.*$", "") + "/*/*")) != null)
				info.RunningSpeed = actionInfo.Speed;
			else
				info.RunningSpeed = 373.850647f;

			info.Skills = AuraData.RaceSkillDb.FindAll(info.Id);

			this.Entries[info.Id] = info;
		}
	}

	public enum Gender : byte { None, Female, Male, Universal }
	public enum Element : byte { None, Ice, Fire, Lightning }
}
