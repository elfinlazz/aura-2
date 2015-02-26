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
		/// <summary>
		/// Time in milliseconds that attacker and creature are stunned for
		/// after use.
		/// </summary>
		private const short StunTime = 3000;

		/// <summary>
		/// Units the enemy is knocked back.
		/// </summary>
		private const int KnockbackDistance = 450;

		/// <summary>
		/// Handles skill preparation.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public override bool Prepare(Creature creature, Skill skill, Packet packet)
		{
			Send.SkillFlashEffect(creature);
			Send.SkillPrepare(creature, skill.Info.Id, skill.GetCastTime());

			return true;
		}

		/// <summary>
		/// Handles redying the skill, called when finishing casting it.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public override bool Ready(Creature creature, Skill skill, Packet packet)
		{
			Send.SkillReady(creature, skill.Info.Id);

			// Training
			if (skill.Info.Rank == SkillRank.RF)
				skill.Train(1); // Use Counterattack.

			return true;
		}

		/// <summary>
		/// Returns true if target has counter active and used it.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="attacker"></param>
		/// <returns></returns>
		public static bool Handle(Creature target, Creature attacker)
		{
			if (!target.Skills.IsReady(SkillId.Counterattack))
				return false;

			var handler = ChannelServer.Instance.SkillManager.GetHandler<Counterattack>(SkillId.Counterattack);
			handler.Use(target, attacker);

			// TODO: Centralize this so we don't have to maintain the active
			//   skill and the regens in multiple places.
			// TODO: Remove the need for this null check... AIs reset ActiveSkill
			//   in Complete, which is called from the combat action handler
			//   before we get back here.
			if (target.Skills.ActiveSkill != null)
				target.Skills.ActiveSkill.State = SkillState.Used;
			target.Regens.Remove("ActiveSkillWait");

			return true;
		}

		/// <summary>
		/// Handles usage of the skill.
		/// </summary>
		/// <param name="attacker"></param>
		/// <param name="target"></param>
		public void Use(Creature attacker, Creature target)
		{
			var skill = attacker.Skills.Get(SkillId.Counterattack);

			var aAction = new AttackerAction(CombatActionType.RangeHit, attacker, SkillId.Counterattack, target.EntityId);
			aAction.Options |= AttackerOptions.Result | AttackerOptions.KnockBackHit2;

			var tAction = new TargetAction(CombatActionType.CounteredHit2, target, attacker, target.Skills.IsReady(SkillId.Smash) ? SkillId.Smash : SkillId.CombatMastery);
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

			attacker.Shove(target, KnockbackDistance);

			// Update both weapons
			SkillHelper.UpdateWeapon(attacker, target, attacker.RightHand, attacker.LeftHand);

			Send.SkillUseStun(attacker, skill.Info.Id, StunTime, 1);

			this.Training(aAction, tAction);

			cap.Handle();
		}

		/// <summary>
		/// Trains the skill for attacker and target, based on what happened.
		/// </summary>
		/// <param name="aAction"></param>
		/// <param name="tAction"></param>
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
			else if (tAction.Creature.LearningSkillsEnabled)
				tAction.Creature.Skills.Give(SkillId.Counterattack, SkillRank.Novice); // Obtaining the Skill
		}
	}
}
