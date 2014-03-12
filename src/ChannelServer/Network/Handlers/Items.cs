// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Aura.Channel.Network.Sending;
using Aura.Channel.Scripting;
using Aura.Channel.World;
using Aura.Channel.World.Entities;
using Aura.Data.Database;
using Aura.Shared.Mabi.Const;
using Aura.Shared.Mabi.Structs;
using Aura.Shared.Network;
using Aura.Shared.Util;

namespace Aura.Channel.Network.Handlers
{
	public partial class ChannelServerHandlers : PacketHandlerManager<ChannelClient>
	{
		/// <summary>
		/// Sent when interacting with items, using the cursor.
		/// </summary>
		/// <example>
		/// ...
		/// </example>
		[PacketHandler(Op.ItemMove)]
		public void ItemMove(ChannelClient client, Packet packet)
		{
			var entityId = packet.GetLong();
			var source = (Pocket)packet.GetByte();
			var target = (Pocket)packet.GetByte();
			var unk = packet.GetByte();
			var targetX = packet.GetByte();
			var targetY = packet.GetByte();

			var creature = client.GetCreature(packet.Id);
			if (creature == null)
				return;

			var item = creature.Inventory.GetItem(entityId);
			if (item == null || item.Data.Type == ItemType.Hair || item.Data.Type == ItemType.Face)
			{
				Send.ItemMoveR(creature, false);
				return;
			}

			// Stop moving when changing weapons
			if ((target >= Pocket.RightHand1 && target <= Pocket.Magazine2) || (source >= Pocket.RightHand1 && source <= Pocket.Magazine2))
				creature.StopMove();

			// Try to move item
			if (!creature.Inventory.Move(item, target, targetX, targetY))
			{
				Send.ItemMoveR(creature, false);
				return;
			}

			Send.ItemMoveR(creature, true);
		}

		/// <summary>
		/// Sent when dropping an item.
		/// </summary>
		/// <example>
		/// ...
		/// </example>
		[PacketHandler(Op.ItemDrop)]
		public void ItemDrop(ChannelClient client, Packet packet)
		{
			var entityId = packet.GetLong();
			var unk = packet.GetByte();

			var creature = client.GetCreature(packet.Id);
			if (creature == null || creature.Region == null)
				return;

			var item = creature.Inventory.GetItem(entityId);
			if (item == null || item.Data.Type == ItemType.Hair || item.Data.Type == ItemType.Face)
			{
				Log.Warning("Player '{0}' ({1}) tried to drop invalid item.", creature.Name, creature.EntityIdHex);
				Send.ItemDropR(creature, false);
				return;
			}

			if (!creature.Inventory.Remove(item))
			{
				Send.ItemDropR(creature, false);
				return;
			}

			//if (HandleDungeonDrop(client, creature, item))
			//    return;

			item.Drop(creature.Region, creature.GetPosition());

			Send.ItemDropR(creature, true);

			ChannelServer.Instance.Events.OnPlayerRemovesItem(creature, item.Info.Id, item.Info.Amount);
		}

		/// <summary>
		/// Sent when clicking an item on the ground, to pick it up.
		/// </summary>
		/// <example>
		/// ...
		/// </example>
		[PacketHandler(Op.ItemPickUp)]
		public void ItemPickUp(ChannelClient client, Packet packet)
		{
			var entityId = packet.GetLong();

			var creature = client.GetCreature(packet.Id);
			if (creature == null || creature.Region == null)
				return;

			var item = creature.Region.GetItem(entityId);
			if (item == null)
			{
				Send.ItemPickUpR(creature, false);
				return;
			}

			var success = creature.Inventory.PickUp(item);
			if (!success)
				Send.SystemMessage(creature, Localization.Get("world.insufficient_space")); // Not enough space.

			Send.ItemPickUpR(creature, success);
		}

		/// <summary>
		/// Sent when destroying an item (right click option).
		/// </summary>
		/// <example>
		/// ...
		/// </example>
		[PacketHandler(Op.ItemDestroy)]
		public void ItemDestroy(ChannelClient client, Packet packet)
		{
			var itemId = packet.GetLong();

			var creature = client.GetCreature(packet.Id);
			if (creature == null)
				return;

			var item = creature.Inventory.GetItem(itemId);
			if (item == null || item.Data.Type == ItemType.Hair || item.Data.Type == ItemType.Face)
			{
				Log.Warning("Player '{0}' ({1}) tried to destroy invalid item.", creature.Name, creature.EntityIdHex);
				Send.ItemDestroyR(creature, false);
				return;
			}

			if (!creature.Inventory.Remove(item))
			{
				Send.ItemDestroyR(creature, false);
				return;
			}

			Send.ItemDestroyR(creature, true);

			ChannelServer.Instance.Events.OnPlayerRemovesItem(creature, item.Info.Id, item.Info.Amount);
		}

