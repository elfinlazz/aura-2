// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.World.Entities.Creatures;
using Aura.Shared.Mabi.Const;
using Aura.Shared.Network;

namespace Aura.Channel.Network.Sending.Helpers
{
	public static class RegenHelper
	{
		public static Packet AddRegen(this Packet packet, StatRegen regen)
		{
			packet.PutInt(regen.Id);

			// It makes more sense for us to *increase* the hunger, but the
			// client wants to *decrease* the amount of available Stamina.
			packet.PutFloat(regen.Stat != Stat.Hunger ? regen.Change : -regen.Change);

			packet.PutInt(regen.TimeLeft);
			packet.PutInt((int)regen.Stat);
			packet.PutByte(0); // ?
			packet.PutFloat(regen.Max);

			return packet;
		}
	}
}
