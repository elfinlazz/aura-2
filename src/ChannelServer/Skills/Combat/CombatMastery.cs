// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Channel.Skills.Base;
using Aura.Shared.Mabi.Const;
using Aura.Channel.World.Entities;
using Aura.Shared.Util;
using Aura.Data.Database;
using Aura.Channel.World;

namespace Aura.Channel.Skills.Combat
{
	/// <summary>
	/// Combat Mastery
	/// </summary>
	/// <remarks>
	/// Normal attack for 99% of all races.
	/// </remarks>
	[Skill(SkillId.CombatMastery)]
	public class CombatMastery : ICombatSkill
	{
		private const int KnockBackDistance = 450;

		public CombatSkillResult Use(Creature attacker, Skill skill, long targetEntityId)
		{
			if (attacker.IsStunned)
				return CombatSkillResult.Okay;

			var target = attacker.Region.GetCreature(targetEntityId);
			if (target == null)
				return CombatSkillResult.Okay;

			if (!attacker.GetPosition().InRange(target.GetPosition(), attacker.AttackRangeFor(target)))
				return CombatSkillResult.OutOfRange;

			attacker.StopMove();
			var targetPosition = target.StopMove();

			var rightWeapon = attacker.Inventory.RightHand;
			var leftWeapon = attacker.Inventory.LeftHand;
			var magazine = attacker.Inventory.Magazine;
			var dualWield = (rightWeapon != null && leftWeapon != null);
			var maxHits = (byte)(dualWield ? 2 : 1);
			int prevId = 0;

			for (byte i = 1; i <= maxHits; ++i)
			{
				var weapon = (i == 1 ? rightWeapon : leftWeapon);

				var cap = new CombatActionPack(attacker, skill.Info.Id);
				var aAction = new AttackerAction(CombatActionType.Hit, attacker, skill.Info.Id, targetEntityId);
				var tAction = new TargetAction(CombatActionType.TakeHit, target, attacker, skill.Info.Id);
				cap.Add(aAction, tAction);

				cap.Hit = i;
				cap.HitsMax = maxHits;
				cap.PrevCombatActionId = prevId;
				prevId = cap.CombatActionId;

				aAction.Set(AttackerOptions.Result);
				if (dualWield)
					aAction.Set(AttackerOptions.DualWield);

				var damage = attacker.GetRndDamage(weapon);
				tAction.Damage = damage;

				target.TakeDamage(tAction.Damage, attacker);

				if (!target.IsDead)
				{
					target.KnockBack += this.GetKnockBack(weapon) / maxHits;
					if (target.KnockBack >= 100 && target.Is(RaceStands.KnockBackable))
						tAction.Set(tAction.Has(TargetOptions.Critical) ? TargetOptions.KnockDown : TargetOptions.KnockBack);
				}
				else
				{
					tAction.Set(TargetOptions.FinishingKnockDown);
				}

				if (tAction.IsKnockBack)
				{
					var newPos = attacker.GetPosition().GetRelative(targetPosition, KnockBackDistance);

					Position intersection;
					if (target.Region.Collissions.Find(targetPosition, newPos, out intersection))
						newPos = targetPosition.GetRelative(intersection, -50);

					target.SetPosition(newPos.X, newPos.Y);

					aAction.Set(AttackerOptions.KnockBackHit2);

					cap.HitsMax = cap.Hit;
				}

				aAction.StunTime = this.GetAttackerStun(weapon, tAction.IsKnockBack);
				tAction.StunTime = this.GetTargetStun(weapon, tAction.IsKnockBack);

				cap.Handle();

				if (tAction.IsKnockBack)
					break;
			}

			return CombatSkillResult.Okay;
		}

		/// <summary>
		/// Returns stun time for the attacker.
		/// </summary>
		/// <param name="weaponSpeed"></param>
		/// <param name="knockback"></param>
		/// <returns></returns>
		public short GetAttackerStun(Item weapon, bool knockback)
		{
			if (weapon == null)
				return (!knockback ? (short)CombatStunAttacker.Normal : (short)CombatKnockbackStunAttacker.Normal);

			switch (weapon.Data.AttackSpeed)
			{
				case 00: return (!knockback ? (short)CombatStunAttacker.VeryFast : (short)CombatKnockbackStunAttacker.VeryFast);
				case 01: return (!knockback ? (short)CombatStunAttacker.Fast : (short)CombatKnockbackStunAttacker.Fast);
				case 02: return (!knockback ? (short)CombatStunAttacker.Normal : (short)CombatKnockbackStunAttacker.Normal);
				case 03: return (!knockback ? (short)CombatStunAttacker.Slow : (short)CombatKnockbackStunAttacker.Slow);
				default: return (!knockback ? (short)CombatStunAttacker.VerySlow : (short)CombatKnockbackStunAttacker.VerySlow);
			}
		}

		/// <summary>
		/// Returns stun time for the target.
		/// </summary>
		/// <param name="weaponSpeed"></param>
		/// <param name="knockback"></param>
		/// <returns></returns>
		public short GetTargetStun(Item weapon, bool knockback)
		{
			if (weapon == null)
				return (!knockback ? (short)CombatStunTarget.Normal : (short)CombatKnockbackStunTarget.Normal);

			switch (weapon.Data.AttackSpeed)
			{
				case 00: return (!knockback ? (short)CombatStunTarget.VeryFast : (short)CombatKnockbackStunTarget.VeryFast);
				case 01: return (!knockback ? (short)CombatStunTarget.Fast : (short)CombatKnockbackStunTarget.Fast);
				case 02: return (!knockback ? (short)CombatStunTarget.Normal : (short)CombatKnockbackStunTarget.Normal);
				case 03: return (!knockback ? (short)CombatStunTarget.Slow : (short)CombatKnockbackStunTarget.Slow);
				default: return (!knockback ? (short)CombatStunTarget.VerySlow : (short)CombatKnockbackStunTarget.VerySlow);
			}
		}

		/// <summary>
		/// Returns knock down increase for weapon.
		/// </summary>
		/// <remarks>
		/// http://wiki.mabinogiworld.com/view/Knock_down_gauge#Knockdown_Timer_Rates
		/// </remarks>
		/// <param name="weapon"></param>
		/// <returns></returns>
		public float GetKnockBack(Item weapon)
		{
			var count = weapon != null ? weapon.Info.KnockCount + 1 : 3;
			var speed = weapon != null ? (AttackSpeed)weapon.Data.AttackSpeed : AttackSpeed.Normal;

			switch (count)
			{
				default:
				case 1:
					return 100;
				case 2:
					switch (speed)
					{
						default:
						case AttackSpeed.VerySlow: return 70;
						case AttackSpeed.Slow: return 68;
						case AttackSpeed.Normal: return 68; // ?
						case AttackSpeed.Fast: return 68; // ?
					}
				case 3:
					switch (speed)
					{
						default:
						case AttackSpeed.VerySlow: return 60;
						case AttackSpeed.Slow: return 56; // ?
						case AttackSpeed.Normal: return 53;
						case AttackSpeed.Fast: return 50;
					}
				case 5:
					switch (speed)
					{
						default:
						case AttackSpeed.Fast: return 40; // ?
						case AttackSpeed.VeryFast: return 35; // ?
					}
			}
		}
	}
}
