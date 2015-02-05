// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.World;
using Aura.Channel.World.Entities;
using Aura.Shared.Mabi.Const;
using Aura.Shared.Network;

namespace Aura.Channel.Skills.Combat
{
	/// <summary>
	/// Handler for the Counterattack skill.
	/// </summary>
	/// <remarks>
	/// Var 1: Target damage rate
	/// Var 2: Attacker damage rate
	/// Var 3: Crit bonus
	/// </remarks>
	[Skill(SkillId.Counterattack)]
	public class Counterattack : StandardPrepareHandler, IInitiableSkillHandler
	{
		private const short StunTime = 3000;
		private const int KnockbackDistance = 450;

		public void Init()
		{
			ChannelServer.Instance.Events.CreatureAttack += this.OnCreatureAttack;
		}

		public override void Prepare(Creature creature, Skill skill, int castTime, Packet packet)
		{
			Send.SkillFlashEffect(creature);
			Send.SkillPrepare(creature, skill.Info.Id, castTime);

			creature.Skills.ActiveSkill = skill;
		}

		public override void Ready(Creature creature, Skill skill, Packet packet)
		{
			Send.SkillReady(creature, skill.Info.Id);

			// Training
			if (skill.Info.Rank == SkillRank.RF)
				skill.Train(1); // Use the Defense skill.
		}

		public void Use(Creature attacker, Creature target)
		{
			var skill = attacker.Skills.Get(SkillId.Counterattack);

			var aAction = new AttackerAction(CombatActionType.RangeHit, attacker, SkillId.Counterattack, target.EntityId);
			aAction.Options |= AttackerOptions.Result | AttackerOptions.KnockBackHit2;

			var tAction = new TargetAction(CombatActionType.CounteredHit2, target, attacker, SkillId.CombatMastery);
			tAction.Options |= TargetOptions.Result | TargetOptions.Smash;

			var cap = new CombatActionPack(attacker, skill.Info.Id);
			cap.Add(aAction, tAction);

			var damage =
				(attacker.GetRndTotalDamage() * (skill.RankData.Var2 / 100f)) +
				(target.GetRndTotalDamage() * (skill.RankData.Var1 / 100f));

			SkillHelper.HandleCritical(attacker, (target.GetCritChanceFor(attacker) + skill.RankData.Var3), ref damage, tAction);
			SkillHelper.HandleDefenseProtection(target, ref damage, true, true);

			target.TakeDamage(tAction.Damage = damage, attacker);

			if (target.IsDead)
				tAction.Options |= TargetOptions.FinishingKnockDown;

			aAction.Stun = StunTime;
			tAction.Stun = StunTime;

			var targetPosition = target.GetPosition();
			Position intersection;
			var knockbackPos = attacker.GetPosition().GetRelative(targetPosition, KnockbackDistance);
			if (target.Region.Collisions.Find(targetPosition, knockbackPos, out intersection))
				knockbackPos = targetPosition.GetRelative(intersection, -50);

			target.SetPosition(knockbackPos.X, knockbackPos.Y);

			// Update both weapons
			SkillHelper.UpdateWeapon(attacker, target, attacker.RightHand, attacker.LeftHand);

			Send.SkillUseStun(attacker, skill.Info.Id, StunTime, 1);

			cap.Handle();
		}

		/// <summary>
		/// Training, called when someone attacks something.
		/// </summary>
		/// <param name="tAction"></param>
		public void OnCreatureAttack(TargetAction tAction)
		{
			// We're only interested in hits on creatures using Defense
			if (!tAction.Creature.Skills.IsActive(SkillId.Defense))
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
