// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Shared.Network;
using Aura.Channel.Network.Sending;

namespace Aura.Channel.Network.Handlers
{
	public partial class ChannelServerHandlers : PacketHandlerManager<ChannelClient>
	{
		/// <summary>
		/// Sent when pressing "Complete Quest".
		/// </summary>
		/// <example>
		/// 001 [0060F00000000004] Long   : 27285480554889220
		/// </example>
		[PacketHandler(Op.CompleteQuest)]
		public void CompleteQuest(ChannelClient client, Packet packet)
		{
			var uniqueQuestId = packet.GetLong();

			var creature = client.GetCreatureSafe(packet.Id);

			var quest = creature.Quests.GetSafe(uniqueQuestId);
			if (!quest.IsDone) goto L_Fail;

			if (!creature.Quests.Complete(quest, true)) goto L_Fail;

			Send.CompleteQuestR(creature, true);
			return;

		L_Fail:
			Send.CompleteQuestR(creature, false);
		}

		/// <summary>
		/// Sent when pressing "Give Up".
		/// </summary>
		/// <example>
		/// 001 [0060F00000000004] Long   : 27285480554889220
		/// </example>
		[PacketHandler(Op.GiveUpQuest)]
		public void GiveUpQuest(ChannelClient client, Packet packet)
		{
			var uniqueQuestId = packet.GetLong();

			var creature = client.GetCreatureSafe(packet.Id);

			var quest = creature.Quests.GetSafe(uniqueQuestId);

			if (!creature.Quests.GiveUp(quest)) goto L_Fail;

			Send.GiveUpQuestR(creature, true);
			return;

		L_Fail:
			Send.GiveUpQuestR(creature, false);
		}
	}
}
