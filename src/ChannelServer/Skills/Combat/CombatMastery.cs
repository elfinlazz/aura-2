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
using Aura.Channel.Skills.Life;
using Aura.Shared.Mabi;

namespace Aura.Channel.Skills.Combat
{
	/// <summary>
	/// Combat Mastery
	/// </summary>
	/// <remarks>
	/// Normal attack for 99% of all races.
	/// </remarks>
	[Skill(SkillId.CombatMastery)]
	public class CombatMastery : ICombatSkill, IInitiableSkillHandler
	{
		private const int KnockBackDistance = 450;

		public void Init()
		{
			ChannelServer.Instance.Events.CreatureAttackedByPlayer += this.OnCreatureAttackedByPlayer;
		}

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

				var aAction = new AttackerAction(CombatActionType.Hit, attacker, skill.Info.Id, targetEntityId);
				var tAction = new TargetAction(CombatActionType.TakeHit, target, attacker, skill.Info.Id);

				var cap = new CombatActionPack(attacker, skill.Info.Id, aAction, tAction);
				cap.Hit = i;
				cap.MaxHits = maxHits;
				cap.PrevId = prevId;
				prevId = cap.Id;

				// Default attacker options
				aAction.Set(AttackerOptions.Result);
				if (dualWield)
					aAction.Set(AttackerOptions.DualWield);

				// Base damage
				var damage = attacker.GetRndDamage(weapon);

				// Critical Hit
				SkillHelper.HandleCritical(attacker, attacker.GetCritChanceFor(target), ref damage, tAction);

				// Subtract target def/prot
				SkillHelper.HandleDefenseProtection(target, ref damage);

				// Defense
				SkillHelper.HandleDefense(aAction, tAction, ref damage);

				// Mana Shield
				SkillHelper.HandleManaShield(target, ref damage, tAction);

				// Deal with it!
				if (damage > 0)
					target.TakeDamage(tAction.Damage = damage, attacker);

				// Evaluate caused damage
				if (!target.IsDead)
				{
					if (tAction.Type != CombatActionType.Defended)
					{
						target.KnockBack += this.GetKnockBack(weapon) / maxHits;
						if (target.KnockBack >= 100 && target.Is(RaceStands.KnockBackable))
							tAction.Set(tAction.Has(TargetOptions.Critical) ? TargetOptions.KnockDown : TargetOptions.KnockBack);
					}
				}
				else
				{
					tAction.Set(TargetOptions.FinishingKnockDown);
				}

				// React to knock back
				if (tAction.IsKnockBack)
				{
					var newPos = attacker.GetPosition().GetRelative(targetPosition, KnockBackDistance);

					Position intersection;
					if (target.Region.Collisions.Find(targetPosition, newPos, out intersection))
						newPos = targetPosition.GetRelative(intersection, -50);

					target.SetPosition(newPos.X, newPos.Y);

					aAction.Set(AttackerOptions.KnockBackHit2);

					cap.MaxHits = cap.Hit;
				}

				// Set stun time
				if (tAction.Type != CombatActionType.Defended)
				{
					aAction.Stun = this.GetAttackerStun(weapon, tAction.IsKnockBack);
					tAction.Stun = this.GetTargetStun(weapon, tAction.IsKnockBack);
				}

				// Second hit doubles stun time for normal hits
				if (cap.Hit == 2 && !tAction.IsKnockBack)
					aAction.Stun *= 2;

				cap.Handle();

				// No second hit if target was knocked back
				if (tAction.IsKnockBack)
					break;
			}

