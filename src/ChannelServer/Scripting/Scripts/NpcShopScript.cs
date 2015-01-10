// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using Aura.Channel.Network.Sending;
using Aura.Channel.World.Entities;
using Aura.Shared.Util;

namespace Aura.Channel.Scripting.Scripts
{
	/// <summary>
	/// Represents a regular NPC shop.
	/// </summary>
	/// <remarks>
	/// The difference between a gold and a ducat shop is: none.
	/// The secret lies in the DucatPrice value of ItemOptionInfo. If it is > 0,
	/// the client shows that, instead of the gold price, and assumes that
	/// you're paying with ducats.
	/// 
	/// TODO: Find the best way to add ducat items. Mixing the two should
	/// be possible I suppose. AddItem/AddDucatItem?
	/// 
	/// Selling items always uses gold (option to sell for ducats?).
	/// 
	/// Aside from Ducats and Gold there are two more currencies,
	/// Stars and Pons. The client will show the buy currency based on
	/// the values set, Duncan > Stars > Gold.
	/// Pons overweights everything, but it's displayed alongside
	/// other prices if they aren't 0.
	/// </remarks>
	public class NpcShopScript : IScript
	{
		protected Dictionary<string, NpcShopTab> _tabs;

		/// <summary>
		/// Creates new NpcShopScript
		/// </summary>
		public NpcShopScript()
		{
			_tabs = new Dictionary<string, NpcShopTab>();
		}

		/// <summary>
		/// Initializes shop, calling setup and adding it to the script manager.
		/// </summary>
		/// <remarks>
		/// TODO: Uncouple.
		/// </remarks>
		/// <returns></returns>
		public bool Init()
		{
			if (ChannelServer.Instance.ScriptManager.ShopExists(this.GetType().Name))
			{
				Log.Error("NpcShop.Init: Duplicate '{0}'", this.GetType().Name);
				return false;
			}

			this.Setup();

			ChannelServer.Instance.ScriptManager.AddShop(this);

			return true;
		}

		/// <summary>
		/// Called when creating the shop.
		/// </summary>
		public virtual void Setup()
		{
		}

		/// <summary>
		/// Adds empty tab.
		/// </summary>
		/// <param name="tabTitle">Tab title displayed in-game</param>
		/// <param name="shouldDisplay">Function that determines whether tab should be displayed, set null if not used.</param>
		/// <returns></returns>
		public NpcShopTab Add(string tabTitle, Func<Creature, NPC, bool> shouldDisplay = null)
		{
			NpcShopTab tab;
			lock (_tabs)
				_tabs.Add(tabTitle, (tab = new NpcShopTab(tabTitle, _tabs.Count, shouldDisplay)));
			return tab;
		}

		/// <summary>
		/// Adds empty tabs.
		/// </summary>
		/// <param name="tabTitles"></param>
		/// <returns></returns>
		public void Add(params string[] tabTitles)
		{
			foreach (var title in tabTitles)
				this.Add(title);
		}

		/// <summary>
		/// Adds item to tab.
		/// </summary>
		/// <param name="tabTitle"></param>
		/// <param name="itemId"></param>
		/// <param name="amount"></param>
		/// <param name="price">Uses db value if lower than 0 (default).</param>
		public void Add(string tabTitle, int itemId, int amount = 1, int price = -1)
		{
			var item = new Item(itemId);
			item.Amount = amount;

			this.Add(tabTitle, item, price);
		}

		/// <summary>
		/// Adds item to tab.
		/// </summary>
		/// <param name="tabTitle"></param>
		/// <param name="itemId"></param>
		/// <param name="color1"></param>
		/// <param name="color2"></param>
		/// <param name="color3"></param>
		/// <param name="price">Uses db value if lower than 0 (default).</param>
		public void Add(string tabTitle, int itemId, uint color1, uint color2, uint color3, int price = -1)
		{
			var item = new Item(itemId);
			item.Info.Color1 = color1;
			item.Info.Color2 = color2;
			item.Info.Color3 = color3;

			this.Add(tabTitle, item, price);
		}

		/// <summary>
		/// Adds item to tab.
		/// </summary>
		/// <param name="tabTitle"></param>
		/// <param name="itemId"></param>
		/// <param name="metaData"></param>
		/// <param name="price">Uses db value if lower than 0 (default).</param>
		public void Add(string tabTitle, int itemId, string metaData, int price = -1)
		{
			var item = new Item(itemId);
			item.MetaData1.Parse(metaData);

			this.Add(tabTitle, item, price);
		}

