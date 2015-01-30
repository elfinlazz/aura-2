// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence.txt in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using Aura.Channel.World.Entities;
using Aura.Data.Database;
using Aura.Shared.Mabi.Const;

namespace Aura.Channel.World.Inventory
{
	public abstract class InventoryPocket
	{
		public Pocket Pocket { get; protected set; }
		public abstract IEnumerable<Item> Items { get; }

		/// <summary>
		/// Returns amount of items.
		/// </summary>
		public abstract int Count { get; }

		protected InventoryPocket(Pocket pocket)
		{
			this.Pocket = pocket;
		}

		/// <summary>
		/// Attempts to put item at the coordinates. If another item is
		/// in the new item's space it's returned in colliding.
		/// Returns whether the attempt was successful.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="targetX"></param>
		/// <param name="targetY"></param>
		/// <param name="colliding"></param>
		/// <returns></returns>
		public abstract bool TryAdd(Item item, byte targetX, byte targetY, out Item colliding);

		/// <summary>
		/// Adds item to pocket, at the first possible position.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public abstract bool Add(Item item);

		/// <summary>
		/// Adds item to pocket, at its position.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public abstract void AddUnsafe(Item item);

		/// <summary>
		/// Fills stacks that take this item. Returns true if item has been
		/// completely added to stacks/sacs.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="changed"></param>
		/// <returns></returns>
		public abstract bool FillStacks(Item item, out List<Item> changed);

		/// <summary>
		/// Removes item from pocket.
		/// </summary>
		/// <param name="item"></param>
		public abstract bool Remove(Item item);

		/// <summary>
		/// Returns whether the item exists in this pocket.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public abstract bool Has(Item item);

		/// <summary>
		/// Returns the item at the location, or null.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public abstract Item GetItemAt(int x, int y);

		/// <summary>
		/// Returns item by entity id, or null.
		/// </summary>
		/// <param name="entityId"></param>
		/// <returns></returns>
		public abstract Item GetItem(long entityId);

		/// <summary>
		/// Removes items by item id and amount. Modified items are returned
		/// as a list.
		/// </summary>
		/// <param name="itemId"></param>
		/// <param name="amount"></param>
		/// <param name="changed"></param>
		/// <returns></returns>
		public abstract int Remove(int itemId, int amount, ref List<Item> changed);

		/// <summary>
		/// Returns amount of items by item id.
		/// </summary>
		/// <param name="itemId"></param>
		/// <returns></returns>
		public abstract int CountItem(int itemId);
	}

	/// <summary>
	/// Normal inventory with a specific size.
	/// </summary>
	public class InventoryPocketNormal : InventoryPocket
	{
		protected Dictionary<long, Item> _items;
		protected Item[,] _map;
		protected int _width, _height;

		public InventoryPocketNormal(Pocket pocket, int width, int height)
			: base(pocket)
		{
			_items = new Dictionary<long, Item>();
			_map = new Item[width, height];

			_width = width;
			_height = height;
		}

		public override IEnumerable<Item> Items
		{
			get
			{
				return _items.Values;
			}
		}

		public override bool TryAdd(Item newItem, byte targetX, byte targetY, out Item collidingItem)
		{
			collidingItem = null;

			if (targetX + newItem.Data.Width > _width || targetY + newItem.Data.Height > _height)
				return false;

			var collidingItems = this.GetCollidingItems(targetX, targetY, newItem);
			if (collidingItems.Count > 1)
				return false;

			if (collidingItems.Count > 0)
				collidingItem = collidingItems[0];

			if (collidingItem != null && ((collidingItem.Data.StackType == StackType.Sac && (collidingItem.Data.StackItem == newItem.Info.Id || collidingItem.Data.StackItem == newItem.Data.StackItem)) || (newItem.Data.StackType == StackType.Stackable && newItem.Info.Id == collidingItem.Info.Id)))
			{
				if (collidingItem.Info.Amount < collidingItem.Data.StackMax)
				{
					var diff = (ushort)(collidingItem.Data.StackMax - collidingItem.Info.Amount);

					collidingItem.Info.Amount += Math.Min(diff, newItem.Info.Amount);
					newItem.Info.Amount -= Math.Min(diff, newItem.Info.Amount);

					return true;
				}
			}

			if (collidingItem != null)
			{
				_items.Remove(collidingItem.EntityId);
				collidingItem.Move(newItem.Info.Pocket, newItem.Info.X, newItem.Info.Y);
				this.ClearFromMap(collidingItem);
			}

			_items.Add(newItem.EntityId, newItem);
			newItem.Move(this.Pocket, targetX, targetY);
			this.AddToMap(newItem);

			return true;
		}

