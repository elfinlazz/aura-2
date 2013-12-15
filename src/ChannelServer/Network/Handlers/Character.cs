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
		/// Sent when changing titles.
		/// </summary>
		/// <example>
		/// 0001 [............0001] Short  : 1
		/// 0002 [............0002] Short  : 2
		/// </example>
		[PacketHandler(Op.ChangeTitle)]
		public void ChangeTitle(ChannelClient client, Packet packet)
		{
			var titleId = packet.GetUShort();
			var optionTitleId = packet.GetUShort();

			var creature = client.GetCreature(packet.Id);
			if (creature == null)
				return;

			var titleSuccess = false;
			var optionSuccess = false;

			// Make sure the creature has this title
			if (titleId == 0 || creature.Titles.Usable(titleId))
			{
				creature.Titles.SelectedTitle = titleId;
				titleSuccess = true;
			}

			if (optionTitleId == 0 || creature.Titles.Usable(optionTitleId))
			{
				creature.Titles.SelectedOptionTitle = optionTitleId;
				optionSuccess = true;
			}

			if (titleSuccess || optionSuccess)
			{
				creature.Titles.Applied = DateTime.Now;
				Send.TitleUpdate(creature);
			}

			Send.ChangeTitleR(creature, titleSuccess, optionSuccess);
		}
	}
}
