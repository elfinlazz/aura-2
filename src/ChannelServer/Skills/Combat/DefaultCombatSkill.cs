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

namespace Aura.Channel.Skills.Combat
{
	public abstract class DefaultCombatSkill : CombatSkillHandler
	{
		protected const int DefaultKnockBackDistance = 450;

		public override void Prepare(Creature creature, Skill skill, int castTime, Packet packet)
		{
			throw new NotImplementedException();
		}

		public override void Ready(Creature creature, Skill skill, Packet packet)
		{
			throw new NotImplementedException();
		}

		public override void Complete(Creature creature, Skill skill, Packet packet)
		{
			throw new NotImplementedException();
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

			// Preapare random
			var rnd = RandomProvider.Get();

			// Stop movement
			// if...
			attacker.StopMove();
			target.StopMove();

			// Prepare combat actions
			var aAction = new AttackerAction(this.GetAttackerActionType(), attacker, skill.Info.Id, targetEntityId);
			this.SetAttackerOptions(aAction);

			var tAction = new TargetAction(this.GetTargetActionType(), target, attacker, skill.Info.Id);
			this.SetTargetOptions(tAction);

			var cap = new CombatActionPack(attacker, skill.Info.Id, aAction, tAction);

			// Calculate damage
			var damage = this.GetDamage(attacker, skill);
			var critChance = this.GetCritChance(attacker, target, skill);

			// Add crit
			if (rnd.NextDouble() <= critChance)
			{
				damage *= 1.5f;
				tAction.Set(TargetOptions.Critical);
			}

			// Subtract target def/prot
			damage = Math.Max(1, damage - target.Defense);
			if (damage > 1)
				damage = Math.Max(1, damage - (damage * target.Protection));

			// Mana Shield...

			// Apply damage
			target.TakeDamage(tAction.Damage = damage, attacker);

			// Cheack vitals
			if (!target.IsDead)
			{
				target.KnockBack += this.GetKnockBack();
				if (target.KnockBack >= 100 && target.Is(RaceStands.KnockBackable))
					tAction.Set(tAction.Has(TargetOptions.Critical) ? TargetOptions.KnockDown : TargetOptions.KnockBack);
			}
			else
			{
				tAction.Set(this.GetTargetDeathOptions());
			}

			// React to knock back
			if (tAction.IsKnockBack)
			{
				var knockbackPos = attacker.GetPosition().GetRelative(targetPosition, this.GetKnockBackDistance());

				// Check collissions
				Position intersection;
				if (target.Region.Collissions.Find(targetPosition, knockbackPos, out intersection))
					knockbackPos = targetPosition.GetRelative(intersection, -50);

				target.SetPosition(knockbackPos.X, knockbackPos.Y);

				aAction.Set(AttackerOptions.KnockBackHit2);
			}

			// Set Stun
			attacker.Stun = aAction.Stun = this.GetAttackerStunTime();
			target.Stun = tAction.Stun = this.GetTargetStunTime();

			// Response
			this.Response(attacker, skill);

			// Action!
			cap.Handle();

			return CombatSkillResult.Okay;
		}

		/// <summary>
		/// Returns raw damage dealt to main target
		/// </summary>
		/// <param name="attacker"></param>
		/// <param name="skill"></param>
		/// <returns></returns>
		protected abstract float GetDamage(Creature attacker, Skill skill);

		/// <summary>
		///  Returns crit chance of attacker towards target,
		///  defaults to attacker crit - target protection.
		/// </summary>
		/// <param name="attacker"></param>
		/// <param name="target"></param>
		/// <param name="skill"></param>
		/// <returns></returns>
		protected virtual float GetCritChance(Creature attacker, Creature target, Skill skill)
		{
			var result = (attacker.CriticalBase - target.Protection);

			return result;
		}

		/// <summary>
		/// Returns stun time in ms for attacker after attack.
		/// </summary>
		/// <returns></returns>
		protected abstract short GetAttackerStunTime();

		/// <summary>
		/// Returns stun time in ms for target after attack.
		/// </summary>
		/// <returns></returns>
		protected abstract short GetTargetStunTime();

		/// <summary>
		/// Returns knockback amount for target after attack.
		/// </summary>
		/// <returns></returns>
		protected abstract float GetKnockBack();

		/// <summary>
		/// Returns knockback distance for target.
		/// </summary>
		/// <returns></returns>
		protected virtual int GetKnockBackDistance()
		{
			return DefaultKnockBackDistance;
		}

		/// <summary>
		/// Called right before handling of the action pack begins,
		/// use to send SkillUse and similar things.
		/// </summary>
		/// <param name="attacker"></param>
		/// <param name="skill"></param>
		protected abstract void Response(Creature attacker, Skill skill);

		/// <summary>
		/// Sets options for attacker action.
		/// </summary>
		/// <param name="aAction"></param>
		protected virtual void SetAttackerOptions(AttackerAction aAction)
		{
		}

		/// <summary>
		/// Sets options for target action.
		/// </summary>
		/// <param name="tAction"></param>
		protected virtual void SetTargetOptions(TargetAction tAction)
		{
		}

		protected abstract CombatActionType GetAttackerActionType();
		protected abstract CombatActionType GetTargetActionType();

		protected abstract TargetOptions GetTargetDeathOptions();
	}
}
