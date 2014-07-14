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
	public class Smash : CombatSkillHandler
	{
		private const int StunTime = 3000;
		private const int AfterUseStun = 600;
		private const float Knockback = 120;
		private const int KnockbackDistance = 450;

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

			// Counter...

			// Apply damage
			target.TakeDamage(tAction.Damage = damage, attacker);

			if (target.IsDead)
				tAction.Set(TargetOptions.FinishingHit | TargetOptions.Finished);

			// Set Stun/Knockback
			attacker.Stun = aAction.Stun = StunTime;
			target.Stun = tAction.Stun = StunTime;
			target.KnockBack = Knockback;

			// Check collissions
			Position intersection;
			var knockbackPos = attacker.GetPosition().GetRelative(targetPosition, KnockbackDistance);
			if (target.Region.Collissions.Find(targetPosition, knockbackPos, out intersection))
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
			var result = (attacker.CriticalBase - target.Protection);

			// +5% crit for 2H
			if (attacker.RightHand != null && attacker.RightHand.Data.Type == ItemType.Weapon2H)
				result *= 1.05f;

			return result;
		}
	}
}
