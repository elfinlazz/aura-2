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

namespace Aura.Channel.Network.Handlers
{
	public partial class ChannelServerHandlers : PacketHandlerManager<ChannelClient>
	{
		/// <summary>
		/// Starting/Activating a skill.
		/// </summary>
		/// <example>
		/// ...
		/// </example>
		[PacketHandler(Op.SkillStart)]
		public void SkillStart(ChannelClient client, Packet packet)
		{
		}

		/// <summary>
		/// Stopping/Deactivating a skill.
		/// </summary>
		/// <example>
		/// ...
		/// </example>
		[PacketHandler(Op.SkillStop)]
		public void SkillStop(ChannelClient client, Packet packet)
		{
		}
	}
}
