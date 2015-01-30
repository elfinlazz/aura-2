// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.World.Entities;
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
				packet.PutString(prop.Name);
				packet.PutString(prop.Title);
				packet.PutBin(prop.Info);
				packet.PutString(prop.State);
				packet.PutLong(0);

				if (prop.HasXml)
				{
					packet.PutByte(true);
					packet.PutString(prop.Xml.ToString());
				}
				else
				{
					packet.PutByte(false);
				}

				packet.PutInt(0);
				// if ^ 1 ?
				//   010 [........000000CA] Int    : 202
				//   011 [........0000044C] Int    : 1100
				//   012 [................] String : 
				//   013 [..............02] Byte   : 2
				//   014 [................] String : message:s:Do you wish to enter the room?;condition:s:notin(220189,194241,1354);

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
