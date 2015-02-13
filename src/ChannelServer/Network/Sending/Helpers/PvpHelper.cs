// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.World.Entities;
using Aura.Shared.Network;

namespace Aura.Channel.Network.Sending.Helpers
{
	public static class PvpHelper
	{
		public static Packet AddPvPInfo(this Packet packet, Creature creature)
		{
			packet.PutByte(0);
			packet.PutInt(0);
			packet.PutByte(0);
			packet.PutInt(0);
			packet.PutByte(0);
			packet.PutByte(0);
			packet.PutByte(0); // 1 enables PvP in fields
			packet.PutLong(0);
			packet.PutLong(0);
			packet.PutInt(0);
			packet.PutByte(1);
			// --v

			//var arena = creature.ArenaPvPManager != null;

			//packet.PutByte(arena); // ArenaPvP
			//packet.PutInt(arena ? creature.ArenaPvPManager.GetTeam(creature) : (uint)0);
			//packet.PutByte(creature.TransPvPEnabled);
			//packet.PutInt(arena ? creature.ArenaPvPManager.GetStars(creature) : 0);
			//packet.PutByte(creature.EvGEnabled);
			//packet.PutByte(creature.EvGSupportRace);
			//packet.PutByte(1); // IsPvPMode
			//packet.PutLong(creature.PvPWins);
			//packet.PutLong(creature.PvPLosses);
			//packet.PutInt(0);// PenaltyPoints
			//packet.PutByte(1);  // unk

			// [170300] ?
			{
				packet.PutByte(0);
				packet.PutInt(0);
				packet.PutInt(0);
				packet.PutInt(0);
				packet.PutInt(0);
			}

			return packet;
		}
	}
}
