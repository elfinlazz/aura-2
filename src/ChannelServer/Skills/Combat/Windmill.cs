// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.Skills.Magic;
using Aura.Channel.World.Entities;
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
	/// <summary>
	/// Windmill skill handler
	/// </summary>
	/// <remarks>
	/// Var 1: Damage multiplicator
	/// Var 2: ? (Life reduction? "10.0" for each rank)
	/// Var 3: ?
	/// </remarks>
	[Skill(SkillId.Windmill)]
	public class Windmill : IPreparable, IReadyable, IUseable, ICompletable, ICancelable
	{
		/// <summary>
		/// Amount added to the Knockback meter.
		/// </summary>
		private const float Knockback = 120;

		/// <summary>
		/// Units the enemy is knocked back.
		/// </summary>
		private const int KnockbackDistance = 450;

		/// <summary>
		/// Prepares WM, empty skill init for only the loading sound.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		/// <returns></returns>
		public bool Prepare(Creature creature, Skill skill, Packet packet)
		{
			creature.StopMove();

			Send.SkillInitEffect(creature, null);
			Send.SkillPrepare(creature, skill.Info.Id, skill.GetCastTime());

			return true;
		}

		/// <summary>
		/// Readies WM, sets stack so we know when it was used.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		/// <returns></returns>
		public bool Ready(Creature creature, Skill skill, Packet packet)
		{
			skill.Stacks = 1;

			Send.SkillReady(creature, skill.Info.Id);

			return true;
		}

		/// <summary>
		/// Uses WM, attacking targets.
		/// </summary>
		/// <param name="attacker"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public void Use(Creature attacker, Skill skill, Packet packet)
		{
			var targetAreaId = packet.GetLong();
			var unkInt1 = packet.GetInt();
			var unkInt2 = packet.GetInt();

			var range = 400;
			var targets = attacker.GetTargetableCreaturesInRange(range);

			// Check targets
			if (targets.Count == 0)
			{
				Send.Notice(attacker, Localization.Get("There isn't a target nearby to use that on."));
				Send.SkillUseSilentCancel(attacker);
				return;
			}

			// Create actions
			var cap = new CombatActionPack(attacker, skill.Info.Id);

			var aAction = new AttackerAction(CombatActionType.SpecialHit, attacker, skill.Info.Id, targetAreaId);
			aAction.Set(AttackerOptions.Result);

			cap.Add(aAction);

			foreach (var target in targets)
			{
				target.StopMove();

				var tAction = new TargetAction(CombatActionType.TakeHit, target, attacker, skill.Info.Id);
				tAction.Delay = 300; // Usually 300, sometimes 350?

				// Calculate damage and crit
				var damage = attacker.GetRndTotalDamage();
				var critChance = attacker.CriticalBase;

				damage *= skill.RankData.Var1 / 100f;

				// Handle skills and reductions
				CriticalHit.Handle(attacker, critChance, ref damage, tAction);
				SkillHelper.HandleDefenseProtection(target, ref damage);
				Defense.Handle(aAction, tAction, ref damage);
				ManaShield.Handle(target, ref damage, tAction);

				// Clean Hit if not defended nor critical
				if (!tAction.Is(CombatActionType.Defended) && !tAction.Has(TargetOptions.Critical))
					tAction.Set(TargetOptions.CleanHit);

				// Tage damage if any is left
				if (damage > 0)
					target.TakeDamage(tAction.Damage = damage, attacker);

				// Finish if dead, knock down if not defended
				if (target.IsDead)
					tAction.Set(TargetOptions.KnockDownFinish);
				else if (!tAction.Is(CombatActionType.Defended))
					tAction.Set(TargetOptions.KnockDown);

				// Stun & knock back
				aAction.Stun = CombatMastery.GetAttackerStun(attacker.AverageKnockCount, attacker.AverageAttackSpeed, true);

				if (!tAction.Is(CombatActionType.Defended))
				{
					tAction.Stun = CombatMastery.GetTargetStun(attacker.AverageKnockCount, attacker.AverageAttackSpeed, true);
					target.KnockBack = Knockback;
					attacker.Shove(target, KnockbackDistance);
				}

				// Add action
				cap.Add(tAction);
			}

			// Spin it~
			Send.UseMotion(attacker, 8, 4);

			cap.Handle();

			Send.SkillUse(attacker, skill.Info.Id, targetAreaId, unkInt1, unkInt2);

			skill.Stacks = 0;
		}

		/// <summary>
		/// Completes WM.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public void Complete(Creature creature, Skill skill, Packet packet)
		{
			Send.SkillComplete(creature, skill.Info.Id);
		}

		/// <summary>
		/// Cancels WM.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		public void Cancel(Creature creature, Skill skill)
		{
		}

		/// <summary>
		/// Calculates range based on equipment and skill rank.
		/// </summary>
		/// <param name="attacker"></param>
		/// <param name="skill"></param>
		/// <returns></returns>
		protected virtual int GetRange(Creature attacker, Skill skill)
		{
			var range = 300f;
			var knuckleMod = 0.4f;

			if (skill.Info.Rank >= SkillRank.R5)
			{
				range = 400f;
				knuckleMod = 0.5f;
			}
			else if (skill.Info.Rank >= SkillRank.R1)
			{
				range = 500f;
				knuckleMod = 0.6f;
			}

			if (attacker.RightHand != null && attacker.RightHand.Data.WeaponType == 9)
				range *= knuckleMod;

			range += attacker.RaceData.AttackRange;

			return (int)range;
		}
	}
}
