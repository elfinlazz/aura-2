// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Channel.Skills.Base;
using Aura.Shared.Mabi.Const;
using Aura.Channel.World.Entities;

namespace Aura.Channel.Skills.Combat
{
	/// <summary>
	/// Combat Master
	/// </summary>
	/// <remarks>
	/// Combat Mastery is the skill 99% of the creatures use when attacking
	/// normally. It's also used with various other skills; eg when using
	/// Smash the client sends a Combat packet, instead of using the skill,
	/// which leads us here as well.
	/// </remarks>
	[Skill(SkillId.CombatMastery)]
	public class CombatMastery : ICombatSkill
	{
		public CombatSkillResult Use(Creature attacker, Skill skill, long targetEntityId)
		{
			throw new NotImplementedException();
		}
	}
}
