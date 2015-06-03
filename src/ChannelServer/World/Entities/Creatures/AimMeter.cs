// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Mabi.Const;
using System;

namespace Aura.Channel.World.Entities.Creatures
{
	/// <summary>
	/// Aim meter for ranged combat.
	/// </summary>
	public class AimMeter
	{
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
	}
}
