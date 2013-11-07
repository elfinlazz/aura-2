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
		/// Sends system message (special Chat) to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public static void SystemMessage(Creature creature, string format, params object[] args)
		{
			SystemMessage(creature, "<SYSTEM>", format, args);
		}

		/// <summary>
		/// Sends server message (special Chat) to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public static void ServerMessage(Creature creature, string format, params object[] args)
		{
			SystemMessage(creature, "<SERVER>", format, args);
		}

		/// <summary>
		/// Sends combat message (special Chat) to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public static void CombatMessage(Creature creature, string format, params object[] args)
		{
			SystemMessage(creature, "<COMBAT>", format, args);
		}

		/// <summary>
		/// Sends system message (special Chat) to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="format"></param>
		/// <param name="args"></param>
		private static void SystemMessage(Creature creature, string from, string format, params object[] args)
		{
			var packet = new Packet(Op.Chat, creature.EntityId);
			packet.PutByte(0);
			packet.PutString(from);
			packet.PutString(format, args);
			packet.PutByte(1);
			packet.PutInt(-32640);
			packet.PutInt(0);
			packet.PutByte(0);

			creature.Client.Send(packet);
		}
	}
}