		protected void AddToMap(Item item)
		{
			for (var x = item.Info.X; x < item.Info.X + item.Data.Width; ++x)
			{
				for (var y = item.Info.Y; y < item.Info.Y + item.Data.Height; ++y)
				{
					_map[x, y] = item;
				}
			}
		}

		protected void ClearFromMap(Item item)
		{
			int count = 0;
			int max = item.Data.Width * item.Data.Height;

			for (var x = 0; x < _width; ++x)
			{
				for (var y = 0; y < _height; ++y)
				{
					if (_map[x, y] == item)
					{
						_map[x, y] = null;
						if (++count >= max)
							return;
					}
				}
			}
		}

		protected List<Item> GetCollidingItems(uint targetX, uint targetY, Item item)
		{
			var result = new List<Item>();

			for (var x = targetX; x < targetX + item.Data.Width; ++x)
			{
				for (var y = targetY; y < targetY + item.Data.Height; ++y)
				{
					if (x > _width - 1 || y > _height - 1)
						continue;

					if (_map[x, y] != null && !result.Contains(_map[x, y]))
						result.Add(_map[x, y]);
				}
			}

			return result;
		}

		public override bool Remove(Item item)
		{
			if (_items.Remove(item.EntityId))
			{
				this.ClearFromMap(item);
				return true;
			}

			return false;
		}

		public override bool Add(Item item)
		{
			for (byte y = 0; y <= _height - item.Data.Height; ++y)
			{
				for (byte x = 0; x <= _width - item.Data.Width; ++x)
				{
					if (_map[x, y] != null)
						continue;

					if (this.GetCollidingItems(x, y, item).Count == 0)
					{
						item.Move(this.Pocket, x, y);
						this.AddUnsafe(item);
						return true;
					}
				}
			}

			return false;
		}

		public override void AddUnsafe(Item item)
		{
			this.AddToMap(item);
			_items.Add(item.EntityId, item);
			item.Info.Pocket = this.Pocket;
		}

		public void TestMap()
		{
			var items = Items.ToList();
			for (int i = 0; i < items.Count; ++i)
			{
				Console.WriteLine((i + 1) + ") " + items[i].Data.Name);
				items[i].OptionInfo.DucatPrice = i + 1;
			}
			for (var y = 0; y < _height; ++y)
			{
				for (var x = 0; x < _width; ++x)
				{
					if (_map[x, y] != null)
						Console.Write(_map[x, y].OptionInfo.DucatPrice.ToString().PadLeft(2) + " ");
					else
						Console.Write(" 0" + " ");
				}
				Console.WriteLine("|");
				Console.WriteLine();
			}
		}

		public override Item GetItemAt(int x, int y)
		{
			if (x > _width - 1 || y > _height - 1)
				return null;
			return _map[x, y];
		}

		public override bool FillStacks(Item item, out List<Item> changed)
		{
			changed = new List<Item>();

			if (item.Data.StackType != StackType.Stackable)
				return false;

			for (var y = 0; y < _height; ++y)
			{
				for (var x = 0; x < _width; ++x)
				{
					var invItem = _map[x, y];
					if (invItem == null || changed.Contains(invItem))
						continue;

					// If same class or item is stack item of inventory item
					if (item.Info.Id == invItem.Info.Id || invItem.Data.StackItem == item.Info.Id)
					{
						// If item fits into stack 100%
						if ((uint)invItem.Info.Amount + (uint)item.Info.Amount <= (uint)invItem.Data.StackMax)
						{
							invItem.Info.Amount += item.Info.Amount;
							item.Info.Amount = 0;

							changed.Add(invItem);

							return true;
						}

						// If stack is not full
						if (invItem.Info.Amount < invItem.Data.StackMax)
						{
							var diff = Math.Min(item.Info.Amount, (ushort)(invItem.Data.StackMax - invItem.Info.Amount));
							item.Info.Amount -= diff;
							invItem.Info.Amount += diff;

							changed.Add(invItem);
						}
					}
				}
			}

			return false;
		}

		public override bool Has(Item item)
		{
			return _items.ContainsValue(item);
		}

