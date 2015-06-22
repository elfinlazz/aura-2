// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.World.Entities;
using Aura.Mabi.Network;
using Aura.Shared.Network;
using System;

namespace Aura.Channel.Network.Sending.Helpers
{
	public static class PropHelper
	{
		public static Packet AddPropInfo(this Packet packet, Prop prop)
		{
			packet.PutLong(prop.EntityId);
			packet.PutInt(prop.Info.Id);

			// Client side props (A0 range, instead of A1)
			// look a bit different.
			if (prop.ServerSide)
			{
				packet.PutString(prop.Ident);
				packet.PutString(prop.Title);
				packet.PutBin(prop.Info);
				packet.PutString(prop.State);
				packet.PutLong(0);

				packet.PutByte(prop.HasXml);
				if (prop.HasXml)
					packet.PutString(prop.Xml.ToString());

				packet.PutInt(prop.Extensions.Count);
				foreach (var ext in prop.Extensions)
				{
					packet.PutInt((int)ext.SignalType);
					packet.PutInt((int)ext.EventType);
					packet.PutString(ext.Name);
					packet.PutByte(ext.Mode);
					packet.PutString(ext.Value.ToString());
				}

				packet.PutShort(0);
			}
			else
			{
				packet.PutString(prop.State);
				packet.PutLong(DateTime.Now);

				if (prop.HasXml)
				{
					packet.PutByte(true);
					packet.PutString(prop.Xml.ToString());
				}
				else
				{
					packet.PutByte(false);
				}

				packet.PutFloat(prop.Info.Direction);
			}

			return packet;
		}

		public static Packet AddPropUpdateInfo(this Packet packet, Prop prop)
		{
			packet.PutString(prop.State);
			packet.PutLong(DateTime.Now);

			packet.PutByte(prop.HasXml);
			if (prop.HasXml)
				packet.PutString(prop.Xml.ToString());

			packet.PutFloat(prop.Info.Direction);
			packet.PutShort(0);

			return packet;
		}
	}
}
