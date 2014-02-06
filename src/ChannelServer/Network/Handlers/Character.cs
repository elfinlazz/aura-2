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
			var creature = client.GetCreature(packet.Id);
			if (creature == null) return;

			if (!creature.IsDead)
			{
				Send.DeadMenuR(creature, null);
				return;
			}

			// ...

			Send.DeadMenuR(creature, "foo");
		}

		/// <summary>
		/// Revive request (from dead menu).
		/// </summary>
		/// <example>
		/// 001 [........00000001] Int    : 1
		/// </example>
		[PacketHandler(Op.Revive)]
		public void Revive(ChannelClient client, Packet packet)
		{
			var option = packet.GetInt();

			var creature = client.GetCreature(packet.Id);
			if (creature == null) return;

			if (!creature.IsDead)
			{
				Send.Revive_Fail(creature);
				return;
			}

			// ...

			creature.Deactivate(CreatureStates.Dead);

			Send.BackFromTheDead1(creature);
			Send.StatUpdate(creature, StatUpdateType.Private, Stat.Life, Stat.LifeInjured, Stat.LifeMax, Stat.LifeMaxMod);
			Send.StatUpdate(creature, StatUpdateType.Public, Stat.Life, Stat.LifeInjured, Stat.LifeMax, Stat.LifeMaxMod);
			Send.BackFromTheDead2(creature);
			//Send.DeadFeather(creature);
			Send.Revived(creature);
		}
	}
}
