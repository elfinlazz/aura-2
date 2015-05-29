// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.Skills.Magic;
using Aura.Channel.World.Entities;
using Aura.Mabi.Const;
using Aura.Mabi.Network;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.Skills.Combat
{
	[Skill(SkillId.RangedAttack)]
	public class RangedAttack : ISkillHandler, IPreparable, IReadyable, ICompletable, ICancelable, ICombatSkill
	{
		private const int KnockBackDistance = 400;
		private const float StabilityReduction = 45f;

		public bool Prepare(Creature creature, Skill skill, Packet packet)
		{
			Send.SkillPrepare(creature, skill.Info.Id, skill.GetCastTime());

			return true;
		}

		public bool Ready(Creature creature, Skill skill, Packet packet)
		{
			//skill.Stacks = 1;

			Send.SkillReady(creature, skill.Info.Id);

			return true;
		}

		public void Complete(Creature creature, Skill skill, Packet packet)
		{
			Log.Debug("Complete");
			Send.CombatSetAimR(creature, 0, SkillId.None, 0);
			Send.SkillComplete(creature, skill.Info.Id);
		}

		public void Cancel(Creature creature, Skill skill)
		{
			Send.CombatSetAimR(creature, 0, SkillId.None, 0);
		}

		public CombatSkillResult Use(Creature attacker, Skill skill, long targetEntityId)
		{
			// Get target
			var target = attacker.Region.GetCreature(targetEntityId);
			if (target == null)
				return CombatSkillResult.InvalidTarget;

			var targetPos = target.GetPosition();
			var attackerPos = attacker.GetPosition();

			// Check range
			if (!attackerPos.InRange(targetPos, attacker.RightHand.OptionInfo.EffectiveRange + 100))
				return CombatSkillResult.OutOfRange;

			// Calculate chance
			// Unofficial, but seems to be close.
			var rnd = RandomProvider.Get();

			var distance = attackerPos.GetDistance(targetPos);
			var aimTime = (DateTime.Now - attacker.Temp.AimStart).TotalMilliseconds;
			var baseAimTime = 1000f;
			var fullAimTime = ((baseAimTime + distance) / 99f * (99f * 2));

			var chance = Math.Min(99f, 99f / (baseAimTime + distance) * aimTime);

			if (aimTime >= fullAimTime)
				chance = 100;

			//Log.Debug("{0}ms (distance: {1})", (DateTime.Now - attacker.Temp.AimStart).TotalMilliseconds, attackerPos.GetDistance(targetPos));

			if (target.IsMoving)
			{
				if (target.IsWalking)
					chance = Math.Min(95, chance);
				else
					chance = Math.Min(90, chance);
			}

			if (attacker.Titles.SelectedTitle == 60001)
				Send.ServerMessage(attacker, "Debug: Aim {0}", chance);

			// Actions
			var cap = new CombatActionPack(attacker, skill.Info.Id);

			var aAction = new AttackerAction(CombatActionType.RangeHit, attacker, skill.Info.Id, targetEntityId);
			aAction.Set(AttackerOptions.Result);
			aAction.Stun = 800;
			cap.Add(aAction);

			if (rnd.NextDouble() * 100 < chance)
			{
				var tAction = new TargetAction(CombatActionType.TakeHit, target, attacker, skill.Info.Id);
				tAction.Set(TargetOptions.Result);
				tAction.Stun = 2100;
				cap.Add(tAction);

				// Damage
				var damage = attacker.GetRndRangedDamage();

				// Critical Hit
				var critChance = attacker.GetRightCritChance(target.Protection);
				CriticalHit.Handle(attacker, critChance, ref damage, tAction);

				// Subtract target def/prot
				SkillHelper.HandleDefenseProtection(target, ref damage);

				// Defense
				Defense.Handle(aAction, tAction, ref damage);

				// Mana Shield
				ManaShield.Handle(target, ref damage, tAction);

				// Deal with it!
				if (damage > 0)
					target.TakeDamage(tAction.Damage = damage, attacker);

				// Death/Knockback
				if (target.IsDead)
				{
					tAction.Set(TargetOptions.FinishingKnockDown);
					attacker.Shove(target, KnockBackDistance);
				}
				else
				{
					if (target.IsKnockedDown)
					{
						tAction.Stun = 0;
					}
					else if (target.IsUnstable)
					{
						tAction.Set(TargetOptions.KnockDown);
					}
					else
					{
						target.Stability -= StabilityReduction;
						if (target.IsUnstable)
						{
							tAction.Set(TargetOptions.KnockBack);
							attacker.Shove(target, KnockBackDistance);
						}
					}
				}
			}

			// Reduce arrows
			// TODO: option
			if (attacker.Magazine != null)
				attacker.Inventory.Decrement(attacker.Magazine);

			// "Cancels" the skill
			Send.SkillUse(attacker, skill.Info.Id, 800, 1); // 800 = old load time? == aAction.Stun?

			cap.Handle();

			return CombatSkillResult.Okay;
		}
	}
}
