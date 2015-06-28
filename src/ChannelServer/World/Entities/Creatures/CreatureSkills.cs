// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Channel.Util;
using Aura.Mabi.Const;
using Aura.Channel.Skills;
using Aura.Channel.Network.Sending;
using Aura.Shared.Util;
using Aura.Channel.Skills.Base;
using Aura.Data;

namespace Aura.Channel.World.Entities.Creatures
{
	public class CreatureSkills
	{
		private Creature _creature;
		private Dictionary<SkillId, Skill> _skills;
		private Dictionary<SkillId, Action> _callbacks;

		// Skill of creature with highest combat power
		public float HighestSkillCp { get; private set; }

		/// <summary>
		/// Skill of creature with second highest combat power
		/// </summary>
		public float SecondHighestSkillCp { get; private set; }

		/// <summary>
		/// Currently active skill
		/// </summary>
		public Skill ActiveSkill { get; set; }

		/// <summary>
		/// New skill manager for creature.
		/// </summary>
		/// <param name="creature"></param>
		public CreatureSkills(Creature creature)
		{
			_skills = new Dictionary<SkillId, Skill>();
			_callbacks = new Dictionary<SkillId, Action>();
			_creature = creature;
		}

		/// <summary>
		/// Returns new list of all skills.
		/// </summary>
		/// <returns></returns>
		public ICollection<Skill> GetList(Func<Skill, bool> predicate = null)
		{
			lock (_skills)
			{
				// Return all or only the ones matching the predicate.
				var skills = (predicate == null ? _skills.Values : _skills.Values.Where(predicate));
				return skills.ToArray();
			}
		}

		/// <summary>
		/// Adds skill silently. Returns false if the skill already exists,
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
		/// Adds skill silently. Returns false if the skill already exists,
		/// with a rank that's equal or higher.
		/// </summary>
		/// <param name="skillId"></param>
		/// <param name="skillRank"></param>
		/// <param name="raceId"></param>
		public bool Add(SkillId skillId, SkillRank skillRank, int raceId)
		{
			if (!AuraData.SkillDb.Exists((int)skillId))
			{
				Log.Warning("CreatureSkills.Add: Skill '{0}' not found in data.", skillId);
				return false;
			}

			return this.Add(new Skill(_creature, skillId, skillRank, raceId));
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

		public Skill GetSafe(SkillId id)
		{
			var r = this.Get(id);

			if (r == null)
				throw new ModerateViolation("Tried to get nonexistant skill {0}.", id);

			return r;
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
				this.Add(skill = new Skill(_creature, id, rank, _creature.RaceId));

				Send.SkillInfo(_creature, skill);
				Send.RankUp(_creature);
			}
			else
			{
				this.RemoveBonuses(skill);
				skill.ChangeRank(rank);

				Send.SkillRankUp(_creature, skill);
				Send.RankUp(_creature, skill.Info.Id);

				this.AddBonuses(skill);
			}

			ChannelServer.Instance.Events.OnSkillRankChanged(_creature, skill);

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
		/// Trains condition in skill.
		/// </summary>
		/// <param name="skillId"></param>
		/// <param name="condition">Condition nr (1-9)</param>
		/// <param name="amount"></param>
		public void Train(SkillId skillId, int condition, int amount = 1)
		{
			var skill = this.Get(skillId);
			if (skill == null) return;

			skill.Train(condition, amount);
		}

		/// <summary>
		/// Trains condition in skill if it has the given rank.
		/// </summary>
		/// <param name="skillId"></param>
		/// <param name="rank">Rank to which the training is limited.</param>
		/// <param name="condition">Condition nr (1-9)</param>
		/// <param name="amount"></param>
		public void Train(SkillId skillId, SkillRank rank, int condition, int amount = 1)
		{
			var skill = this.Get(skillId);
			if (skill == null || skill.Info.Rank != rank) return;

			skill.Train(condition, amount);
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
				Log.Warning("CancelActiveSkill: Player '{0}' tried to cancel a skill, without one being active.", _creature.Name);
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
			this.ActiveSkill.Stacks = 0;

			Send.SkillCancel(_creature);

			this.ActiveSkill.State = SkillState.Canceled;
			this.ActiveSkill = null;

			_creature.Unlock(Locks.Move);
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

		/// <summary>
		/// Adds callback
		/// </summary>
		/// <param name="skillId"></param>
		/// <param name="action"></param>
		public void Callback(SkillId skillId, Action action)
		{
			lock (_callbacks)
				_callbacks[skillId] = action;
		}

		/// <summary>
		/// Runs and resets callback (it one was set)
		/// </summary>
		/// <param name="skillId"></param>
		public void Callback(SkillId skillId)
		{
			lock (_callbacks)
			{
				Action callback;
				_callbacks.TryGetValue(skillId, out callback);
				if (callback == null) return;

				callback();
				_callbacks[skillId] = null;
			}
		}

		/// <summary>
		/// Returns true if the specified skill is active (somewhere
		/// between Prepare and Complete/Cancel).
		/// </summary>
		/// <param name="skillId"></param>
		/// <returns></returns>
		public bool IsActive(SkillId skillId)
		{
			return (this.ActiveSkill != null && this.ActiveSkill.Info.Id == skillId);
		}

		/// <summary>
		/// Returns true if the specified skill is active and on state "Ready".
		/// </summary>
		/// <param name="skillId"></param>
		/// <returns></returns>
		public bool IsReady(SkillId skillId)
		{
			return (this.IsActive(skillId) && this.ActiveSkill.State == SkillState.Ready);
		}
	}
}
