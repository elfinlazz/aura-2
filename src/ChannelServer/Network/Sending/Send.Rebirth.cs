// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Channel.World.Entities;
using Aura.Shared.Network;
using Aura.Shared.Mabi.Const;

namespace Aura.Channel.Network.Sending
{
	public static partial class Send
	{
		/// <summary>
		/// Sends RequestRebirthR to creature's client.
		/// </summary>
		/// <param name="vehicle"></param>
		public static void RequestRebirthR(Creature creature, bool success)
		{
			var packet = new Packet(Op.RequestRebirthR, MabiId.Login);
			packet.PutByte(success);

			creature.Client.Send(packet);
		}
	}
}
