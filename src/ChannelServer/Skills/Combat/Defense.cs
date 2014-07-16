// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.World.Entities;
using Aura.Shared.Mabi.Const;
using Aura.Shared.Network;
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
	public class Defense : StandardPrepareHandler
	{
		public override void Prepare(Creature creature, Skill skill, int castTime, Packet packet)
		{
			Send.SkillFlashEffect(creature);
			Send.SkillPrepare(creature, skill.Info.Id, castTime);

			creature.Skills.ActiveSkill = skill;
		}

		public override void Ready(Creature creature, Skill skill, Packet packet)
		{
			Send.SkillReady(creature, skill.Info.Id);
		}
	}
}
