// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.World.Entities.Creatures;
using Aura.Shared.Network;

namespace Aura.Channel.Network.Sending.Helpers
{
	public static class ConditionHelper
	{
		public static Packet AddConditions(this Packet packet, CreatureConditions conditions)
		{
			packet.PutULong((ulong)conditions.A);
			packet.PutULong((ulong)conditions.B);
			packet.PutULong((ulong)conditions.C);
			// [150100] New conditions list
			{
				packet.PutULong((ulong)conditions.D);
			}
			// [180300, NA169 (23.10.2013)] New conditions list?
			{
				packet.PutULong((ulong)conditions.E);
			}
			// [190100, NA201 (14.02.2015)] New conditions list?
			{
				packet.PutULong(0);
			}

			// List of additional values for the conditions
			var extra = conditions.GetExtraList();
			packet.PutInt(extra.Count);
			foreach (var e in extra)
			{
				packet.PutInt(e.Key);
				packet.PutString(e.Value.ToString());
			}

			// [180100] ? (old: Zero Talent?)
			{
				// This might not be part of the conditions, but we need that
				// long in both cases (5209 and update).
				packet.PutLong(0);
			}

			return packet;
		}
	}
}
