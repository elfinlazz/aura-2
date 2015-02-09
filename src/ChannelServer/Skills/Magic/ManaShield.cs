// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.World.Entities;
using Aura.Shared.Mabi;
using Aura.Shared.Mabi.Const;

namespace Aura.Channel.Skills.Magic
{
	/// <summary>
	/// Handles the Mana Shield skill.
	/// </summary>
	/// <remarks>
	/// Var1: Base Mana Efficiency
	/// Var2: Mana Use / s, ManaUse has the same value.
	///   Important if we want to automate that.
	/// 
	/// Skill is stopped by client once Mana reaches 0.
	/// </remarks>
	[Skill(SkillId.ManaShield)]
	public class ManaShield : StartStopSkillHandler
	{
		/// <summary>
		/// Starts the skill.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="dict"></param>
		/// <returns></returns>
		public override StartStopResult Start(Creature creature, Skill skill, MabiDictionary dict)
		{
			creature.Conditions.Activate(ConditionsA.ManaShield);
			Send.Effect(creature, Effect.ManaShield);

			creature.Regens.Add("ManaShield", Stat.Mana, -skill.RankData.Var2, creature.ManaMax);

			return StartStopResult.Okay;
		}

		/// <summary>
		/// Stops the skill.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="dict"></param>
		/// <returns></returns>
		public override StartStopResult Stop(Creature creature, Skill skill, MabiDictionary dict)
		{
			creature.Conditions.Deactivate(ConditionsA.ManaShield);

			creature.Regens.Remove("ManaShield");

			return StartStopResult.Okay;
		}

		/// <summary>
		/// Checks if target's Mana Shield is active, calculates mana
		/// damage, and sets target action's Mana Damage property if applicable.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="damage"></param>
		/// <param name="tAction"></param>
		public static void Handle(Creature target, ref float damage, TargetAction tAction)
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
	}
}
