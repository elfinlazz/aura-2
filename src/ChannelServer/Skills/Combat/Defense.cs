// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.World.Entities;
using Aura.Data;
using Aura.Mabi.Const;
using Aura.Mabi.Network;
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
	/// Handle for the Defense skill.
	/// </summary>
	[Skill(SkillId.Defense)]
	public class Defense : StandardPrepareHandler, IInitiableSkillHandler
	{
		/// <summary>
		/// Stuntime in ms for the attacker.
		/// </summary>
		private const int DefenseAttackerStun = 2500;

		/// <summary>
		/// Stuntime in ms for the target.
		/// </summary>
		private const int DefenseTargetStun = 1000;

		/// <summary>
		/// Subscribes the handler to events required for training.
		/// </summary>
		public void Init()
		{
			ChannelServer.Instance.Events.CreatureAttack += this.OnCreatureAttack;
		}

		/// <summary>
		/// Prepares the skill, called to start casting.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public override bool Prepare(Creature creature, Skill skill, Packet packet)
		{
			Send.SkillFlashEffect(creature);
			Send.SkillPrepare(creature, skill.Info.Id, skill.GetCastTime());

			// Default lock is Walk|Run, since renovation you're not able to
			// move while loading anymore.
			if (AuraData.FeaturesDb.IsEnabled("TalentRenovationCloseCombat"))
			{
				creature.StopMove();
			}
			// Since the client locks Walk|Run by default we have to tell it
			// to enable walk but disable run (under any circumstances) if
			// renovation is disabled.
			else
			{
				creature.Lock(Locks.Run, true);
				creature.Unlock(Locks.Walk, true);
			}

			return true;
		}

		/// <summary>
		/// Readies the skill, called when casting is done.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public override bool Ready(Creature creature, Skill skill, Packet packet)
		{
			Send.SkillReady(creature, skill.Info.Id);

			// No default locks, set them depending on whether a shield is
			// equipped or not.
			if (AuraData.FeaturesDb.IsEnabled("TalentRenovationCloseCombat"))
			{
				if (creature.LeftHand == null || !creature.LeftHand.IsShield)
					creature.Lock(Locks.Run);
			}
			// Send lock to client if renovation isn't enabled,
			// so it doesn't let the creature run, no matter what.
			else
			{
				creature.Lock(Locks.Run, true);
			}

			// Training
			if (skill.Info.Rank == SkillRank.RF)
				skill.Train(1); // Use the Defense skill.

			return true;
		}

		/// <summary>
		/// Checks if target has Defense skill activated and makes the necessary
		/// changes to the actions, stun times, and damage.
		/// </summary>
		/// <param name="aAction"></param>
		/// <param name="tAction"></param>
		/// <param name="damage"></param>
		/// <returns></returns>
		public static bool Handle(AttackerAction aAction, TargetAction tAction, ref float damage)
		{
			var activeSkill = tAction.Creature.Skills.ActiveSkill;
			if (activeSkill == null || activeSkill.Info.Id != SkillId.Defense || activeSkill.State != SkillState.Ready)
				return false;

			activeSkill.State = SkillState.Used;

			// Update actions
			tAction.Flags = CombatActionType.Defended;
			tAction.SkillId = SkillId.Defense;
			tAction.Stun = DefenseTargetStun;
			aAction.Stun = DefenseAttackerStun;

			// Reduce damage
			damage = Math.Max(1, damage - activeSkill.RankData.Var3);

			// Updating unlock because of the updating lock for pre-renovation
			// Other skills actually unlock automatically on the client,
			// I guess this isn't the case for Defense because it's never
			// *explicitly* used.
			if (!AuraData.FeaturesDb.IsEnabled("TalentRenovationCloseCombat"))
				tAction.Creature.Unlock(Locks.Run, true);

			Send.SkillUseStun(tAction.Creature, SkillId.Defense, DefenseTargetStun, 0);

			return true;
		}

		/// <summary>
		/// Resets the skill's cooldown in old combat.
		/// </summary>
		/// <remarks>
		/// Defense doesn't use the new cooldown system, but Vars, similar
		/// to Final Hit. Var7 seems to be the cooldown. That's why we have to
		/// reset it here.
		/// </remarks>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public override void Complete(Creature creature, Skill skill, Packet packet)
		{
			base.Complete(creature, skill, packet);

			if (!AuraData.FeaturesDb.IsEnabled("CombatSystemRenewal"))
				Send.ResetCooldown(creature, skill.Info.Id);
		}

		/// <summary>
		/// Cancels special effects.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		public override void Cancel(Creature creature, Skill skill)
		{
			// Updating unlock because of the updating lock for pre-renovation
			if (!AuraData.FeaturesDb.IsEnabled("TalentRenovationCloseCombat"))
				creature.Unlock(Locks.Run, true);
		}

		/// <summary>
		/// Training, called when someone attacks something.
		/// </summary>
		/// <param name="tAction"></param>
		public void OnCreatureAttack(TargetAction tAction)
		{
			// We're only interested in hits on creatures using Defense
			if (!tAction.Creature.Skills.IsReady(SkillId.Defense))
				return;

			// Did the target successfully defend itself?
			var defended = (tAction.Flags == CombatActionType.Defended);

			// Get skill
			var attackerSkill = tAction.Attacker.Skills.Get(SkillId.Defense);
			var targetSkill = tAction.Creature.Skills.Get(SkillId.Defense);

			if (targetSkill != null)
			{
				switch (targetSkill.Info.Rank)
				{
					case SkillRank.RF:
						if (defended) targetSkill.Train(2); // Get hit by an enemy while in the defense stance.
						break;
					case SkillRank.RE:
					case SkillRank.RD:
					case SkillRank.RC:
					case SkillRank.RB:
					case SkillRank.RA:
					case SkillRank.R9:
					case SkillRank.R8:
					case SkillRank.R7:
					case SkillRank.R6:
					case SkillRank.R5:
					case SkillRank.R4:
					case SkillRank.R3:
					case SkillRank.R2:
					case SkillRank.R1:
						if (defended) targetSkill.Train(1); // Get hit by an enemy while in the defense stance.
						break;
				}
			}

			if (attackerSkill != null)
			{
				switch (attackerSkill.Info.Rank)
				{
					case SkillRank.RF:
						if (defended) attackerSkill.Train(3); // Watch an enemy defend itself.
						break;
					case SkillRank.RE:
						if (defended) attackerSkill.Train(2); // Watch an enemy defend itself.
						break;
					case SkillRank.RD:
					case SkillRank.RC:
					case SkillRank.RB:
					case SkillRank.RA:
					case SkillRank.R9:
					case SkillRank.R8:
					case SkillRank.R7:
					case SkillRank.R6:
					case SkillRank.R5:
					case SkillRank.R4:
					case SkillRank.R3:
					case SkillRank.R2:
					case SkillRank.R1:
						if (tAction.AttackerSkillId == SkillId.Smash || tAction.AttackerSkillId == SkillId.MagnumShot || tAction.AttackerSkillId == SkillId.Firebolt || tAction.AttackerSkillId == SkillId.WaterCannon || tAction.AttackerSkillId == SkillId.AssaultSlash)
							attackerSkill.Train(3); // Successful in using a special skill to attack a defending enemy.
						goto case SkillRank.RE;
				}
			}
		}
	}
}
