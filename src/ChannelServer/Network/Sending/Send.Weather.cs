// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Collections.Generic;
using System.Linq;
using Aura.Channel.World.Entities;
using Aura.Shared.Mabi.Const;
using Aura.Shared.Network;
using Aura.Shared.Util;
using Aura.Channel.Network.Sending.Helpers;
using System;
using Aura.Channel.World;

namespace Aura.Channel.Network.Sending
{
	public static partial class Send
	{
		/// <summary>
		/// Broadcasts Weather in region.
		/// </summary>
		public static void Weather(Region region, float from, float to, int transitionTime = 60000)
		{
			var packet = new Packet(Op.Weather, MabiId.Broadcast);
			packet.PutByte(0);
			packet.PutInt(region.Id);
			packet.PutByte(2);
			packet.PutByte(0);
			packet.PutByte(1);
			packet.PutString("constant_smooth");
			packet.PutFloat(to);
			packet.PutLong(DateTime.Now);
			packet.PutLong(DateTime.MinValue);
			packet.PutFloat(from);
			packet.PutFloat(from);
			packet.PutLong(transitionTime);
			packet.PutByte(false);
			packet.PutLong(DateTime.MinValue);
			packet.PutInt(2);
			packet.PutByte(0);

			region.Broadcast(packet);
		}

		/// <summary>
		/// Sends Weather to creature's client.
		/// </summary>
		public static void Weather(Creature creature, float from, float to, int transitionTime = 60000)
		{
			var packet = new Packet(Op.Weather, MabiId.Broadcast);
			packet.PutByte(0);
			packet.PutInt(creature.Region.Id);
			packet.PutByte(2);
			packet.PutByte(0);
			packet.PutByte(1);
			packet.PutString("constant_smooth");
			packet.PutFloat(to);
			packet.PutLong(DateTime.Now);
			packet.PutLong(DateTime.MinValue);
			packet.PutFloat(from);
			packet.PutFloat(from);
			packet.PutLong(transitionTime);
			packet.PutByte(false);
			packet.PutLong(DateTime.MinValue);
			packet.PutInt(2);
			packet.PutByte(0);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends Weather to creature's client.
		/// </summary>
		public static void Weather(Creature creature, int regionId, int groupId, string tableType)
		{
			var packet = new Packet(Op.Weather, MabiId.Broadcast);
			packet.PutByte(0);
			packet.PutInt(regionId);
			packet.PutByte(0);
			packet.PutInt(groupId);
			packet.PutByte(2); // type?
			packet.PutByte(1);
			packet.PutString("table");
			packet.PutString(tableType);
			packet.PutLong(0);
			packet.PutByte(0);

			creature.Client.Send(packet);
		}
	}
}
