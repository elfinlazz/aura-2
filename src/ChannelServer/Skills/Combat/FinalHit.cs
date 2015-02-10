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

namespace Aura.Channel.Skills.Combat
{
	[Skill(SkillId.FinalHit)]
	public class FinalHit : StandardPrepareHandler, IUseable
	{
		private CombatMastery _cm;

		public override bool Prepare(Creature creature, Skill skill, Packet packet)
		{
			Send.SkillFlashEffect(creature);
			Send.SkillPrepare(creature, skill.Info.Id, skill.GetCastTime());

			return true;
		}

		public override bool Ready(Creature creature, Skill skill, Packet packet)
		{
			skill.Stacks = 1;

			Send.Effect(creature, Effect.FinalHit, (byte)1, (byte)1);
			Send.SkillReady(creature, skill.Info.Id);

			return true;
		}

		public override void Complete(Creature creature, Skill skill, Packet packet)
		{
			Send.SkillComplete(creature, skill.Info.Id);
			Send.SkillReady(creature, skill.Info.Id);
			skill.State = SkillState.Ready;
		}

		public override void Cancel(Creature creature, Skill skill)
		{
			Send.Effect(creature, Effect.FinalHit, (byte)0);
		}

		public void Use(Creature creature, Skill skill, Packet packet)
		{
			var targetEntityId = packet.GetLong();
			var unk1 = packet.GetInt();
			var unk2 = packet.GetInt();

			if (_cm == null)
				_cm = ChannelServer.Instance.SkillManager.GetHandler<CombatMastery>(SkillId.CombatMastery);

			var target = ChannelServer.Instance.World.GetCreature(targetEntityId);
			if (target != null)
			{
				var pos = creature.GetPosition();
				var targetPos = target.GetPosition();

				if (!pos.InRange(targetPos, creature.AttackRangeFor(target)))
				{
					var telePos = pos.GetRelative(targetPos, -creature.AttackRangeFor(target) + 100);

					Send.Effect(creature, Effect.SilentMoveTeleport, targetEntityId, (byte)0);

					creature.SetPosition(telePos.X, telePos.Y);
					Send.SkillTeleport(creature, telePos.X, telePos.Y);
				}


				var result = _cm.Use(creature, skill, targetEntityId);
				Send.CombatAttackR(creature, result == CombatSkillResult.Okay);
			}
			else
			{
				Send.CombatAttackR(creature, false);
			}

			Send.SkillUse(creature, skill.Info.Id, targetEntityId, unk1, unk2);
		}
	}
}
