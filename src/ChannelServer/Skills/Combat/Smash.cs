// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.World.Entities;
using Aura.Data.Database;
using Aura.Shared.Mabi.Const;
using Aura.Shared.Network;

namespace Aura.Channel.Skills.Combat
{
	[Skill(SkillId.Smash)]
	public class Smash : DefaultCombatSkill
	{
		private const int AfterUseStun = 600;
		private const float TwoHandBonusDamage = 1.20f;
		private const float TwoHandBonusCritChance = 1.05f;

		public override void Prepare(Creature creature, Skill skill, int castTime, Packet packet)
		{
			Send.Effect(creature, Effect.SkillInit, "flashing");
			Send.SkillPrepare(creature, skill.Info.Id, castTime);

			creature.Skills.ActiveSkill = skill;
		}

		public override void Ready(Creature creature, Skill skill, Packet packet)
		{
			Send.SkillReady(creature, skill.Info.Id, "");
		}

		public override void Complete(Creature creature, Skill skill, Packet packet)
		{
			Send.SkillComplete(creature, skill.Info.Id);
		}

		protected override float GetDamage(Creature attacker, Skill skill)
		{
			var result = attacker.GetRndTotalDamage();
			result *= skill.RankData.Var1 / 100f;

			// +dmg for 2H
			if (attacker.RightHand != null && attacker.RightHand.Data.Type == ItemType.Weapon2H)
				result *= TwoHandBonusDamage;

			return result;
		}

		protected override float GetCritChance(Creature attacker, Creature target, Skill skill)
		{
			var result = base.GetCritChance(attacker, target, skill);

			// +crit for 2H
			if (attacker.RightHand != null && attacker.RightHand.Data.Type == ItemType.Weapon2H)
				result *= TwoHandBonusCritChance;

			return result;
		}

		protected override short GetAttackerStunTime()
		{
			return 3000;
		}

		protected override short GetTargetStunTime()
		{
			return 3000;
		}

		protected override float GetKnockBack()
		{
			return 120f;
		}

		protected override int GetKnockBackDistance()
		{
			return 450;
		}

		protected override void Response(Creature attacker, Skill skill)
		{
			Send.SkillUseStun(attacker, skill.Info.Id, AfterUseStun, 1);
		}

		protected override void SetAttackerOptions(AttackerAction aAction)
		{
			aAction.Set(AttackerOptions.Result | AttackerOptions.KnockBackHit2);
		}

		protected override void SetTargetOptions(TargetAction tAction)
		{
			tAction.Set(TargetOptions.Result | TargetOptions.Smash);
		}

		protected override CombatActionType GetAttackerActionType()
		{
			return CombatActionType.HardHit;
		}

		protected override CombatActionType GetTargetActionType()
		{
			return CombatActionType.TakeHit;
		}

		protected override TargetOptions GetTargetDeathOptions()
		{
			return TargetOptions.Finished | TargetOptions.FinishingHit;
		}
	}
}
