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

namespace Aura.Channel.Skills.Life
{
	/// <summary>
	/// Handles the Rest skill. Also called when using a chair.
	/// </summary>
	[Skill(SkillId.Rest)]
	public class RestSkillHandler : IStartStopable
	{
		public void Start(Creature creature, Skill skill, Packet packet)
		{
			throw new NotImplementedException();
		}

		public void Stop(Creature creature, Skill skill, Packet packet)
		{
			throw new NotImplementedException();
		}
	}
}
