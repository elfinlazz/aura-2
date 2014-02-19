// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Shared.Mabi.Const;
using Aura.Channel.Skills;
using Aura.Channel.Network.Sending;
using Aura.Shared.Util;
using Aura.Channel.Skills.Base;

namespace Aura.Channel.World.Entities.Creatures
{
	public class CreatureSkills
	{
		private Creature _creature;
		private Dictionary<SkillId, Skill> _skills;

		public float HighestSkillCp { get; private set; }
		public float SecondHighestSkillCp { get; private set; }

		public Skill ActiveSkill { get; set; }

		public CreatureSkills(Creature creature)
		{
			_skills = new Dictionary<SkillId, Skill>();
			_creature = creature;
		}

		/// <summary>
		/// Returns new list of all skills.
		/// </summary>
		/// <returns></returns>
		public ICollection<Skill> GetList()
		{
			lock (_skills)
				return _skills.Values.ToArray();
		}

		/// <summary>
		/// Add skill silently. Returns false if the skill already exists,
		/// with a rank that's equal or higher.
		/// </summary>
		/// <param name="skill"></param>
		public bool Add(Skill skill)
		{
			if (this.Has(skill.Info.Id, skill.Info.Rank))
				return false;

			lock (_skills)
				_skills[skill.Info.Id] = skill;

			this.AddBonuses(skill);

			return true;
		}

		/// <summary>
		/// Returns skill by id, or null.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public Skill Get(SkillId id)
		{
			Skill result;
			lock (_skills)
				_skills.TryGetValue(id, out result);
			return result;
		}

		/// <summary>
		/// Returns true if creature has skill and its rank is equal
		/// or greater than the given rank.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="rank"></param>
		/// <returns></returns>
		public bool Has(SkillId id, SkillRank rank = SkillRank.Novice)
		{
			var skill = this.Get(id);
			return (skill != null && skill.Info.Rank >= rank);
		}

		/// <summary>
		/// Returns true if rank of skill is equal.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="rank"></param>
		/// <returns></returns>
		public bool Is(SkillId id, SkillRank rank)
		{
			var skill = this.Get(id);
			return (skill != null && skill.Info.Rank == rank);
		}

		/// <summary>
		/// Adds skill at rank, or updates it.
		/// Sends appropriate packets.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="rank"></param>
		public void Give(SkillId id, SkillRank rank)
		{
			var skill = this.Get(id);
			if (skill == null)
			{
				this.Add(skill = new Skill(id, rank, _creature.Race));

				Send.SkillInfo(_creature, skill);
				Send.RankUp(_creature);

				//EventManager.CreatureEvents.OnCreatureSkillChange(_creature, skill, true);
			}
			else
			{
				this.RemoveBonuses(skill);
				skill.ChangeRank(rank);

				Send.SkillRankUp(_creature, skill);
				Send.RankUp(_creature, skill.Info.Id);

				this.AddBonuses(skill);

				//EventManager.CreatureEvents.OnCreatureSkillChange(_creature, skill, false);
			}

			Send.StatUpdate(_creature, StatUpdateType.Private,
				Stat.Str, Stat.Int, Stat.Dex, Stat.Will, Stat.Luck,
				Stat.Life, Stat.LifeInjured, Stat.LifeMaxMod, Stat.LifeMax, Stat.Mana, Stat.ManaMaxMod, Stat.ManaMax, Stat.Stamina, Stat.Hunger, Stat.StaminaMaxMod, Stat.StaminaMax
			);
			Send.StatUpdate(_creature, StatUpdateType.Public, Stat.Life, Stat.LifeInjured, Stat.LifeMaxMod, Stat.LifeMax);
		}

		/// <summary>
		/// Adds stat bonuses for skill's rank to creature.
		/// </summary>
		/// <param name="skill"></param>
		public void AddBonuses(Skill skill)
		{
			_creature.StrBaseSkill += skill.RankData.StrTotal;
			_creature.IntBaseSkill += skill.RankData.IntTotal;
			_creature.DexBaseSkill += skill.RankData.DexTotal;
			_creature.WillBaseSkill += skill.RankData.WillTotal;
			_creature.LuckBaseSkill += skill.RankData.LuckTotal;
			_creature.LifeMaxBaseSkill += skill.RankData.LifeTotal;
			_creature.Life += skill.RankData.LifeTotal;
			_creature.ManaMaxBaseSkill += skill.RankData.ManaTotal;
			_creature.Mana += skill.RankData.ManaTotal;
			_creature.StaminaMaxBaseSkill += skill.RankData.StaminaTotal;
			_creature.Stamina += skill.RankData.StaminaTotal;

			if (skill.Info.Id == SkillId.CombatMastery)
			{
				_creature.StatMods.Add(Stat.LifeMaxMod, skill.RankData.Var3, StatModSource.SkillRank, skill.Info.Id);
				_creature.Life += skill.RankData.Var3;
			}
			else if (skill.Info.Id == SkillId.MagicMastery)
			{
				_creature.StatMods.Add(Stat.ManaMaxMod, skill.RankData.Var1, StatModSource.SkillRank, skill.Info.Id);
				_creature.Mana += skill.RankData.Var1;
			}
			else if (skill.Info.Id == SkillId.Defense)
			{
				_creature.StatMods.Add(Stat.DefenseBaseMod, skill.RankData.Var1, StatModSource.SkillRank, skill.Info.Id);
			}

			this.UpdateHighestSkills();
		}

