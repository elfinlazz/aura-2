// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.World;
using Aura.Channel.World.Entities;
using Aura.Shared.Mabi.Const;
using Aura.Shared.Network;
using Aura.Shared.Util;

namespace Aura.Channel.Skills.Magic
{
	/// <summary>
	/// Handler for Silent Move skill.
	/// </summary>
	/// <remarks>
	/// Var1: Cooldown
	/// Var2: Max teleport distance
	/// </remarks>
	[Skill(SkillId.SilentMove)]
	public class SilentMove : IPreparable, IReadyable, ICompletable, ICancelable, IUseable
	{
		// Buffer to compensate for using the skill while moving
		private const int DistanceBuffer = 250;

		public bool Prepare(Creature creature, Skill skill, Packet packet)
		{
			Send.SkillFlashEffect(creature);
			Send.SkillPrepare(creature, skill.Info.Id, skill.GetCastTime());

			return true;
		}

		public bool Ready(Creature creature, Skill skill, Packet packet)
		{
			Send.SkillReady(creature, skill.Info.Id);

			return true;
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
			var targetPos = new Position(packet.GetLong());

			// Check distance to target position
			if (!creature.GetPosition().InRange(targetPos, (int)skill.RankData.Var2 + DistanceBuffer))
			{
				Send.Notice(creature, Localization.Get("Out of range."));
				Send.SkillUse(creature, skill.Info.Id, 0);
				return;
			}

			// Stop creature's movement.
			creature.StopMove();

			// Teleport effect (does not actually teleport)
			Send.Effect(creature, Effect.SilentMoveTeleport, (byte)2, targetPos.X, targetPos.Y);

			// Teleport player to target position
			creature.SetPosition(targetPos.X, targetPos.Y);
			Send.SkillTeleport(creature, targetPos.X, targetPos.Y);

			Send.SkillUse(creature, skill.Info.Id, 1);
		}
	}
}
