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

		public IList<Item> GetItemList()
		{
			lock (_items)
				return _items.Values.ToList();
		}
	}

	public enum BankTabRace : byte
	{
		Human, Elf, Giant
	}
}
