// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Channel.World.Entities;
using Aura.Shared.Network;
using Aura.Channel.World;
using Aura.Channel.Network.Sending.Helpers;
using Aura.Mabi.Network;

namespace Aura.Channel.Network.Sending
{
	public static partial class Send
	{
		/// <summary>
		/// Broadcasts HittingProp in range of creature.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="propEntityId"></param>
		/// <param name="stunTime"></param>
		public static void HittingProp(Creature creature, long propEntityId, int stunTime)
		{
			var pos = creature.GetPosition();

			var packet = new Packet(Op.HittingProp, creature.EntityId);
			packet.PutLong(propEntityId);
			packet.PutInt(stunTime);
			packet.PutFloat(pos.X);
			packet.PutFloat(pos.Y);

			creature.Region.Broadcast(packet, creature);
		}

		/// <summary>
		/// Sends HitPropR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		public static void HitPropR(Creature creature, bool success)
		{
			var packet = new Packet(Op.HitPropR, creature.EntityId);
			packet.PutByte(success);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends TouchPropR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		public static void TouchPropR(Creature creature)
		{
			var packet = new Packet(Op.TouchPropR, creature.EntityId);
			packet.PutByte(true);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Broadcasts prop update in its region.
		/// </summary>
		/// <param name="prop"></param>
		public static void PropUpdate(Prop prop)
		{
			var packet = new Packet(Op.PropUpdate, prop.EntityId);
			packet.AddPropUpdateInfo(prop);

			prop.Region.Broadcast(packet);
		}

		/// <summary>
		/// Broadcasts new prop extension.
		/// </summary>
		/// <param name="prop"></param>
		/// <param name="ext"></param>
		public static void AddPropExtension(Prop prop, PropExtension ext)
		{
			var packet = new Packet(Op.AddPropExtension, prop.EntityId);
			packet.PutInt((int)ext.SignalType);
			packet.PutInt((int)ext.EventType);
			packet.PutString(ext.Name);
			packet.PutByte(ext.Mode);
			packet.PutString(ext.Value.ToString());

			prop.Region.Broadcast(packet);
		}

		/// <summary>
		/// Broadcasts prop extension remove.
		/// </summary>
		/// <param name="prop"></param>
		/// <param name="ext"></param>
		public static void RemovePropExtension(Prop prop, PropExtension ext)
		{
			var packet = new Packet(Op.RemovePropExtension, prop.EntityId);
			packet.PutString(ext.Name);

			prop.Region.Broadcast(packet);
		}
	}
}
