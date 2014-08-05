// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.World.Entities;
using Aura.Shared.Network;
using Aura.Shared.Mabi;
using Aura.Shared.Mabi.Const;
using Aura.Channel.World;
using System.Collections.Generic;
using System;

namespace Aura.Channel.Skills.Magic
{
	[Skill(SkillId.SilentMove)]
	public class SilentMoveSkillHandler : IPreparable, IReadyable, ICompletable, ICancelable, IUseable
	{
		public void Prepare(Creature creature, Skill skill, int castTime, Packet packet)
		{
			Send.SkillFlashEffect(creature);
			Send.SkillPrepare(creature, skill.Info.Id, castTime);

			creature.Skills.ActiveSkill = skill;
		}

		public void Ready(Creature creature, Skill skill, Packet packet)
		{
			Send.SkillReady(creature, skill.Info.Id);
		}

		public void Complete(Creature creature, Skill skill, Packet packet)
		{
			Send.SkillComplete(creature, skill.Info.Id);
		}

		public void Cancel(Creature creature, Skill skill)
		{
			// Do nothing by default.
		}

		public void Use(Creature creature, Skill skill, Packet packet)
		{
			var location = new Position(packet.GetLong());

			if (!creature.GetPosition().InRange(location, (int)skill.RankData.Var1))
			{
				Send.Notice(creature, "Out of range.");
				Send.SkillUse(creature, skill.Info.Id, 0);
				return;
			}

			//Stop creature's movement.
			creature.StopMove();

			//Sends teleport effect. Does not teleport.
			Send.Effect(creature, 67, (byte)2, location.X, location.Y);

			//Teleports player to specified location, and updating it on the server.
			Send.SkillTeleport(creature, location.X, location.Y);
			creature.SetLocation(creature.Region.Id, location.X, location.Y);

			Send.SkillUse(creature, skill.Info.Id, 1);
		}

	}

}
