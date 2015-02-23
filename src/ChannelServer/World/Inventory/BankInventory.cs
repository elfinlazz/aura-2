// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.World.Entities;
using Aura.Shared.Mabi.Const;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.World.Inventory
{
	public class BankInventory
	{
		/// <summary>
		/// Bank tabs, indexed by the tab (char) name.
		/// </summary>
		private Dictionary<string, BankTabPocket> Tabs { get; set; }

		/// <summary>
		/// Amount of silver. Use Add/RemoveGold to automatically update the client.
		/// </summary>
		public int Gold { get; set; }

		/// <summary>
		/// Creates new account bank.
		/// </summary>
		public BankInventory()
		{
			this.Tabs = new Dictionary<string, BankTabPocket>();
		}

		/// <summary>
		/// Adds bank tab pocket.
		/// </summary>
		/// <param name="name">Name of the tab (character)</param>
		/// <param name="race">Race filter id (1|2|3)</param>
		/// <param name="width">Width of the tab pocket</param>
		/// <param name="height">Height of the tab pocket</param>
		public void AddTab(string name, BankTabRace race, int width, int height)
		{
			if (this.Tabs.ContainsKey(name))
				throw new InvalidOperationException("Bank tab " + name + " already exists.");

			this.Tabs[name] = new BankTabPocket(name, race, width, height);
		}

		/// <summary>
		/// Returns thread-safe list of tabs.
		/// </summary>
		/// <returns></returns>
		public IList<BankTabPocket> GetTabList()
		{
			lock (this.Tabs)
				return this.Tabs.Values.ToList();
		}

		/// <summary>
		/// Returns thread-safe list of tabs.
		/// </summary>
		/// <param name="race"></param>
		/// <returns></returns>
		public IList<BankTabPocket> GetTabList(BankTabRace race)
		{
			lock (this.Tabs)
				return this.Tabs.Values.Where(a => a.Race == race).ToList();
		}

		/// <summary>
		/// Returns all items in specified tab. Returns an empty
		/// list if tab doesn't exist.
		/// </summary>
		/// <param name="tabName"></param>
		/// <returns></returns>
		public IList<Item> GetTabItems(string tabName)
		{
			if (!this.Tabs.ContainsKey(tabName))
				return new List<Item>();

			return this.Tabs[tabName].GetItemList();
		}

		/// <summary>
		/// Adds item to tab without any checks (only use for initialization).
		/// </summary>
		/// <param name="tabName"></param>
		/// <param name="item"></param>
		public bool InitAdd(string tabName, Item item)
		{
			if (!this.Tabs.ContainsKey(tabName))
				return false;

			this.Tabs[tabName].AddUnsafe(item);

			return true;
		}

		/// <summary>
		/// Adds gold to bank, sends update, and returns the new amount.
		/// </summary>
		/// <param name="creature">Creature removing gold (needed for updating)</param>
		/// <param name="amount">Amount of gold</param>
		/// <returns>New gold amount after adding</returns>
		public int AddGold(Creature creature, int amount)
		{
			this.Gold += amount;
			Send.BankUpdateGold(creature, this.Gold);

			return this.Gold;
		}

		/// <summary>
		/// Removes gold to bank, sends update, and returns the new amount.
		/// </summary>
		/// <param name="creature">Creature adding gold (needed for updating)</param>
		/// <param name="amount">Amount of gold</param>
		/// <returns>New gold amount after removing</returns>
		public int RemoveGold(Creature creature, int amount)
		{
			this.Gold -= amount;
			Send.BankUpdateGold(creature, this.Gold);

			return this.Gold;
		}

		/// <summary>
		/// Moves item from creature's inventory into bank.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="item"></param>
		/// <param name="bankId"></param>
		/// <param name="tabName"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public bool DepositItem(Creature creature, long itemEntityId, string bankId, string tabName, int x, int y)
		{
			// Check tab
			if (!this.Tabs.ContainsKey(tabName))
				return false;

			var tab = this.Tabs[tabName];

			// Check item
			var item = creature.Inventory.GetItem(itemEntityId);
			if (item == null) return false;

			// Add gold directly
			if (item.Info.Id == 2000)
			{
				this.Gold += item.Info.Amount;
				Send.BankUpdateGold(creature, this.Gold);
			}
			// Add check as gold
			else if (item.Info.Id == 2004)
			{
				var amount = item.MetaData1.GetInt("EVALUE");

				this.Gold += amount;
				Send.BankUpdateGold(creature, this.Gold);
			}
			// Normal items
			else
			{
				// Generate a new item, makes moving and updating easier.
				var newItem = new Item(item);

				// Try adding item to tab
				if (!tab.TryAdd(newItem, x, y))
					return false;

				// Update bank id
				newItem.Bank = bankId;

				// Update client
				Send.BankAddItem(creature, newItem, bankId, tabName);
			}

			// Remove item from inventory
			creature.Inventory.Remove(item);

			ChannelServer.Instance.Events.OnPlayerRemovesItem(creature, item.Info.Id, item.Info.Amount);

			return true;
		}

		/// <summary>
		/// Moves item with given id from bank to creature's cursor pocket.
		/// Returns whether it was successful or not.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="itemEntityId"></param>
		/// <returns></returns>
		public bool WithdrawItem(Creature creature, string tabName, long itemEntityId)
		{
			// Check tab
			if (!this.Tabs.ContainsKey(tabName))
				return false;

			var tab = this.Tabs[tabName];

			// Check item
			var item = tab.GetItem(itemEntityId);
			if (item == null) return false;

			// Generate a new item, makes moving and updating easier.
			var newItem = new Item(item);

			// Try moving item into cursor pocket
			if (!creature.Inventory.Add(newItem, Pocket.Cursor))
				return false;

			// Remove item from bank
			tab.Remove(item);
			Send.BankRemoveItem(creature, tabName, itemEntityId);

			ChannelServer.Instance.Events.OnPlayerReceivesItem(creature, item.Info.Id, item.Info.Amount);

			return true;
		}
	}

	public class BankTabPocket : InventoryPocketNormal
	{
		public string Name { get; private set; }
		public BankTabRace Race { get; private set; }
		public int Width { get { return _width; } }
		public int Height { get { return _height; } }

		public BankTabPocket(string name, BankTabRace race, int width, int height)
			: base(Pocket.None, width, height)
		{
			this.Name = name;
			this.Race = race;
		}

		/// <summary>
		/// Returns thread-safe list of items in this pocket.
		/// </summary>
		/// <returns></returns>
		public IList<Item> GetItemList()
		{
			lock (_items)
				return _items.Values.ToList();
		}

		/// <summary>
		/// Attempts to add the item at the given position.
		/// </summary>
		/// <remarks>
		/// TODO: Inventory really needs some refactoring, we shouldn't have
		///   to override such methods.
		/// </remarks>
		/// <param name="item"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public bool TryAdd(Item item, int x, int y)
		{
			if (x + item.Data.Width > _width || y + item.Data.Height > _height)
				return false;

			var collidingItems = this.GetCollidingItems((uint)x, (uint)y, item);
			if (collidingItems.Count > 0)
				return false;

			item.Move(this.Pocket, x, y);
			this.AddUnsafe(item);

			return true;
		}
	}

	public enum BankTabRace : byte
	{
		Human, Elf, Giant
	}
}
