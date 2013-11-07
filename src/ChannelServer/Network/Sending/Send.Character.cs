// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Shared.Mabi;
using Aura.Shared.Network;
using System;
using Aura.Channel.World.Entities;
using Aura.Shared.Mabi.Const;

namespace Aura.Channel.Network.Sending
{
	public static partial class Send
	{
		/// <summary>
		/// Sends CharacterLock to client.
		/// </summary>
		public static void CharacterLock(ChannelClient client, PlayerCreature creature, LockType type)
		{
			var packet = new Packet(Op.CharacterLock, creature.EntityId);
			packet.PutUInt((uint)type);
			packet.PutInt(0);

			client.Send(packet);
		}

		/// <summary>
		/// Sends CharacterUnlock to client.
		/// </summary>
		public static void CharacterUnlock(ChannelClient client, PlayerCreature creature, LockType type)
		{
			var packet = new Packet(Op.CharacterUnlock, creature.EntityId);
			packet.PutUInt((uint)type);

			client.Send(packet);
		}

		/// <summary>
		/// Sends EnterRegion to client.
		/// </summary>
		/// <remarks>
		/// ...
		/// </remarks>
		public static void EnterRegion(ChannelClient client, PlayerCreature creature)
		{
			var pos = creature.GetPosition();

			var packet = new Packet(Op.EnterRegion, MabiId.Channel);
			packet.PutLong(creature.EntityId);
			packet.PutByte(true); // success?
			packet.PutInt(creature.Region);
			packet.PutInt(pos.X);
			packet.PutInt(pos.Y);

			client.Send(packet);
		}

		/// <summary>
		/// Sends EnterRegionRequestR for creature to client.
		/// </summary>
		/// <remarks>
		/// Negative response doesn't actually do anything, stucks.
		/// </remarks>
		/// <param name="client"></param>
		/// <param name="creature">Negative response if null</param>
		public static void EnterRegionRequestR(ChannelClient client, PlayerCreature creature)
		{
			var packet = new Packet(Op.EnterRegionRequestR, MabiId.Channel);
			packet.PutByte(creature != null);

			if (creature != null)
			{
				packet.PutLong(creature.EntityId);
				packet.PutLong(DateTime.Now);
			}

			client.Send(packet);
		}

		/// <summary>
		/// Sends negative ChannelCharacterInfoRequestR to client.
		/// </summary>
		/// <param name="client"></param>
		public static void ChannelCharacterInfoRequestR_Fail(ChannelClient client)
		{
			ChannelCharacterInfoRequestR(client, null);
		}

		/// <summary>
		/// Sends ChannelCharacterInfoRequestR for creature to client.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="creature">Negative response if null</param>
		public static void ChannelCharacterInfoRequestR(ChannelClient client, PlayerCreature creature)
		{
			var packet = new Packet(Op.ChannelCharacterInfoRequestR, MabiId.Channel);
			packet.PutByte(creature != null);

			if (creature != null)
			{
				packet.AddCreatureInfo(creature, CreaturePacketType.Private);
			}

			client.Send(packet);
		}

		/// <summary>
		/// Sends WarpRegion for creature to client.
		/// </summary>
		/// <remarks>
		/// Makes client load the region and move the creature there.
		/// Uses current position of creature, move beforehand.
		/// </remarks>
		public static void WarpRegion(ChannelClient client, PlayerCreature creature)
		{
			var pos = creature.GetPosition();

			var packet = new Packet(Op.WarpRegion, creature.EntityId);
			packet.PutByte(true);
			packet.PutInt(creature.Region);
			packet.PutInt(pos.X);
			packet.PutInt(pos.Y);

			client.Send(packet);
		}
	}

	public enum LockType : uint
	{
		/// <summary>
		/// 0xEFFFFFFE
		/// </summary>
		Unk1 = 0xEFFFFFFE,

		All = 0xFFFFFFFF,
	}
}
