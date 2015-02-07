// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.Skills.Combat;
using Aura.Channel.World.Entities;
using Aura.Shared.Mabi.Const;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.Skills
{
	public static class SkillHelper
	{
		private const int DefenseAttackerStun = 2500;
		private const int DefenseTargetStun = 1000;

		/// <summary>
		/// Checks if target's Mana Shield is active, calculates mana
		/// damage, and sets target action's Mana Damage property if applicable.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="damage"></param>
		/// <param name="tAction"></param>
		public static void HandleManaShield(Creature target, ref float damage, TargetAction tAction)
		{
			// Mana Shield active?
			if (!target.Conditions.Has(ConditionsA.ManaShield))
				return;

			// Get Mana Shield skill to get the rank's vars
			var manaShield = target.Skills.Get(SkillId.ManaShield);
			if (manaShield == null) // Checks for things that should never ever happen, yay.
				return;

			// Var 1 = Efficiency
			var manaDamage = damage / manaShield.RankData.Var1;
			if (target.Mana >= manaDamage)
			{
				// Damage is 0 if target's mana is enough to cover it
				damage = 0;
			}
			else
			{
				// Set mana damage to target's mana and reduce the remaining
				// damage from life if the mana is not enough.
				damage -= (manaDamage - target.Mana) * manaShield.RankData.Var1;
				manaDamage = target.Mana;
			}

			// Reduce mana
			target.Mana -= manaDamage;

			if (target.Mana <= 0)
				ChannelServer.Instance.SkillManager.GetHandler<StartStopSkillHandler>(SkillId.ManaShield).Stop(target, manaShield);

			tAction.ManaDamage = manaDamage;
		}

		/// <summary>
		/// Checks if attacker has Critical Hit and applies crit bonus
		/// by chance. Also sets the target action's critical option if a
		/// crit happens.
		/// </summary>
		/// <param name="attacker"></param>
		/// <param name="critChance"></param>
		/// <param name="damage"></param>
		/// <param name="tAction"></param>
		public static void HandleCritical(Creature attacker, float critChance, ref float damage, TargetAction tAction)
		{
			// Check if attacker actually has critical hit
			var critSkill = attacker.Skills.Get(SkillId.CriticalHit);
			if (critSkill == null)
				return;

			// Does the crit happen?
			if (RandomProvider.Get().NextDouble() > critChance)
				return;

			// Add crit bonus
			var bonus = critSkill.RankData.Var1 / 100f;
			damage = damage + (damage * bonus);

			// Set target option
			tAction.Set(TargetOptions.Critical);
		}

		/// <summary>
		/// Checks if target has Defense skill activated and makes the necessary
		/// changes to the actions, stun times, and damage.
		/// </summary>
		/// <param name="aAction"></param>
		/// <param name="tAction"></param>
		/// <param name="damage"></param>
		/// <returns></returns>
		public static bool HandleDefense(AttackerAction aAction, TargetAction tAction, ref float damage)
		{
			// Defense
			if (!tAction.Creature.Skills.IsActive(SkillId.Defense))
				return false;

			// Update actions
			tAction.Type = CombatActionType.Defended;
			tAction.SkillId = SkillId.Defense;
			tAction.Creature.Stun = tAction.Stun = DefenseTargetStun;
			aAction.Creature.Stun = aAction.Stun = DefenseAttackerStun;

			// Reduce damage
			var defenseSkill = tAction.Creature.Skills.Get(SkillId.Defense);
			if (defenseSkill != null)
				damage -= defenseSkill.RankData.Var3;

			Send.SkillUseStun(tAction.Creature, SkillId.Defense, 1000, 0);

			return true;
		}

		/// <summary>
		/// Reduces damage by target's defense and protection.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="damage"></param>
		/// <param name="defense"></param>
		/// <param name="protection"></param>
		public static void HandleDefenseProtection(Creature target, ref float damage, bool defense = true, bool protection = true)
		{
			if (defense)
				damage = Math.Max(1, damage - target.Defense);
			if (protection && damage > 1)
				damage = Math.Max(1, damage - (damage * target.Protection));
		}

		/// <summary>
		/// Returns true if target has counter active and used it.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="attacker"></param>
		/// <returns></returns>
		public static bool HandleCounter(Creature target, Creature attacker)
		{
			if (!target.Skills.IsActive(SkillId.Counterattack))
				return false;

			var handler = ChannelServer.Instance.SkillManager.GetHandler<Counterattack>(SkillId.Counterattack);
			handler.Use(target, attacker);

			return true;
		}

		/// <summary>
		/// Reduces weapon's durability and increases its proficiency.
		/// Only updates weapon type items that are not null.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="weapon"></param>
		public static void UpdateWeapon(Creature attacker, Creature target, params Item[] weapons)
		{
			if (attacker == null)
				return;

			var rnd = RandomProvider.Get();

			foreach (var weapon in weapons.Where(a => a != null && a.IsTrainableWeapon))
			{
				// Durability
				if (!ChannelServer.Instance.Conf.World.NoDurabilityLoss)
				{
					weapon.Durability -= rnd.Next(1, 30);
					Send.ItemDurabilityUpdate(attacker, weapon);
				}

				// Proficiency
				// Only if the weapon isn't broken and the target is not "Weakest".
				if (weapon.Durability != 0 && attacker != null && attacker.GetPowerRating(target) >= PowerRating.Weak)
				{
					short prof = 0;

					if (attacker.Age >= 10 && attacker.Age <= 12)
						prof = 48;
					else if (attacker.Age >= 13 && attacker.Age <= 19)
						prof = 60;
					else
						prof = 72;

					weapon.Proficiency += prof;

					Send.ItemExpUpdate(attacker, weapon);
				}
			}
		}
	}
}
