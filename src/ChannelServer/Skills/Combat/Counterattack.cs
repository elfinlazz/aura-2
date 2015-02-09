// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.World;
using Aura.Channel.World.Entities;
using Aura.Shared.Mabi.Const;
using Aura.Shared.Network;

namespace Aura.Channel.Skills.Combat
{
	/// <summary>
	/// Handler for the Counterattack skill.
	/// </summary>
	/// <remarks>
	/// Var 1: Target damage rate
	/// Var 2: Attacker damage rate
	/// Var 3: Crit bonus
	/// </remarks>
	[Skill(SkillId.Counterattack)]
	public class Counterattack : StandardPrepareHandler
	{
		private const short StunTime = 3000;
		private const int KnockbackDistance = 450;

		public override void Prepare(Creature creature, Skill skill, int castTime, Packet packet)
		{
			Send.SkillFlashEffect(creature);
			Send.SkillPrepare(creature, skill.Info.Id, castTime);

			creature.Skills.ActiveSkill = skill;
		}

		public override void Ready(Creature creature, Skill skill, Packet packet)
		{
			Send.SkillReady(creature, skill.Info.Id);

			// Training
			if (skill.Info.Rank == SkillRank.RF)
				skill.Train(1); // Use Counterattack.
		}

		/// <summary>
		/// Returns true if target has counter active and used it.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="attacker"></param>
		/// <returns></returns>
		public static bool Handle(Creature target, Creature attacker)
		{
			if (!target.Skills.IsActive(SkillId.Counterattack))
				return false;

			var handler = ChannelServer.Instance.SkillManager.GetHandler<Counterattack>(SkillId.Counterattack);
			handler.Use(target, attacker);

			return true;
		}

		public void Use(Creature attacker, Creature target)
		{
			var skill = attacker.Skills.Get(SkillId.Counterattack);

			var aAction = new AttackerAction(CombatActionType.RangeHit, attacker, SkillId.Counterattack, target.EntityId);
			aAction.Options |= AttackerOptions.Result | AttackerOptions.KnockBackHit2;

			var tAction = new TargetAction(CombatActionType.CounteredHit2, target, attacker, target.Skills.IsActive(SkillId.Smash) ? SkillId.Smash : SkillId.CombatMastery);
			tAction.Options |= TargetOptions.Result | TargetOptions.Smash;

			var cap = new CombatActionPack(attacker, skill.Info.Id);
			cap.Add(aAction, tAction);

			var damage =
				(attacker.GetRndTotalDamage() * (skill.RankData.Var2 / 100f)) +
				(target.GetRndTotalDamage() * (skill.RankData.Var1 / 100f));

			CriticalHit.Handle(attacker, (target.GetCritChanceFor(attacker) + skill.RankData.Var3), ref damage, tAction, true);
			SkillHelper.HandleDefenseProtection(target, ref damage, true, true);

			target.TakeDamage(tAction.Damage = damage, attacker);

			if (target.IsDead)
				tAction.Options |= TargetOptions.FinishingKnockDown;

			aAction.Stun = StunTime;
			tAction.Stun = StunTime;

			var targetPosition = target.GetPosition();
			Position intersection;
			var knockbackPos = attacker.GetPosition().GetRelative(targetPosition, KnockbackDistance);
			if (target.Region.Collisions.Find(targetPosition, knockbackPos, out intersection))
				knockbackPos = targetPosition.GetRelative(intersection, -50);

			target.SetPosition(knockbackPos.X, knockbackPos.Y);

			// Update both weapons
			SkillHelper.UpdateWeapon(attacker, target, attacker.RightHand, attacker.LeftHand);

			Send.SkillUseStun(attacker, skill.Info.Id, StunTime, 1);

			this.Training(aAction, tAction);

			cap.Handle();
		}

		public void Training(AttackerAction aAction, TargetAction tAction)
		{
			var attackerSkill = aAction.Creature.Skills.Get(SkillId.Counterattack);
			var targetSkill = tAction.Creature.Skills.Get(SkillId.Counterattack);

			if (attackerSkill.Info.Rank == SkillRank.RF)
			{
				attackerSkill.Train(2); // Successfully counter enemy's attack.

				if (tAction.SkillId == SkillId.Smash)
					attackerSkill.Train(4); // Counter enemy's special attack.

				if (tAction.Has(TargetOptions.Critical))
					attackerSkill.Train(5); // Counter with critical hit.
			}
			else
			{
				attackerSkill.Train(1); // Successfully counter enemy's attack.

				if (tAction.SkillId == SkillId.Smash)
					attackerSkill.Train(2); // Counter enemy's special attack.

				if (tAction.Has(TargetOptions.Critical))
					attackerSkill.Train(4); // Counter with critical hit.
			}

			if (targetSkill != null)
				targetSkill.Train(3); // Learn from the enemy's counter attack.
			else
				tAction.Creature.Skills.Give(SkillId.Counterattack, SkillRank.Novice); // Obtaining the Skill
		}
	}
}
