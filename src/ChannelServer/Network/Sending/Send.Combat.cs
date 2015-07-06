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
using Aura.Mabi.Const;
using Aura.Mabi.Network;

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
			packet.PutByte(creature.IsInBattleStance);
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
			packet.PutInt(pack.Id);
			packet.PutInt(pack.PrevId);
			packet.PutByte(pack.Hit);
			packet.PutByte((byte)pack.Type);
			packet.PutByte(pack.Flags);
			if ((pack.Flags & 1) == 1)
			{
				packet.PutInt(pack.BlockedByShieldPosX);
				packet.PutInt(pack.BlockedByShieldPosY);
				packet.PutLong(pack.ShieldCasterId);
			}

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
				actionPacket.PutInt(pack.Id);
				actionPacket.PutLong(action.Creature.EntityId);
				actionPacket.PutByte((byte)action.Flags);
				actionPacket.PutShort(action.Stun);
				actionPacket.PutUShort((ushort)action.SkillId);
				actionPacket.PutUShort((ushort)action.SecondarySkillId);

				// Official name for CombatAction is CombatScene, and both Actions are Combatant.
				// Official client distinguish between Attacker and Defender simply by checking Flags. tachiorz

				// AttackerAction
				//if ((action.Flags & CombatActionFlags.Attacker) != 0)
				if (action.Category == CombatActionCategory.Attack)
				{
					var aAction = action as AttackerAction;

					actionPacket.PutLong(aAction.TargetId);
					actionPacket.PutUInt((uint)aAction.Options);
					actionPacket.PutByte(aAction.UsedWeaponSet);
					actionPacket.PutByte(aAction.WeaponParameterType); // !aAction.Has(AttackerOptions.KnockBackHit2) ? 2 : 1)); // ?
					actionPacket.PutInt(pos.X);
					actionPacket.PutInt(pos.Y);

					if ((aAction.Options & AttackerOptions.Dashed) != 0)
					{
						actionPacket.PutFloat(0); // DashedPosX
						actionPacket.PutFloat(0); // DashedPosY
						actionPacket.PutUInt(0); // DashDelay
					}

					if ((aAction.Options & AttackerOptions.PhaseAttack) != 0)
						actionPacket.PutByte(aAction.Phase);

					if ((aAction.Options & AttackerOptions.UseEffect) != 0)
						actionPacket.PutLong(aAction.PropId);
				}
				// TargetAction
				//if ((action.Flags & CombatActionFlags.TakeHit) != 0)
				else if (action.Category == CombatActionCategory.Target && !action.Is(CombatActionType.None))
				{
					var tAction = action as TargetAction;

					// Target used Defense or Counter
					if ((action.Flags & CombatActionType.Attacker) != 0)
					{
						actionPacket.PutLong(tAction.Attacker.EntityId);
						actionPacket.PutInt(0); // attacker Options
						actionPacket.PutByte(tAction.UsedWeaponSet);
						actionPacket.PutByte(tAction.WeaponParameterType);
						actionPacket.PutInt(pos.X);
						actionPacket.PutInt(pos.Y);
					}

					actionPacket.PutUInt((uint)tAction.Options);
					actionPacket.PutFloat(tAction.Damage);
					actionPacket.PutFloat(tAction.Wound);
					actionPacket.PutInt((int)tAction.ManaDamage);

					actionPacket.PutFloat(attackerPos.X - pos.X);
					actionPacket.PutFloat(attackerPos.Y - pos.Y);
					if (tAction.IsKnockBack)
					{
						actionPacket.PutFloat(pos.X);
						actionPacket.PutFloat(pos.Y);

						// [190200, NA203 (22.04.2015)]
						{
							actionPacket.PutInt(0);
						}
					}

					if ((tAction.Options & TargetOptions.MultiHit) != 0)
					{
						actionPacket.PutUInt(0); // MultiHitDamageCount
						actionPacket.PutUInt(0); // MultiHitDamageShowTime
					}

					actionPacket.PutByte(tAction.EffectFlags); // PDef? Seen as 0x20 in a normal attack (G18)
					actionPacket.PutInt(tAction.Delay);
					actionPacket.PutLong(tAction.Attacker.EntityId);
				}

				packet.PutBin(actionPacket);
			}

			pack.Attacker.Region.Broadcast(packet, pack.Attacker);
		}

		/// <summary>
		/// Broadcasts CombatActionEnd in range of creature.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="combatActionId"></param>
		public static void CombatActionEnd(Creature creature, int combatActionId)
		{
			var packet = new Packet(Op.CombatActionEnd, MabiId.Broadcast);
			packet.PutInt(combatActionId);

			creature.Region.Broadcast(packet, creature);
		}

		/// <summary>
		/// Broadcasts SetCombatTarget in range of creature.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="targetEntityId"></param>
		/// <param name="mode"></param>
		public static void SetCombatTarget(Creature creature, long targetEntityId, TargetMode mode)
		{
			var packet = new Packet(Op.SetCombatTarget, creature.EntityId);
			packet.PutLong(targetEntityId);
			packet.PutByte((byte)mode);
			packet.PutString("");

			creature.Region.Broadcast(packet, creature);
		}

		/// <summary>
		/// Sends CombatTargetUpdate to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="targetEntityId"></param>
		public static void CombatTargetUpdate(Creature creature, long targetEntityId)
		{
			var packet = new Packet(Op.CombatTargetUpdate, creature.EntityId);
			packet.PutLong(targetEntityId);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends CombatAttackR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		public static void CombatAttackR(Creature creature, bool success)
		{
			var packet = new Packet(Op.CombatAttackR, creature.EntityId);
			packet.PutByte(success);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends CombatAttackR to creature's client.
		/// </summary>
		/// <remarks>
		/// Contains creature's and target's position, sent for out of range,
		/// so the client knows it has to adjust the creature's position.
		/// </remarks>
		/// <param name="creature"></param>
		/// <param name="target"></param>
		public static void CombatAttackR(Creature creature, Creature target)
		{
			var creaturePos = creature.GetPosition();
			var targetPos = target.GetPosition();

			var packet = new Packet(Op.CombatAttackR, creature.EntityId);
			packet.PutByte(100);
			packet.PutLong(target.EntityId);
			packet.PutByte(0);
			packet.PutByte(0);
			packet.PutInt(creaturePos.X);
			packet.PutInt(creaturePos.Y);
			packet.PutByte(0);
			packet.PutInt(targetPos.X);
			packet.PutInt(targetPos.Y);
			packet.PutString("");

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends CombatUsedSkill to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skillId"></param>
		public static void CombatUsedSkill(Creature creature, SkillId skillId)
		{
			var packet = new Packet(Op.CombatUsedSkill, creature.EntityId);
			packet.PutUShort((ushort)skillId);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Broadcasts SetFinisher in range of creature.
		/// </summary>
		/// <remarks>
		/// Displays flashing "Finish" if player is finisher.
		/// 
		/// </remarks>
		/// <param name="creature"></param>
		/// <param name="finisherEntityId"></param>
		public static void SetFinisher(Creature creature, long finisherEntityId)
		{
			var packet = new Packet(Op.SetFinisher, creature.EntityId);
			packet.PutLong(finisherEntityId);

			creature.Region.Broadcast(packet, creature);
		}

		/// <summary>
		/// Broadcasts SetFinisher2 in range of creature.
		/// </summary>
		/// <remarks>
		/// Purpose unknown, sent shortly after SetFinisher.
		/// </remarks>
		/// <param name="creature"></param>
		public static void SetFinisher2(Creature creature)
		{
			var packet = new Packet(Op.SetFinisher2, creature.EntityId);

			creature.Region.Broadcast(packet, creature);
		}

		/// <summary>
		/// Broadcasts IsNowDead in range of creature.
		/// </summary>
		/// <remarks>
		/// Creature isn't targetable anymore after this.
		/// </remarks>
		/// <param name="creature"></param>
		public static void IsNowDead(Creature creature)
		{
			var packet = new Packet(Op.IsNowDead, creature.EntityId);

			creature.Region.Broadcast(packet, creature);
		}

		/// <summary>
		/// Sends StabilityMeterInit to receiver, containing meter information
		/// of creature.
		/// </summary>
		/// <remarks>
		/// Init is sent officially the first time you get these information,
		/// afterwards it sends Update. The only visible difference so far
		/// seems to be that Init doesn't work with negative values.
		/// </remarks>
		/// <param name="receiver"></param>
		/// <param name="creature"></param>
		public static void StabilityMeterInit(Creature receiver, Creature creature)
		{
			var packet = new Packet(Op.StabilityMeterInit, receiver.EntityId);
			packet.PutLong(creature.EntityId);
			packet.PutByte(1);
			packet.PutFloat(creature.Stability);

			receiver.Client.Send(packet);
		}

		/// <summary>
		/// Sends StabilityMeterUpdate to receiver, containing meter information
		/// of creature.
		/// </summary>
		/// <param name="receiver"></param>
		/// <param name="creature"></param>
		public static void StabilityMeterUpdate(Creature receiver, Creature creature)
		{
			var packet = new Packet(Op.StabilityMeterUpdate, receiver.EntityId);
			packet.PutLong(creature.EntityId);
			packet.PutFloat(creature.Stability);
			packet.PutByte(1);

			receiver.Client.Send(packet);
		}

		/// <summary>
		/// Sends TouchMimicR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		public static void TouchMimicR(Creature creature)
		{
			var packet = new Packet(Op.TouchMimicR, creature.EntityId);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends UnkCombatR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		public static void UnkCombatR(Creature creature)
		{
			var packet = new Packet(Op.UnkCombatR, creature.EntityId);
			packet.PutByte(true);

			creature.Client.Send(packet);
		}
	}
}
