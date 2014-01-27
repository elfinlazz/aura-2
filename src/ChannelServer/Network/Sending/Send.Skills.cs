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
		/// <remarks>
		/// I don't remember the exact purpose of the optional skill id,
		/// possibly needed for something that we don't have atm.
		/// </remarks>
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

		/// <summary>
		/// Sends negative SkillStart to creature's client.
		/// </summary>
		/// <remarks>
		/// This isn't actually a negative response,
		/// it's just for unstucking the player after a fail.
		/// </remarks>
		/// <param name="creature"></param>
		/// <param name="skillId"></param>
		public static void SkillStart_Fail(Creature creature, SkillId skillId)
		{
			var packet = new Packet(Op.SkillStart, creature.EntityId);
			packet.PutUShort((ushort)skillId);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends negative SkillStop to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skillId"></param>
		public static void SkillStop_Fail(Creature creature, SkillId skillId)
		{
			var packet = new Packet(Op.SkillStop, creature.EntityId);
			packet.PutUShort((ushort)skillId);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends SkillStart to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skillId"></param>
		/// <param name="extra"></param>
		public static void SkillStart(Creature creature, SkillId skillId, string extra)
		{
			var packet = new Packet(Op.SkillStart, creature.EntityId);
			packet.PutUShort((ushort)skillId);
			packet.PutString(extra);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends SkillStart to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skillId"></param>
		/// <param name="unkByte"></param>
		public static void SkillStart(Creature creature, SkillId skillId, byte unkByte)
		{
			var packet = new Packet(Op.SkillStart, creature.EntityId);
			packet.PutUShort((ushort)skillId);
			packet.PutByte(unkByte);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends SkillStop to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skillId"></param>
		/// <param name="extra"></param>
		public static void SkillStop(Creature creature, SkillId skillId, string extra)
		{
			var packet = new Packet(Op.SkillStop, creature.EntityId);
			packet.PutUShort((ushort)skillId);
			packet.PutString(extra);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends SkillStop to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skillId"></param>
		/// <param name="unkByte"></param>
		public static void SkillStop(Creature creature, SkillId skillId, byte unkByte)
		{
			var packet = new Packet(Op.SkillStop, creature.EntityId);
			packet.PutUShort((ushort)skillId);
			packet.PutByte(unkByte);

			creature.Client.Send(packet);
		}

		public static void SkillTrainingUp(Creature creature, Skill skill, float exp)
		{
			var packet = new Packet(Op.SkillTrainingUp, creature.EntityId);
			packet.PutBin(skill.Info);
			packet.PutFloat(exp);
			packet.PutByte(1);
			packet.PutString("" /* (Specialized Skill Bonus: x2) */);

			creature.Client.Send(packet);
		}
	}
}
