// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Channel.Skills.Base;
using Aura.Shared.Network;
using Aura.Channel.World.Entities;
using Aura.Shared.Mabi.Const;
using Aura.Shared.Mabi;
using Aura.Channel.Network.Sending;

namespace Aura.Channel.Skills.Life
{
	/// <summary>
	/// Handles the Rest skill. Also called when using a chair.
	/// </summary>
	[Skill(SkillId.Rest)]
	public class RestSkillHandler : StartStopSkillHandler
	{
		public override void Start(Creature creature, Skill skill, MabiDictionary dict)
		{
			creature.Activate(CreatureStates.SitDown);
			Send.SitDown(creature);

			creature.Skills.GiveExp(skill, 20);
		}

		public override void Stop(Creature creature, Skill skill, MabiDictionary dict)
		{
			creature.Deactivate(CreatureStates.SitDown);
			Send.StandUp(creature);
		}
	}
}
