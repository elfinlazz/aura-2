// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Channel.World.Entities;
using Aura.Shared.Network;

namespace Aura.Channel.Network.Sending
{
	public static partial class Send
	{
		/// <summary>
		/// Sends empty VehicleInfo in range of vehicle.
		/// </summary>
		/// <param name="vehicle"></param>
		public static void VehicleInfo(Creature vehicle)
		{
			var packet = new Packet(Op.VehicleInfo, vehicle.EntityId);
			packet.PutInt(0);
			packet.PutInt(1);
			packet.PutLong(vehicle.EntityId);
			packet.PutInt(32);
			packet.PutByte(0);

			vehicle.Region.Broadcast(packet, vehicle);
		}
	}
}
