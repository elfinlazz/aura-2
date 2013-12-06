// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Shared.Network;
using Aura.Channel.World.Entities;

namespace Aura.Channel.Network.Sending
{
	public static partial class Send
	{
		/// <summary>
		/// Sends NpcTalk to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="xml"></param>
		public static void NpcTalk(Creature creature, string xml)
		{
			var packet = new Packet(Op.NpcTalk, creature.EntityId);
			packet.PutString(xml);
			packet.PutBin();

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends negative NpcTalkStartR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		public static void NpcTalkStartR_Fail(Creature creature)
		{
			NpcTalkStartR(creature, 0);
		}

		/// <summary>
		/// Sends NpcTalkStartR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="npcId">Negative response if 0.</param>
		public static void NpcTalkStartR(Creature creature, long npcId)
		{
			var packet = new Packet(Op.NpcTalkStartR, creature.EntityId);
			packet.PutByte(npcId != 0);
			if (npcId != 0)
				packet.PutLong(npcId);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends NpcTalkEndR to creature's client.
		/// </summary>
		/// <remarks>
		/// If no message is specified "<end/>" is sent,
		/// to close the dialog box immediately.
		/// </remarks>
		/// <param name="creature"></param>
		/// <param name="npcId"></param>
		/// <param name="message">Last message before closing.</param>
		public static void NpcTalkEndR(Creature creature, long npcId, string message = null)
		{
			var p = new Packet(Op.NpcTalkEndR, creature.EntityId);
			p.PutByte(true);
			p.PutLong(npcId);
			p.PutString(message ?? "<end/>");

			creature.Client.Send(p);
		}
	}
}