		/// <summary>
		/// Adds item to tab.
		/// </summary>
		/// <param name="tabTitle"></param>
		/// <param name="item"></param>
		/// <param name="price">Uses db value if lower than 0 (default).</param>
		public void Add(string tabTitle, Item item, int price = -1)
		{
			NpcShopTab tab;
			lock (_tabs)
				_tabs.TryGetValue(tabTitle, out tab);
			if (tab == null)
				tab = this.Add(tabTitle);

			if (price >= 0)
			{
				item.OptionInfo.Price = price;

				if (item.OptionInfo.SellingPrice > item.OptionInfo.Price)
					Log.Warning("NpcShop.Add: Selling price of '{0}' higher than buying price.", item.Info.Id);
			}

			tab.Add(item);
		}

		/// <summary>
		/// Returns new item based on target item from one of the tabs by id,
		/// or null.
		/// </summary>
		/// <param name="entityId"></param>
		/// <returns></returns>
		public Item GetItem(long entityId)
		{
			lock (_tabs)
			{
				foreach (var tab in _tabs.Values)
				{
					var item = tab.Get(entityId);
					if (item != null)
						return new Item(item);
				}
			}

			return null;
		}

		/// <summary>
		/// Sends OpenNpcShop for creature and this shop.
		/// </summary>
		/// <param name="creature">Creature opening the shop</param>
		/// <param name="owner">NPC owning the shop</param>
		public void OpenFor(Creature creature, NPC owner)
		{
			// Shops without tabs are weird.
			if (_tabs.Count == 0)
				this.Add("Empty");

			creature.Temp.CurrentShop = this;

			Send.OpenNpcShop(creature, this.GetTabs(creature, owner));
		}

		/// <summary>
		/// Returns thread-safe list of all tabs.
		/// </summary>
		/// <returns></returns>
		protected IList<NpcShopTab> GetTabs()
		{
			return this.GetTabs(null, null);
		}

		/// <summary>
		/// Returns thread-safe list of visible tabs, or all tabs if one of
		/// the parameters is null.
		/// </summary>
		/// <remarks>
		/// TODO: This could be cached.
		/// </remarks>
		/// <param name="creature">Creature opening the shop</param>
		/// <param name="owner">NPC owning the shop</param>
		/// <returns></returns>
		protected IList<NpcShopTab> GetTabs(Creature creature, NPC owner)
		{
			lock (_tabs)
				return creature == null || owner == null
					? _tabs.Values.ToList()
					: _tabs.Values.Where(t => t.ShouldDisplay(creature, owner)).ToList();
		}
	}

	/// <summary>
	/// Represents tab in an NPC shop, containing items.
	/// </summary>
	public class NpcShopTab
	{
		public Dictionary<long, Item> _items;

		/// <summary>
		/// All items in the tab.
		/// </summary>
		public ICollection<Item> Items { get { return _items.Values; } }

		/// <summary>
		/// Title of the tab.
		/// </summary>
		public string Title { get; protected set; }

		/// <summary>
		/// Index number in official tabs... order? (to be tested)
		/// </summary>
		public int Order { get; protected set; }

		/// <summary>
		/// Function that determines whether tab should be displayed.
		/// </summary>
		public Func<Creature, NPC, bool> ShouldDisplay { get; protected set; }

		/// <summary>
		/// Creatures new NpcShopTab
		/// </summary>
		/// <param name="title">Tab title display in-game.</param>
		/// <param name="order"></param>
		/// <param name="display">Function that determines whether tab should be displayed, set null if not used.</param>
		public NpcShopTab(string title, int order, Func<Creature, NPC, bool> display)
		{
			_items = new Dictionary<long, Item>();
			this.Title = title;
			this.Order = order;

			this.ShouldDisplay = display ?? ((c, n) => true);
		}

		/// <summary>
		/// Adds item.
		/// </summary>
		/// <param name="item"></param>
		public void Add(Item item)
		{
			_items[item.EntityId] = item;
		}

		/// <summary>
		/// Returns item by entity id, or null.
		/// </summary>
		/// <param name="entityId"></param>
		/// <returns></returns>
		public Item Get(long entityId)
		{
			Item result;
			_items.TryGetValue(entityId, out result);
			return result;
		}
	}
}
