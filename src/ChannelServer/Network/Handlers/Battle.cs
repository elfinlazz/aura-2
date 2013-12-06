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
		/// Sent when switching between idle and attack mode.
		/// </summary>
		/// <example>
		/// 001 [..............01] Byte   : 1
		/// </example>
		[PacketHandler(Op.ChangeStanceRequest)]
		public void ChangeStance(ChannelClient client, Packet packet)
		{
			var stance = (BattleStance)packet.GetByte();

			var creature = client.GetCreature(packet.Id);
			if (creature == null)
				return;

			if (stance > BattleStance.Ready)
			{
				Log.Warning("HandleChangeStance: Unknown battle stance '{0}'.", stance);
				return;
			}

			// Change stance
			creature.BattleStance = stance;
			Send.ChangeStance(creature);

			// Response (unlocks the char)
			Send.ChangeStanceRequestR(creature);
		}

		/// <summary>
		/// Sent when prop is "attacked".
		/// </summary>
		/// <example>
		/// 001 [00A1000100090001] Long   : 45317475545972737
		/// </example>
		[PacketHandler(Op.HitProp)]
		public void HitProp(ChannelClient client, Packet packet)
		{
			var entityId = packet.GetLong();

			// Check creature and region
			var creature = client.GetCreature(packet.Id);
			if (creature == null || creature.Region == null || creature.IsDead)
				return;

			// Check prop
			var prop = creature.Region.GetProp(entityId);
			if (prop == null)
			{
				Send.ServerMessage(creature, "Unknown target.");
			}
			else
			{
				Send.HittingProp(creature, prop.EntityId);

				if (prop.Behavior != null)
				{
					prop.Behavior(creature, prop);
				}
				else
					Log.Unimplemented("No prop behavior for '{0}'.", prop.EntityIdHex);
			}

			Send.HitPropR(creature);
		}
	}
}
