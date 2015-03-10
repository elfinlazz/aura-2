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
	/// <summary>
	/// Lightningbolt skill handler
	/// </summary>
	/// <remarks>
	/// Var1: Min damage
	/// Var2: Max damage
	/// Var3: ?
	/// 
	/// Contrary to what the Wiki says, training seems to be exactly the
	/// same as Icebolt, the Wiki is either outdated or incorrect.
	/// </remarks>
	[Skill(SkillId.Lightningbolt)]
	public class Lightningbolt : Icebolt
	{
		/// <summary>
		/// Splash Range for target search.
		/// </summary>
		private int SplashRange = 500;

		/// <summary>
		/// ID of the skill, used in training.
		/// </summary>
		protected override SkillId SkillId { get { return SkillId.Lightningbolt; } }

		/// <summary>
		/// String used in effect packets.
		/// </summary>
		protected override string EffectSkillName { get { return "lightningbolt"; } }

		/// <summary>
		/// Weapon tag that's looked for in range calculation.
		/// </summary>
		protected override string SpecialWandTag { get { return "lightning_wand"; } }


		/// <summary>
		/// Bolt specific use code.
		/// </summary>
		/// <param name="attacker"></param>
		/// <param name="skill"></param>
		/// <param name="target"></param>
		protected override void UseSkillOnTarget(Creature attacker, Skill skill, Creature mainTarget)
		{
			// Create actions
			var aAction = new AttackerAction(CombatActionType.RangeHit, attacker, skill.Info.Id, mainTarget.EntityId);
			aAction.Set(AttackerOptions.Result);

			var cap = new CombatActionPack(attacker, skill.Info.Id, aAction);

			var targets = new List<Creature>();
			targets.Add(mainTarget);
			targets.AddRange(mainTarget.Region.GetCreaturesInRange(mainTarget.GetPosition(), SplashRange).Where(a => attacker.CanTarget(a)));

			var max = targets.Count;

			for (int i = 0; i < max; ++i)
			{
				var target = targets[i];

				target.StopMove();

				var tAction = new TargetAction(CombatActionType.TakeHit, target, attacker, skill.Info.Id);
				tAction.Set(TargetOptions.Result);
				tAction.Stun = TargetStun;

				// Damage
				// Full damage for the first target, -10% for every subsequent one.
				var damage = this.GetDamage(attacker, skill);
				damage -= (damage * 0.1f) * i;

				// Reduce damage
				Defense.Handle(aAction, tAction, ref damage);
				SkillHelper.HandleMagicDefenseProtection(target, ref damage);
				ManaShield.Handle(target, ref damage, tAction);

				// Deal damage
				if (damage > 0)
					target.TakeDamage(tAction.Damage = damage, attacker);

				if (target == mainTarget)
					target.Aggro(attacker);

				// Death/Knockback
				this.HandleKnockBack(attacker, target, tAction);

				cap.Add(tAction);
			}

			// Override stun set by defense
			aAction.Stun = AttackerStun;

			Send.Effect(attacker, Effect.UseMagic, EffectSkillName);
			Send.SkillUseStun(attacker, skill.Info.Id, aAction.Stun, 1);

			this.BeforeHandlingPack(attacker, skill);

			cap.Handle();
		}

		/// <summary>
		/// Actions to be done before the combat action pack is handled.
		/// </summary>
		/// <param name="attacker"></param>
		/// <param name="skill"></param>
		protected virtual void BeforeHandlingPack(Creature attacker, Skill skill)
		{
			skill.Stacks = 0;
		}

		/// <summary>
		/// Returns damage for creature using skill.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <returns></returns>
		protected override float GetDamage(Creature creature, Skill skill)
		{
			var damage = creature.GetRndMagicDamage(skill, skill.RankData.Var1, skill.RankData.Var2);

			if (skill.Stacks < 5)
				damage *= skill.Stacks;
			else
				damage *= 6.5f;

			return damage;
		}
	}
}
