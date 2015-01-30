// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Channel.World.Entities;
using Aura.Shared.Network;
using Aura.Channel.World;
using Aura.Shared.Mabi.Const;
using Aura.Shared.Util;
using Aura.Channel.Network.Sending.Helpers;

namespace Aura.Channel.Network.Sending
{
	public static partial class Send
	{
		/// <summary>
		/// Sends MoonGateInfoRequestR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		public static void MoonGateInfoRequestR(Creature creature)
		{
			var packet = new Packet(Op.MoonGateInfoRequestR, creature.EntityId);
			//packet.PutString("_moongate_tara_west");
			//packet.PutString("_moongate_tirchonaill");

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends MailsRequestR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		public static void MailsRequestR(Creature creature)
		{
			var packet = new Packet(Op.MailsRequestR, creature.EntityId);
			// ...

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends SosButtonRequestR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="enabled"></param>
		public static void SosButtonRequestR(Creature creature, bool enabled)
		{
			var packet = new Packet(Op.SosButtonRequestR, creature.EntityId);
			packet.PutByte(enabled);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends HomesteadInfoRequestR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		public static void HomesteadInfoRequestR(Creature creature)
		{
			var packet = new Packet(Op.HomesteadInfoRequestR, creature.EntityId);
			packet.PutByte(0);
			packet.PutByte(0);
			packet.PutByte(1);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends Disappear to creature's client.
		/// </summary>
		/// <remarks>
		/// Should this be broadcasted? What does it even do? TODO.
		/// </remarks>
		/// <param name="creature"></param>
		public static void Disappear(Creature creature)
		{
			var packet = new Packet(Op.Disappear, MabiId.Channel);
			packet.PutLong(creature.EntityId);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends ChannelLoginUnkR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		public static void ChannelLoginUnkR(Creature creature)
		{
			var packet = new Packet(Op.ChannelLoginUnkR, creature.EntityId);
			packet.PutByte(1); // success?
			packet.PutInt(0);
			packet.PutInt(0);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends ContinentWarpCoolDownR to creature's client.
		/// </summary>
		/// <remarks>
		/// On login the first parameter always seems to be a 1 byte.
		/// If it's not, after a continent warp for example, the packet
		/// has two more date parameters, with times 18 minutes apart
		/// from each other.
		/// The first date is the time of the last continent warp reset,
		/// 00:00 or 12:00. The second date is the time of the next reset.
		/// Based on those two times the skill icon cool down is displayed.
		/// </remarks>
		/// <param name="creature"></param>
		public static void ContinentWarpCoolDownR(Creature creature)
		{
			var packet = new Packet(Op.ContinentWarpCoolDownR, creature.EntityId);
			packet.PutByte(1);

			// Alternative structure: (Conti and Nao warps)
			// 001 [..............00]  Byte   : 0
			// 002 [000039BA86EA43C0]  Long   : 000039BA86EA43C0 // 2012-May-22 15:30:00
			// 003 [000039BA86FABE80]  Long   : 000039BA86FABE80 // 2012-May-22 15:48:00
			//packet.PutByte(0);
			//packet.PutLong(DateTime.Now.AddMinutes(1));
			//packet.PutLong(DateTime.Now.AddMinutes(5));

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Broadcasts PlayDead in range of creature.
		/// </summary>
		/// <param name="creature"></param>
		public static void PlayDead(Creature creature)
		{
			var pos = creature.GetPosition();

			var packet = new Packet(Op.PlayDead, creature.EntityId);
			packet.PutByte(true); // ?
			packet.PutFloat(pos.X);
			packet.PutFloat(pos.Y);
			packet.PutInt(5000);

			creature.Region.Broadcast(packet, creature);
		}

		/// <summary>
		/// Broadcasts RemoveDeathScreen in range of creature.
		/// </summary>
		/// <remarks>
		/// Removes black bars and unlocks player.
		/// 
		/// Update: This has to be broadcasted, otherwise other players
		///   are visually stuck in death mode. TODO: Maybe change name.
		/// </remarks>
		/// <param name="creature"></param>
		public static void RemoveDeathScreen(Creature creature)
		{
			var packet = new Packet(Op.RemoveDeathScreen, creature.EntityId);

			creature.Region.Broadcast(packet);
		}

		/// <summary>
		/// Broadcasts RiseFromTheDead in range of creature.
		/// </summary>
		/// <remarks>
		/// Makes creature stand up.
		/// </remarks>
		/// <param name="creature"></param>
		public static void RiseFromTheDead(Creature creature)
		{
			var packet = new Packet(Op.RiseFromTheDead, creature.EntityId);

			creature.Region.Broadcast(packet, creature);
		}

		/// <summary>
		/// Broadcasts DeadFeather in range of creature.
		/// </summary>
		/// <param name="creature"></param>
		public static void DeadFeather(Creature creature)
		{
			var packet = new Packet(Op.DeadFeather, creature.EntityId);
			packet.PutShort(1);
			packet.PutInt(0);
			packet.PutByte(0);

			creature.Region.Broadcast(packet, creature);
		}

		/// <summary>
		/// Sends PlayCutscene to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="cutscene"></param>
		public static void PlayCutscene(Creature creature, Cutscene cutscene)
		{
			var packet = new Packet(Op.PlayCutscene, MabiId.Channel);
			packet.PutLong(creature.EntityId);
			packet.PutLong(cutscene.Leader.EntityId);
			packet.PutString(cutscene.Name);

			packet.PutInt(cutscene.Actors.Count);
			foreach (var actor in cutscene.Actors)
			{
				var subPacket = Packet.Empty();
				subPacket.AddCreatureInfo(actor.Value, CreaturePacketType.Public);
				var bArr = subPacket.Build();

				packet.PutString(actor.Key);
				packet.PutShort((short)bArr.Length);
				packet.PutBin(bArr);
			}

			packet.PutInt(1); // count?
			packet.PutLong(creature.EntityId);

			// TODO: Send to whole party?
			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends CutsceneEnd to cutscene's leader.
		/// </summary>
		/// <param name="cutscene"></param>
		public static void CutsceneEnd(Cutscene cutscene)
		{
			var packet = new Packet(Op.CutsceneEnd, MabiId.Channel);
			packet.PutLong(cutscene.Leader.EntityId);

			// TODO: Send to whole party?
			cutscene.Leader.Client.Send(packet);
		}

		/// <summary>
		/// Sends CutsceneUnk to cutscene's leader.
		/// </summary>
		/// <remarks>
		/// Doesn't seem to be required, but it's usually sent after unlocking
		/// the character after watching the cutscene.
		/// </remarks>
		/// <param name="cutscene"></param>
		public static void CutsceneUnk(Cutscene cutscene)
		{
			var packet = new Packet(Op.CutsceneUnk, MabiId.Channel);
			packet.PutLong(cutscene.Leader.EntityId);

			cutscene.Leader.Client.Send(packet);
		}

		/// <summary>
		/// Sends UseGestureR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		public static void UseGestureR(Creature creature, bool success)
		{
			var packet = new Packet(Op.UseGestureR, creature.EntityId);
			packet.PutByte(success);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Broadcasts UseMotion and CancelMotion (cancel is true) around creature.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="category"></param>
		/// <param name="type"></param>
		/// <param name="loop"></param>
		/// <param name="cancel"></param>
		public static void UseMotion(Creature creature, int category, int type, bool loop = false, bool cancel = false)
		{
			if (cancel)
			{
				// Cancel motion
				var cancelPacket = new Packet(Op.CancelMotion, creature.EntityId);
				cancelPacket.PutByte(0);

				creature.Region.Broadcast(cancelPacket, creature);
			}

			// Do motion
			var packet = new Packet(Op.UseMotion, creature.EntityId);
			packet.PutInt(category);
			packet.PutInt(type);
			packet.PutByte(loop);
			packet.PutShort(0);

			creature.Region.Broadcast(packet, creature);
		}

		/// <summary>
		/// Sends SetBgm to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="file"></param>
		/// <param name="type"></param>
		public static void SetBgm(Creature creature, string file, BgmRepeat type)
		{
			var packet = new Packet(Op.SetBgm, creature.EntityId);
			packet.PutString(file);
			packet.PutInt((int)type);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends UnsetBgm to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="file"></param>
		public static void UnsetBgm(Creature creature, string file)
		{
			var packet = new Packet(Op.UnsetBgm, creature.EntityId);
			packet.PutString(file);

			creature.Client.Send(packet);
		}
	}

	/// <summary>
	/// Repeat modes for SetBgm
	/// </summary>
	public enum BgmRepeat : int { Indefinitely = 0, Once = 1 }
}
