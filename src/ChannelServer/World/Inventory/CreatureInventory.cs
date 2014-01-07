// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence.txt in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using Aura.Channel.Network.Sending;
using Aura.Channel.World.Entities;
using Aura.Data.Database;
using Aura.Shared.Mabi.Const;
using Aura.Shared.Util;

namespace Aura.Channel.World
{
	public class CreatureInventory
	{
		private const int DefaultWidth = 6;
		private const int DefaultHeight = 10;
		private const int GoldItemId = 2000;
		private const int GoldStackMax = 1000;

		private Creature _creature;
		private Dictionary<Pocket, InventoryPocket> _pockets;

		/// <summary>
		/// List of all items in this inventory.
		/// </summary>
		public IEnumerable<Item> Items
		{
			get
			{
				foreach (var pocket in _pockets.Values)
					foreach (var item in pocket.Items.Where(a => a != null))
						yield return item;
			}
		}

		/// <summary>
		/// List of all items sitting in equipment pockets in this inventory.
		/// </summary>
		public IEnumerable<Item> Equipment
		{
			get
			{
				foreach (var pocket in _pockets.Values.Where(a => a.Pocket.IsEquip()))
					foreach (var item in pocket.Items.Where(a => a != null))
						yield return item;
			}
		}

		/// <summary>
		/// List of all items in equipment slots, minus hair and face.
		/// </summary>
		public IEnumerable<Item> ActualEquipment
		{
			get
			{
				foreach (var pocket in _pockets.Values.Where(a => a.Pocket.IsEquip() && a.Pocket != Pocket.Hair && a.Pocket != Pocket.Face))
					foreach (var item in pocket.Items.Where(a => a != null))
						yield return item;
			}
		}

		private WeaponSet _weaponSet;
		/// <summary>
		/// Sets or returns the selected weapon set.
		/// </summary>
		public WeaponSet WeaponSet
		{
			get { return _weaponSet; }
			set
			{
				_weaponSet = value;
				this.UpdateEquipReferences(Pocket.RightHand1, Pocket.LeftHand1, Pocket.Magazine1);
			}
		}

		/// <summary>
		/// Reference to the item currently equipped in the right hand.
		/// </summary>
		public Item RightHand { get; protected set; }

		/// <summary>
		/// Reference to the item currently equipped in the left hand.
		/// </summary>
		public Item LeftHand { get; protected set; }

		/// <summary>
		/// Reference to the item currently equipped in the magazine (eg arrows).
		/// </summary>
		public Item Magazine { get; protected set; }

		/// <summary>
		/// Returns the amount of gold items in the inventory.
		/// </summary>
		public int Gold
		{
			get { return this.Count(GoldItemId); }
		}

		public CreatureInventory(Creature creature)
		{
			_creature = creature;

			_pockets = new Dictionary<Pocket, InventoryPocket>();

			// Cursor, Temp
			this.Add(new InventoryPocketStack(Pocket.Temporary));
			this.Add(new InventoryPocketSingle(Pocket.Cursor));

			// Equipment
			for (var i = Pocket.Face; i <= Pocket.Accessory2; ++i)
				this.Add(new InventoryPocketSingle(i));

			// Style
			for (var i = Pocket.ArmorStyle; i <= Pocket.RobeStyle; ++i)
				this.Add(new InventoryPocketSingle(i));
		}

		/// <summary>
		/// Adds pocket to inventory.
		/// </summary>
		/// <param name="inventoryPocket"></param>
		public void Add(InventoryPocket inventoryPocket)
		{
			if (_pockets.ContainsKey(inventoryPocket.Pocket))
				Log.Warning("Replacing pocket '{0}' in '{1}'s inventory.", inventoryPocket.Pocket, _creature);

			_pockets[inventoryPocket.Pocket] = inventoryPocket;
		}

