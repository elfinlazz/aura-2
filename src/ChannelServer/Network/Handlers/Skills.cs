// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Channel.Util;
using Aura.Shared.Network;
using Aura.Channel.Network.Sending;
using Aura.Shared.Util;
using Aura.Shared.Mabi.Const;
using Aura.Channel.Skills.Base;
using Aura.Channel.Skills;

namespace Aura.Channel.Network.Handlers
{
	public partial class ChannelServerHandlers : PacketHandlerManager<ChannelClient>
	{
		/// <summary>
		/// Sent when increasing skill rank.
		/// </summary>
		/// <example>
		/// 001 [............2714] Short  : 10004
		/// </example>
		[PacketHandler(Op.SkillAdvance)]
		public void SkillAdvance(ChannelClient client, Packet packet)
		{
			var skillId = (SkillId)packet.GetUShort();

			var creature = client.GetCreatureSafe(packet.Id);

			var skill = creature.Skills.GetSafe(skillId);
			if (!skill.IsRankable) goto L_Fail;

			var nextRank = skill.Data.GetRankData((int)skill.Info.Rank + 1, creature.Race);
			if (nextRank == null)
			{
				Log.Warning("Player '{0}' tried to advance skill '{1}' to unknown rank '{2}'.", creature.EntityIdHex, skill.Info.Id, skill.Info.Rank + 1);
				goto L_Fail;
			}

			if (creature.AbilityPoints < nextRank.AP)
			{
				Send.MsgBox(creature, Localization.Get("You don't have enough AP."));
				goto L_Fail;
			}

			creature.GiveAp(-nextRank.AP);
			creature.Skills.Give(skill.Info.Id, skill.Info.Rank + 1);

			return;

		L_Fail:
			Send.SkillAdvance_Fail(creature);
		}

		/// <summary>
		/// Starting/Activating a skill.
		/// </summary>
		/// <remarks>
		/// The second paramter seems to always be a string if the
		/// main player uses the skill. When pets use it you get a byte.
		/// The string is a MabiDictionary with optional extra information,
		/// like chair ids for Rest.
		/// </remarks>
		/// <example>
		/// 0001 [............2714] Short  : 10004
		/// 0002 [................] String : 
		/// 
		/// 0001 [............2714] Short  : 10004
		/// 0002 [..............01] Byte   : 1
		/// </example>
		[PacketHandler(Op.SkillStart)]
		public void SkillStart(ChannelClient client, Packet packet)
		{
			var skillId = (SkillId)packet.GetUShort();

			var creature = client.GetCreatureSafe(packet.Id);

			var skill = creature.Skills.GetSafe(skillId);

			var handler = ChannelServer.Instance.SkillManager.GetHandler<IStartable>(skillId);
			if (handler == null)
			{
				Log.Unimplemented("SkillStart: Skill handler or interface for '{0}'.", skillId);
				Send.ServerMessage(creature, Localization.Get("This skill isn't implemented yet."));
				Send.SkillStartSilentCancel(creature, skillId);
				return;
			}

			try
			{
				handler.Start(creature, skill, packet);
			}
			catch (NotImplementedException)
			{
				Log.Unimplemented("SkillStart: Skill start method for '{0}'.", skillId);
				Send.ServerMessage(creature, Localization.Get("This skill isn't implemented completely yet."));
				Send.SkillStartSilentCancel(creature, skillId);
			}
		}

		/// <summary>
		/// Stopping/Deactivating a skill.
		/// </summary>
		/// <remarks>
		/// The second parameter seems to be dependent on the way the skill
		/// is stopped. When using Rest again, while it's active, you get a
		/// string, maybe for potential extra data. When you stop it by
		/// moving you get a byte.
		/// </remarks>
		/// <example>
		/// 0001 [............2714] Short  : 10004
		/// 0002 [................] String :
		/// 
		/// 0001 [............2714] Short  : 10004
		/// 0002 [..............01] Byte   : 1
		/// </example>
		[PacketHandler(Op.SkillStop)]
		public void SkillStop(ChannelClient client, Packet packet)
		{
			var skillId = (SkillId)packet.GetUShort();

			var creature = client.GetCreatureSafe(packet.Id);

			var skill = creature.Skills.GetSafe(skillId);

			var handler = ChannelServer.Instance.SkillManager.GetHandler<IStoppable>(skillId);
			if (handler == null)
			{
				Log.Unimplemented("SkillStop: Skill handler or interface for '{0}'.", skillId);
				Send.ServerMessage(creature, Localization.Get("This skill isn't implemented yet."));
				Send.SkillStopSilentCancel(creature, skillId);
				return;
			}

			try
			{
				handler.Stop(creature, skill, packet);
			}
			catch (NotImplementedException)
			{
				Log.Unimplemented("SkillStop: Skill stop method for '{0}'.", skillId);
				Send.ServerMessage(creature, Localization.Get("This skill isn't implemented completely yet."));
				Send.SkillStopSilentCancel(creature, skillId);
			}
		}

