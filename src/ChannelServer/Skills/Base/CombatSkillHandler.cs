// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Channel.World.Entities;
using Aura.Shared.Network;

namespace Aura.Channel.Skills.Base
{
	/// <summary>
	/// Combat skill handler
	/// </summary>
	/// <remarks>
	/// Base class for skills like Smash, that are prepared, readied, etc,
	/// but not used like normal skills, but via Combat Mastery.
	/// </remarks>
	public abstract class CombatSkillHandler : IPreparable, IReadyable, ICompletable, ICancelable, ICombatSkill
	{
		public virtual bool Prepare(Creature creature, Skill skill, Packet packet)
		{
			throw new NotImplementedException();
		}

		public virtual bool Ready(Creature creature, Skill skill, Packet packet)
		{
			throw new NotImplementedException();
		}

		public virtual void Complete(Creature creature, Skill skill, Packet packet)
		{
			throw new NotImplementedException();
		}

		public virtual void Cancel(Creature creature, Skill skill)
		{
			// Do nothing by default.
		}

		public virtual CombatSkillResult Use(Creature attacker, Skill skill, long targetEntityId)
		{
			throw new NotImplementedException();
		}
	}

	public interface ICombatSkill : ISkillHandler
	{
		CombatSkillResult Use(Creature attacker, Skill skill, long targetEntityId);
	}

	public enum CombatSkillResult { Okay, OutOfRange, InvalidTarget }
}
