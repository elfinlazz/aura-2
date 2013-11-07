// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Text;
using System.Linq;
using Aura.Shared.Database;
using Aura.Shared.Mabi;
using Aura.Shared.Mabi.Const;
using Aura.Shared.Network;
using Aura.Shared.Util;
using System.Collections.Generic;
using Aura.Channel.Database;
using Aura.Channel.Network.Sending;
using Aura.Channel.World.Entities;

namespace Aura.Channel.Network.Handlers
{
	public partial class ChannelServerHandlers : PacketHandlerManager<ChannelClient>
	{
		/// <summary>
		/// Disconnection request.
		/// </summary>
		/// <remarks>
		/// Client doesn't disconnect till we answer.
		/// </remarks>
		/// <example>
		/// ...
		/// </example>
		[PacketHandler(Op.ChannelDisconnect)]
		public void HandleDisconnect(ChannelClient client, Packet packet)
		{
			var unk1 = packet.GetByte(); // 1 | 2 (maybe login vs exit?)

			Log.Info("'{0}' is closing the connection. Saving...", client.Account.Id);

			//ChannelDb.Instance.SaveAccount(client.Account);

			//foreach (var pc in client.Creatures.Where(cr => cr is PlayerCreature))
			//    WorldManager.Instance.RemoveCreature(pc);

			client.Creatures.Clear();
			client.Character = null;
			client.Account = null;

			Send.ChannelDisconnectR(client);
		}
	}
}
