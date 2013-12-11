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
				if (creature.GetPosition().InRange(prop.GetPosition(), 400))
				{
					Send.HittingProp(creature, prop.EntityId);

					if (prop.Behavior != null)
					{
						prop.Behavior(creature, prop);
					}
					else
					{
						Log.Unimplemented("No prop behavior for '{0}'.", prop.EntityIdHex);
					}
				}
				else
				{
					Send.Notice(creature, NoticeType.MiddleLower, Localization.Get("world.too_far"));
					Log.Warning("Player '{0}' tried to hit prop out of range.", creature.Name);
				}
			}

			Send.HitPropR(creature);
		}

		/// <summary>
		/// Sent when prop is "touched".
		/// </summary>
		/// <example>
		/// 001 [00A000010009042A] Long   : 45036000569263146
		/// </example>
		[PacketHandler(Op.TouchProp)]
		public void TouchProp(ChannelClient client, Packet packet)
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
				if (creature.GetPosition().InRange(prop.GetPosition(), 400))
				{
					Send.HittingProp(creature, prop.EntityId);

					if (prop.Behavior != null)
					{
						prop.Behavior(creature, prop);
					}
					else
					{
						Log.Unimplemented("No prop behavior for '{0}'.", prop.EntityIdHex);
					}
				}
				else
				{
					Send.Notice(creature, NoticeType.MiddleLower, Localization.Get("world.too_far"));
					Log.Warning("Player '{0}' tried to touch prop out of range.", creature.Name);
				}
			}

			Send.TouchPropR(creature);
		}
	}
}
