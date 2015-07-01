// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Shared.Network;
using Aura.Channel.Network.Sending;
using Aura.Shared.Util;
using Aura.Mabi.Const;
using Aura.Channel.World.Entities;
using Aura.Channel.Skills.Base;
using Aura.Channel.Skills;
using Aura.Mabi.Network;

namespace Aura.Channel.Network.Handlers
{
	public partial class ChannelServerHandlers : PacketHandlerManager<ChannelClient>
	{
		/// <summary>
		/// Sent when switching between idle and attack mode.
		/// </summary>
		/// <example>
		/// 001 [..............01] Byte   : 1
		/// </example>
		[PacketHandler(Op.ChangeStanceRequest)]
		public void ChangeStance(ChannelClient client, Packet packet)
		{
			var stance = packet.GetByte();

			var creature = client.GetCreatureSafe(packet.Id);

			if (stance > 1)
			{
				Log.Warning("HandleChangeStance: Unknown battle stance '{0}'.", stance);
				return;
			}

			// Change stance
			if (creature.Can(Locks.ChanceStance))
				creature.IsInBattleStance = Convert.ToBoolean(stance);
			else
				Log.Debug("ChanceStance locked for '{0}'.", creature.Name);

			// Response (unlocks the char)
			Send.ChangeStanceRequestR(creature);
		}

		/// <summary>
		/// Sent for targetting an enemy.
		/// </summary>
		/// <remarks>
		/// The mode seems to specify in what way the creature is targetting.
		/// With NPCs mode 1 is notice, mode 2 is aggro ("!" and "!!").
		/// I have only one log of a player sending one != 0, coming from
		/// a mount that's able to attack.
		/// 
		/// The purpose of CombatTargetUpdate is unknown, it seems to be only
		/// active for a few ms before it's reset again, maybe it's not even
		/// related to this.
		/// </remarks>
		/// <example>
		/// 0001 [0010F00000005B3B] Long   : 4767482418060091
		/// 0002 [..............00] Byte   : 0
		/// 0003 [................] String : 
		/// </example>
		[PacketHandler(Op.SetCombatTarget)]
		public void SetCombatTarget(ChannelClient client, Packet packet)
		{
			var targetEntityId = packet.GetLong();
			var mode = (TargetMode)packet.GetByte();
			var unkString = packet.GetString();

			var creature = client.GetCreatureSafe(packet.Id);

			// Id == 0 means untargetting, go with the null
			Creature target = null;
			if (targetEntityId != 0 && (target = creature.Region.GetCreature(targetEntityId)) == null)
			{
				Log.Warning("Creature '{0}' targetted invalid entity '{1}'.", creature.Name, targetEntityId.ToString("X16"));
				Send.Notice(creature, "Invalid target");
				Send.SetCombatTarget(creature, 0, 0);
				return;
			}

			creature.Target = target;

			Send.SetCombatTarget(creature, targetEntityId, mode);

			// Purpose unknown, without this the client doesn't seem to
			// accept the stun time, you can spam attacks.
			Send.CombatTargetUpdate(creature, targetEntityId);
		}

