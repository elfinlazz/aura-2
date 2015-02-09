using Aura.Channel.Skills.Base;
using Aura.Channel.World.Entities;
using Aura.Shared.Mabi.Const;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.Skills.Combat
{
	/// <summary>
	/// Handles Critical Hit training.
	/// </summary>
	[Skill(SkillId.CriticalHit)]
	public class CriticalHit : ISkillHandler, IInitiableSkillHandler
	{
		/// <summary>
		/// Subscribes handler to required events.
		/// </summary>
		public void Init()
		{
			ChannelServer.Instance.Events.CreatureAttack += this.OnCreatureAttack;
		}

		/// <summary>
		/// Handles training based on what happened in the combat action.
		/// </summary>
		/// <param name="tAction"></param>
		private void OnCreatureAttack(TargetAction tAction)
		{
			if (!tAction.Has(TargetOptions.Critical))
				return;

			var attackerSkill = tAction.Attacker.Skills.Get(SkillId.CriticalHit);
			var targetSkill = tAction.Creature.Skills.Get(SkillId.CriticalHit);

			if (attackerSkill.Info.Rank == SkillRank.Novice)
			{
				if (tAction.Is(CombatActionType.CounteredHit2))
					attackerSkill.Train(1); // Novice -> RF
			}
			else
			{
				attackerSkill.Train(1); // Land a critical hit.

				if (tAction.Creature.IsDead)
					attackerSkill.Train(3); // Finish off with critical hit.
			}

			if (targetSkill != null && targetSkill.Info.Rank >= SkillRank.RF)
				attackerSkill.Train(2); // Learn from enemy's critical hit.
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
		public static void Handle(Creature attacker, float critChance, ref float damage, TargetAction tAction, bool bypassNoviceCheck = false)
		{
			// Check if attacker actually has critical hit
			var critSkill = attacker.Skills.Get(SkillId.CriticalHit);
			if (critSkill == null || (critSkill.Info.Rank == SkillRank.Novice && !bypassNoviceCheck))
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
