// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Channel.World.Entities;
using Aura.Shared.Network;
using Aura.Channel.World;
using Aura.Channel.Skills;
using Aura.Shared.Mabi.Const;

namespace Aura.Channel.Network.Sending
{
	public static partial class Send
	{
		/// <summary>
		/// Broadcasts ChangeStance in range of creature.
		/// </summary>
		/// <param name="creature"></param>
		public static void ChangeStance(Creature creature)
		{
			var packet = new Packet(Op.ChangeStance, creature.EntityId);
			packet.PutByte((byte)creature.BattleStance);
			packet.PutByte(1);

			creature.Region.Broadcast(packet, creature);
		}

		/// <summary>
		/// Sends ChangeStanceRequestR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		public static void ChangeStanceRequestR(Creature creature)
		{
			var packet = new Packet(Op.ChangeStanceRequestR, creature.EntityId);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Broadcasts CombatActionPack in range of pack's attacker.
		/// </summary>
		/// <param name="pack"></param>
		public static void CombatAction(CombatActionPack pack)
		{
			var attackerPos = pack.Attacker.GetPosition();

			var packet = new Packet(Op.CombatActionPack, MabiId.Broadcast);
			packet.PutInt(pack.CombatActionId);
			packet.PutInt(pack.PrevCombatActionId);
			packet.PutByte(pack.Hit);
			packet.PutByte(pack.HitsMax);
			packet.PutByte(0);

			// Actions
			packet.PutInt(pack.Actions.Count);
			Packet actionPacket = null;
			foreach (var action in pack.Actions)
			{
				var pos = action.Creature.GetPosition();

				if (actionPacket == null)
					actionPacket = new Packet(Op.CombatAction, action.Creature.EntityId);
				else
					actionPacket.Clear(Op.CombatAction, action.Creature.EntityId);
				actionPacket.PutInt(pack.CombatActionId);
				actionPacket.PutLong(action.Creature.EntityId);
				actionPacket.PutByte((byte)action.Type);
				actionPacket.PutShort(action.StunTime);
				actionPacket.PutUShort((ushort)action.SkillId);
				actionPacket.PutShort(0);

				// AttackerAction
				if (action.Category == CombatActionCategory.Attack)
				{
					var aAction = action as AttackerAction;

					actionPacket.PutLong(aAction.TargetId);
					actionPacket.PutUInt((uint)aAction.Options);
					actionPacket.PutByte(0);
					actionPacket.PutByte((byte)(!aAction.Has(AttackerOptions.KnockBackHit2) ? 2 : 1));
					actionPacket.PutInt(pos.X);
					actionPacket.PutInt(pos.Y);
					if (aAction.PropId != 0)
						actionPacket.PutLong(aAction.PropId);
				}
				// TargetAction
				else if (action.Category == CombatActionCategory.Target && !action.Is(CombatActionType.None))
				{
					var tAction = action as TargetAction;

					// Target used Defense or Counter
					if (tAction.Is(CombatActionType.Defended) || tAction.Is(CombatActionType.CounteredHit))
					{
						actionPacket.PutLong(tAction.Attacker.EntityId);
						actionPacket.PutInt(0);
						actionPacket.PutByte(0);
						actionPacket.PutByte(1);
						actionPacket.PutInt(pos.X);
						actionPacket.PutInt(pos.Y);
					}

					actionPacket.PutUInt((uint)tAction.Options);
					actionPacket.PutFloat(tAction.Damage);
					actionPacket.PutFloat(tAction.ManaDamage);
					actionPacket.PutInt(0);

					actionPacket.PutFloat(attackerPos.X - pos.X);
					actionPacket.PutFloat(attackerPos.Y - pos.Y);
					if (tAction.IsKnockBack)
					{
						actionPacket.PutFloat(pos.X);
						actionPacket.PutFloat(pos.Y);
					}

					actionPacket.PutByte(0); // PDef
					actionPacket.PutInt(tAction.Delay);
					actionPacket.PutLong(tAction.Attacker.EntityId);
				}

				packet.PutBin(actionPacket);
			}

			pack.Attacker.Region.Broadcast(packet, pack.Attacker);
		}
	}
}
