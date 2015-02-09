// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
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
			// Defense
			if (!tAction.Creature.Skills.IsReady(SkillId.Defense))
				return false;

			// Update actions
			tAction.Type = CombatActionType.Defended;
			tAction.SkillId = SkillId.Defense;
			tAction.Creature.Stun = tAction.Stun = DefenseTargetStun;
			aAction.Creature.Stun = aAction.Stun = DefenseAttackerStun;

			// Reduce damage
			var defenseSkill = tAction.Creature.Skills.Get(SkillId.Defense);
			if (defenseSkill != null)
				damage -= defenseSkill.RankData.Var3;

			Send.SkillUseStun(tAction.Creature, SkillId.Defense, 1000, 0);

			return true;
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
			var defended = (tAction.Type == CombatActionType.Defended);

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
