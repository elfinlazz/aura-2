// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence.txt in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using Aura.Shared.Util;
using Aura.Shared.Mabi.Const;
using Aura.Channel.World.Entities;
using Aura.Data.Database;
using Aura.Channel.Network.Sending;

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

		public Item RightHand { get; protected set; }
		public Item LeftHand { get; protected set; }
		public Item Magazine { get; protected set; }

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

		public void Add(InventoryPocket inventoryPocket)
		{
			if (_pockets.ContainsKey(inventoryPocket.Pocket))
				Log.Warning("Replacing pocket '{0}' in '{1}'s inventory.", inventoryPocket.Pocket, _creature);

			_pockets[inventoryPocket.Pocket] = inventoryPocket;
		}

		/// <summary>
		/// Adds main inventories (Inv, personal, VIP). Call after creature's
		/// defaults (RaceInfo) have been loaded.
		/// </summary>
		public void AddMainInventory()
		{
			if (_creature.RaceInfo == null)
				Log.Warning("Race for creature '{0}' ({1:X016}) not loaded before initializing main inventory.", _creature.Name, _creature.EntityId);

			var width = (_creature.RaceInfo != null ? _creature.RaceInfo.InvWidth : DefaultWidth);
			var height = (_creature.RaceInfo != null ? _creature.RaceInfo.InvHeight : DefaultHeight);

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
		/// Returns Item with the id, or null if it couldn't be found.
		/// </summary>
		/// <param name="itemId"></param>
		/// <returns></returns>
		public Item GetItem(long itemId)
		{
			foreach (var pocket in _pockets.Values)
			{
				var item = pocket.GetItem(itemId);
				if (item != null)
					return item;
			}

			return null;
		}

		/// <summary>
		/// Returns item at the location, or null if there is no item there.
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
				//Send.ItemAmount(_creature, collidingItem);

				// Left overs, update
				if (item.Info.Amount > 0)
				{
					//Send.ItemAmount(_creature, item);
				}
				// All in, remove from cursor.
				else
				{
					_pockets[item.Info.Pocket].Remove(item);
					//Send.ItemRemove(_creature, item);
				}
			}
			else
			{
				// Remove the item from the source pocket
				_pockets[source].Remove(item);

				// Toss it in, it should be the cursor.
				if (collidingItem != null)
					_pockets[source].ForceAdd(collidingItem);

				//Send.ItemMoveInfo(_creature, item, source, collidingItem);
			}

			this.UpdateInventory(item, source, target);

			return true;
		}

		/// <summary>
		/// Tries to put item into pocket, sends ItemInfo on success.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="pocket"></param>
		/// <returns></returns>
		public bool Add(Item item, Pocket pocket)
		{
			var success = _pockets[pocket].Add(item);
			if (success)
			{
				//Send.ItemInfo(_creature.Client, _creature, item);
				this.UpdateEquipReferences(pocket);
			}

			return success;
		}

		/// <summary>
		/// Adds item to pocket, without boundary and space checks.
		/// Only use when initializing!
		/// </summary>
		/// <param name="item"></param>
		/// <param name="pocket"></param>
		public void ForceAdd(Item item, Pocket pocket)
		{
			_pockets[pocket].ForceAdd(item);
			this.UpdateEquipReferences(pocket);
		}

		/// <summary>
		/// Attempts to store item somewhere in the inventory.
		/// If temp is true, it will fallback to the temp inv, if there's not space.
		/// Returns whether the item was successfully stored somewhere.
		/// Sends ItemNew on success.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="temp"></param>
		/// <returns></returns>
		public bool Add(Item item, bool tempFallback, bool sendItemNew = true)
		{
			bool success;

			// Try inv
			success = _pockets[Pocket.Inventory].Add(item);

			// Try temp
			if (!success && tempFallback)
				success = _pockets[Pocket.Temporary].Add(item);

			// Inform about new item
			//if (success && sendItemNew)
			//    Send.ItemInfo(_creature.Client, _creature, item);

			return success;
		}

		/// <summary>
		/// Puts item into inventory, if possible. Tries to fill stacks first.
		/// If tempFallback is true, leftovers will be put into temp.
		/// Sends ItemAmount and ItemNew if required/enabled.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="tempFallback"></param>
		/// <returns></returns>
		public bool Insert(Item item, bool tempFallback, bool sendItemNew)
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

					//Send.ItemInfo(_creature.Client, _creature, newStackItem);
					item.Info.Amount -= item.Data.StackMax;
				}

				if (item.Info.Amount == 0)
					return true;
			}

			return this.Add(item, tempFallback, sendItemNew);
		}

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
		/// Removes item from inventory. Sends ItemRemove on success,
		/// and possibly others, if equipment is removed.
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

					//Send.ItemRemove(_creature, item);
					return true;
				}
			}

			return false;
		}

		private void UpdateInventory(Item item, Pocket source, Pocket target)
		{
			this.CheckLeftHand(item, source, target);
			this.UpdateEquipReferences(source, target);
			this.CheckEquipMoved(item, source, target);
		}

		private void UpdateChangedItems(IEnumerable<Item> items)
		{
			if (items == null)
				return;

			foreach (var item in items)
			{
				//if (item.Info.Amount > 0 || item.StackType == StackType.Sac)
				//    Send.ItemAmount(_creature, item);
				//else
				//    Send.ItemRemove(_creature, item);
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

						//Send.ItemMoveInfo(_creature, leftItem, leftPocket, null);
						//Send.EquipmentMoved(_creature, leftPocket);
					}
				}
			}
		}

		/// <summary>
		/// Updates RightHand, LeftHand, and Magazine, if necessary.
		/// </summary>
		/// <param name="toCheck"></param>
		private void UpdateEquipReferences(params Pocket[] toCheck)
		{
			var firstSet = (this.WeaponSet == World.WeaponSet.First);
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
		/// Sends EquipmentMoved and EquipmentChanged, if necessary.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="source"></param>
		/// <param name="target"></param>
		private void CheckEquipMoved(Item item, Pocket source, Pocket target)
		{
			//if (source.IsEquip())
			//    Send.EquipmentMoved(_creature, source);

			if (target.IsEquip())
			{
				//Send.EquipmentChanged(_creature, item);

				// TODO: Equip/Unequip item scripts
				switch (item.Info.ItemId)
				{
					// Umbrella Skill
					case 41021:
					case 41022:
					case 41023:
					case 41025:
					case 41026:
					case 41027:
					case 41061:
					case 41062:
					case 41063:
						//if (!_creature.Skills.Has(SkillConst.Umbrella))
						//    _creature.Skills.Give(SkillConst.Umbrella, SkillRank.Novice);
						break;

					// Spread Wings
					case 19138:
					case 19139:
					case 19140:
					case 19141:
					case 19142:
					case 19143:
					case 19157:
					case 19158:
					case 19159:
						//if (!_creature.Skills.Has(SkillConst.SpreadWings))
						//    _creature.Skills.Give(SkillConst.SpreadWings, SkillRank.Novice);
						break;
				}
			}
		}

		/// <summary>
		/// Decrements the item by amount, if it exists in this inventory.
		/// Sends ItemAmount/ItemRemove, depending on the resulting amount.
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
				//Send.ItemAmount(_creature, item);
			}
			else
			{
				this.Remove(item);
				//Send.ItemRemove(_creature, item);
			}

			return true;
		}

		/// <summary>
		/// Returns whether the item exists in this inventory.
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
		/// Puts new item(s) of class 'id' into the inventory.
		/// If item is stackable it is "fit in", filling stacks first.
		/// A sack is set to the amount and added as one item.
		/// If it's not a sac/stackable you'll get multiple new items.
		/// Uses temp inv if necessary.
		/// </summary>
		/// <param name="itemId"></param>
		/// <param name="amount"></param>
		/// <returns></returns>
		public bool Add(int itemId, int amount = 1)
		{
			var newItem = new Item(itemId);

			if (newItem.Data.StackType == StackType.Stackable)
			{
				newItem.Info.Amount = (ushort)Math.Min(amount, ushort.MaxValue);
				return this.Insert(newItem, true, true);
			}
			else if (newItem.Data.StackType == StackType.Sac)
			{
				newItem.Info.Amount = (ushort)Math.Min(amount, ushort.MaxValue);
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
		/// Adds amount of gold to the inventory.
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
		/// Removes items with itemId from the inventory.
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
		/// Removes gold from the inventory.
		/// </summary>
		/// <param name="amount"></param>
		/// <returns></returns>
		public bool RemoveGold(int amount)
		{
			return this.Remove(GoldItemId, amount);
		}

		/// <summary>
		/// Returns amount of items with itemId in the inventory.
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
		/// Returns true if amount of items with itemId in inventory is equal
		/// or greater than amount.
		/// </summary>
		/// <param name="itemId"></param>
		/// <param name="amount"></param>
		/// <returns></returns>
		public bool Has(int itemId, int amount = 1)
		{
			return (this.Count(itemId) >= amount);
		}

		/// <summary>
		/// Returns true if amount of gold items in inventory is equal or
		/// greater than amount.
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
