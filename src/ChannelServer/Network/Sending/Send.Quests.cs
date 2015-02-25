// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Channel.World.Entities;
using Aura.Shared.Network;
using Aura.Channel.World.Quests;
using System.Collections;
using Aura.Channel.Network.Sending.Helpers;
using Aura.Shared.Mabi.Const;

namespace Aura.Channel.Network.Sending
{
	public static partial class Send
	{
		/// <summary>
		///  Sends NewQuest to creature's client.
		/// </summary>
		/// <param name="character"></param>
		/// <param name="quest"></param>
		public static void NewQuest(Creature character, Quest quest)
		{
			var packet = new Packet(Op.NewQuest, character.EntityId);
			packet.AddQuest(quest);

			character.Client.Send(packet);
		}

		/// <summary>
		/// Sends QuestUpdate to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="quest"></param>
		public static void QuestUpdate(Creature creature, Quest quest)
		{
			var progress = quest.GetList();

			var packet = new Packet(Op.QuestUpdate, creature.EntityId);
			packet.PutLong(quest.UniqueId);
			packet.PutByte(1);
			packet.PutInt(progress.Count);
			foreach (var p in progress)
			{
				packet.PutInt(p.Count);
				// [180600, NA187 (25.06.2014)] ?
				{
					packet.PutFloat(0);
				}
				packet.PutByte(p.Done);
				packet.PutByte(p.Unlocked);
			}
			packet.PutByte(0);
			packet.PutByte(0);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Broadcasts QuestOwlNew in range of creature.
		/// </summary>
		/// <remarks>
		/// Effect of an owl delivering the new quest.
		/// </remarks>
		/// <param name="creature"></param>
		/// <param name="questId"></param>
		public static void QuestOwlNew(Creature creature, long questId)
		{
			var packet = new Packet(Op.QuestOwlNew, creature.EntityId);
			packet.PutLong(questId);

			// Creature don't have a region in Soul Stream.
			if (creature.Region != null)
				creature.Region.Broadcast(packet, creature);
		}

		/// <summary>
		/// Broadcasts QuestOwlComplete in range of creature.
		/// </summary>
		/// <remarks>
		/// Effect of an owl delivering the rewards.
		/// </remarks>
		/// <param name="creature"></param>
		/// <param name="questId"></param>
		public static void QuestOwlComplete(Creature creature, long questId)
		{
			var packet = new Packet(Op.QuestOwlComplete, creature.EntityId);
			packet.PutLong(questId);

			creature.Region.Broadcast(packet, creature);
		}

		/// <summary>
		/// Sends QuestClear to creature's client.
		/// </summary>
		/// <remarks>
		/// Removes quest from quest log.
		/// </remarks>
		/// <param name="creature"></param>
		/// <param name="questId"></param>
		public static void QuestClear(Creature creature, long questId)
		{
			var packet = new Packet(Op.QuestClear, creature.EntityId);
			packet.PutLong(questId);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends CompleteQuestR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		public static void CompleteQuestR(Creature creature, bool success)
		{
			var packet = new Packet(Op.CompleteQuestR, creature.EntityId);
			packet.PutByte(success);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends CompleteQuestR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		public static void GiveUpQuestR(Creature creature, bool success)
		{
			var packet = new Packet(Op.GiveUpQuestR, creature.EntityId);
			packet.PutByte(success);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends QuestStartPtj to creature's client, which starts the clock.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="uniqueQuestId"></param>
		public static void QuestStartPtj(Creature creature, long uniqueQuestId)
		{
			var packet = new Packet(Op.QuestStartPtj, creature.EntityId);
			packet.PutLong(uniqueQuestId);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends QuestEndPtj to creature's client, which stops the clock.
		/// </summary>
		/// <param name="creature"></param>
		public static void QuestEndPtj(Creature creature)
		{
			var packet = new Packet(Op.QuestEndPtj, creature.EntityId);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends QuestUpdatePtj to creature's client, updates the char info window.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="type"></param>
		/// <param name="done"></param>
		/// <param name="success"></param>
		public static void QuestUpdatePtj(Creature creature, PtjType type, int done, int success)
		{
			var packet = new Packet(Op.QuestUpdatePtj, creature.EntityId);
			packet.PutInt((int)type);
			packet.PutInt(done);
			packet.PutInt(success);

			creature.Client.Send(packet);
		}
	}
}
