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
using Aura.Shared.Mabi.Const;
using Aura.Channel.World.Inventory;

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
		public static void OpenNpcShop(Creature creature, IList<NpcShopTab> tabs)
		{
			var packet = new Packet(Op.OpenNpcShop, creature.EntityId);
			packet.PutString("shopname");
			packet.PutByte(0);
			packet.PutByte(0);
			packet.PutInt(0);
			packet.PutByte((byte)tabs.Count);
			foreach (var tab in tabs)
			{
				packet.PutString("[{0}]{1}", tab.Order, tab.Title);

				// [160200] ?
				{
					packet.PutByte(0);
				}

				var items = tab.GetItems();
				packet.PutShort((short)items.Count);
				foreach (var item in items)
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

		/// <summary>
		/// Sends OpenBank to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="bank"></param>
		/// <param name="race"></param>
		public static void OpenBank(Creature creature, BankInventory bank, BankTabRace race)
		{
			var packet = new Packet(Op.OpenBank, creature.EntityId);

			packet.PutByte(1);
			packet.PutByte((byte)race);
			packet.PutLong(DateTime.Now);
			packet.PutByte(0);
			packet.PutString(creature.Client.Account.Id);
			packet.PutString("Global"); // Current bank id
			packet.PutString("Bank"); // Current bank title
			packet.PutInt(bank.Gold);

			var tabList = bank.GetTabList(race);
			packet.PutInt(tabList.Count);
			foreach (var tab in tabList)
			{
				packet.PutString(tab.Name);
				packet.PutByte((byte)tab.Race);
				packet.PutInt(tab.Width);
				packet.PutInt(tab.Height);

				var itemList = tab.GetItemList();
				packet.PutInt(itemList.Count);
				foreach (var item in itemList)
				{
					packet.PutString("Global"); // Bank id
					packet.PutULong(18446744017659355058);
					packet.PutULong(0);
					packet.AddItemInfo(item, ItemPacketType.Private);
				}
			}

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends CloseBankR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		public static void CloseBankR(Creature creature)
		{
			var packet = new Packet(Op.CloseBankR, creature.EntityId);

			packet.PutByte(true);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends BankDepositGoldR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		public static void BankDepositGoldR(Creature creature, bool success)
		{
			var packet = new Packet(Op.BankDepositGoldR, creature.EntityId);

			packet.PutByte(success);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends BankWithdrawGoldR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		public static void BankWithdrawGoldR(Creature creature, bool success)
		{
			var packet = new Packet(Op.BankWithdrawGoldR, creature.EntityId);

			packet.PutByte(success);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends BankUpdateGold to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="amount"></param>
		public static void BankUpdateGold(Creature creature, int amount)
		{
			var packet = new Packet(Op.BankUpdateGold, creature.EntityId);

			packet.PutInt(amount);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends BankDepositItemR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		public static void BankDepositItemR(Creature creature, bool success)
		{
			var packet = new Packet(Op.BankDepositItemR, creature.EntityId);

			packet.PutByte(success);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends BankWithdrawItemR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		public static void BankWithdrawItemR(Creature creature, bool success)
		{
			var packet = new Packet(Op.BankWithdrawItemR, creature.EntityId);

			packet.PutByte(success);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends BankAddItem to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="item"></param>
		/// <param name="bankId"></param>
		/// <param name="tabName"></param>
		public static void BankAddItem(Creature creature, Item item, string bankId, string tabName)
		{
			var packet = new Packet(Op.BankAddItem, creature.EntityId);

			packet.PutString(tabName);
			packet.PutString(bankId);
			packet.PutLong(0);
			packet.PutLong(0);
			packet.AddItemInfo(item, ItemPacketType.Private);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends BankRemoveItem to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="tabName"></param>
		/// <param name="itemEntityId"></param>
		public static void BankRemoveItem(Creature creature, string tabName, long itemEntityId)
		{
			var packet = new Packet(Op.BankRemoveItem, creature.EntityId);

			packet.PutString(tabName);
			packet.PutLong(itemEntityId);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends NpcTalkEgoR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		/// <param name="npcEntityId"></param>
		/// <param name="npcName"></param>
		/// <param name="description"></param>
		public static void NpcTalkEgoR(Creature creature, bool success, long npcEntityId, string npcName, string description)
		{
			var packet = new Packet(Op.NpcTalkEgoR, creature.EntityId);

			packet.PutByte(success);
			if (success)
			{
				packet.PutLong(npcEntityId);
				packet.PutString(npcName);
				packet.PutString(description);
			}

			creature.Client.Send(packet);
		}
	}
}
