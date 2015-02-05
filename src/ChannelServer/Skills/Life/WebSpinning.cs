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

namespace Aura.Channel.Skills.Life
{
	/// <summary>
	/// Skill used by spiders to drop cobweb.
	/// </summary>
	/// <remarks>
	/// This implementation is guessed. It can be used by players and
	/// monsters, but it's unlikely that it's official.
	/// </remarks>
	[Skill(SkillId.WebSpinning)]
	public class WebSpinning : IPreparable, ICompletable, ICancelable
	{
		public void Prepare(Creature creature, Skill skill, int castTime, Packet packet)
		{
			Send.SkillUse(creature, skill.Info.Id, 0);
		}

		public void Cancel(Creature creature, Skill skill)
		{
		}

		public void Complete(Creature creature, Skill skill, Packet packet)
		{
			var cobweb = new Item(60008);
			cobweb.Drop(creature.Region, creature.GetPosition().GetRandomInRange(200, RandomProvider.Get()));

			Send.SkillComplete(creature, skill.Info.Id);
		}
	}
}