		/// <summary>
		/// Sent when splitting stacks
		/// </summary>
		/// <example>
		/// ...
		/// </example>
		[PacketHandler(Op.ItemSplit)]
		public void ItemSplit(ChannelClient client, Packet packet)
		{
			var itemId = packet.GetLong();
			var amount = packet.GetUShort();
			var unk1 = packet.GetByte();

			var creature = client.GetCreature(packet.Id);
			if (creature == null)
				return;

			// Check item
			var item = creature.Inventory.GetItem(itemId);
			if (item == null || item.Data.StackType == StackType.None)
			{
				Send.ItemSplitR(creature, false);
				return;
			}

			// Check requested amount
			if (item.Info.Amount < amount)
				amount = item.Info.Amount;

			// Clone item or create new one based on stack item
			Item splitItem;
			if (item.Data.StackItem == 0)
				splitItem = new Item(item);
			else
				splitItem = new Item(item.Data.StackItem);
			splitItem.Info.Amount = amount;

			// New item on cursor
			if (!creature.Inventory.Add(splitItem, Pocket.Cursor))
			{
				Send.ItemSplitR(creature, false);
				return;
			}

			// Update amount or remove
			item.Info.Amount -= amount;

			if (item.Info.Amount > 0 || item.Data.StackItem != 0)
			{
				Send.ItemAmount(creature, item);
			}
			else
			{
				creature.Inventory.Remove(item);
			}

			Send.ItemSplitR(creature, true);
		}

		/// <summary>
		/// Sent when switching weapon sets (eg, on Tab/W).
		/// </summary>
		/// <example>
		/// ...
		/// </example>
		[PacketHandler(Op.SwitchSet)]
		public void SwitchSet(ChannelClient client, Packet packet)
		{
			var set = (WeaponSet)packet.GetByte();

			var creature = client.GetCreature(packet.Id);
			if (creature == null)
				return;

			creature.StopMove();

			creature.Inventory.WeaponSet = set;

			Send.UpdateWeaponSet(creature);
			//ChannelServer.Instance.World.CreatureStatsUpdate(creature);

			Send.SwitchSetR(creature, true);
		}

		/// <summary>
		/// Sent when changing an item state, eg hood on robes.
		/// </summary>
		/// <example>
		/// ...
		/// </example>
		[PacketHandler(Op.ItemStateChange)]
		public void ItemStateChange(ChannelClient client, Packet packet)
		{
			var firstTarget = (Pocket)packet.GetByte();
			var secondTarget = (Pocket)packet.GetByte();
			var unk = packet.GetByte();

			var creature = client.GetCreature(packet.Id);
			if (creature == null)
				return;

			// This might not be entirely correct, but works well.
			// Robe is opened first, Helm secondly, then Robe and Helm are both closed.

			foreach (var target in new Pocket[] { firstTarget, secondTarget })
			{
				if (target > 0)
				{
					var item = creature.Inventory.GetItemAt(target, 0, 0);
					if (item != null)
					{
						item.Info.State = (byte)(item.Info.State == 1 ? 0 : 1);
						Send.EquipmentChanged(creature, item);
					}
				}
			}

			Send.ItemStateChangeR(creature);
		}

