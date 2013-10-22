// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Shared.Network;
using Aura.Shared.Const;
using Aura.Shared.Util;

namespace Aura.Login.Network
{
	public static partial class Send
	{
		/// <summary>
		/// Sends ClientIdentR to client.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="success"></param>
		public static void CheckIdentR(LoginClient client, bool success)
		{
			var packet = new MabiPacket(Op.ClientIdentR, MabiId.Login);
			packet.PutByte(success);
			packet.PutLong(MabiTime.Now.TimeStamp);

			client.Send(packet);
		}
	}
}
