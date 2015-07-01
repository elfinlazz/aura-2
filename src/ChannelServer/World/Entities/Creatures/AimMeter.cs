// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Mabi.Const;
using Aura.Shared.Util;
using System;

namespace Aura.Channel.World.Entities.Creatures
{
	/// <summary>
	/// Aim meter for ranged combat.
	/// </summary>
	public class AimMeter
	{
		/// <summary>
		/// Maximum aim for walking target
		/// </summary>
		private const int MaxChanceWalking = 95;

		/// <summary>
		/// Maximum aim for running target
		/// </summary>
		private const int MaxChanceRunning = 90;

		/// <summary>
		/// Creature this aim meter belongs to.
		/// </summary>
		public Creature Creature { get; private set; }

		/// <summary>
		/// Time at which aiming was started, equal to MinValue if not aiming.
		/// </summary>
		public DateTime StartTime { get; private set; }

		/// <summary>
		/// Returns true if meter's creature is aiming.
		/// </summary>
		public bool IsAiming { get { return this.StartTime != DateTime.MinValue; } }

		/// <summary>
		/// Creatues new aim meter for creature.
		/// </summary>
		/// <param name="creature"></param>
		public AimMeter(Creature creature)
		{
			this.StartTime = DateTime.MinValue;

			this.Creature = creature;
		}

		/// <summary>
		/// Starts aiming timer and sends CombatSetAimR.
		/// </summary>
		/// <param name="targetEntityId"></param>
		public void Start(long targetEntityId)
		{
			// Use 0 as fallback for now, until we're sure there's no
			// "no skill" ranged.
			var activeSkillId = this.Creature.Skills.ActiveSkill == null ? 0 : this.Creature.Skills.ActiveSkill.Info.Id;

			this.Creature.StopMove();

			this.StartTime = DateTime.Now;
			Send.CombatSetAimR(this.Creature, targetEntityId, activeSkillId, 0);
		}

		/// <summary>
		/// Stops aiming timer and sends CombatSetAimR.
		/// </summary>
		public void Stop()
		{
			this.StartTime = DateTime.MinValue;
			Send.CombatSetAimR(this.Creature, 0, SkillId.None, 0);
		}

		/// <summary>
		/// Returns the time since Start was called.
		/// </summary>
		/// <returns></returns>
		public double GetAimTime()
		{
			if (this.StartTime == DateTime.MinValue)
				return 0;

			return (DateTime.Now - this.StartTime).TotalMilliseconds;
		}

		/// <summary>
		/// Returns the chance to hit target at the current aim time.
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public double GetAimChance(Creature target)
		{
			// Check collision, 0 chance if the client didn't prevent this shot.
			if (this.Creature.Region.Collisions.Any(this.Creature.GetPosition(), target.GetPosition()))
			{
				// Only warn, could be caused by lag.
				Log.Warning("GetAimChance: Creature '{0:X16}' tried to shoot through a wall.", this.Creature.EntityId);
				return 0;
			}

			var activeSkill = this.Creature.Skills.ActiveSkill;

			var d1 = 5000.0;
			var d2 = 500.0;

			if (activeSkill != null && activeSkill.Info.Id == SkillId.MagnumShot)
			{
				d1 = 8000.0;
				d2 = 1000.0;
			}

			var distance = this.Creature.GetPosition().GetDistance(target.GetPosition());
			var bowRange = this.Creature.RightHand == null ? 0 : this.Creature.RightHand.OptionInfo.EffectiveRange;

			if (distance > bowRange || distance <= 0)
				return 0;

			var aimTime = this.GetAimTime();
			var aimMod = aimTime;

			// Bonus for ranged attack
			if (activeSkill != null && activeSkill.Info.Id == SkillId.RangedAttack)
				aimMod *= activeSkill.RankData.Var3 / 100f;

			var hitRatio = 1.0;
			hitRatio = ((d1 - d2) / bowRange) * distance * hitRatio + d2;

			var chance = Math.Sqrt(aimMod / hitRatio) * 100f;

			// 100% after x time (unofficial)
			if (chance >= 120)
				chance = 100;
			else
				chance = Math.Min(99, chance);

			if (target.IsMoving)
			{
				if (target.IsWalking)
					chance = Math.Min(MaxChanceWalking, chance);
				else
					chance = Math.Min(MaxChanceRunning, chance);
			}

			// Debug for devCATs
			if (this.Creature.Titles.SelectedTitle == 60001)
				Send.ServerMessage(this.Creature, "Debug: Aim {0}, Distance {1}, Time {2}", chance, distance, aimTime);

			return chance;
		}
	}
}