		/// <summary>
		/// Sent when using an item.
		/// </summary>
		/// <remarks>
		/// Not all usable items send this packet. The client has some
		/// items send different packets, like starting hidden skills
		/// (eg res, hw, dye, etc).
		/// </remarks>
		/// <example>
		/// 0001 [005000CBB535EFC6] Long   : 22518873055424454
		/// </example>
		[PacketHandler(Op.UseItem)]
		public void UseItem(ChannelClient client, Packet packet)
		{
			var entityId = packet.GetLong();

			var creature = client.GetCreature(packet.Id);
			if (creature == null) return;

			// Check states
			if (creature.IsDead)
			{
				Log.Warning("Player '{0}' tried to use item while being dead.", creature.Name);
				goto L_Fail;
			}

			// Get item
			var item = creature.Inventory.GetItem(entityId);
			if (item == null)
			{
				Log.Warning("Player '{0}' tried to use item he doesn't possess.", creature.Name);
				goto L_Fail;
			}

			// Meta Data Scripts
			var gotMetaScript = false;
			{
				// Sealed books
				if (item.MetaData1.Has("MGCWRD") && item.MetaData1.Has("MGCSEL"))
				{
					var magicWords = item.MetaData1.GetString("MGCWRD");
					try
					{
						var sms = new MagicWordsScript(magicWords);
						sms.Run(creature, item);

						gotMetaScript = true;
					}
					catch (Exception ex)
					{
						Log.Exception(ex, "Problem while running magic words script '{0}'", magicWords);
						Send.ServerMessage(creature, Localization.Get("aura.unimplemented_item")); // Unimplemented item.
						goto L_Fail;
					}
				}
			}

			// Aura Scripts
			if (!gotMetaScript)
			{
				// Get script
				var script = ChannelServer.Instance.ScriptManager.GetItemScript(item.Info.Id);
				if (script == null)
				{
					Log.Unimplemented("Item script for '{0}' not found.", item.Info.Id);
					Send.ServerMessage(creature, Localization.Get("aura.unimplemented_item")); // This item isn't implemented yet.
					goto L_Fail;
				}

				// Run script
				try
				{
					script.OnUse(creature, item);
				}
				catch (NotImplementedException)
				{
					Log.Unimplemented("UseItem: Item OnUse method for '{0}'.", item.Info.Id);
					Send.ServerMessage(creature, Localization.Get("aura.unimplemented_item2")); // This item isn't implemented completely yet.
					goto L_Fail;
				}
			}

			// Decrease item count
			if (item.Data.Consumed)
			{
				creature.Inventory.Decrement(item);
				ChannelServer.Instance.Events.OnPlayerRemovesItem(creature, item.Info.Id, 1);
			}

			// Break seal after use
			if (item.MetaData1.Has("MGCSEL"))
			{
				item.MetaData1.Remove("MGCWRD");
				item.MetaData1.Remove("MGCSEL");
				Send.ItemUpdate(creature, item);
			}

			// Mandatory stat update
			Send.StatUpdate(creature, StatUpdateType.Private, Stat.Life, Stat.LifeInjured, Stat.Mana, Stat.Stamina, Stat.Hunger);
			Send.StatUpdate(creature, StatUpdateType.Public, Stat.Life, Stat.LifeInjured);

			Send.UseItemR(creature, true, item.Info.Id);
			return;

		L_Fail:
			Send.UseItemR(creature, false, 0);
		}

		/// <summary>
		/// Sent after regular dye was prepared.
		/// </summary>
		/// <remarks>
		/// What's sent back are the parameters for the wave algorithm,
		/// creating the random pattern.
		/// </remarks>
		/// <example>
		/// No parameters.
		/// </example>
		[PacketHandler(Op.DyePaletteReq)]
		public void DyePaletteReq(ChannelClient client, Packet packet)
		{
			var creature = client.GetCreature(packet.Id);
			if (creature == null) return;

			Send.DyePaletteReqR(creature, 0, 0, 0, 0);
		}

		/// <summary>
		/// Sent when clicking "Pick Color".
		/// </summary>
		/// <remarks>
		/// Generates the randomly placed pickers' positions for regular dyes.
		/// They are placed relative to the cursor, if the whole struct
		/// is 0 all pickers will be at the cursor, giving all 5 options
		/// the same color.
		/// </remarks>
		/// <example>
		/// 0001 [005000CB994586F1] Long   : 22518872586684145
		/// </example>
		[PacketHandler(Op.DyePickColor)]
		public void DyePickColor(ChannelClient client, Packet packet)
		{
			var itemEntityId = packet.GetLong();

			var creature = client.GetCreature(packet.Id);
			if (creature == null) return;

			var pickers = new DyePickers();
			//pickers.Picker2.X = 10;
			//pickers.Picker2.Y = 10;
			//pickers.Picker3.X = -10;
			//pickers.Picker3.Y = 10;

			creature.Temp.RegularDyePickers = pickers;

			Send.DyePickColorR(creature, true);
		}
	}
}
