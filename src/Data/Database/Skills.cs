// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Aura.Data.Database
{
	[Serializable]
	public class SkillData
	{
		public ushort Id { get; set; }
		public string Name { get; set; }
		public ushort MasterTitle { get; set; }
		public byte MaxRank { get; set; }
		public SkillType Type { get; set; }

		public Dictionary<int, Dictionary<int, SkillRankData>> RankData { get; set; }

		public SkillRankData GetRankData(int rank, int raceId)
		{
			if (this.RankData == null)
				return null;

			raceId = raceId & ~3;

			// Check race specific first, fall back to default (0).
			var skill = this.RankData.GetValueOrDefault(raceId);
			if (skill == null)
				if ((skill = this.RankData.GetValueOrDefault(0)) == null)
					return null;

			return skill.GetValueOrDefault(rank);
		}

		public SkillRankData GetFirstRankData(int raceId)
		{
			if (this.RankData == null)
				return null;

			raceId = raceId & ~3;

			// Check race specific first, fall back to default (0).
			var skill = this.RankData.GetValueOrDefault(raceId);
			if (skill == null)
				if ((skill = this.RankData.GetValueOrDefault(0)) == null)
					return null;

			return skill.Values.First();
		}
	}

	[Serializable]
	public class SkillRankData
	{
		public ushort SkillId { get; set; }
		public int Race { get; set; }
		public byte Rank { get; set; }
		public byte AP { get; set; }
		public float CP { get; set; }
		public int Range { get; set; }

		public byte Stack { get; set; }
		public byte StackMax { get; set; }

		public int LoadTime { get; set; }
		public int NewLoadTime { get; set; }
		public int CoolDown { get; set; }

		public float StaminaCost { get; set; }
		public float StaminaPrepare { get; set; }
		public float StaminaWait { get; set; }
		public float StaminaActive { get; set; }

		public float ManaCost { get; set; }
		public float ManaPrepare { get; set; }
		public float ManaWait { get; set; }
		public float ManaActive { get; set; }

		public float Life { get; set; }
		public float Mana { get; set; }
		public float Stamina { get; set; }
		public float Str { get; set; }
		public float Int { get; set; }
		public float Dex { get; set; }
		public float Will { get; set; }
		public float Luck { get; set; }

		public float LifeTotal { get; set; }
		public float ManaTotal { get; set; }
		public float StaminaTotal { get; set; }
		public float StrTotal { get; set; }
		public float IntTotal { get; set; }
		public float DexTotal { get; set; }
		public float WillTotal { get; set; }
		public float LuckTotal { get; set; }

		public float Var1 { get; set; }
		public float Var2 { get; set; }
		public float Var3 { get; set; }
		public float Var4 { get; set; }
		public float Var5 { get; set; }
		public float Var6 { get; set; }
		public float Var7 { get; set; }
		public float Var8 { get; set; }
		public float Var9 { get; set; }

		public List<TrainingsConditionData> Conditions { get; set; }
	}

	[Serializable]
	public class TrainingsConditionData
	{
		public float Exp { get; set; }
		public int Count { get; set; }
		public bool Visible { get; set; }
	}

	/// <summary>
	/// Indexed by skill id.
	/// Depends on: SkillRankDb
	/// </summary>
	public class SkillDb : DatabaseJsonIndexed<int, SkillData>
	{
		public List<SkillData> FindAll(string name)
		{
			name = name.ToLower();
			return this.Entries.FindAll(a => a.Value.Name.ToLower().Contains(name));
		}

		protected override void ReadEntry(JObject entry)
		{
			entry.AssertNotMissing("id", "name", "masterTitle", "ranks");

			var skillInfo = new SkillData();
			skillInfo.Id = entry.ReadUShort("id");
			skillInfo.Name = entry.ReadString("name");
			skillInfo.MasterTitle = entry.ReadUShort("masterTitle");
			skillInfo.Type = (SkillType)entry.ReadInt("type", -1);

			// Ranks
			skillInfo.RankData = new Dictionary<int, Dictionary<int, SkillRankData>>();
			foreach (JObject rank in entry["ranks"].Where(a => a.Type == JTokenType.Object))
			{
				rank.AssertNotMissing("rank", "ap", "cp", "range", "stack", "stackMax", "loadTime", "newLoadTime", "coolDown", "staminaCost", "staminaPrepare", "staminaWait", "staminaActive", "manaCost", "manaPrepare", "manaWait", "manaActive", "life", "mana", "stamina", "str", "int", "dex", "will", "luck", "var1", "var2", "var3", "var4", "var5", "var6", "var7", "var8", "var9", "training");

				var rankInfo = new SkillRankData();
				rankInfo.SkillId = skillInfo.Id;
				rankInfo.Race = rank.ReadInt("race");
				rankInfo.Rank = rank.ReadByte("rank");
				rankInfo.AP = rank.ReadByte("ap");
				rankInfo.CP = rank.ReadFloat("cp");
				rankInfo.Range = rank.ReadInt("range");
				rankInfo.Stack = rank.ReadByte("stack");
				rankInfo.StackMax = rank.ReadByte("stackMax");
				rankInfo.LoadTime = rank.ReadInt("loadTime");
				rankInfo.NewLoadTime = rank.ReadInt("newLoadTime");
				rankInfo.CoolDown = rank.ReadInt("coolDown");
				rankInfo.StaminaCost = rank.ReadFloat("staminaCost");
				rankInfo.StaminaPrepare = rank.ReadFloat("staminaPrepare");
				rankInfo.StaminaWait = rank.ReadFloat("staminaWait");
				rankInfo.StaminaActive = rank.ReadFloat("staminaActive");
				rankInfo.ManaCost = rank.ReadFloat("manaCost");
				rankInfo.ManaPrepare = rank.ReadFloat("manaPrepare");
				rankInfo.ManaWait = rank.ReadFloat("manaWait");
				rankInfo.ManaActive = rank.ReadFloat("manaActive");
				rankInfo.Life = rank.ReadFloat("life");
				rankInfo.Mana = rank.ReadFloat("mana");
				rankInfo.Stamina = rank.ReadFloat("stamina");
				rankInfo.Str = rank.ReadFloat("str");
				rankInfo.Int = rank.ReadFloat("int");
				rankInfo.Dex = rank.ReadFloat("dex");
				rankInfo.Will = rank.ReadFloat("will");
				rankInfo.Luck = rank.ReadFloat("luck");
				rankInfo.Var1 = rank.ReadFloat("var1");
				rankInfo.Var2 = rank.ReadFloat("var2");
				rankInfo.Var3 = rank.ReadFloat("var3");
				rankInfo.Var4 = rank.ReadFloat("var4");
				rankInfo.Var5 = rank.ReadFloat("var5");
				rankInfo.Var6 = rank.ReadFloat("var6");
				rankInfo.Var7 = rank.ReadFloat("var7");
				rankInfo.Var8 = rank.ReadFloat("var8");
				rankInfo.Var9 = rank.ReadFloat("var9");

				rankInfo.Conditions = new List<TrainingsConditionData>();
				foreach (JObject training in rank["training"].Where(a => a.Type == JTokenType.Object))
				{
					training.AssertNotMissing("exp", "count", "visible");

					var condition = new TrainingsConditionData();
					condition.Exp = training.ReadFloat("exp");
					condition.Count = training.ReadInt("count");
					condition.Visible = training.ReadBool("visible");

					rankInfo.Conditions.Add(condition);
				}

				if (!skillInfo.RankData.ContainsKey(rankInfo.Race))
					skillInfo.RankData[rankInfo.Race] = new Dictionary<int, SkillRankData>();

				skillInfo.RankData[rankInfo.Race][rankInfo.Rank] = rankInfo;
			}

			// Max Rank
			if (skillInfo.RankData.Count > 0)
			{
				// Rank list for first race
				var rankList = skillInfo.RankData.Values.First();
				// Last rank is the max
				skillInfo.MaxRank = rankList.Values.Last().Rank;
			}

			this.Entries[skillInfo.Id] = skillInfo;
		}

		protected override void AfterLoad()
		{
			// Total bonuses for all ranks of all skills
			foreach (var skillList in this.Entries.Values.Select(a => a.RankData))
			{
				foreach (var raceList in skillList.Values)
				{
					float lifeT = 0, manaT = 0, staminaT = 0, strT = 0, intT = 0, dexT = 0, willT = 0, luckT = 0;

					for (byte i = 0; i <= 18; i++) // Novice -> D3
					{
						var sInfo = raceList.GetValueOrDefault(i);

						if (sInfo != null)
						{
							sInfo.DexTotal = (dexT += sInfo.Dex);
							sInfo.IntTotal = (intT += sInfo.Int);
							sInfo.LifeTotal = (lifeT += sInfo.Life);
							sInfo.LuckTotal = (luckT += sInfo.Luck);
							sInfo.ManaTotal = (manaT += sInfo.Mana);
							sInfo.StaminaTotal = (staminaT += sInfo.Stamina);
							sInfo.StrTotal = (strT += sInfo.Str);
							sInfo.WillTotal = (willT += sInfo.Will);
						}
					}
				}
			}
		}
	}

	public enum SkillType
	{
		None = -1,
		Unk1 = 0,
		Combat = 1,
		RangedCombat = 2,
		RangedCombat2 = 3,
		Production = 4,
		MaterialProduction = 5,
		Enchanting = 6,
		BroadcastStartStop = 7,
		StartStop = 8,
		Passive = 10,
	}
}
