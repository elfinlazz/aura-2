// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.World.Entities;
using Aura.Mabi.Const;
using Aura.Mabi.Network;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.Skills.Magic
{
	/// <summary>
	/// Healing skill handler
	/// </summary>
	/// <remarks>
	/// Var1: Healing Amount Min
	/// Var2: Healing Wand Effectiveness
	/// Var3: Healing Amount Max
	/// 
	/// Allows enemy click, triggers combat attack.
	/// </remarks>
	[Skill(SkillId.Healing)]
	public class Healing : ISkillHandler, IPreparable, IReadyable, IUseable, ICancelable, ICompletable, IInitiableSkillHandler, ICustomHitCanceler
	{
		/// <summary>
		/// Range in which the skill may be used.
		/// </summary>
		private const int Range = 1000;

		/// <summary>
		/// Subscrives to events required for skill training.
		/// </summary>
		public void Init()
		{
			ChannelServer.Instance.Events.PlayerUsesItem += this.OnPlayerUsesItem;
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
			Send.SkillInitEffect(creature, "healing");
			Send.SkillPrepare(creature, skill.Info.Id, skill.GetCastTime());

			return true;
		}

		/// <summary>
		/// Readies skill after prepared.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		/// <returns></returns>
		public bool Ready(Creature creature, Skill skill, Packet packet)
		{
			skill.Stacks = 5;

			Send.Effect(creature, Effect.StackUpdate, "healing_stack", (byte)skill.Stacks, (byte)0);
			Send.Effect(creature, Effect.HoldMagic, "healing");
			Send.SkillReady(creature, skill.Info.Id);

			return true;
		}

		/// <summary>
		/// Uses skill on target.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public void Use(Creature creature, Skill skill, Packet packet)
		{
			var entityId = packet.GetLong();
			var unkInt1 = packet.GetInt();
			var unkInt2 = packet.GetInt();

			// Get creature
			var target = creature.Region.GetCreature(entityId);
			if (target == null)
				goto L_End;

			// Check range
			if (!creature.GetPosition().InRange(target.GetPosition(), Range))
			{
				Send.Notice(creature, Localization.Get("Not in range.")); // Unofficial
				goto L_End;
			}

			// TODO: Check target validity once we have skill target support

			// Calculate heal amount
			var rnd = RandomProvider.Get();
			var healAmount = rnd.Next((int)skill.RankData.Var1, (int)skill.RankData.Var3 + 1);

			// Add magic attack bonus
			healAmount += (int)(creature.MagicAttack / 10);

			// Add wand bonus
			if (creature.RightHand != null && creature.RightHand.HasTag("/healing_wand/"))
				healAmount += 5;

			// Reduce user's stamina
			if (target.Life < target.LifeInjured)
			{
				creature.Stamina -= healAmount;
				Send.StatUpdate(creature, StatUpdateType.Private, Stat.Stamina, Stat.Hunger, Stat.StaminaMax);
			}

			// Skill training
			// Call before heal to calculate if in distress
			this.OnUseSkillOnTarget(creature, target);

			// Heal target
			target.Life += healAmount;
			Send.StatUpdateDefault(target);
			Send.Effect(target, Effect.HealLife, healAmount);

			// Reduce stacks
			skill.Stacks--;

		L_End:
			Send.Effect(creature, Effect.StackUpdate, "healing_stack", (byte)skill.Stacks, (byte)0);
			Send.Effect(creature, Effect.UseMagic, "healing", entityId);
			Send.SkillUse(creature, skill.Info.Id, entityId, unkInt1, unkInt2);
		}

		/// <summary>
		/// Cancels skill, removing spheres.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		public void Cancel(Creature creature, Skill skill)
		{
			Send.Effect(creature, Effect.StackUpdate, "healing_stack", (byte)0, (byte)0);
			Send.MotionCancel2(creature, 1);
		}

		/// <summary>
		/// Completes skill.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public void Complete(Creature creature, Skill skill, Packet packet)
		{
			Send.SkillComplete(creature, skill.Info.Id);
		}

		/// <summary>
		/// Called from Use, handles most of the skill's training.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="target"></param>
		private void OnUseSkillOnTarget(Creature user, Creature target)
		{
			var userSkill = user.Skills.Get(SkillId.Healing);
			var targetSkill = target.Skills.Get(SkillId.Healing);
			var targetInDistress = (target.Life < target.LifeMax * 0.20f);

			if (userSkill != null)
			{
				if (userSkill.Info.Rank >= SkillRank.RF && userSkill.Info.Rank <= SkillRank.RB)
				{
					userSkill.Train(1); // Use Healing.
					if (targetInDistress)
						userSkill.Train(2); // Use Healing on a person in distress.
				}

				if (userSkill.Info.Rank >= SkillRank.RA && userSkill.Info.Rank <= SkillRank.R8)
				{
					userSkill.Train(1); // Use Healing.
					if (targetInDistress)
						userSkill.Train(2); // Use Healing on a person in distress.
				}

				if (userSkill.Info.Rank >= SkillRank.R7 && userSkill.Info.Rank <= SkillRank.R4)
				{
					userSkill.Train(1); // Use Healing.
					if (targetInDistress)
						userSkill.Train(2); // Use Healing on a person in distress.
				}

				if (userSkill.Info.Rank >= SkillRank.R3 && userSkill.Info.Rank <= SkillRank.R1)
				{
					if (targetInDistress)
						userSkill.Train(1); // Use Healing on a person in distress.
				}
			}

			if (targetSkill != null)
			{
				if (targetSkill.Info.Rank >= SkillRank.RF && targetSkill.Info.Rank <= SkillRank.RB)
				{
					targetSkill.Train(3); // Receive Healing.
					if (targetInDistress)
						targetSkill.Train(4); // Receive Healing while in distress.
				}

				if (targetSkill.Info.Rank >= SkillRank.RA && targetSkill.Info.Rank <= SkillRank.R8)
				{
					if (targetInDistress)
						targetSkill.Train(3); // Receive Healing while in distress.
				}
			}
		}

		/// <summary>
		/// Called when player uses an item.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="item"></param>
		private void OnPlayerUsesItem(Creature creature, Item item)
		{
			var userSkill = creature.Skills.Get(SkillId.Healing);

			if (userSkill != null && userSkill.Info.Rank >= SkillRank.RF && userSkill.Info.Rank <= SkillRank.RE && item.HasTag("/potion/hp/"))
				userSkill.Train(5); // Drink a Life Potion.
		}

		/// <summary>
		/// Called when creature is hit while Healing is active.
		/// </summary>
		/// <param name="creature"></param>
		public void CustomHitCancel(Creature creature)
		{
			// Lose only 2 stacks if r1
			var skill = creature.Skills.ActiveSkill;
			if (skill.Info.Rank < SkillRank.R1 || skill.Stacks <= 2)
			{
				creature.Skills.CancelActiveSkill();
				return;
			}

			skill.Stacks -= 2;
			Send.Effect(creature, Effect.StackUpdate, "healing_stack", (byte)skill.Stacks, (byte)0);
		}
	}
}
