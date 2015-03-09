// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.Skills.Combat;
using Aura.Channel.World.Entities;
using Aura.Shared.Mabi.Const;
using Aura.Shared.Network;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.Skills.Magic
{
	public class firebolt : Icebolt
	{
		protected new const int AttackerStun = 500;
	}

	/// <summary>
	/// Icebolt skill handler
	/// </summary>
	/// <remarks>
	/// Var1: Min damage
	/// Var2: Max damage
	/// Var3: ?
	/// </remarks>
	[Skill(SkillId.Icebolt)]
	public class Icebolt : IPreparable, IReadyable, ICombatSkill, ICompletable, ICancelable
	{
		/// <summary>
		/// Stun time of attacker after use in ms.
		/// </summary>
		protected const int AttackerStun = 500;

		/// <summary>
		/// Stun time of target after being hit in ms.
		/// </summary>
		protected const int TargetStun = 2000;

		/// <summary>
		/// Units the target is knocked back.
		/// </summary>
		protected const int KnockbackDistance = 400;

		/// <summary>
		/// String used in effect packets.
		/// </summary>
		protected const string EffectSkillName = "icebolt";

		/// <summary>
		/// Prepares skill, showing a casting motion.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		/// <returns></returns>
		public bool Prepare(Creature creature, Skill skill, Packet packet)
		{
			creature.StopMove();

			Send.Effect(creature, Effect.Casting, (short)skill.Info.Id, (byte)0, (byte)1, (short)0);
			Send.SkillPrepare(creature, skill.Info.Id, skill.GetCastTime());

			return true;
		}

		/// <summary>
		/// Finishes preparing, adds stack.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		/// <returns></returns>
		public bool Ready(Creature creature, Skill skill, Packet packet)
		{
			// Note: The client only prevents casting if stacks = max, if you go above the limit
			//   it lets you keep casting.

			var addStacks = (!creature.Skills.Has(SkillId.ChainCasting) ? skill.RankData.Stack : skill.RankData.StackMax);
			skill.Stacks = Math.Min(skill.RankData.StackMax, skill.Stacks + addStacks);

			Send.Effect(creature, Effect.StackUpdate, EffectSkillName, (byte)skill.Stacks, (byte)0);
			Send.Effect(creature, Effect.Casting, (short)skill.Info.Id, (byte)0, (byte)2, (short)0);

			Send.SkillReady(creature, skill.Info.Id);

			return true;
		}

		/// <summary>
		/// Completes skill usage, ready is called automatically again if
		/// there are stacks left.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public void Complete(Creature creature, Skill skill, Packet packet)
		{
			Send.SkillComplete(creature, skill.Info.Id);
		}

		/// <summary>
		/// Cancels skill, setting stacks to 0.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		public void Cancel(Creature creature, Skill skill)
		{
			skill.Stacks = 0;
			Send.Effect(creature, Effect.StackUpdate, EffectSkillName, (byte)skill.Stacks, (byte)0);
			Send.MotionCancel2(creature);
		}

		/// <summary>
		/// Handles skill usage.
		/// </summary>
		/// <param name="attacker"></param>
		/// <param name="skill"></param>
		/// <param name="targetEntityId"></param>
		/// <returns></returns>
		public CombatSkillResult Use(Creature attacker, Skill skill, long targetEntityId)
		{
			// Check target
			var target = attacker.Region.GetCreature(targetEntityId);
			if (target == null)
				return CombatSkillResult.InvalidTarget;

			// Check distance
			var targetPosition = target.GetPosition();
			var attackerPosition = attacker.GetPosition();

			if (!attackerPosition.InRange(targetPosition, this.GetRange(attacker, skill)))
				return CombatSkillResult.OutOfRange;

			target.StopMove();

			// Create actions
			var aAction = new AttackerAction(CombatActionType.RangeHit, attacker, skill.Info.Id, targetEntityId);
			aAction.Set(AttackerOptions.Result);

			var tAction = new TargetAction(CombatActionType.TakeHit, target, attacker, skill.Info.Id);
			tAction.Set(TargetOptions.Result);
			tAction.Stun = TargetStun;

			var cap = new CombatActionPack(attacker, skill.Info.Id, aAction, tAction);

			// Damage
			var damage = this.GetDamage(attacker, skill);

			Defense.Handle(aAction, tAction, ref damage);
			ManaShield.Handle(target, ref damage, tAction);

			// Deal damage
			if (damage > 0)
				target.TakeDamage(tAction.Damage = damage, attacker);
			target.Aggro(attacker);

			// Death/Knockback
			if (target.IsDead)
			{
				tAction.Set(TargetOptions.FinishingKnockDown);
				attacker.Shove(target, KnockbackDistance);
			}
			else
			{
				if (target.KnockBack >= 100)
				{
					tAction.Set(TargetOptions.KnockDown);
				}
				else
				{
					target.KnockBack += 45;
					if (target.KnockBack >= 100)
					{
						tAction.Set(TargetOptions.KnockBack);
						attacker.Shove(target, KnockbackDistance);
					}
				}
			}

			// Override stun set by defense
			aAction.Stun = AttackerStun;

			Send.Effect(attacker, Effect.UseMagic, EffectSkillName);
			Send.SkillUseStun(attacker, skill.Info.Id, aAction.Stun, 1);

			skill.Stacks--;

			cap.Handle();

			return CombatSkillResult.Okay;
		}

		/// <summary>
		/// Returns range for Icebolt.
		/// </summary>
		/// <param name="creature"></param>
		/// <returns></returns>
		public int GetRange(Creature creature, Skill skill)
		{
			var range = skill.RankData.Range;

			// 1400 for ice wands, 1200 as default
			if (creature.RightHand != null && creature.RightHand.HasTag("/ice_wand/"))
				range += 200;

			return range;
		}

		/// <summary>
		/// Returns base damage for creature using skill.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <returns></returns>
		public float GetDamage(Creature creature, Skill skill)
		{
			var rnd = RandomProvider.Get();

			var damage = creature.MagicAttack;

			// Skill damage
			damage += (float)(skill.RankData.Var1 + rnd.NextDouble() * (skill.RankData.Var2 - skill.RankData.Var1));

			return damage;
		}
	}
}
