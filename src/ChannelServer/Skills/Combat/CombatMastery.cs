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
using Aura.Channel.Skills.Magic;

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
		/// <summary>
		/// Units an enemy is knocked back.
		/// </summary>
		private const int KnockBackDistance = 450;

		/// <summary>
		/// Subscribes skill to events needed for training.
		/// </summary>
		public void Init()
		{
			ChannelServer.Instance.Events.CreatureAttackedByPlayer += this.OnCreatureAttackedByPlayer;
		}

		/// <summary>
		/// Handles attack.
		/// </summary>
		/// <param name="attacker">The creature attacking.</param>
		/// <param name="skill">The skill being used.</param>
		/// <param name="targetEntityId">The entity id of the target.</param>
		/// <returns></returns>
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

			// Counter
			if (Counterattack.Handle(target, attacker))
				return CombatSkillResult.Okay;

			var rightWeapon = attacker.Inventory.RightHand;
			var leftWeapon = attacker.Inventory.LeftHand;
			var magazine = attacker.Inventory.Magazine;
			var dualWield = (rightWeapon != null && leftWeapon != null && leftWeapon.Data.WeaponType != 0);
			var maxHits = (byte)(dualWield ? 2 : 1);
			int prevId = 0;

			for (byte i = 1; i <= maxHits; ++i)
			{
				var weapon = (i == 1 ? rightWeapon : leftWeapon);
				var weaponIsKnuckle = (weapon != null && weapon.Data.HasTag("/knuckle/"));

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
				CriticalHit.Handle(attacker, attacker.GetCritChanceFor(target), ref damage, tAction);

				// Subtract target def/prot
				SkillHelper.HandleDefenseProtection(target, ref damage);

				// Defense
				Defense.Handle(aAction, tAction, ref damage);

				// Mana Shield
				ManaShield.Handle(target, ref damage, tAction);

				// Deal with it!
				if (damage > 0)
					target.TakeDamage(tAction.Damage = damage, attacker);

				// Evaluate caused damage
				if (!target.IsDead)
				{
					if (tAction.Type != CombatActionType.Defended)
					{
						target.KnockBack += this.GetKnockBack(weapon) / maxHits;

						// React normal for CombatMastery, knock down if 
						// FH and not dual wield, don't knock at all if dual.
						if (skill.Info.Id != SkillId.FinalHit)
						{
							if (target.KnockBack >= 100 && target.Is(RaceStands.KnockBackable))
								tAction.Set(tAction.Has(TargetOptions.Critical) ? TargetOptions.KnockDown : TargetOptions.KnockBack);
						}
						else if (!dualWield && !weaponIsKnuckle)
						{
							target.KnockBack = 120;
							tAction.Set(TargetOptions.KnockDown);
						}
					}
				}
				else
				{
					tAction.Set(TargetOptions.FinishingKnockDown);
				}

				// React to knock back
				if (tAction.IsKnockBack)
				{
					attacker.Shove(target, KnockBackDistance);

					aAction.Set(AttackerOptions.KnockBackHit2);

					// Remove dual wield option if last hit doesn't come from
					// the second weapon.
					if (cap.MaxHits != cap.Hit)
						aAction.Options &= ~AttackerOptions.DualWield;
				}

				// Set stun time
				if (tAction.Type != CombatActionType.Defended)
				{
					aAction.Stun = this.GetAttackerStun(weapon, tAction.IsKnockBack && (skill.Info.Id != SkillId.FinalHit || !dualWield));
					tAction.Stun = this.GetTargetStun(weapon, tAction.IsKnockBack);
				}

				// Second hit doubles stun time for normal hits
				if (cap.Hit == 2 && !tAction.IsKnockBack)
					aAction.Stun *= 2;

				// Update current weapon
				SkillHelper.UpdateWeapon(attacker, target, weapon);

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
			//public enum CombatStunAttacker { VeryFast = 450, Fast = 520, Normal = 600, Slow = 800, VerySlow = 1000 }
			//public enum CombatKnockbackStunAttacker { VeryFast = 2500, Fast = 2500, Normal = 2500, Slow = 2500, VerySlow = 2500 }

			var count = weapon != null ? weapon.Info.KnockCount + 1 : 3;
			var speed = weapon != null ? (AttackSpeed)weapon.Data.AttackSpeed : AttackSpeed.Normal;

			if (knockback)
				return 2500;

			switch (count)
			{
				case 1:
					switch (speed)
					{
						case AttackSpeed.VerySlow: return 2500;
					}
					break;

				case 2:
					switch (speed)
					{
						case AttackSpeed.VerySlow: return 1000;
						case AttackSpeed.Slow: return 800;
					}
					break;

				case 3:
					switch (speed)
					{
						case AttackSpeed.VerySlow: return 1000;
						case AttackSpeed.Slow: return 800;
						case AttackSpeed.Normal: return 600;
						case AttackSpeed.Fast: return 520;
					}
					break;

				case 5:
					switch (speed)
					{
						case AttackSpeed.VeryFast: return 450;
					}
					break;
			}

			Log.Unimplemented("GetAttackerStun: Combination {0} {1} Hit", speed, count);

			return 600;
		}

		/// <summary>
		/// Returns stun time for the target.
		/// </summary>
		/// <param name="weapon"></param>
		/// <param name="knockback"></param>
		/// <returns></returns>
		public short GetTargetStun(Item weapon, bool knockback)
		{
			//public enum CombatStunTarget { VeryFast = 1200, Fast = 1700, Normal = 2000, Slow = 2800, VerySlow = 3000 }
			//public enum CombatKnockbackStunTarget { VeryFast = 3000, Fast = 3000, Normal = 3000, Slow = 3000, VerySlow = 3000 }

			var count = weapon != null ? weapon.Info.KnockCount + 1 : 3;
			var speed = weapon != null ? (AttackSpeed)weapon.Data.AttackSpeed : AttackSpeed.Normal;

			if (knockback)
				return 3000;

			switch (count)
			{
				case 1:
					switch (speed)
					{
						case AttackSpeed.VerySlow: return 3000;
					}
					break;

				case 2:
					switch (speed)
					{
						case AttackSpeed.VerySlow: return 3000;
						case AttackSpeed.Slow: return 2800;
					}
					break;

				case 3:
					switch (speed)
					{
						case AttackSpeed.VerySlow: return 2200;
						case AttackSpeed.Slow: return 2100;
						case AttackSpeed.Normal: return 2000;
						case AttackSpeed.Fast: return 1700;
					}
					break;

				case 5:
					switch (speed)
					{
						case AttackSpeed.VeryFast: return 1200;
					}
					break;
			}

			Log.Unimplemented("GetAttackerStun: Combination {0} {1} Hit", speed, count);

			return 2000;
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
