// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Skills.Base;
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
	}
}
