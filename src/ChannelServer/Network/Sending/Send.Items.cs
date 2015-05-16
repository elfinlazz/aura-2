// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Channel.World.Entities;
using Aura.Shared.Network;
using Aura.Channel.World;
using Aura.Mabi.Const;
using Aura.Channel.Network.Sending.Helpers;
using Aura.Mabi.Network;

namespace Aura.Channel.Network.Sending
{
	public static partial class Send
	{
		/// <summary>
		/// Sends ItemNew to creature's client.
		/// </summary>
		/// 
		/// <param name="creature"></param>
		/// <param name="item"></param>
		public static void ItemNew(Creature creature, Item item)
		{
			var packet = new Packet(Op.ItemNew, creature.EntityId);
			packet.AddItemInfo(item, ItemPacketType.Private);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends ItemUpdate to creature's client.
		/// </summary>
		/// 
		/// <param name="creature"></param>
		/// <param name="item"></param>
		public static void ItemUpdate(Creature creature, Item item)
		{
			var packet = new Packet(Op.ItemUpdate, creature.EntityId);
			packet.AddItemInfo(item, ItemPacketType.Private);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends ItemMoveR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		public static void ItemMoveR(Creature creature, bool success)
		{
			var packet = new Packet(Op.ItemMoveR, creature.EntityId);
			packet.PutByte(success);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends ItemMoveInfo or ItemSwitchInfo to creature's client,
		/// depending on whether collidingItem is null.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="item"></param>
		/// <param name="source"></param>
		/// <param name="target"></param>
		/// <param name="collidingItem"></param>
		public static void ItemMoveInfo(Creature creature, Item item, Pocket source, Item collidingItem)
		{
			var packet = new Packet((collidingItem == null ? Op.ItemMoveInfo : Op.ItemSwitchInfo), creature.EntityId);
			packet.PutLong(item.EntityId);
			packet.PutByte((byte)source);
			packet.PutByte((byte)item.Info.Pocket);
			packet.PutByte(2);
			packet.PutByte((byte)item.Info.X);
			packet.PutByte((byte)item.Info.Y);
			if (collidingItem != null)
			{
				packet.PutLong(collidingItem.EntityId);
				packet.PutByte((byte)item.Info.Pocket);
				packet.PutByte((byte)collidingItem.Info.Pocket);
				packet.PutByte(2);
				packet.PutByte((byte)collidingItem.Info.X);
				packet.PutByte((byte)collidingItem.Info.Y);
			}

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends ItemAmount to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="item"></param>
		public static void ItemAmount(Creature creature, Item item)
		{
			var packet = new Packet(Op.ItemAmount, creature.EntityId);
			packet.PutLong(item.EntityId);
			packet.PutUShort(item.Info.Amount);
			packet.PutByte(2); // ? (related to the 2 in move item?)

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends ItemRemove to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="item"></param>
		public static void ItemRemove(Creature creature, Item item)
		{
			ItemRemove(creature, item, item.Info.Pocket);
		}

		/// <summary>
		/// Sends ItemRemove to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="item"></param>
		/// <param name="pocket"></param>
		public static void ItemRemove(Creature creature, Item item, Pocket pocket)
		{
			var packet = new Packet(Op.ItemRemove, creature.EntityId);
			packet.PutLong(item.EntityId);
			packet.PutByte((byte)pocket);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends ItemPickUpR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		public static void ItemPickUpR(Creature creature, bool success)
		{
			var packet = new Packet(Op.ItemPickUpR, creature.EntityId);
			packet.PutByte(success ? (byte)1 : (byte)2);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends ItemSplitR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		public static void ItemSplitR(Creature creature, bool success)
		{
			var packet = new Packet(Op.ItemSplitR, creature.EntityId);
			packet.PutByte(success);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends ItemDropR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		public static void ItemDropR(Creature creature, bool success)
		{
			var packet = new Packet(Op.ItemDropR, creature.EntityId);
			packet.PutByte(success);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends ItemDestroyR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		public static void ItemDestroyR(Creature creature, bool success)
		{
			var packet = new Packet(Op.ItemDestroyR, creature.EntityId);
			packet.PutByte(success);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Broadcasts EquipmentMoved in creature's range.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="from"></param>
		public static void EquipmentMoved(Creature creature, Pocket from)
		{
			var packet = new Packet(Op.EquipmentMoved, creature.EntityId);
			packet.PutByte((byte)from);
			packet.PutByte(1);

			creature.Region.Broadcast(packet, creature);
		}

		/// <summary>
		/// Broadcasts EquipmentChanged in creature's range.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="item"></param>
		public static void EquipmentChanged(Creature creature, Item item)
		{
			var packet = new Packet(Op.EquipmentChanged, creature.EntityId);
			packet.PutBin(item.Info);
			packet.PutByte(1);

			creature.Region.Broadcast(packet, creature);
		}

		/// <summary>
		/// Sends ItemStateChangeR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		public static void ItemStateChangeR(Creature creature)
		{
			var packet = new Packet(Op.ItemStateChangeR, creature.EntityId);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Broadcasts UpdateWeaponSet in creature's range.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		public static void UpdateWeaponSet(Creature creature)
		{
			var packet = new Packet(Op.UpdateWeaponSet, creature.EntityId);
			packet.PutByte((byte)creature.Inventory.WeaponSet);

			creature.Region.Broadcast(packet, creature);
		}

		/// <summary>
		/// Sends SwitchSetR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		public static void SwitchSetR(Creature creature, bool success)
		{
			var packet = new Packet(Op.SwitchSetR, creature.EntityId);
			packet.PutByte(success);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends UseItemR to creature's client.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		/// <param name="itemId">Doesn't matter if success is false.</param>
		public static void UseItemR(Creature creature, bool success, int itemId)
		{
			var packet = new Packet(Op.UseItemR, creature.EntityId);
			packet.PutByte(success);
			if (success)
				packet.PutInt(itemId);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends ItemDurabilityUpdate to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="item"></param>
		public static void ItemDurabilityUpdate(Creature creature, Item item)
		{
			var packet = new Packet(Op.ItemDurabilityUpdate, creature.EntityId);
			packet.PutLong(item.EntityId);
			packet.PutInt(item.OptionInfo.Durability);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends ItemDurabilityUpdate to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="item"></param>
		public static void ItemDurabilityUpdate(Creature creature, IList<Item> items)
		{
			var packet = new Packet(Op.ItemDurabilityUpdate, creature.EntityId);
			foreach (var item in items)
			{
				packet.PutLong(item.EntityId);
				packet.PutInt(item.OptionInfo.Durability);
			}

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends ItemMaxDurabilityUpdate to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="item"></param>
		public static void ItemMaxDurabilityUpdate(Creature creature, Item item)
		{
			var packet = new Packet(Op.ItemMaxDurabilityUpdate, creature.EntityId);
			packet.PutLong(item.EntityId);
			packet.PutInt(item.OptionInfo.DurabilityMax);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends ItemRepairResult to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="item"></param>
		/// <param name="successes"></param>
		public static void ItemRepairResult(Creature creature, Item item, int successes)
		{
			var packet = new Packet(Op.ItemRepairResult, creature.EntityId);
			packet.PutLong(item.EntityId);
			packet.PutInt(successes);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends ItemUpgradeResult to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="item"></param>
		/// <param name="appliedUpgrade"></param>
		public static void ItemUpgradeResult(Creature creature, Item item, string appliedUpgrade)
		{
			var packet = new Packet(Op.ItemUpgradeResult, creature.EntityId);
			packet.PutLong(item.EntityId);
			packet.PutString(appliedUpgrade);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends ItemExpUpdate to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="item"></param>
		public static void ItemExpUpdate(Creature creature, Item item)
		{
			var packet = new Packet(Op.ItemExpUpdate, creature.EntityId);
			packet.PutLong(item.EntityId);
			packet.PutShort(item.OptionInfo.Experience);
			packet.PutByte(item.OptionInfo.EP);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends DyePaletteReqR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="var1"></param>
		/// <param name="var2"></param>
		/// <param name="var3"></param>
		/// <param name="var4"></param>
		public static void DyePaletteReqR(Creature creature, int var1, int var2, int var3, int var4)
		{
			var packet = new Packet(Op.DyePaletteReqR, creature.EntityId);
			packet.PutByte(true);
			packet.PutInt(var1); // PutInt(62);
			packet.PutInt(var2); // PutInt(123);
			packet.PutInt(var3); // PutInt(6);
			packet.PutInt(var4); // PutInt(238);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends DyePickColorR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		public static void DyePickColorR(Creature creature, bool success)
		{
			var packet = new Packet(Op.DyePickColorR, creature.EntityId);
			packet.PutByte(success);
			if (success)
				packet.PutBin(creature.Temp.RegularDyePickers);

			creature.Client.Send(packet);
		}

		public static void GiftItemR(Creature creature, bool success)
		{
			var packet = new Packet(Op.GiftItemR, creature.EntityId);
			packet.PutByte(success);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends UnequipBagR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		public static void UnequipBagR(Creature creature, bool success)
		{
			var packet = new Packet(Op.UnequipBagR, creature.EntityId);
			packet.PutByte(success);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends ItemBlessed to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="items"></param>
		public static void ItemBlessed(Creature creature, params Item[] items)
		{
			var packet = new Packet(Op.ItemBlessed, creature.EntityId);

			if (items != null)
			{
				foreach (var item in items)
				{
					packet.PutLong(item.EntityId);
					packet.PutByte(item.IsBlessed);
				}
			}

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends BurnItemR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		public static void BurnItemR(Creature creature, bool success)
		{
			var packet = new Packet(Op.BurnItemR, creature.EntityId);
			packet.PutByte(success);

			creature.Client.Send(packet);
		}
	}
}