		public override int Remove(int itemId, int amount, ref List<Item> changed)
		{
			var result = 0;

			for (int y = (int)_height - 1; y >= 0; --y)
			{
				for (int x = (int)_width - 1; x >= 0; --x)
				{
					var item = _map[x, y];
					if (item == null || changed.Contains(item))
						continue;

					// Normal
					if (item.Info.Id == itemId && item.Data.StackType == StackType.None)
					{
						result++;
						amount--;
						item.Info.Amount = 0;
						changed.Add(item);
						_items.Remove(item.EntityId);
					}

					// Sacs/Stackables
					if (item.Data.StackItem == itemId || (item.Info.Id == itemId && item.Data.StackType == StackType.Stackable))
					{
						if (amount >= item.Info.Amount)
						{
							result += item.Info.Amount;
							amount -= item.Info.Amount;
							item.Info.Amount = 0;
							changed.Add(item);
							if (item.Data.StackType != StackType.Sac)
							{
								_items.Remove(item.EntityId);
								this.ClearFromMap(item);
							}
						}
						else
						{
							result += amount;
							item.Info.Amount -= (ushort)amount;
							amount = 0;
							changed.Add(item);
						}
					}

					if (amount == 0)
						goto L_Result;
				}
			}

		L_Result:
			return result;
		}

		public override int CountItem(int itemId)
		{
			return _items.Values.Where(item => item.Info.Id == itemId || item.Data.StackItem == itemId)
				.Aggregate(0, (current, item) => current + item.Info.Amount);
		}

		public override Item GetItem(long id)
		{
			Item item;
			_items.TryGetValue(id, out item);
			return item;
		}

		public override int Count
		{
			get { return _items.Count; }
		}
	}

	/// <summary>
	/// Pocket only holding a single item (eg Equipment).
	/// </summary>
	public class InventoryPocketSingle : InventoryPocket
	{
		private Item _item;

		public InventoryPocketSingle(Pocket pocket)
			: base(pocket)
		{
		}

		public override IEnumerable<Item> Items
		{
			get
			{
				yield return _item;
			}
		}

		public override bool TryAdd(Item item, byte targetX, byte targetY, out Item collidingItem)
		{
			collidingItem = null;
			if (_item != null)
			{
				collidingItem = _item;
				collidingItem.Move(item.Info.Pocket, item.Info.X, item.Info.Y);
			}

			_item = item;
			_item.Move(this.Pocket, 0, 0);

			return true;
		}

		public override bool Remove(Item item)
		{
			if (_item == item)
			{
				_item = null;
				return true;
			}

			return false;
		}

		public override bool Add(Item item)
		{
			if (_item != null)
				return false;

			this.AddUnsafe(item);
			return true;
		}

		public override void AddUnsafe(Item item)
		{
			item.Move(this.Pocket, 0, 0);
			_item = item;
		}

		public override Item GetItemAt(int x, int y)
		{
			return _item;
		}

		public override bool FillStacks(Item item, out List<Item> changed)
		{
			changed = null;
			return false;
		}

		public override bool Has(Item item)
		{
			return _item == item;
		}

		public override int Remove(int itemId, int amount, ref List<Item> changed)
		{
			return 0;
		}

		public override int CountItem(int itemId)
		{
			if (_item != null && _item.Info.Id == itemId)
				return _item.Info.Amount;
			return 0;
		}

		public override Item GetItem(long id)
		{
			if (_item != null && _item.EntityId == id)
				return _item;
			return null;
		}

		public override int Count
		{
			get { return (_item != null ? 1 : 0); }
		}
	}

	/// <summary>
	/// Pocket that holds an infinite number of items.
	/// </summary>
	public class InventoryPocketStack : InventoryPocket
	{
		private List<Item> _items;

		public InventoryPocketStack(Pocket pocket)
			: base(pocket)
		{
			_items = new List<Item>();
		}

		public override IEnumerable<Item> Items
		{
			get
			{
				return _items;
			}
		}

		public override bool TryAdd(Item item, byte targetX, byte targetY, out Item colliding)
		{
			colliding = null;
			_items.Add(item);
			return true;
		}

		public override bool Remove(Item item)
		{
			return _items.Remove(item);
		}

		public override bool Add(Item item)
		{
			this.AddUnsafe(item);
			return true;
		}

		public override void AddUnsafe(Item item)
		{
			item.Move(this.Pocket, 0, 0);
			_items.Add(item);
		}

		public override Item GetItemAt(int x, int y)
		{
			return _items.FirstOrDefault();
		}

		public override bool FillStacks(Item item, out List<Item> changed)
		{
			changed = null;
			return false;
		}

		public override bool Has(Item item)
		{
			return _items.Contains(item);
		}

		public override int Remove(int itemId, int amount, ref List<Item> changed)
		{
			return 0;
		}

		public override int CountItem(int itemId)
		{
			return _items.Where(item => item.Info.Id == itemId || item.Data.StackItem == itemId)
				.Aggregate(0, (current, item) => current + item.Info.Amount);
		}

		public override Item GetItem(long id)
		{
			return _items.FirstOrDefault(a => a.EntityId == id);
		}

		public override int Count
		{
			get { return _items.Count; }
		}
	}
}