		/// <summary>
		/// Preparing a skill before it can be used.
		/// </summary>
		/// <remarks>
		/// Preparing a skill is the first step of getting a skill ready
		/// to be used. The next packet usually is "Ready", once the skill
		/// is loaded. Some skills skip that step though.
		/// </remarks>
		/// <example>
		/// 0001 [............2714] Short  : 50057
		/// 0002 [................] String : 
		/// </example>
		[PacketHandler(Op.SkillPrepare)]
		public void SkillPrepare(ChannelClient client, Packet packet)
		{
			var skillId = (SkillId)packet.GetUShort();

			var creature = client.GetCreatureSafe(packet.Id);

			// Don't start another while one is active. If you cast another
			// skill with one already active the client sends Cancel first.
			// This should prevent a simultaneous Prepare.
			// If it's the same skill as the active one it *probably* is stackable.
			if (creature.Skills.ActiveSkill != null && creature.Skills.ActiveSkill.Info.Id != skillId)
			{
				Send.SkillPrepareSilentCancel(creature, skillId);
				return;
			}

			var skill = creature.Skills.GetSafe(skillId);
			skill.State = SkillState.None;

			var handler = ChannelServer.Instance.SkillManager.GetHandler<IPreparable>(skillId);
			if (handler == null)
			{
				Log.Unimplemented("SkillPrepare: Skill handler or interface for '{0}'.", skillId);
				Send.ServerMessage(creature, Localization.Get("This skill isn't implemented yet."));
				Send.SkillPrepareSilentCancel(creature, skillId);
				return;
			}

			// Check Mana
			if (creature.Mana < skill.RankData.ManaCost)
			{
				Send.SystemMessage(creature, Localization.Get("Insufficient Mana"));
				Send.SkillPrepareSilentCancel(creature, skillId);
				return;
			}

			// Check Stamina
			if (creature.Stamina < skill.RankData.StaminaCost)
			{
				Send.SystemMessage(creature, Localization.Get("Insufficient Stamina"));
				Send.SkillPrepareSilentCancel(creature, skillId);
				return;
			}

			try
			{
				// Run handler
				var success = handler.Prepare(creature, skill, packet);
				if (!success)
				{
					Send.SkillPrepareSilentCancel(creature, skillId);
					return;
				}

				// Reduce Mana/Stamina
				// TODO: Use regens, for shiny gradually depleting bars
				if (skill.RankData.ManaCost != 0 || skill.RankData.StaminaCost != 0)
				{
					creature.Mana -= skill.RankData.ManaCost;
					creature.Stamina -= skill.RankData.StaminaCost;
					Send.StatUpdate(creature, StatUpdateType.Private, Stat.Mana, Stat.Stamina);
				}

				// Set active skill
				creature.Skills.ActiveSkill = skill;

				// Only set state if the handler didn't skip states.
				if (skill.State == SkillState.None)
				{
					skill.CastEnd = DateTime.Now.AddMilliseconds(skill.GetCastTime());
					skill.State = SkillState.Prepared;
				}
			}
			catch (NotImplementedException)
			{
				Log.Unimplemented("SkillPrepare: Skill prepare method for '{0}'.", skillId);
				Send.ServerMessage(creature, Localization.Get("This skill isn't implemented completely yet."));
				Send.SkillPrepareSilentCancel(creature, skillId);
			}
		}

		/// <summary>
		/// Sent after skill was loaded completely.
		/// </summary>
		/// <example>
		/// 0001 [............2714] Short  : 20002
		/// 0002 [................] String : 
		/// </example>
		[PacketHandler(Op.SkillReady)]
		public void SkillReady(ChannelClient client, Packet packet)
		{
			var skillId = (SkillId)packet.GetUShort();

			var creature = client.GetCreatureSafe(packet.Id);

			var skill = creature.Skills.GetSafe(skillId);

			var handler = ChannelServer.Instance.SkillManager.GetHandler<IReadyable>(skillId);
			if (handler == null)
			{
				Log.Unimplemented("SkillReady: Skill handler or interface for '{0}'.", skillId);
				Send.ServerMessage(creature, Localization.Get("This skill isn't implemented yet."));
				// Cancel?
				return;
			}

			// Can only ready prepared skills
			if (skill.State != SkillState.Prepared)
			{
				Log.Error("SkillReady: Skill '{0}' wasn't prepared first.", skillId);
				Send.ServerMessage(creature, Localization.Get("Error: Skill wasn't prepared."));
				// Cancel?
				return;
			}

			// Check if cast is over
			if (skill.CastEnd > DateTime.Now)
			{
				// Only an error for now, unsure if this could happen accidentally.
				Log.Error("SkillReady: Skill '{0}' wasn't fully casted yet.", skillId);
				Send.ServerMessage(creature, Localization.Get("Error: Skill wasn't fully casted yet."));
				// Cancel?
				return;
			}

			try
			{
				var success = handler.Ready(creature, skill, packet);
				if (!success)
					return;

				creature.Regens.Add("ActiveSkillWait", Stat.Mana, skill.RankData.ManaWait, creature.ManaMax);
				creature.Regens.Add("ActiveSkillWait", Stat.Stamina, skill.RankData.StaminaWait, creature.StaminaMax);

				skill.State = SkillState.Ready;
			}
			catch (NotImplementedException)
			{
				Log.Unimplemented("SkillReady: Skill ready method for '{0}'.", skillId);
				Send.ServerMessage(creature, Localization.Get("This skill isn't implemented completely yet."));
				// Cancel?
			}
		}

