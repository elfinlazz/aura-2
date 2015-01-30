// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.World;
using Aura.Channel.World.Entities;
using Aura.Data.Database;
using Aura.Shared.Mabi.Const;
using Aura.Shared.Network;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.Skills.Combat
{
	[Skill(SkillId.Smash)]
	public class Smash : CombatSkillHandler, IInitiableSkillHandler
	{
		private const int StunTime = 3000;
		private const int AfterUseStun = 600;
		private const float Knockback = 120;
		private const int KnockbackDistance = 450;

		public void Init()
		{
			ChannelServer.Instance.Events.CreatureAttackedByPlayer += this.OnCreatureAttackedByPlayer;
		}

		public override void Prepare(Creature creature, Skill skill, int castTime, Packet packet)
		{
			Send.SkillFlashEffect(creature);
			Send.SkillPrepare(creature, skill.Info.Id, castTime);

			creature.Skills.ActiveSkill = skill;
		}

		public override void Ready(Creature creature, Skill skill, Packet packet)
		{
			Send.SkillReady(creature, skill.Info.Id);
		}

		public override void Complete(Creature creature, Skill skill, Packet packet)
		{
			Send.SkillComplete(creature, skill.Info.Id);
		}

		public override CombatSkillResult Use(Creature attacker, Skill skill, long targetEntityId)
		{
			// Check target
			var target = attacker.Region.GetCreature(targetEntityId);
			if (target == null)
				return CombatSkillResult.InvalidTarget;

			// Check range
			var targetPosition = target.GetPosition();
			if (!attacker.GetPosition().InRange(targetPosition, attacker.AttackRangeFor(target)))
				return CombatSkillResult.OutOfRange;

			// Stop movement
			attacker.StopMove();
			target.StopMove();

			// Prepare combat actions
			var aAction = new AttackerAction(CombatActionType.HardHit, attacker, skill.Info.Id, targetEntityId);
			aAction.Set(AttackerOptions.Result | AttackerOptions.KnockBackHit2);

			var tAction = new TargetAction(CombatActionType.TakeHit, target, attacker, skill.Info.Id);
			tAction.Set(TargetOptions.Result | TargetOptions.Smash);

			var cap = new CombatActionPack(attacker, skill.Info.Id, aAction, tAction);

			// Calculate damage
			var damage = this.GetDamage(attacker, skill);
			var critChance = this.GetCritChance(attacker, target, skill);

			// Critical Hit
			SkillHelper.HandleCritical(attacker, critChance, ref damage, tAction);

			// Subtract target def/prot
			SkillHelper.HandleDefenseProtection(target, ref damage);

			// Mana Shield
			SkillHelper.HandleManaShield(target, ref damage, tAction);

			// Counter...

			// Apply damage
			target.TakeDamage(tAction.Damage = damage, attacker);

			if (target.IsDead)
				tAction.Set(TargetOptions.FinishingHit | TargetOptions.Finished);

			// Set Stun/Knockback
			attacker.Stun = aAction.Stun = StunTime;
			target.Stun = tAction.Stun = StunTime;
			target.KnockBack = Knockback;

			// Check collisions
			Position intersection;
			var knockbackPos = attacker.GetPosition().GetRelative(targetPosition, KnockbackDistance);
			if (target.Region.Collisions.Find(targetPosition, knockbackPos, out intersection))
				knockbackPos = targetPosition.GetRelative(intersection, -50);

			// Set knockbacked position
			target.SetPosition(knockbackPos.X, knockbackPos.Y);

			// Response
			Send.SkillUseStun(attacker, skill.Info.Id, AfterUseStun, 1);

			// Action!
			cap.Handle();

			return CombatSkillResult.Okay;
		}

		protected float GetDamage(Creature attacker, Skill skill)
		{
			var result = attacker.GetRndTotalDamage();
			result *= skill.RankData.Var1 / 100f;

			// +20% dmg for 2H
			if (attacker.RightHand != null && attacker.RightHand.Data.Type == ItemType.Weapon2H)
				result *= 1.20f;

			return result;
		}

		protected float GetCritChance(Creature attacker, Creature target, Skill skill)
		{
			var result = attacker.GetCritChanceFor(target);

			// +5% crit for 2H
			if (attacker.RightHand != null && attacker.RightHand.Data.Type == ItemType.Weapon2H)
				result *= 1.05f;

			return result;
		}

		/// <summary>
		/// Training, called when someone attacks something.
		/// </summary>
		/// <param name="action"></param>
		public void OnCreatureAttackedByPlayer(TargetAction action)
		{
			// Only train if used skill was Smash
			if (action.SkillId != SkillId.Smash)
				return;

			// Get skill
			var attackerSkill = action.Attacker.Skills.Get(SkillId.Smash);
			if (attackerSkill == null) return; // Should be impossible.

			// Learning by attacking
			switch (attackerSkill.Info.Rank)
			{
				case SkillRank.RF:
				case SkillRank.RE:
					attackerSkill.Train(1); // Use the skill successfully.
					if (action.Has(TargetOptions.Critical)) attackerSkill.Train(2); // Critical Hit with Smash.
					if (action.Creature.IsDead) attackerSkill.Train(3); // Finishing blow with Smash.
					break;

				case SkillRank.RD:
				case SkillRank.RC:
				case SkillRank.RB:
				case SkillRank.RA:
				case SkillRank.R9:
				case SkillRank.R8:
				case SkillRank.R7:
					if (action.Has(TargetOptions.Critical) && action.Creature.IsDead)
						attackerSkill.Train(4); // Finishing blow with Critical Hit.
					goto case SkillRank.RF;

				case SkillRank.R6:
				case SkillRank.R5:
				case SkillRank.R4:
				case SkillRank.R3:
				case SkillRank.R2:
				case SkillRank.R1:
					if (action.Has(TargetOptions.Critical)) attackerSkill.Train(1); // Critical Hit with Smash.
					if (action.Creature.IsDead) attackerSkill.Train(2); // Finishing blow with Smash.
					if (action.Has(TargetOptions.Critical) && action.Creature.IsDead) attackerSkill.Train(3); // Finishing blow with Critical Hit.
					break;
			}
		}
	}
}
