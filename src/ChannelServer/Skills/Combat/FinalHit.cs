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
	/// Final Hit handler
	/// </summary>
	/// <remarks>
	/// Var1: Duration
	/// Var2: Cooldown (not using the new system)
	/// Var3: Teleport Distance
	/// </remarks>
	[Skill(SkillId.FinalHit)]
	public class FinalHit : StandardPrepareHandler, IUseable, IInitiableSkillHandler
	{
		/// <summary>
		/// Reference to the Combat Mastery handler.
		/// </summary>
		private CombatMastery _cm;

		public void Init()
		{
			ChannelServer.Instance.Events.CreatureAttack += this.OnCreatureAttack;
		}

		/// <summary>
		/// Prepares (loads) the skill.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		/// <returns></returns>
		public override bool Prepare(Creature creature, Skill skill, Packet packet)
		{
			Send.SkillFlashEffect(creature);
			Send.SkillPrepare(creature, skill.Info.Id, skill.GetCastTime());

			return true;
		}

		/// <summary>
		/// Readies the skill, called when cast is over.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		/// <returns></returns>
		public override bool Ready(Creature creature, Skill skill, Packet packet)
		{
			skill.Stacks = 1;

			creature.Temp.FinalHitKillCount = 0;
			creature.Temp.FinalHitKillCountStrong = 0;
			creature.Temp.FinalHitKillCountAwful = 0;
			creature.Temp.FinalHitKillCountBoss = 0;

			Send.Effect(creature, Effect.FinalHit, (byte)1, (byte)1);
			Send.SkillReady(creature, skill.Info.Id);

			return true;
		}

		/// <summary>
		/// Completes skill, readying for next use.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public override void Complete(Creature creature, Skill skill, Packet packet)
		{
			Send.SkillComplete(creature, skill.Info.Id);

			// Ready again if it wasn't canceled in Use (Counter)? o~o
			// TODO: Find a way to do this properly.
			if (skill.State != SkillState.Canceled)
			{
				Send.SkillReady(creature, skill.Info.Id);
				skill.State = SkillState.Ready;
			}
		}

		/// <summary>
		/// Cancels skill's effects.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		public override void Cancel(Creature creature, Skill skill)
		{
			Send.Effect(creature, Effect.FinalHit, (byte)0);
		}

		/// <summary>
		/// Handles skill usage.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public void Use(Creature creature, Skill skill, Packet packet)
		{
			var targetEntityId = packet.GetLong();
			var unk1 = packet.GetInt();
			var unk2 = packet.GetInt();

			if (_cm == null)
				_cm = ChannelServer.Instance.SkillManager.GetHandler<CombatMastery>(SkillId.CombatMastery);

			// TODO: Check duration

			var attackResult = false;

			var target = creature.Region.GetCreature(targetEntityId);
			if (target != null && !creature.IsStunned && creature.CanTarget(target))
			{
				var pos = creature.GetPosition();
				var targetPos = target.GetPosition();
				var inRange = pos.InRange(targetPos, creature.AttackRangeFor(target));

				if (!inRange)
				{
					var telePos = pos.GetRelative(targetPos, -creature.AttackRangeFor(target) + 100);

					// Check teleport distance
					if (pos.GetDistance(telePos) > skill.RankData.Var3 + 100)
					{
						Send.Notice(creature, "Out of range");
					}
					else
					{
						Send.Effect(creature, Effect.SilentMoveTeleport, targetEntityId, (byte)0);

						creature.SetPosition(telePos.X, telePos.Y);
						Send.SkillTeleport(creature, telePos.X, telePos.Y);

						inRange = true;
					}
				}

				if (inRange)
					attackResult = (_cm.Use(creature, skill, targetEntityId) == CombatSkillResult.Okay);
			}

			Send.CombatAttackR(creature, attackResult);

			Send.SkillUse(creature, skill.Info.Id, targetEntityId, unk1, unk2);
		}

		/// <summary>
		/// Handles training.
		/// </summary>
		/// <param name="tAction"></param>
		private void OnCreatureAttack(TargetAction tAction)
		{
			if (tAction.AttackerSkillId != SkillId.FinalHit)
				return;

			// Increase counters for collective kill conditions
			if (tAction.Creature.IsDead)
			{
				switch (tAction.Attacker.GetPowerRating(tAction.Creature))
				{
					case PowerRating.Strong: tAction.Attacker.Temp.FinalHitKillCountStrong++; goto default;
					case PowerRating.Awful: tAction.Attacker.Temp.FinalHitKillCountAwful++; goto default;
					case PowerRating.Boss: tAction.Attacker.Temp.FinalHitKillCountBoss++; goto default;
					default: tAction.Attacker.Temp.FinalHitKillCount++; break;
				}
			}

			var attackerSkill = tAction.Attacker.Skills.Get(SkillId.FinalHit);

			if (attackerSkill.Info.Rank >= SkillRank.RF && attackerSkill.Info.Rank <= SkillRank.RE)
			{
				if (tAction.Creature.IsDead)
				{
					attackerSkill.Train(1);

					if (tAction.Attacker.GetPowerRating(tAction.Creature) == PowerRating.Strong)
						attackerSkill.Train(2);
				}

				if (tAction.Attacker.Temp.FinalHitKillCount >= 2)
				{
					attackerSkill.Train(3);
					tAction.Attacker.Temp.FinalHitKillCount = 0;
				}

				if (attackerSkill.Info.Rank == SkillRank.RE && tAction.Attacker.Temp.FinalHitKillCountStrong >= 2)
				{
					attackerSkill.Train(4);
					tAction.Attacker.Temp.FinalHitKillCountStrong = 0;
				}
			}

			if (attackerSkill.Info.Rank == SkillRank.RD)
			{
				if (tAction.Creature.IsDead)
				{
					attackerSkill.Train(1);

					if (tAction.Attacker.GetPowerRating(tAction.Creature) == PowerRating.Strong)
						attackerSkill.Train(2);

					if (tAction.Attacker.GetPowerRating(tAction.Creature) == PowerRating.Awful)
						attackerSkill.Train(3);
				}

				if (tAction.Attacker.Temp.FinalHitKillCount >= 2)
				{
					attackerSkill.Train(4);
					tAction.Attacker.Temp.FinalHitKillCount = 0;
				}

				if (tAction.Attacker.Temp.FinalHitKillCountStrong >= 2)
				{
					attackerSkill.Train(5);
					tAction.Attacker.Temp.FinalHitKillCountStrong = 0;
				}

				if (tAction.Attacker.Temp.FinalHitKillCountAwful >= 2)
				{
					attackerSkill.Train(6);
					tAction.Attacker.Temp.FinalHitKillCountAwful = 0;
				}
			}

			if (attackerSkill.Info.Rank >= SkillRank.RC && attackerSkill.Info.Rank <= SkillRank.RA)
			{
				if (tAction.Creature.IsDead)
				{
					attackerSkill.Train(1);

					if (tAction.Attacker.GetPowerRating(tAction.Creature) == PowerRating.Strong)
						attackerSkill.Train(2);

					if (tAction.Attacker.GetPowerRating(tAction.Creature) == PowerRating.Awful)
						attackerSkill.Train(3);
				}

				if (tAction.Attacker.Temp.FinalHitKillCount >= 3)
				{
					attackerSkill.Train(4);
					tAction.Attacker.Temp.FinalHitKillCount = 0;
				}

				if (tAction.Attacker.Temp.FinalHitKillCountStrong >= 3)
				{
					attackerSkill.Train(5);
					tAction.Attacker.Temp.FinalHitKillCountStrong = 0;
				}

				if (tAction.Attacker.Temp.FinalHitKillCountAwful >= 3)
				{
					attackerSkill.Train(6);
					tAction.Attacker.Temp.FinalHitKillCountAwful = 0;
				}
			}

			if (attackerSkill.Info.Rank == SkillRank.R9)
			{
				if (tAction.Creature.IsDead)
				{
					attackerSkill.Train(1);

					if (tAction.Attacker.GetPowerRating(tAction.Creature) == PowerRating.Strong)
						attackerSkill.Train(2);

					if (tAction.Attacker.GetPowerRating(tAction.Creature) == PowerRating.Awful)
						attackerSkill.Train(3);
				}

				if (tAction.Attacker.Temp.FinalHitKillCount >= 4)
				{
					attackerSkill.Train(4);
					tAction.Attacker.Temp.FinalHitKillCount = 0;
				}

				if (tAction.Attacker.Temp.FinalHitKillCountStrong >= 4)
				{
					attackerSkill.Train(5);
					tAction.Attacker.Temp.FinalHitKillCountStrong = 0;
				}

				if (tAction.Attacker.Temp.FinalHitKillCountAwful >= 4)
				{
					attackerSkill.Train(6);
					tAction.Attacker.Temp.FinalHitKillCountAwful = 0;
				}
			}

			if (attackerSkill.Info.Rank == SkillRank.R9)
			{
				if (tAction.Creature.IsDead)
				{
					attackerSkill.Train(1);

					if (tAction.Attacker.GetPowerRating(tAction.Creature) == PowerRating.Strong)
						attackerSkill.Train(2);

					if (tAction.Attacker.GetPowerRating(tAction.Creature) == PowerRating.Awful)
						attackerSkill.Train(3);
				}

				if (tAction.Attacker.Temp.FinalHitKillCount >= 4)
				{
					attackerSkill.Train(4);
					tAction.Attacker.Temp.FinalHitKillCount = 0;
				}

				if (tAction.Attacker.Temp.FinalHitKillCountStrong >= 4)
				{
					attackerSkill.Train(5);
					tAction.Attacker.Temp.FinalHitKillCountStrong = 0;
				}

				if (tAction.Attacker.Temp.FinalHitKillCountAwful >= 4)
				{
					attackerSkill.Train(6);
					tAction.Attacker.Temp.FinalHitKillCountAwful = 0;
				}
			}

			if (attackerSkill.Info.Rank >= SkillRank.R8 && attackerSkill.Info.Rank <= SkillRank.R7)
			{
				if (tAction.Creature.IsDead)
				{
					if (tAction.Attacker.GetPowerRating(tAction.Creature) == PowerRating.Strong)
						attackerSkill.Train(1);

					if (tAction.Attacker.GetPowerRating(tAction.Creature) == PowerRating.Awful)
						attackerSkill.Train(2);

					if (tAction.Attacker.GetPowerRating(tAction.Creature) == PowerRating.Boss)
						attackerSkill.Train(3);
				}

				if (tAction.Attacker.Temp.FinalHitKillCount >= 4)
				{
					attackerSkill.Train(4);
					tAction.Attacker.Temp.FinalHitKillCount = 0;
				}

				if (tAction.Attacker.Temp.FinalHitKillCountStrong >= 4)
				{
					attackerSkill.Train(5);
					tAction.Attacker.Temp.FinalHitKillCountStrong = 0;
				}

				if (tAction.Attacker.Temp.FinalHitKillCountAwful >= 4)
				{
					attackerSkill.Train(6);
					tAction.Attacker.Temp.FinalHitKillCountAwful = 0;
				}
			}

			if (attackerSkill.Info.Rank >= SkillRank.R6 && attackerSkill.Info.Rank <= SkillRank.R4)
			{
				if (tAction.Creature.IsDead)
				{
					if (tAction.Attacker.GetPowerRating(tAction.Creature) == PowerRating.Strong)
						attackerSkill.Train(1);

					if (tAction.Attacker.GetPowerRating(tAction.Creature) == PowerRating.Awful)
						attackerSkill.Train(2);

					if (tAction.Attacker.GetPowerRating(tAction.Creature) == PowerRating.Boss)
						attackerSkill.Train(3);
				}

				if (tAction.Attacker.Temp.FinalHitKillCount >= 5)
				{
					attackerSkill.Train(4);
					tAction.Attacker.Temp.FinalHitKillCount = 0;
				}

				if (tAction.Attacker.Temp.FinalHitKillCountStrong >= 5)
				{
					attackerSkill.Train(5);
					tAction.Attacker.Temp.FinalHitKillCountStrong = 0;
				}

				if (tAction.Attacker.Temp.FinalHitKillCountAwful >= 5)
				{
					attackerSkill.Train(6);
					tAction.Attacker.Temp.FinalHitKillCountAwful = 0;
				}
			}

			if (attackerSkill.Info.Rank >= SkillRank.R3 && attackerSkill.Info.Rank <= SkillRank.R1)
			{
				if (tAction.Creature.IsDead)
				{
					if (tAction.Attacker.GetPowerRating(tAction.Creature) == PowerRating.Strong)
						attackerSkill.Train(1);

					if (tAction.Attacker.GetPowerRating(tAction.Creature) == PowerRating.Awful)
						attackerSkill.Train(2);

					if (tAction.Attacker.GetPowerRating(tAction.Creature) == PowerRating.Boss)
						attackerSkill.Train(3);
				}

				if (tAction.Attacker.Temp.FinalHitKillCountStrong >= 5)
				{
					attackerSkill.Train(4);
					tAction.Attacker.Temp.FinalHitKillCountStrong = 0;
				}

				if (tAction.Attacker.Temp.FinalHitKillCountAwful >= 5)
				{
					attackerSkill.Train(5);
					tAction.Attacker.Temp.FinalHitKillCountAwful = 0;
				}

				if (tAction.Attacker.Temp.FinalHitKillCountBoss >= 5)
				{
					attackerSkill.Train(6);
					tAction.Attacker.Temp.FinalHitKillCountBoss = 0;
				}
			}
		}
	}
}