		/// <summary>
		/// Removes stat bonuses for skill's rank from creature.
		/// (To be run before changing a skills rank.)
		/// </summary>
		/// <param name="skill"></param>
		private void RemoveBonuses(Skill skill)
		{
			_creature.StrBaseSkill -= skill.RankData.StrTotal;
			_creature.IntBaseSkill -= skill.RankData.IntTotal;
			_creature.DexBaseSkill -= skill.RankData.DexTotal;
			_creature.WillBaseSkill -= skill.RankData.WillTotal;
			_creature.LuckBaseSkill -= skill.RankData.LuckTotal;
			_creature.Life -= skill.RankData.LifeTotal;
			_creature.LifeMaxBaseSkill -= skill.RankData.LifeTotal;
			_creature.Mana -= skill.RankData.ManaTotal;
			_creature.ManaMaxBaseSkill -= skill.RankData.ManaTotal;
			_creature.Stamina -= skill.RankData.StaminaTotal;
			_creature.StaminaMaxBaseSkill -= skill.RankData.StaminaTotal;

			if (skill.Info.Id == SkillId.CombatMastery)
			{
				_creature.Life -= skill.RankData.Var3;
				_creature.StatMods.Remove(Stat.LifeMaxMod, StatModSource.SkillRank, skill.Info.Id);
			}
			else if (skill.Info.Id == SkillId.MagicMastery)
			{
				_creature.Mana -= skill.RankData.Var1;
				_creature.StatMods.Remove(Stat.ManaMaxMod, StatModSource.SkillRank, skill.Info.Id);
			}
			else if (skill.Info.Id == SkillId.Defense)
			{
				_creature.StatMods.Remove(Stat.DefenseBaseMod, StatModSource.SkillRank, skill.Info.Id);
			}

			this.UpdateHighestSkills();
		}

		/// <summary>
		/// Updates highest skill CPs.
		/// </summary>
		private void UpdateHighestSkills()
		{
			var highest = 0f;
			var second = 0f;

			lock (_skills)
			{
				foreach (var skill in _skills.Values)
				{
					if (skill.RankData.CP > highest)
					{
						second = highest;
						highest = skill.RankData.CP;
					}
					else if (skill.RankData.CP > second)
					{
						second = skill.RankData.CP;
					}
				}
			}

			this.HighestSkillCp = highest;
			this.SecondHighestSkillCp = second;
		}

		/// <summary>
		/// Adds exp to skill.
		/// </summary>
		/// <param name="skill"></param>
		/// <param name="exp"></param>
		public void GiveExp(SkillId skillId, int exp)
		{
			var skill = this.Get(skillId);
			if (skill == null) return;

			this.GiveExp(skill, exp);
		}

		/// <summary>
		/// Adds exp to skill.
		/// </summary>
		/// <param name="skill"></param>
		/// <param name="exp"></param>
		public void GiveExp(Skill skill, int exp)
		{
			if (skill.Info.Experience >= 100000)
				return;

			skill.Info.Experience = Math2.MinMax(0, 100000, skill.Info.Experience + exp * 1000);
			if (skill.IsRankable)
				skill.Info.Flag |= SkillFlags.Rankable;

			Send.SkillTrainingUp(_creature, skill, exp);
		}

		/// <summary>
		/// Cancels active skill.
		/// </summary>
		/// <remarks>
		/// SkillCancel is sent in any case, even if something goes wrong,
		/// like the method not being implemented. Unless no skill is active.
		/// </remarks>
		public void CancelActiveSkill()
		{
			if (this.ActiveSkill == null)
			{
				Log.Warning("Player '{0}' tried to cancel skill, without it being active.", _creature.Name);
				return;
			}

			var handler = ChannelServer.Instance.SkillManager.GetHandler<ICancelable>(this.ActiveSkill.Info.Id);
			if (handler == null)
			{
				Log.Unimplemented("CancelActiveSkill: Skill handler or interface for '{0}'.", this.ActiveSkill.Info.Id);
				goto L_Cancel;
			}

			try
			{
				handler.Cancel(_creature, this.ActiveSkill);
			}
			catch (NotImplementedException)
			{
				Log.Unimplemented("CancelActiveSkill: Skill cancel method for '{0}'.", this.ActiveSkill.Info.Id);
				goto L_Cancel;
			}

		L_Cancel:
			Send.SkillCancel(_creature);

			this.ActiveSkill = null;
		}

		/// <summary>
		/// Sums the ranks of the given skills.
		/// </summary>
		/// <param name="skillIds"></param>
		public int CountRanks(params SkillId[] skillIds)
		{
			var found = 0;
			var result = 0;

			lock (_skills)
			{
				foreach (var skill in _skills.Values)
				{
					if (skillIds.Length > 0 && !skillIds.Contains(skill.Info.Id))
						continue;

					found++;
					result += (int)skill.Info.Rank;

					if (skillIds.Length > 0 && found >= skillIds.Length)
						break;
				}
			}

			return result;
		}
	}
}
