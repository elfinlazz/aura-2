// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Channel.Scripting.Scripts;
using Aura.Channel.World.Entities;
using Aura.Shared.Network;
using Aura.Channel.Network.Sending.Helpers;

namespace Aura.Channel.Network.Sending
{
	public static partial class Send
	{
		/// <summary>
		/// Sends NpcTalk to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="xml"></param>
		public static void NpcTalk(Creature creature, string xml)
		{
			var packet = new Packet(Op.NpcTalk, creature.EntityId);
			packet.PutString(xml);
			packet.PutBin();

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends negative NpcTalkStartR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		public static void NpcTalkStartR_Fail(Creature creature)
		{
			NpcTalkStartR(creature, 0);
		}

		/// <summary>
		/// Sends NpcTalkStartR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="npcId">Negative response if 0.</param>
		public static void NpcTalkStartR(Creature creature, long npcId)
		{
			var packet = new Packet(Op.NpcTalkStartR, creature.EntityId);
			packet.PutByte(npcId != 0);
			if (npcId != 0)
				packet.PutLong(npcId);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends NpcTalkEndR to creature's client.
		/// </summary>
		/// <remarks>
		/// If no message is specified "<end/>" is sent,
		/// to close the dialog box immediately.
		/// </remarks>
		/// <param name="creature"></param>
		/// <param name="npcId"></param>
		/// <param name="message">Last message before closing.</param>
		public static void NpcTalkEndR(Creature creature, long npcId, string message = null)
		{
			var p = new Packet(Op.NpcTalkEndR, creature.EntityId);
			p.PutByte(true);
			p.PutLong(npcId);
			p.PutString(message ?? "<end/>");

			creature.Client.Send(p);
		}

		/// <summary>
		/// Sends negative NpcTalkKeywordR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="keyword"></param>
		public static void NpcTalkKeywordR_Fail(Creature creature)
		{
			NpcTalkKeywordR(creature, null);
		}

		/// <summary>
		/// Sends NpcTalkKeywordR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="keyword">Negative response if null</param>
		public static void NpcTalkKeywordR(Creature creature, string keyword)
		{
			var packet = new Packet(Op.NpcTalkKeywordR, creature.EntityId);
			packet.PutByte(keyword != null);
			if (keyword != null)
				packet.PutString(keyword);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends OpenNpcShop to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="shop"></param>
		public static void OpenNpcShop(Creature creature, NpcShopScript shop)
		{
			var packet = new Packet(Op.OpenNpcShop, creature.EntityId);
			packet.PutString("shopname");
			packet.PutByte(0);
			packet.PutByte(0);
			packet.PutInt(0);
			packet.PutByte((byte)shop.Tabs.Count);
			foreach (var tab in shop.Tabs)
			{
				packet.PutString("[{0}]{1}", tab.Order, tab.Title);

				// [160200] ?
				{
					packet.PutByte(0);
				}

				packet.PutShort((short)tab.Items.Count);
				foreach (var item in tab.Items)
					packet.AddItemInfo(item, ItemPacketType.Private);
			}

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends ShopBuyItemR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		public static void NpcShopBuyItemR(Creature creature, bool success)
		{
			var packet = new Packet(Op.NpcShopBuyItemR, creature.EntityId);
			if (!success)
				packet.PutByte(success);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends ShopSellItemR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		public static void NpcShopSellItemR(Creature creature)
		{
			var packet = new Packet(Op.NpcShopSellItemR, creature.EntityId);

			creature.Client.Send(packet);
		}
	}
}
