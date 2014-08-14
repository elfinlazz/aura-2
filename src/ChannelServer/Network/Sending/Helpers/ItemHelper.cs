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
				packet.PutString(item.MetaData1.ToString());
				packet.PutString(item.MetaData2.ToString());

				// Egos (example)
				//0969 [................] String : Navi
				//0970 [..............02] Byte   : 2
				//0971 [..............0E] Byte   : 14
				//0972 [..............0E] Byte   : 14
				//0973 [........00000F50] Int    : 3920
				//0974 [..............11] Byte   : 17
				//0975 [........000013B6] Int    : 5046
				//0976 [..............05] Byte   : 5
				//0977 [........00000092] Int    : 146
				//0978 [..............04] Byte   : 4
				//0979 [........000000C9] Int    : 201
				//0980 [..............03] Byte   : 3
				//0981 [........0000007E] Int    : 126
				//0982 [..............03] Byte   : 3
				//0983 [........000000B5] Int    : 181
				//0984 [..............00] Byte   : 0
				//0985 [........00000000] Int    : 0
				//0986 [0000000000000000] Long   : 0
				//0987 [000039C6DF641228] Long   : 63526314185256 (26.01.2014 06:23:05)
				//0988 [........00000000] Int    : 0

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
