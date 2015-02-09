// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.World.Entities;
using Aura.Shared.Network;
using System;

namespace Aura.Channel.Skills.Base
{
	public class StandardPrepareHandler : IPreparable, IReadyable, ICompletable, ICancelable
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
			Send.SkillComplete(creature, skill.Info.Id);
		}

		public virtual void Cancel(Creature creature, Skill skill)
		{
		}
	}
}
