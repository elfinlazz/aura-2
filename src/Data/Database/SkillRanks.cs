// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Aura.Data.Database
{
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
	/// Indexed by skill id, race id, and rank.
	/// </summary>
	public class SkillRankDb : DatabaseCsvIndexed<int, Dictionary<int, Dictionary<int, SkillRankData>>>
	{
		public override int Load(string path, bool clear)
		{
			var res = base.Load(path, clear);
			this.CalculateTotals();
			return res;
		}

		protected void CalculateTotals()
		{
			//var skills = this.Entries.Select(c => c.SkillId).Distinct().ToList();

			foreach (var skillList in this.Entries.Values)
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

		[MinFieldCount(45)]
		protected override void ReadEntry(CsvEntry entry)
		{
			var info = new SkillRankData();
			info.SkillId = entry.ReadUShort();
			info.Race = entry.ReadInt();
			info.Rank = entry.ReadByte();
			info.AP = entry.ReadByte();
			info.CP = entry.ReadFloat();
			info.Range = entry.ReadInt();
			info.Stack = entry.ReadByte();
			info.StackMax = entry.ReadByte();
			info.LoadTime = entry.ReadInt();
			info.NewLoadTime = entry.ReadInt();
			info.CoolDown = entry.ReadInt();
			info.StaminaCost = entry.ReadFloat();
			info.StaminaPrepare = entry.ReadFloat();
			info.StaminaWait = entry.ReadFloat();
			info.StaminaActive = entry.ReadFloat();
			info.ManaCost = entry.ReadFloat();
			info.ManaPrepare = entry.ReadFloat();
			info.ManaWait = entry.ReadFloat();
			info.ManaActive = entry.ReadFloat();
			info.Life = entry.ReadFloat();
			info.Mana = entry.ReadFloat();
			info.Stamina = entry.ReadFloat();
			info.Str = entry.ReadFloat();
			info.Int = entry.ReadFloat();
			info.Dex = entry.ReadFloat();
			info.Will = entry.ReadFloat();
			info.Luck = entry.ReadFloat();
			info.Var1 = entry.ReadFloat();
			info.Var2 = entry.ReadFloat();
			info.Var3 = entry.ReadFloat();
			info.Var4 = entry.ReadFloat();
			info.Var5 = entry.ReadFloat();
			info.Var6 = entry.ReadFloat();
			info.Var7 = entry.ReadFloat();
			info.Var8 = entry.ReadFloat();
			info.Var9 = entry.ReadFloat();

			info.Conditions = new List<TrainingsConditionData>();
			for (int i = 0; i < 9; ++i)
			{
				var conditionSplit = entry.ReadStringList();

				var condition = new TrainingsConditionData();
				condition.Exp = float.Parse(conditionSplit[0], NumberStyles.Float, CultureInfo.InvariantCulture);
				condition.Count = int.Parse(conditionSplit[1]);
				condition.Visible = (conditionSplit[2] == "yes");

				info.Conditions.Add(condition);
			}

			if (!this.Entries.ContainsKey(info.SkillId))
				this.Entries[info.SkillId] = new Dictionary<int, Dictionary<int, SkillRankData>>();
			if (!this.Entries[info.SkillId].ContainsKey(info.Race))
				this.Entries[info.SkillId][info.Race] = new Dictionary<int, SkillRankData>();

			this.Entries[info.SkillId][info.Race][info.Rank] = info;
		}
	}
}
