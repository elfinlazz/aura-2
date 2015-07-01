// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.World;
using Aura.Channel.World.Entities;
using Aura.Data;
using Aura.Mabi.Const;
using Aura.Mabi.Network;
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

			// Default lock is Walk|Run,  since renovation you are stopped when
			// loading counter, previously you kept running till you were at your
			// destination.
			if (AuraData.FeaturesDb.IsEnabled("TalentRenovationCloseCombat"))
				creature.StopMove();

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

			// Default lock is Run, lock Walk if no combat weapon is equipped.
			if (AuraData.FeaturesDb.IsEnabled("TalentRenovationCloseCombat"))
			{
				if (creature.RightHand == null || !creature.RightHand.HasTag("/weapontype_combat/"))
					creature.Lock(Locks.Walk);
			}
			// Tell client to lock any movement if renovation isn't enabled.
			else
				creature.Lock(Locks.Move, true);

			// Training
			if (skill.Info.Rank == SkillRank.RF)
				skill.Train(1); // Use Counterattack.

			return true;
		}

		/// <summary>
		/// Resets the skill's cooldown in old combat.
		/// </summary>
		/// <remarks>
		/// Counter doesn't use the new cooldown system, but Vars, similar
		/// to Final Hit. Var10 is the cooldown for normal hits, Var11 is for
		/// knuckles. That's why we have to reset it here.
		/// </remarks>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public override void Complete(Creature creature, Skill skill, Packet packet)
		{
			base.Complete(creature, skill, packet);

			if (!AuraData.FeaturesDb.IsEnabled("CombatSystemRenewal"))
				Send.ResetCooldown(creature, skill.Info.Id);
		}

		/// <summary>
		/// Cancels special effects.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		public override void Cancel(Creature creature, Skill skill)
		{
			// Updating unlock because of the updating lock for pre-renovation.
			// Since moving isn't locked by default when using a skill it's
			// apparently not unlocked by default either.
			if (!AuraData.FeaturesDb.IsEnabled("TalentRenovationCloseCombat"))
				creature.Unlock(Locks.Move, true);
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
			// Updating unlock because of the updating lock for pre-renovation
			// Has to be done here because we can't have an updating unlock
			// after the combat action, it resets the stun.
			if (!AuraData.FeaturesDb.IsEnabled("TalentRenovationCloseCombat"))
				attacker.Unlock(Locks.Move, true);

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

			var critChance = attacker.GetTotalCritChance(target.Protection) + skill.RankData.Var3;

			CriticalHit.Handle(attacker, critChance, ref damage, tAction, true);
			SkillHelper.HandleDefenseProtection(target, ref damage, true, true);

			target.TakeDamage(tAction.Damage = damage, attacker);

			target.Aggro(attacker);

			if (target.IsDead)
				tAction.Options |= TargetOptions.FinishingKnockDown;

			aAction.Stun = StunTime;
			tAction.Stun = StunTime;

			target.Stability = Creature.MinStability;
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
