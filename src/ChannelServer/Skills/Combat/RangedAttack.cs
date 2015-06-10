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
	/// <summary>
	/// Ranged Attack handler
	/// </summary>
	/// <remarks>
	/// Var1: Min Ranged Attack Damage
	/// Var2: Max Ranged Attack Damage
	/// Var3: Aim Speed
	/// Var5: Ranged Attack Balance
	/// </remarks>
	[Skill(SkillId.RangedAttack)]
	public class RangedAttack : ISkillHandler, IPreparable, IReadyable, ICompletable, ICancelable, ICombatSkill, IInitiableSkillHandler
	{
		/// <summary>
		/// Distance to knock the target back.
		/// </summary>
		private const int KnockBackDistance = 400;

		/// <summary>
		/// Amount of stability lost on hit.
		/// </summary>
		private const float StabilityReduction = 60f;

		/// <summary>
		/// Stun for the attacker
		/// </summary>
		private const int AttackerStun = 800;

		/// <summary>
		/// Stun for the target
		/// </summary>
		private const int TargetStun = 2100;

		/// <summary>
		/// Bonus damage for fire arrows
		/// </summary>
		public const float FireBonus = 1.5f;

		/// <summary>
		/// Sets up subscriptions for skill training.
		/// </summary>
		public void Init()
		{
			ChannelServer.Instance.Events.CreatureAttack += this.OnCreatureAttacks;
		}

		/// <summary>
		/// Prepares skill.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		/// <returns></returns>
		public bool Prepare(Creature creature, Skill skill, Packet packet)
		{
			Send.SkillPrepare(creature, skill.Info.Id, skill.GetCastTime());

			return true;
		}

		/// <summary>
		/// Readies skill, activates fire effect.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		/// <returns></returns>
		public bool Ready(Creature creature, Skill skill, Packet packet)
		{
			creature.Temp.FireArrow = creature.Region.GetProps(a => a.Info.Id == 203 && a.GetPosition().InRange(creature.GetPosition(), 500)).Count > 0;
			if (creature.Temp.FireArrow)
				Send.Effect(creature, Effect.FireArrow, true);

			Send.SkillReady(creature, skill.Info.Id);

			return true;
		}

		/// <summary>
		/// Completes skill, stopping the aim meter and disabling the fire effect.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public void Complete(Creature creature, Skill skill, Packet packet)
		{
			this.Cancel(creature, skill);
			Send.SkillComplete(creature, skill.Info.Id);
		}

		/// <summary>
		/// Cancels skill, stopping the aim meter and disabling the fire effect.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		public void Cancel(Creature creature, Skill skill)
		{
			creature.AimMeter.Stop();

			// Disable fire arrow effect
			if (creature.Temp.FireArrow)
				Send.Effect(creature, Effect.FireArrow, false);
		}

		/// <summary>
		/// Uses the skill.
		/// </summary>
		/// <param name="attacker"></param>
		/// <param name="skill"></param>
		/// <param name="targetEntityId"></param>
		/// <returns></returns>
		public CombatSkillResult Use(Creature attacker, Skill skill, long targetEntityId)
		{
			// Get target
			var target = attacker.Region.GetCreature(targetEntityId);
			if (target == null)
				return CombatSkillResult.InvalidTarget;

			var targetPos = target.GetPosition();
			var attackerPos = attacker.GetPosition();

			// Check range
			//if (!attackerPos.InRange(targetPos, attacker.RightHand.OptionInfo.EffectiveRange + 100))
			//	return CombatSkillResult.OutOfRange;

			// Actions
			var cap = new CombatActionPack(attacker, skill.Info.Id);

			var aAction = new AttackerAction(CombatActionType.RangeHit, attacker, skill.Info.Id, targetEntityId);
			aAction.Set(AttackerOptions.Result);
			aAction.Stun = AttackerStun;
			cap.Add(aAction);

			// Hit by chance
			var chance = attacker.AimMeter.GetAimChance(target);
			var rnd = RandomProvider.Get();
			if (rnd.NextDouble() * 100 < chance)
			{
				var tAction = new TargetAction(CombatActionType.TakeHit, target, attacker, skill.Info.Id);
				tAction.Set(TargetOptions.Result);
				tAction.Stun = TargetStun;
				cap.Add(tAction);

				// Damage
				var damage = attacker.GetRndRangedDamage();

				// More damage with fire arrow
				if (attacker.Temp.FireArrow)
					damage *= FireBonus;

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

				// Aggro
				target.Aggro(attacker);

				// Death/Knockback
				if (target.IsDead)
				{
					tAction.Set(TargetOptions.FinishingKnockDown);
					attacker.Shove(target, KnockBackDistance);
				}
				else
				{
					// Insta-recover in knock down
					// TODO: Tied to stability?
					if (target.IsKnockedDown)
					{
						tAction.Stun = 0;
					}
					// Knock down if hit repeatedly
					else if (target.Stability < 30)
					{
						tAction.Set(TargetOptions.KnockDown);
					}
					// Normal stability reduction
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

			// Skill training
			if (skill.Info.Rank == SkillRank.Novice || skill.Info.Rank == SkillRank.RF)
				skill.Train(1); // Try ranged attack.

			// Reduce arrows
			if (attacker.Magazine != null && !ChannelServer.Instance.Conf.World.InfiniteArrows)
				attacker.Inventory.Decrement(attacker.Magazine);

			// Disable fire arrow effect
			if (attacker.Temp.FireArrow)
				Send.Effect(attacker, Effect.FireArrow, false);

			// "Cancels" the skill
			// 800 = old load time? == aAction.Stun? Varies? Doesn't seem to be a stun.
			Send.SkillUse(attacker, skill.Info.Id, 800, 1);

			cap.Handle();

			return CombatSkillResult.Okay;
		}

		/// <summary>
		/// Handles the majority of the skill training.
		/// </summary>
		/// <param name="obj"></param>
		private void OnCreatureAttacks(TargetAction tAction)
		{
			if (tAction.SkillId != SkillId.RangedAttack)
				return;

			var attackerSkill = tAction.Attacker.Skills.Get(SkillId.RangedAttack);
			var targetSkill = tAction.Creature.Skills.Get(SkillId.RangedAttack);
			var targetPowerRating = tAction.Attacker.GetPowerRating(tAction.Creature);
			var attackerPowerRating = tAction.Creature.GetPowerRating(tAction.Attacker);

			if (attackerSkill != null)
			{
				if (attackerSkill.Info.Rank == SkillRank.RF)
				{
					attackerSkill.Train(2); // Attack an enemy.

					if (tAction.Has(TargetOptions.KnockDown))
						attackerSkill.Train(3); // Down the enemy with continuous hit.

					if (tAction.Creature.IsDead)
						attackerSkill.Train(4); // Kill an enemy.
				}
				else if (attackerSkill.Info.Rank == SkillRank.RE)
				{
					if (targetPowerRating == PowerRating.Normal)
						attackerSkill.Train(3); // Attack a same level enemy.

					if (tAction.Has(TargetOptions.KnockDown))
					{
						attackerSkill.Train(1); // Down the enemy with continuous hit.

						if (targetPowerRating == PowerRating.Normal)
							attackerSkill.Train(4); // Down a same level enemy. 

						if (targetPowerRating == PowerRating.Strong)
							attackerSkill.Train(6); // Down a strong enemy.
					}

					if (tAction.Creature.IsDead)
					{
						attackerSkill.Train(2); // Kill an enemy.

						if (targetPowerRating == PowerRating.Normal)
							attackerSkill.Train(5); // Kill a same level enemy. 

						if (targetPowerRating == PowerRating.Strong)
							attackerSkill.Train(7); // Kill a strong enemy.
					}
				}
				else if (attackerSkill.Info.Rank == SkillRank.RD)
				{
					attackerSkill.Train(1); // Attack any enemy. 

					if (targetPowerRating == PowerRating.Normal)
						attackerSkill.Train(4); // Attack a same level enemy.

					if (tAction.Has(TargetOptions.KnockDown))
					{
						attackerSkill.Train(2); // Down the enemy with continuous hit.

						if (targetPowerRating == PowerRating.Normal)
							attackerSkill.Train(5); // Down a same level enemy. 

						if (targetPowerRating == PowerRating.Strong)
							attackerSkill.Train(7); // Down a strong enemy.
					}

					if (tAction.Creature.IsDead)
					{
						attackerSkill.Train(3); // Kill an enemy.

						if (targetPowerRating == PowerRating.Normal)
							attackerSkill.Train(6); // Kill a same level enemy. 

						if (targetPowerRating == PowerRating.Strong)
							attackerSkill.Train(8); // Kill a strong enemy.
					}
				}
				else if (attackerSkill.Info.Rank >= SkillRank.RC && attackerSkill.Info.Rank <= SkillRank.RB)
				{
					if (targetPowerRating == PowerRating.Normal)
						attackerSkill.Train(1); // Attack a same level enemy.

					if (tAction.Has(TargetOptions.KnockDown))
					{
						if (targetPowerRating == PowerRating.Normal)
							attackerSkill.Train(2); // Down a same level enemy. 

						if (targetPowerRating == PowerRating.Strong)
							attackerSkill.Train(4); // Down a strong enemy.

						if (targetPowerRating == PowerRating.Awful)
							attackerSkill.Train(6); // Down an awful enemy.
					}

					if (tAction.Creature.IsDead)
					{
						if (targetPowerRating == PowerRating.Normal)
							attackerSkill.Train(3); // Kill a same level enemy. 

						if (targetPowerRating == PowerRating.Strong)
							attackerSkill.Train(5); // Kill a strong enemy.

						if (targetPowerRating == PowerRating.Awful)
							attackerSkill.Train(7); // Kill an awful enemy.
					}
				}
				else if (attackerSkill.Info.Rank >= SkillRank.RA && attackerSkill.Info.Rank <= SkillRank.R8)
				{
					if (tAction.Has(TargetOptions.KnockDown))
					{
						if (targetPowerRating == PowerRating.Normal)
							attackerSkill.Train(1); // Down a same level enemy. 

						if (targetPowerRating == PowerRating.Strong)
							attackerSkill.Train(3); // Down a strong enemy.

						if (targetPowerRating == PowerRating.Awful)
							attackerSkill.Train(5); // Down an awful enemy.

						if (targetPowerRating == PowerRating.Boss && attackerSkill.Info.Rank == SkillRank.R8)
							attackerSkill.Train(7); // Down a boss level enemy.
					}

					if (tAction.Creature.IsDead)
					{
						if (targetPowerRating == PowerRating.Normal)
							attackerSkill.Train(2); // Kill a same level enemy. 

						if (targetPowerRating == PowerRating.Strong)
							attackerSkill.Train(4); // Kill a strong enemy.

						if (targetPowerRating == PowerRating.Awful)
							attackerSkill.Train(6); // Kill an awful enemy.

						if (targetPowerRating == PowerRating.Boss && attackerSkill.Info.Rank == SkillRank.R8)
							attackerSkill.Train(8); // Kill a boss level enemy.
					}
				}
				else if (attackerSkill.Info.Rank >= SkillRank.R7 && attackerSkill.Info.Rank <= SkillRank.R1)
				{
					if (tAction.Has(TargetOptions.KnockDown))
					{
						if (targetPowerRating == PowerRating.Strong)
							attackerSkill.Train(1); // Down a strong enemy.

						if (targetPowerRating == PowerRating.Awful)
							attackerSkill.Train(3); // Down an awful enemy.

						if (targetPowerRating == PowerRating.Boss)
							attackerSkill.Train(5); // Down a boss level enemy.
					}

					if (tAction.Creature.IsDead)
					{
						if (targetPowerRating == PowerRating.Strong)
							attackerSkill.Train(2); // Kill a strong enemy.

						if (targetPowerRating == PowerRating.Awful)
							attackerSkill.Train(4); // Kill an awful enemy.

						if (targetPowerRating == PowerRating.Boss)
							attackerSkill.Train(6); // Kill a boss level enemy.
					}
				}
			}

			if (targetSkill != null)
			{
				if (targetSkill.Info.Rank == SkillRank.RF)
				{
					if (tAction.Has(TargetOptions.KnockDown))
						targetSkill.Train(5); // Learn by falling down.

					if (tAction.Creature.IsDead)
						targetSkill.Train(6); // Learn through losing.
				}
				else if (targetSkill.Info.Rank == SkillRank.RD)
				{
					if (attackerPowerRating == PowerRating.Strong)
						targetSkill.Train(8); // Receive a powerful attack from a powerful enemy.
				}
				else if (targetSkill.Info.Rank == SkillRank.R7)
				{
					if (attackerPowerRating == PowerRating.Strong)
						targetSkill.Train(7); // Receive a powerful attack from a powerful enemy.
				}
			}
		}
	}
}