		/// <summary>
		/// Using a skill.
		/// </summary>
		/// <remarks>
		/// Sent after Prepare and maybe Ready.
		/// </remarks>
		/// <example>
		/// 0001 [............2714] Short  : 50057
		/// 0002 [................] String : 
		/// </example>
		[PacketHandler(Op.SkillUse)]
		public void SkillUse(ChannelClient client, Packet packet)
		{
			var skillId = (SkillId)packet.GetUShort();

			var creature = client.GetCreatureSafe(packet.Id);

			var skill = creature.Skills.GetSafe(skillId);

			var handler = ChannelServer.Instance.SkillManager.GetHandler<IUseable>(skillId);
			if (handler == null)
			{
				Log.Unimplemented("SkillUse: Skill handler or interface for '{0}'.", skillId);
				Send.ServerMessage(creature, Localization.Get("This skill isn't implemented yet."));
				Send.SkillUseSilentCancel(creature);
				return;
			}

			// Can only use ready skills
			if (skill.State != SkillState.Ready)
			{
				Log.Error("SkillUse: Skill '{0}' wasn't readied first.", skillId);
				Send.ServerMessage(creature, Localization.Get("Error: Skill wasn't ready."));
				Send.SkillUseSilentCancel(creature);
				return;
			}

			try
			{
				handler.Use(creature, skill, packet);

				// Only if it wasn't canceled in Use? (Counter)
				if (skill.State != SkillState.Canceled)
					skill.State = SkillState.Used;

				creature.Regens.Remove("ActiveSkillWait");
			}
			catch (NotImplementedException)
			{
				Log.Unimplemented("SkillUse: Skill use method for '{0}'.", skillId);
				Send.ServerMessage(creature, Localization.Get("This skill isn't implemented completely yet."));
				Send.SkillUseSilentCancel(creature);
			}
		}

		/// <summary>
		/// Completing skill usage.
		/// </summary>
		/// <remarks>
		/// Sent after skill was used successfully.
		/// </remarks>
		/// <example>
		/// 0001 [............2714] Short  : 50057
		/// 0002 [................] String : 
		/// </example>
		[PacketHandler(Op.SkillComplete)]
		public void SkillComplete(ChannelClient client, Packet packet)
		{
			var skillId = (SkillId)packet.GetUShort();

			var creature = client.GetCreatureSafe(packet.Id);

			var skill = creature.Skills.GetSafe(skillId);

			var handler = ChannelServer.Instance.SkillManager.GetHandler<ICompletable>(skillId);
			if (handler == null)
			{
				Log.Unimplemented("SkillComplete: Skill handler or interface for '{0}'.", skillId);
				Send.ServerMessage(creature, Localization.Get("This skill isn't implemented yet."));
				// Cancel?
				goto L_End;
			}

			try
			{
				handler.Complete(creature, skill, packet);
			}
			catch (NotImplementedException)
			{
				Log.Unimplemented("SkillComplete: Skill complete method for '{0}'.", skillId);
				Send.ServerMessage(creature, Localization.Get("This skill isn't implemented completely yet."));
				// Cancel?
			}

		L_End:
			// Reset active skill if all stacks are used up
			if (skill.Stacks == 0)
			{
				creature.Skills.ActiveSkill = null;
				skill.State = SkillState.Completed;
			}
		}

		/// <summary>
		/// Canceling a skill.
		/// </summary>
		/// <remarks>
		/// Only prepareable skills are canceled, startables use Stop.
		/// </remarks>
		/// <example>
		/// 0001 [..............00] Byte   : 0
		/// 0002 [..............01] Byte   : 1
		/// </example>
		[PacketHandler(Op.SkillCancel)]
		public void SkillCancel(ChannelClient client, Packet packet)
		{
			var unkByte1 = packet.GetByte(); // true/false: automatic?
			var unkByte2 = packet.GetByte();

			var creature = client.GetCreatureSafe(packet.Id);

			creature.Regens.Remove("ActiveSkillWait");

			creature.Skills.CancelActiveSkill();
		}
	}
}