		/// <summary>
		/// Adds main inventories (inv, personal, VIP). Call after creature's
		/// defaults (RaceInfo) have been loaded.
		/// </summary>
		public void AddMainInventory()
		{
			if (_creature.RaceData == null)
				Log.Warning("Race for creature '{0}' ({1}) not loaded before initializing main inventory.", _creature.Name, _creature.EntityIdHex);

			var width = (_creature.RaceData != null ? _creature.RaceData.InventoryWidth : DefaultWidth);
			var height = (_creature.RaceData != null ? _creature.RaceData.InventoryHeight : DefaultHeight);

			this.Add(new InventoryPocketNormal(Pocket.Inventory, width, height));
			this.Add(new InventoryPocketNormal(Pocket.PersonalInventory, width, height));
			this.Add(new InventoryPocketNormal(Pocket.VIPInventory, width, height));
		}

		/// <summary>
		/// Returns true if pocket exists in this inventory.
		/// </summary>
		/// <param name="pocket"></param>
		/// <returns></returns>
		public bool Has(Pocket pocket)
		{
			return _pockets.ContainsKey(pocket);
		}

		/// <summary>
		/// Returns item with the id, or null.
		/// </summary>
		/// <param name="entityId"></param>
		/// <returns></returns>
		public Item GetItem(long entityId)
		{
			foreach (var pocket in _pockets.Values)
			{
				var item = pocket.GetItem(entityId);
				if (item != null)
					return item;
			}

			return null;
		}

		/// <summary>
		/// Returns item at the location, or null.
		/// </summary>
		/// <param name="pocket"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public Item GetItemAt(Pocket pocket, int x, int y)
		{
			if (!this.Has(pocket))
				return null;

			return _pockets[pocket].GetItemAt(x, y);
		}

		/// <summary>
		/// Adds item at target location. Returns true if successful.
		/// </summary>
		public bool Move(Item item, Pocket target, byte targetX, byte targetY)
		{
			if (!this.Has(target))
				return false;

			var source = item.Info.Pocket;
			var amount = item.Info.Amount;

			Item collidingItem = null;
			if (!_pockets[target].TryAdd(item, targetX, targetY, out collidingItem))
				return false;

			// If amount differs (item was added to stack)
			if (collidingItem != null && item.Info.Amount != amount)
			{
				Send.ItemAmount(_creature, collidingItem);

				// Left overs, update
				if (item.Info.Amount > 0)
				{
					Send.ItemAmount(_creature, item);
				}
				// All in, remove from cursor.
				else
				{
					_pockets[item.Info.Pocket].Remove(item);
					Send.ItemRemove(_creature, item);
				}
			}
			else
			{
				// Remove the item from the source pocket
				_pockets[source].Remove(item);

				// Toss it in, it should be the cursor.
				if (collidingItem != null)
					_pockets[source].Add(collidingItem);

				Send.ItemMoveInfo(_creature, item, source, collidingItem);
			}

			this.UpdateInventory(item, source, target);

			return true;
		}

		/// <summary>
		/// Tries to add item to pocket. Returns false if the pocket
		/// doesn't exist or there was no space.
		/// </summary>
		public bool Add(Item item, Pocket pocket)
		{
			if (!_pockets.ContainsKey(pocket))
				return false;

			var success = _pockets[pocket].Add(item);
			if (success)
			{
				Send.ItemNew(_creature, item);
				this.UpdateEquipReferences(pocket);
			}

			return success;
		}

		/// <summary>
		/// Adds item to pocket at the position it currently has.
		/// Returns false if pocket doesn't exist.
		/// </summary>
		public bool InitAdd(Item item, Pocket pocket)
		{
			if (!_pockets.ContainsKey(pocket))
				return false;

			_pockets[pocket].AddUnsafe(item);
			return true;
		}

