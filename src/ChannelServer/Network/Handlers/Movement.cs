// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Channel.Util;
using Aura.Shared.Network;
using Aura.Channel.World;
using Aura.Shared.Util;
using Aura.Data;
using Aura.Data.Database;

namespace Aura.Channel.Network.Handlers
{
	public partial class ChannelServerHandlers : PacketHandlerManager<ChannelClient>
	{
		/// <summary>
		/// Simple walking/running.
		/// </summary>
		/// <example>
		/// 001 [........0000321F] Int    : 12831
		/// 002 [........0000966F] Int    : 38511
		/// 003 [..............01] Byte   : 1
		/// 004 [..............00] Byte   : 0
		/// </example>
		[PacketHandler(Op.Run, Op.Walk)]
		public void Run(ChannelClient client, Packet packet)
		{
			var x = packet.GetInt();
			var y = packet.GetInt();

			var creature = client.GetCreatureSafe(packet.Id);

			var from = creature.GetPosition();
			var to = new Position(x, y);

			//Position intersection;
			//if (creature.Region.Collissions.Find(from, to, out intersection))
			//{
			//    Aura.Shared.Util.Log.Debug("Intersection at '{0}'", intersection);
			//}

			creature.Region.ActivateAis(creature, from, to);

			creature.Move(to, (packet.Op == Op.Walk));
		}

		/// <summary>
		/// Sent when a client side event is triggered.
		/// </summary>
		/// <remarks>
		/// For example, when entering a new area.
		/// The client events also trigger new BGM and stuff like that.
		/// </remarks>
		/// <example>
		/// 001 [00B0000100090576] Long   : 49539600196633974
		/// 002 [........00000065] Int    : 101
		/// 003 [................] String : 
		/// </example>
		[PacketHandler(Op.EventInform)]
		public void EventInform(ChannelClient client, Packet packet)
		{
			var eventId = packet.GetLong();
			var signalType = (SignalType)packet.GetInt();
			var unkString = packet.GetString();

			var creature = client.GetCreatureSafe(packet.Id);

			// Do something with this information?

			// Get event
			var eventData = AuraData.RegionInfoDb.FindEvent(eventId);
			if (eventData == null)
			{
				Log.Warning("EventInform: Player '{0:X16}' triggered unknown event '{1:X16}'.", creature.EntityId, eventId);
				return;
			}

			// Check range
			// TODO: Checking region id doesn't work because a "leave" event
			//   can be triggered after we've changed the creature's region.
			//   Checking position doesn't work because the events can be
			//   quite large.
			if (eventData.RegionId != creature.RegionId && signalType == SignalType.Enter)
			{
				Log.Warning("EventInform: Player '{0:X16}' triggered event '{1:X16}' out of range.", creature.EntityId, eventId);
				return;
			}

			// Check handler
			var clientEvent = ChannelServer.Instance.ScriptManager.GetClientEventHandler(eventId, signalType);
			if (clientEvent == null) return;

			// Run
			clientEvent(creature, eventData);
		}
	}
}
