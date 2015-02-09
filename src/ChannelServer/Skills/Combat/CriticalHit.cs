using Aura.Channel.Skills.Base;
using Aura.Shared.Mabi.Const;
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
		public void Init()
		{
			ChannelServer.Instance.Events.CreatureAttack += this.OnCreatureAttack;
		}

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
	}
}