		/// <summary>
		/// Tries to add item to one of the main inventories, using the temp
		/// inv as fallback (if specified to do so). Returns false if
		/// there was no space.
		/// </summary>
		public bool Add(Item item, bool tempFallback)
		{
			bool success;

			// Try inv
			success = _pockets[Pocket.Inventory].Add(item);

			// Try temp
			if (!success && tempFallback)
				success = _pockets[Pocket.Temporary].Add(item);

			// Inform about new item
			if (success)
				Send.ItemNew(_creature, item);

			return success;
		}

		/// <summary>
		/// Tries to add item to one of the main inventories,
		/// using temp as fallback. Unlike "Add" the item will be filled
		/// into stacks first, if possible, before calling Add.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="tempFallback"></param>
		/// <returns></returns>
		public bool Insert(Item item, bool tempFallback)
		{
			if (item.Data.StackType == StackType.Stackable)
			{
				// Try stacks/sacs first
				List<Item> changed;
				_pockets[Pocket.Inventory].FillStacks(item, out changed);
				this.UpdateChangedItems(changed);

				// Add new item stacks as long as needed.
				while (item.Info.Amount > item.Data.StackMax)
				{
					var newStackItem = new Item(item);
					newStackItem.Info.Amount = item.Data.StackMax;

					// Break if no new items can be added (no space left)
					if (!_pockets[Pocket.Inventory].Add(newStackItem))
						break;

					Send.ItemNew(_creature, newStackItem);
					item.Info.Amount -= item.Data.StackMax;
				}

				if (item.Info.Amount == 0)
					return true;
			}

			return this.Add(item, tempFallback);
		}

		/// <summary>
		/// Tries to put item into the inventory, by filling stacks and adding it.
		/// If it was completely added to the inventory, it's removed
		/// from the region the inventory's creature is in.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool PickUp(Item item)
		{
			// Try stacks/sacs first
			if (item.Data.StackType == StackType.Stackable)
			{
				List<Item> changed;
				_pockets[Pocket.Inventory].FillStacks(item, out changed);
				this.UpdateChangedItems(changed);
			}

			// Add new items as long as needed
			while (item.Info.Amount > 0)
			{
				// Making a copy of the item, and generating a new temp id,
				// ensures that we can still remove the item from the ground
				// after moving it (region, x, y) to the pocket.
				// We also need the new id to prevent conflicts in the db
				// (SVN r67).

				var newStackItem = new Item(item);
				newStackItem.Info.Amount = Math.Min(item.Info.Amount, item.Data.StackMax);

				// Stop if no new items can be added (no space left)
				if (!_pockets[Pocket.Inventory].Add(newStackItem))
					break;

				Send.ItemNew(_creature, newStackItem);
				item.Info.Amount -= newStackItem.Info.Amount;
			}

			// Remove from map if item is in inv 100%
			if (item.Info.Amount == 0)
			{
				_creature.Region.RemoveItem(item);
				return true;
			}

			return false;
		}

		/// <summary>
		/// Prints some debug information in chat box and server console.
		/// (To be removed)
		/// </summary>
		public void Debug()
		{
			(_pockets[Pocket.Inventory] as InventoryPocketNormal).TestMap();

			Send.ServerMessage(_creature, this.WeaponSet.ToString());
			if (this.RightHand == null)
				Send.ServerMessage(_creature, "null");
			else
				Send.ServerMessage(_creature, this.RightHand.ToString());
			if (this.LeftHand == null)
				Send.ServerMessage(_creature, "null");
			else
				Send.ServerMessage(_creature, this.LeftHand.ToString());
			if (this.Magazine == null)
				Send.ServerMessage(_creature, "null");
			else
				Send.ServerMessage(_creature, this.Magazine.ToString());
		}