			return CombatSkillResult.Okay;
		}

		/// <summary>
		/// Returns stun time for the attacker.
		/// </summary>
		/// <param name="weapon"></param>
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
		/// <param name="weapon"></param>
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

		/// <summary>
		/// Training, called when someone attacks something.
		/// </summary>
		/// <param name="action"></param>
		public void OnCreatureAttackedByPlayer(TargetAction action)
		{
			// Get skill
			var attackerSkill = action.Attacker.Skills.Get(SkillId.CombatMastery);
			if (attackerSkill == null) return; // Should be impossible.
			var targetSkill = action.Creature.Skills.Get(SkillId.CombatMastery);
			if (targetSkill == null) return; // Should be impossible.

			var rating = action.Attacker.GetPowerRating(action.Creature);
			var targetRating = action.Creature.GetPowerRating(action.Attacker);

			// TODO: Check for multiple hits...?

			// Learning by attacking
			switch (attackerSkill.Info.Rank)
			{
				case SkillRank.Novice:
					attackerSkill.Train(1); // Attack Anything.
					break;

				case SkillRank.RF:
					attackerSkill.Train(1); // Attack anything.
					attackerSkill.Train(2); // Attack an enemy.
					if (action.IsKnockBack) attackerSkill.Train(3); // Knock down an enemy with multiple hits.
					if (action.Creature.IsDead) attackerSkill.Train(4); // Kill an enemy.
					break;

				case SkillRank.RE:
					if (rating == PowerRating.Normal) attackerSkill.Train(3); // Attack a same level enemy.

					if (action.IsKnockBack)
					{
						attackerSkill.Train(1); // Knock down an enemy with multiple hits.
						if (rating == PowerRating.Normal) attackerSkill.Train(4); // Knockdown a same level enemy.
						if (rating == PowerRating.Strong) attackerSkill.Train(7); // Knockdown a strong enemy.
					}

					if (action.Creature.IsDead)
					{
						attackerSkill.Train(2); // Kill an enemy.
						if (rating == PowerRating.Normal) attackerSkill.Train(6); // Kill a same level enemy.
						if (rating == PowerRating.Strong) attackerSkill.Train(8); // Kill a strong enemy.
					}

					break;

				case SkillRank.RD:
					attackerSkill.Train(1); // Attack an enemy.
					if (rating == PowerRating.Normal) attackerSkill.Train(4); // Attack a same level enemy.

					if (action.IsKnockBack)
					{
						attackerSkill.Train(2); // Knock down an enemy with multiple hits.
						if (rating == PowerRating.Normal) attackerSkill.Train(5); // Knockdown a same level enemy.
						if (rating == PowerRating.Strong) attackerSkill.Train(7); // Knockdown a strong enemy.
					}

					if (action.Creature.IsDead)
					{
						attackerSkill.Train(3); // Kill an enemy.
						if (rating == PowerRating.Normal) attackerSkill.Train(6); // Kill a same level enemy.
						if (rating == PowerRating.Strong) attackerSkill.Train(8); // Kill a strong enemy.
					}

					break;

				case SkillRank.RC:
				case SkillRank.RB:
					if (rating == PowerRating.Normal) attackerSkill.Train(1); // Attack a same level enemy.

					if (action.IsKnockBack)
					{
						if (rating == PowerRating.Normal) attackerSkill.Train(2); // Knockdown a same level enemy.
						if (rating == PowerRating.Strong) attackerSkill.Train(4); // Knockdown a strong level enemy.
						if (rating == PowerRating.Awful) attackerSkill.Train(6); // Knockdown an awful level enemy.
					}

					if (action.Creature.IsDead)
					{
						if (rating == PowerRating.Normal) attackerSkill.Train(3); // Kill a same level enemy.
						if (rating == PowerRating.Strong) attackerSkill.Train(5); // Kill a strong level enemy.
						if (rating == PowerRating.Awful) attackerSkill.Train(7); // Kill an awful level enemy.
					}

					break;

				case SkillRank.RA:
				case SkillRank.R9:
					if (action.IsKnockBack)
					{
						if (rating == PowerRating.Normal) attackerSkill.Train(1); // Knockdown a same level enemy.
						if (rating == PowerRating.Strong) attackerSkill.Train(3); // Knockdown a strong level enemy.
						if (rating == PowerRating.Awful) attackerSkill.Train(5); // Knockdown an awful level enemy.
					}

					if (action.Creature.IsDead)
					{
						if (rating == PowerRating.Normal) attackerSkill.Train(2); // Kill a same level enemy.
						if (rating == PowerRating.Strong) attackerSkill.Train(4); // Kill a strong level enemy.
						if (rating == PowerRating.Awful) attackerSkill.Train(6); // Kill an awful level enemy.
					}

					break;

				case SkillRank.R8:
					if (action.IsKnockBack)
					{
						if (rating == PowerRating.Normal) attackerSkill.Train(1); // Knockdown a same level enemy.
						if (rating == PowerRating.Strong) attackerSkill.Train(3); // Knockdown a strong level enemy.
						if (rating == PowerRating.Awful) attackerSkill.Train(5); // Knockdown an awful level enemy.
						if (rating == PowerRating.Boss) attackerSkill.Train(7); // Knockdown a boss level enemy.
					}

					if (action.Creature.IsDead)
					{
						if (rating == PowerRating.Normal) attackerSkill.Train(2); // Kill a same level enemy.
						if (rating == PowerRating.Strong) attackerSkill.Train(4); // Kill a strong level enemy.
						if (rating == PowerRating.Awful) attackerSkill.Train(6); // Kill an awful level enemy.
						if (rating == PowerRating.Boss) attackerSkill.Train(8); // Kill a boss level enemy.
					}

					break;

				case SkillRank.R7:
					if (action.IsKnockBack)
					{
						if (rating == PowerRating.Strong) attackerSkill.Train(2); // Knockdown a strong level enemy.
						if (rating == PowerRating.Awful) attackerSkill.Train(4); // Knockdown an awful level enemy.
						if (rating == PowerRating.Boss) attackerSkill.Train(6); // Knockdown a boss level enemy.
					}

					if (action.Creature.IsDead)
					{
						if (rating == PowerRating.Normal) attackerSkill.Train(1); // Kill a same level enemy.
						if (rating == PowerRating.Strong) attackerSkill.Train(3); // Kill a strong level enemy.
						if (rating == PowerRating.Awful) attackerSkill.Train(5); // Kill an awful level enemy.
						if (rating == PowerRating.Boss) attackerSkill.Train(7); // Kill a boss level enemy.
					}

					break;

				case SkillRank.R6:
				case SkillRank.R5:
				case SkillRank.R4:
				case SkillRank.R3:
				case SkillRank.R2:
				case SkillRank.R1:
					if (action.IsKnockBack)
					{
						if (rating == PowerRating.Strong) attackerSkill.Train(1); // Knockdown a strong level enemy.
						if (rating == PowerRating.Awful) attackerSkill.Train(3); // Knockdown an awful level enemy.
						if (rating == PowerRating.Boss) attackerSkill.Train(5); // Knockdown a boss level enemy.
					}

					if (action.Creature.IsDead)
					{
						if (rating == PowerRating.Strong) attackerSkill.Train(2); // Kill a strong level enemy.
						if (rating == PowerRating.Awful) attackerSkill.Train(4); // Kill an awful level enemy.
						if (rating == PowerRating.Boss) attackerSkill.Train(6); // Kill a boss level enemy.
					}

					break;
			}

			// Learning by being attacked
			switch (targetSkill.Info.Rank)
			{
				case SkillRank.RF:
					if (action.IsKnockBack) targetSkill.Train(5); // Learn something by falling down.
					if (action.Creature.IsDead) targetSkill.Train(6); // Learn through losing.
					break;

				case SkillRank.RE:
					if (action.IsKnockBack) targetSkill.Train(5); // Get knocked down. 
					break;

				case SkillRank.RD:
					if (targetRating == PowerRating.Strong) targetSkill.Train(9); // Get hit by an awful level enemy.
					break;
			}
		}
	}
}
