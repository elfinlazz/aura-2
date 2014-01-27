// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Shared.Network;
using Aura.Channel.Network.Sending;
using Aura.Shared.Util;
using Aura.Shared.Mabi.Const;
using Aura.Channel.Skills.Base;

namespace Aura.Channel.Network.Handlers
{
	public partial class ChannelServerHandlers : PacketHandlerManager<ChannelClient>
	{
		/// <summary>
		/// Starting/Activating a skill.
		/// </summary>
		/// <remarks>
		/// The second paramter seems to always be a string if the
		/// main player uses the skill. When pets use it you get a byte.
		/// The string is a MabiDictionary, with optional extra information,
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

			var creature = client.GetCreature(packet.Id);
			if (creature == null)
				return;

			var skill = creature.Skills.Get(skillId);
			if (skill == null)
			{
				Log.Warning("SkillStart: Player '{0}' tried to use skill '{1}', which he doesn't have.", creature.Name, skillId);
				Send.SkillStart_Fail(creature, skillId);
				return;
			}

			var handler = ChannelServer.Instance.SkillManager.GetHandler<IStartable>(skillId);
			if (handler == null)
			{
				Log.Unimplemented("Skill handler for '{0}'.", skillId);
				Send.ServerMessage(creature, "This skill isn't implemented yet.");
				Send.SkillStart_Fail(creature, skillId);
				return;
			}

			try
			{
				handler.Start(creature, skill, packet);
			}
			catch (NotImplementedException)
			{
				Log.Unimplemented("Skill start method for '{0}'.", skillId);
				Send.ServerMessage(creature, "This skill isn't implemented completely yet.");
				Send.SkillStart_Fail(creature, skillId);
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

			var creature = client.GetCreature(packet.Id);
			if (creature == null)
				return;

			var skill = creature.Skills.Get(skillId);
			if (skill == null)
			{
				Log.Warning("SkillStop: Player '{0}' tried to use skill '{1}', which he doesn't have.", creature.Name, skillId);
				Send.SkillStop_Fail(creature, skillId);
				return;
			}

			var handler = ChannelServer.Instance.SkillManager.GetHandler<IStoppable>(skillId);
			if (handler == null)
			{
				Log.Unimplemented("Skill handler for '{0}'.", skillId);
				Send.ServerMessage(creature, "This skill isn't implemented yet.");
				Send.SkillStop_Fail(creature, skillId);
				return;
			}

			try
			{
				handler.Stop(creature, skill, packet);
			}
			catch (NotImplementedException)
			{
				Log.Unimplemented("Skill stop method for '{0}'.", skillId);
				Send.ServerMessage(creature, "This skill isn't implemented completely yet.");
				Send.SkillStop_Fail(creature, skillId);
			}
		}
	}
}
