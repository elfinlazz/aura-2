// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Util;
using Aura.Mabi.Const;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.World.Entities.Creatures
{
	public class AimMeter
	{
		public Creature Creature { get; private set; }
		public DateTime StartTime { get; private set; }

		public bool IsAiming { get { return this.StartTime != DateTime.MinValue; } }

		public AimMeter(Creature creature)
		{
			this.StartTime = DateTime.MinValue;

			this.Creature = creature;
		}

		public void Start(long targetEntityId)
		{
			// Use 0 as fallback for now, until we're sure there's no
			// "no skill" ranged.
			var activeSkillId = this.Creature.Skills.ActiveSkill == null ? 0 : this.Creature.Skills.ActiveSkill.Info.Id;

			this.StartTime = DateTime.Now;
			Send.CombatSetAimR(this.Creature, targetEntityId, activeSkillId, 0);
		}

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
