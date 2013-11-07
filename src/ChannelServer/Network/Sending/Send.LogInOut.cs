// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using Aura.Shared.Mabi.Const;
using Aura.Shared.Network;

namespace Aura.Channel.Network.Sending
{
	public static partial class Send
	{
		/// <summary>
		/// Sends ChannelLoginR to client.
		/// </summary>
		/// <remarks>
		/// Sending a negative response doesn't do anything.
		/// </remarks>
		public static void ChannelLoginR(ChannelClient client)
		{
			var packet = new Packet(Op.ChannelLoginR, MabiId.Channel);
			packet.PutByte(true);
			packet.PutLong(client.Character.EntityId);
			packet.PutLong(DateTime.Now);
			packet.PutInt((int)DateTime.Now.DayOfWeek);
			packet.PutString(""); // http://211.218.233.238/korea/

			client.Send(packet);
		}

		/// <summary>
		/// Sends ChannelDisconnectR to client.
		/// </summary>
		/// <param name="client"></param>
		public static void ChannelDisconnectR(ChannelClient client)
		{
			var packet = new Packet(Op.DisconnectRequestR, MabiId.Channel);
			packet.PutByte(0);

			client.Send(packet);
		}
	}
}