		/// <summary>
		/// Removes item from inventory, if it is in it, and sends update packets.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool Remove(Item item)
		{
			foreach (var pocket in _pockets.Values)
			{
				if (pocket.Remove(item))
				{
					this.UpdateInventory(item, item.Info.Pocket, Pocket.None);

					Send.ItemRemove(_creature, item);
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Checks and updates all equipment references for source and target.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="source"></param>
		/// <param name="target"></param>
		private void UpdateInventory(Item item, Pocket source, Pocket target)
		{
			this.CheckLeftHand(item, source, target);
			this.UpdateEquipReferences(source, target);
			this.CheckEquipMoved(item, source, target);
		}

		/// <summary>
		/// Sends amount update or remove packets for all items, depending on
		/// their amount.
		/// </summary>
		/// <param name="items"></param>
		private void UpdateChangedItems(IEnumerable<Item> items)
		{
			if (items == null)
				return;

			foreach (var item in items)
			{
				if (item.Info.Amount > 0 || item.Data.StackType == StackType.Sac)
					Send.ItemAmount(_creature, item);
				else
					Send.ItemRemove(_creature, item);
			}
		}

		/// <summary>
		/// Unequips item in left hand/magazine, if item in right hand is moved.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="source"></param>
		/// <param name="target"></param>
		private void CheckLeftHand(Item item, Pocket source, Pocket target)
		{
			var pocketOfInterest = Pocket.None;

			if (source == Pocket.RightHand1 || source == Pocket.RightHand2)
				pocketOfInterest = source;
			if (target == Pocket.RightHand1 || target == Pocket.RightHand2)
				pocketOfInterest = target;

			if (pocketOfInterest != Pocket.None)
			{
				var leftPocket = pocketOfInterest + 2; // Left Hand 1/2
				var leftItem = _pockets[leftPocket].GetItemAt(0, 0);
				if (leftItem == null)
				{
					leftPocket += 2; // Magazine 1/2
					leftItem = _pockets[leftPocket].GetItemAt(0, 0);
				}
				if (leftItem != null)
				{
					// Try inventory first.
					// TODO: List of pockets stuff can be auto-moved to.
					var success = _pockets[Pocket.Inventory].Add(leftItem);

					// Fallback, temp inv
					if (!success)
						success = _pockets[Pocket.Temporary].Add(leftItem);

					if (success)
					{
						_pockets[leftPocket].Remove(leftItem);

						Send.ItemMoveInfo(_creature, leftItem, leftPocket, null);
						Send.EquipmentMoved(_creature, leftPocket);
					}
				}
			}
		}

		/// <summary>
		/// Updates quick access equipment refernces.
		/// </summary>
		/// <param name="toCheck"></param>
		private void UpdateEquipReferences(params Pocket[] toCheck)
		{
			var firstSet = (this.WeaponSet == WeaponSet.First);
			var updatedHands = false;

			foreach (var pocket in toCheck)
			{
				// Update all "hands" at once, easier.
				if (!updatedHands && pocket >= Pocket.RightHand1 && pocket <= Pocket.Magazine2)
				{
					this.RightHand = _pockets[firstSet ? Pocket.RightHand1 : Pocket.RightHand2].GetItemAt(0, 0);
					this.LeftHand = _pockets[firstSet ? Pocket.LeftHand1 : Pocket.LeftHand2].GetItemAt(0, 0);
					this.Magazine = _pockets[firstSet ? Pocket.Magazine1 : Pocket.Magazine2].GetItemAt(0, 0);

					// Don't do it twice.
					updatedHands = true;
				}
			}
		}

		/// <summary>
		/// Runs equipment updates if necessary.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="source"></param>
		/// <param name="target"></param>
		private void CheckEquipMoved(Item item, Pocket source, Pocket target)
		{
			if (source.IsEquip())
				Send.EquipmentMoved(_creature, source);

			if (target.IsEquip())
				Send.EquipmentChanged(_creature, item);
		}

		/// <summary>
		/// Reduces item's amount and sends the necessary update packets.
		/// Also removes the item, if it's not a sack and its amount reaches 0.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="amount"></param>
		/// <returns></returns>
		public bool Decrement(Item item, ushort amount = 1)
		{
			if (!this.Has(item) || item.Info.Amount == 0 || item.Info.Amount < amount)
				return false;

			item.Info.Amount -= amount;

			if (item.Info.Amount > 0 || item.Data.StackType == StackType.Sac)
			{
				Send.ItemAmount(_creature, item);
			}
			else
			{
				this.Remove(item);
				Send.ItemRemove(_creature, item);
			}

			return true;
		}

		/// <summary>
		/// Returns true uf the item exists in this inventory.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool Has(Item item)
		{
			foreach (var pocket in _pockets.Values)
				if (pocket.Has(item))
					return true;

			return false;
		}

		/// <summary>
		/// Adds a new item to the inventory.
		/// </summary>
		/// <remarks>
		/// For stackables and sacs the amount is capped at stack max.
		/// - If item is stackable, it will be added to existing stacks first.
		///   New stacks are added afterwards, if necessary.
		/// - If item is a sac it's simply added as one item.
		/// - If it's a normal item, it's added times the amount.
		/// </remarks>
		/// <param name="itemId"></param>
		/// <param name="amount"></param>
		/// <returns></returns>
		public bool Add(int itemId, int amount = 1)
		{
			var newItem = new Item(itemId);
			newItem.Amount = amount;

			if (newItem.Data.StackType == StackType.Stackable)
			{
				return this.Insert(newItem, true);
			}
			else if (newItem.Data.StackType == StackType.Sac)
			{
				return this.Add(newItem, true);
			}
			else
			{
				for (int i = 0; i < amount; ++i)
					this.Add(new Item(itemId), true);
				return true;
			}
		}

		/// <summary>
		/// Adds new gold stacks to the inventory until the amount was added.
		/// Spare gold will simply be ignored.
		/// TODO: Add it to temp? Drop it?
		/// </summary>
		/// <param name="amount"></param>
		/// <returns></returns>
		public bool AddGold(int amount)
		{
			// Add gold, stack for stack
			do
			{
				var stackAmount = Math.Min(GoldStackMax, amount);
				this.Add(GoldItemId, stackAmount);
				amount -= stackAmount;
			}
			while (amount > 0);

			return true;
		}

		/// <summary>
		/// Removes the amount of items with the id from the inventory.
		/// Returns true if the specified amount was removed.
		/// </summary>
		/// <param name="itemId"></param>
		/// <param name="amount"></param>
		/// <returns></returns>
		public bool Remove(int itemId, int amount = 1)
		{
			if (amount < 0)
				amount = 0;

			var changed = new List<Item>();

			foreach (var pocket in _pockets.Values)
			{
				amount -= pocket.Remove(itemId, amount, ref changed);

				if (amount == 0)
					break;
			}

			this.UpdateChangedItems(changed);

			return (amount == 0);
		}

		/// <summary>
		/// Removes the amount of gold from the inventory.
		/// Returns true if the specified amount was removed.
		/// </summary>
		/// <param name="amount"></param>
		/// <returns></returns>
		public bool RemoveGold(int amount)
		{
			return this.Remove(GoldItemId, amount);
		}

		/// <summary>
		/// Returns the amount of items with this id in the inventory.
		/// </summary>
		/// <param name="itemId"></param>
		/// <returns></returns>
		public int Count(int itemId)
		{
			var result = 0;

			foreach (var pocket in _pockets.Values)
				result += pocket.Count(itemId);

			return result;
		}

		/// <summary>
		/// Returns wheather inventory contains the item in this amount.
		/// </summary>
		/// <param name="itemId"></param>
		/// <param name="amount"></param>
		/// <returns></returns>
		public bool Has(int itemId, int amount = 1)
		{
			return (this.Count(itemId) >= amount);
		}

		/// <summary>
		/// Returns wheather inventory contains gold in this amount.
		/// </summary>
		/// <param name="amount"></param>
		/// <returns></returns>
		public bool HasGold(int amount)
		{
			return this.Has(GoldItemId, amount);
		}
	}

	public enum WeaponSet : byte
	{
		First = 0,
		Second = 1,
	}
}
