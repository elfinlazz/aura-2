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
using Aura.Data;

namespace Aura.Channel.Network.Handlers
{
	public partial class ChannelServerHandlers : PacketHandlerManager<ChannelClient>
	{
		/// <summary>
		/// Sent regularly to request the current moon gates (?).
		/// </summary>
		/// <remarks>
		/// It seems strange that the moon gates are requested over and over,
		/// but the official answer is always the names of 2 moon gates.
		/// </remarks>
		/// <example>
		/// No Parameters.
		/// </example>
		[PacketHandler(Op.MoonGateInfoRequest)]
		public void MoonGateInfoRequest(ChannelClient client, Packet packet)
		{
			var creature = client.GetCreatureSafe(packet.Id);

			// Empty answer for now.
			Send.MoonGateInfoRequestR(creature);
		}

		/// <summary>
		/// Sent on login to request a list of new mails.
		/// </summary>
		/// <remarks>
		/// Only here to get rid of the unimplemented log for now.
		/// </remarks>
		/// <example>
		/// No parameters.
		/// </example>
		[PacketHandler(Op.MailsRequest)]
		public void MailsRequest(ChannelClient client, Packet packet)
		{
			var creature = client.GetCreatureSafe(packet.Id);

			// Empty answer for now.
			Send.MailsRequestR(creature);
		}

		/// <summary>
		/// Sent on login, answer determines whether the SOS button is displayed.
		/// </summary>
		/// <example>
		/// No parameters.
		/// </example>
		[PacketHandler(Op.SosButtonRequest)]
		public void SosButtonRequest(ChannelClient client, Packet packet)
		{
			var creature = client.GetCreatureSafe(packet.Id);

			// Disable by default, until we have the whole thing.
			Send.SosButtonRequestR(creature, false);
		}

		/// <summary>
		/// Sent on login to get homestead information.
		/// </summary>
		/// <remarks>
		/// Only called once, a few seconds after the player logged in.
		/// This makes it a good place for OnPlayerLoggedIn,
		/// at that point it's safe to do anything.
		/// </remarks>
		/// <example>
		/// 001 [..............00] Byte   : 0
		/// </example>
		[PacketHandler(Op.HomesteadInfoRequest)]
		public void HomesteadInfoRequest(ChannelClient client, Packet packet)
		{
			var unkByte = packet.GetByte();

			var creature = client.GetCreatureSafe(packet.Id);

			// Default answer for now
			Send.HomesteadInfoRequestR(creature);

			ChannelServer.Instance.Events.OnPlayerLoggedIn(creature);
		}

		/// <summary>
		/// ?
		/// </summary>
		/// <remarks>
		/// Dummy handler. Answer is a 1 byte with 2 0 ints.
		/// Sent together with Homestead info request on login.
		/// </remarks>
		/// <example>
		/// No parameters.
		/// </example>
		[PacketHandler(Op.ChannelLoginUnk)]
		public void ChannelLoginUnk(ChannelClient client, Packet packet)
		{
			var creature = client.GetCreatureSafe(packet.Id);

			// Default answer
			Send.ChannelLoginUnkR(creature);
		}

		/// <summary>
		/// ?
		/// </summary>
		/// <remarks>
		/// Dummy handler. Sent on login and after certain warps.
		/// Appears to be a request for the cool down
		/// of the continent warp (see response).
		/// </remarks>
		/// <example>
		/// No parameters.
		/// </example>
		[PacketHandler(Op.ContinentWarpCoolDown)]
		public void ContinentWarpCoolDown(ChannelClient client, Packet packet)
		{
			var creature = client.GetCreatureSafe(packet.Id);

			// Default answer
			Send.ContinentWarpCoolDownR(creature);
		}

		/// <summary>
		/// Sent when the cutscene is over.
		/// </summary>
		/// <example>
		/// 001 [........000186A4] Int    : 100004
		/// </example>
		[PacketHandler(Op.FinishedCutscene)]
		public void FinishedCutscene(ChannelClient client, Packet packet)
		{
			var unkInt = packet.GetInt();

			var creature = client.GetCreatureSafe(packet.Id);

			if (creature.Temp.CurrentCutscene == null)
			{
				Log.Error("FinishedCutscene: Player '{0}' tried to finish invalid cutscene.", creature.EntityIdHex);
				return;
			}

			if (creature.Temp.CurrentCutscene.Leader != creature)
			{
				// TODO: Do we have to send the no-leader message here?
				Log.Warning("FinishedCutscene: Player '{0}' tried to finish cutscene without being the leader.", creature.EntityIdHex);
				return;
			}

			creature.Temp.CurrentCutscene.Finish();
		}

		/// <summary>
		/// Sent to use gesture.
		/// </summary>
		/// <example>
		/// 001 [................] String : rare_1
		/// </example>
		[PacketHandler(Op.UseGesture)]
		public void UseGesture(ChannelClient client, Packet packet)
		{
			var gestureName = packet.GetString();

			var creature = client.GetCreatureSafe(packet.Id);

			creature.StopMove();

			var motionData = AuraData.MotionDb.Find(gestureName);
			if (motionData == null)
			{
				Log.Warning("Creature '{0}' tried to use missing gesture '{1}'.", creature.EntityIdHex, gestureName);
				Send.UseGestureR(creature, false);
				return;
			}

			Send.UseMotion(creature, motionData.Category, motionData.Type, motionData.Loop);
			Send.UseGestureR(creature, true);
		}

		/// <summary>
		/// ?
		/// </summary>
		/// <remarks>
		/// Purpose unknown, sent when pressing escape and switching weapon sets.
		/// </remarks>
		/// <example>
		/// No parameters.
		/// </example>
		[PacketHandler(Op.UnkEsc)]
		public void UnkEsc(ChannelClient client, Packet packet)
		{
		}
	}
}
