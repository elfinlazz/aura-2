// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.World.Entities;
using Aura.Shared.Network;

namespace Aura.Channel.Network.Sending.Helpers
{
	public static class ItemHelper
	{
		public static Packet AddItemInfo(this Packet packet, Item item, ItemPacketType type)
		{
			packet.PutLong(item.EntityId);
			packet.PutByte((byte)type);
			packet.PutBin(item.Info);

			if (type == ItemPacketType.Public)
			{
				packet.PutByte(1);
				packet.PutByte(0);

				//packet.PutByte(0); // Bitmask
				// if & 1
				//     float
				packet.PutByte(1);
				packet.PutFloat(1); // Size multiplicator *hint: Server side giant key mod*

				packet.PutByte(item.FirstTimeAppear); // 0: No bouncing, 1: Bouncing, 2: Delayed bouncing
			}
			else if (type == ItemPacketType.Private)
			{
				packet.PutBin(item.OptionInfo);

				// Ego data
				if (item.Data.HasTag("/ego_weapon/"))
				{
					packet.PutString(item.EgoInfo.Name);
					packet.PutByte((byte)item.EgoInfo.Race);
					packet.PutByte(0); // ? increased from 14 to 18 when I fed a bottle to a sword

					packet.PutByte(item.EgoInfo.SocialLevel);
					packet.PutInt(item.EgoInfo.SocialExp);
					packet.PutByte(item.EgoInfo.StrLevel);
					packet.PutInt(item.EgoInfo.StrExp);
					packet.PutByte(item.EgoInfo.IntLevel);
					packet.PutInt(item.EgoInfo.IntExp);
					packet.PutByte(item.EgoInfo.DexLevel);
					packet.PutInt(item.EgoInfo.DexExp);
					packet.PutByte(item.EgoInfo.WillLevel);
					packet.PutInt(item.EgoInfo.WillExp);
					packet.PutByte(item.EgoInfo.LuckLevel);
					packet.PutInt(item.EgoInfo.LuckExp);
					packet.PutByte(item.EgoInfo.AwakeningEnergy);
					packet.PutInt(item.EgoInfo.AwakeningExp);

					packet.PutLong(0);
					packet.PutLong(item.EgoInfo.LastFeeding); // Last feeding time?
					packet.PutInt(0);
				}

				packet.PutString(item.MetaData1.ToString());
				packet.PutString(item.MetaData2.ToString());

				// Upgrades?
				packet.PutByte(0); // Count
				// for upgrades
				//     Bin    : 01 00 00 00 68 21 11 00 00 00 00 00 05 00 1E 00 00 00 00 00 0A 00 00 00 D3 E4 90 65 0A 00 00 00 F0 18 9E 65

				// Special upgrades? (example)
				//0608 [0000000000000000] Long   : 0
				//0609 [........00000002] Int    : 2
				//0610 [........00000024] Int    : 36
				//0611 [........00000008] Int    : 8
				//0612 [........00000026] Int    : 38
				//0613 [........00000004] Int    : 4

				packet.PutLong(item.QuestId);

				// [190100, NA200 (2015-01-15)] ?
				{
					packet.PutByte(item.IsNew);
					packet.PutByte(0);
				}
			}

			return packet;
		}
	}

	public enum ItemPacketType : byte
	{
		/// <summary>
		/// Used in public appears, doesn't include option info.
		/// </summary>
		Public = 1,

		/// <summary>
		/// Used in private item packets, includes option info.
		/// </summary>
		Private = 2
	}
}