		/// <summary>
		/// Sent for Combat Mastery and many other skills.
		/// </summary>
		/// <remarks>
		/// The packet doesn't specify the skill to use,
		/// we use the currently loaded one.
		/// </remarks>
		/// <example>
		/// 0001 [0010F00000005B3B] Long   : 4767482418060091
		/// 0002 [................] String : 
		/// </example>
		[PacketHandler(Op.CombatAttack)]
		public void CombatAttack(ChannelClient client, Packet packet)
		{
			var targetEntityId = packet.GetLong();
			var unkString = packet.GetString();

			var creature = client.GetCreatureSafe(packet.Id);

			// Check lock
			if (!creature.Can(Locks.Attack))
			{
				Log.Debug("Attack locked for '{0}'.", creature.Name);
				goto L_End;
			}

			// Check target
			var target = creature.Region.GetCreature(targetEntityId);
			if (target == null || !creature.CanTarget(target))
				goto L_End;

			// Check Stun
			if (creature.IsStunned)
				goto L_End;

			// Get skill
			var skill = creature.Skills.ActiveSkill;
			if (skill == null && (skill = creature.Skills.Get(SkillId.CombatMastery)) == null)
			{
				Log.Warning("CombatAttack: Creature '{0}' doesn't have Combat Mastery.", creature.Name);
				goto L_End;
			}

			// Get handler
			var skillHandler = ChannelServer.Instance.SkillManager.GetHandler<ISkillHandler>(skill.Info.Id);
			if (skillHandler == null)
			{
				Log.Unimplemented("CombatAttack: Skill handler or interface for '{0}'.", skill.Info.Id);
				Send.ServerMessage(creature, "This combat skill isn't implemented yet.");
				goto L_End;
			}

			var handler = skillHandler as ICombatSkill;
			if (handler == null)
			{
				// Skill exists, but doesn't seem to be a combat skill, ignore.
				// Example: Clicking a monster while Healing is active.
				goto L_End;
			}

			try
			{
				var result = handler.Use(creature, skill, targetEntityId);

				if (result == CombatSkillResult.Okay)
				{
					Send.CombatAttackR(creature, true);
					skill.State = SkillState.Used;

					creature.Regens.Remove("ActiveSkillWait");
				}
				else if (result == CombatSkillResult.OutOfRange)
				{
					Send.CombatAttackR(creature, target);
				}
				else
				{
					Send.CombatAttackR(creature, false);
				}

				return;
			}
			catch (NotImplementedException)
			{
				Log.Unimplemented("CombatAttack: Skill use method for '{0}'.", skill.Info.Id);
				Send.ServerMessage(creature, "This combat skill isn't implemented completely yet.");
			}

		L_End:
			Send.CombatAttackR(creature, false);
		}

		/// <summary>
		/// ?
		/// </summary>
		/// <remarks>
		/// Sent once when creature is first targeted, with its entity id,
		/// and again when it dies, with 0. Assumed to be a subscription
		/// request for the stability meter. We send it to everyone around.
		/// </remarks>
		/// <example>
		/// 0001 [0010F00000046344] Long   : 4767482418324292
		/// </example>
		[PacketHandler(Op.SubsribeStabilityMeter)]
		public void SubsribeStabilityMeter(ChannelClient client, Packet packet)
		{
			// ...

			// Answer:
			// Op: 0000AA1E, Id: 00100000000XXXXX
			// 0001 [0010F00000046344] Long   : 4767482418324292
			// 0002 [..............01] Byte   : 1
			// 0003 [........40590000] Float  : 100.0
		}

		/// <summary>
		/// Sent when touching a sleeping mimic.
		/// </summary>
		/// <example>
		/// 0001 [0010F00000046344] Long   : 4767482418324292
		/// </example>
		[PacketHandler(Op.TouchMimic)]
		public void TouchMimic(ChannelClient client, Packet packet)
		{
			var targetEntityId = packet.GetLong();
			var creature = client.GetCreatureSafe(packet.Id);

			var target = creature.Region.GetCreature(targetEntityId);
			target.Aggro(creature);

			Send.TouchMimicR(creature);
		}

		/// <summary>
		/// ?
		/// </summary>
		/// <remarks>
		/// Sent sometimes during combat? Reproducable by spamming attack
		/// and movement packets. Response seems to be a single byte,
		/// probably a bool.
		/// If this is triggered, and you suddenly log out, the client
		/// might send a SetCombatTarget packet after/during DisconnectRequest,
		/// which might cause a security violation, because the creature
		/// has been removed from the client by then. This isn't too
		/// much of a problem but we should probably look into it some time.
		/// </remarks>
		/// <example>
		/// No parameters.
		/// </example>
		[PacketHandler(Op.UnkCombat)]
		public void UnkCombat(ChannelClient client, Packet packet)
		{
			var creature = client.GetCreatureSafe(packet.Id);

			Send.UnkCombatR(creature);
		}
	}
}
