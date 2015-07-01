// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using Aura.Mabi.Const;
using Aura.Shared.Network;
using Aura.Shared.Network.Sending.Helpers;
using Aura.Channel.World.Entities;
using Aura.Channel.Network.Sending.Helpers;
using Aura.Mabi.Network;

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
		public static void ChannelLoginR(ChannelClient client, long creatureId)
		{
			var packet = new Packet(Op.ChannelLoginR, MabiId.Channel);
			packet.PutByte(true);
			packet.PutLong(creatureId);
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

		/// <summary>
		/// Sends SpecialLogin to creature's client.
		/// </summary>
		/// <remarks>
		/// One of those packets with a success parameter,
		/// that doesn't actually support failing.
		/// Sends character to a special, client-side-instanced region,
		/// where he is to meet the given NPC. EnterRegion isn't needed
		/// for this.
		/// </remarks>
		/// <param name="creature"></param>
		/// <param name="regionId"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="npcEntityId"></param>
		public static void SpecialLogin(Creature creature, int regionId, int x, int y, long npcEntityId)
		{
			var packet = new Packet(Op.SpecialLogin, MabiId.Channel);
			packet.PutByte(true);
			packet.PutInt(regionId);
			packet.PutInt(x);
			packet.PutInt(y);
			packet.PutLong(npcEntityId);
			packet.AddCreatureInfo(creature, CreaturePacketType.Private);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends LeaveSoulStreamR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		public static void LeaveSoulStreamR(Creature creature)
		{
			var packet = new Packet(Op.LeaveSoulStreamR, MabiId.Channel);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends SwitchChannelR to creature's client
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="channel">Negative answer if null.</param>
		public static void SwitchChannelR(Creature creature, ChannelInfo channel)
		{
			var packet = new Packet(Op.SwitchChannelR, MabiId.Channel);
			packet.PutByte(channel != null);

			if (channel != null)
			{
				packet.PutString(channel.ServerName);
				packet.PutString(channel.Name);
				packet.PutShort(1); // Channel id
				packet.PutString(channel.Host);
				packet.PutString(channel.Host);
				packet.PutUShort((ushort)channel.Port);
				packet.PutUShort((ushort)(channel.Port + 4));
			}

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends GetChannelList to client.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="server"></param>
		public static void GetChannelListR(ChannelClient client, ServerInfo server)
		{
			var packet = new Packet(Op.GetChannelListR, MabiId.Channel);
			packet.AddServerInfo(server, ServerInfoType.Client);

			client.Send(packet);
		}
	}
}
