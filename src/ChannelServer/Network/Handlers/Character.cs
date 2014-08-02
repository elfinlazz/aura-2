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
using Aura.Channel.World.Entities.Creatures;

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

			var creature = client.GetCreatureSafe(packet.Id);

			var titleSuccess = creature.Titles.ChangeTitle(titleId, false);
			var optionSuccess = creature.Titles.ChangeTitle(optionTitleId, true);

			Send.ChangeTitleR(creature, titleSuccess, optionSuccess);
		}

		/// <summary>
		/// Request for receiving the available revive methods.
		/// </summary>
		/// <remarks>
		/// Sent automatically after dying and when toggling the window
		/// afterwards.
		/// </remarks>
		/// <example>
		/// No parameters.
		/// </example>
		[PacketHandler(Op.DeadMenu)]
		public void DeadMenu(ChannelClient client, Packet packet)
		{
			var creature = client.GetCreatureSafe(packet.Id);

			if (!creature.IsDead)
			{
				Send.DeadMenuR(creature, null);
				return;
			}

			// ...

			var menu = new CreatureDeadMenu();
			menu.Add(ReviveOptions.HereNoPenalty);

			Send.DeadMenuR(creature, menu);
		}

		/// <summary>
		/// Revive request (from dead menu).
		/// </summary>
		/// <example>
		/// Town
		/// 001 [........00000001] Int    : 1
		/// 
		/// ArenaWaitingRoom
		/// 001 [........00000001] Int    : 21
		/// </example>
		[PacketHandler(Op.Revive)]
		public void Revive(ChannelClient client, Packet packet)
		{
			var option = (ReviveOptions)(1 << (packet.GetInt() - 1));

			var creature = client.GetCreatureSafe(packet.Id);
			if (creature == null) return;

			if (!creature.IsDead)
			{
				Send.Revive_Fail(creature);
				return;
			}

			// ...

			creature.Revive();
		}
	}
}
