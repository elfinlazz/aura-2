// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Channel.World.Entities;
using Aura.Shared.Network;
using Aura.Channel.World;
using Aura.Channel.Skills;
using Aura.Shared.Mabi.Const;

namespace Aura.Channel.Network.Sending
{
	public static partial class Send
	{
		/// <summary>
		/// Sends SkillInfo to creature's client.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		public static void SkillInfo(Creature creature, Skill skill)
		{
			var packet = new Packet(Op.SkillInfo, creature.EntityId);
			packet.PutBin(skill.Info);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends SkillRankUp to creature's client.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		public static void SkillRankUp(Creature creature, Skill skill)
		{
			var packet = new Packet(Op.SkillRankUp, creature.EntityId);
			packet.PutByte(1);
			packet.PutBin(skill.Info);
			packet.PutFloat(0);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Broadcasts RankUp in range of creature.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skillId">Excluded if 0</param>
		public static void RankUp(Creature creature, SkillId skillId = 0)
		{
			var packet = new Packet(Op.RankUp, creature.EntityId);
			if (skillId > 0)
				packet.PutUShort((ushort)skillId);
			packet.PutShort(1); // Rank?

			creature.Region.Broadcast(packet, creature);
		}
	}
}
