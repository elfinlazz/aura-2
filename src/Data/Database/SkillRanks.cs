// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Collections.Generic;
using System.Linq;

namespace Aura.Data.Database
{
	public class SkillRankData
	{
		public ushort SkillId { get; internal set; }
		public int Race { get; internal set; }
		public byte Rank { get; internal set; }
		public byte AP { get; internal set; }
		public float CP { get; internal set; }
		public int Range { get; internal set; }

		public byte Stack { get; internal set; }
		public byte StackMax { get; internal set; }

		public int LoadTime { get; internal set; }
		public int NewLoadTime { get; internal set; }
		public int CoolDown { get; internal set; }

		public float StaminaCost { get; internal set; }
		public float StaminaPrepare { get; internal set; }
		public float StaminaWait { get; internal set; }
		public float StaminaUse { get; internal set; }

		public float ManaCost { get; internal set; }
		public float ManaPrepare { get; internal set; }
		public float ManaWait { get; internal set; }
		public float ManaUse { get; internal set; }

		public float Life { get; internal set; }
		public float Mana { get; internal set; }
		public float Stamina { get; internal set; }
		public float Str { get; internal set; }
		public float Int { get; internal set; }
		public float Dex { get; internal set; }
		public float Will { get; internal set; }
		public float Luck { get; internal set; }

		public float LifeTotal { get; internal set; }
		public float ManaTotal { get; internal set; }
		public float StaminaTotal { get; internal set; }
		public float StrTotal { get; internal set; }
		public float IntTotal { get; internal set; }
		public float DexTotal { get; internal set; }
		public float WillTotal { get; internal set; }
		public float LuckTotal { get; internal set; }

		public float Var1 { get; internal set; }
		public float Var2 { get; internal set; }
		public float Var3 { get; internal set; }
		public float Var4 { get; internal set; }
		public float Var5 { get; internal set; }
		public float Var6 { get; internal set; }
		public float Var7 { get; internal set; }
		public float Var8 { get; internal set; }
		public float Var9 { get; internal set; }
	}

	/// <summary>
	/// Indexed by skill id, race id, and rank.
	/// </summary>
	public class SkillRankDb : DatabaseCSVIndexed<int, Dictionary<int, Dictionary<int, SkillRankData>>>
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

		[MinFieldCount(36)]
		protected override void ReadEntry(CSVEntry entry)
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
			info.StaminaUse = entry.ReadFloat();
			info.ManaCost = entry.ReadFloat();
			info.ManaPrepare = entry.ReadFloat();
			info.ManaWait = entry.ReadFloat();
			info.ManaUse = entry.ReadFloat();
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

			if (!this.Entries.ContainsKey(info.SkillId))
				this.Entries[info.SkillId] = new Dictionary<int, Dictionary<int, SkillRankData>>();
			if (!this.Entries[info.SkillId].ContainsKey(info.Race))
				this.Entries[info.SkillId][info.Race] = new Dictionary<int, SkillRankData>();

			this.Entries[info.SkillId][info.Race][info.Rank] = info;
		}
	}
}
