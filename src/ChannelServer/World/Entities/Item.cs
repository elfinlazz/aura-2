// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Threading;
using Aura.Data;
using Aura.Data.Database;
using Aura.Shared.Mabi.Const;
using Aura.Shared.Mabi.Structs;
using Aura.Shared.Util;
using Aura.Shared.Mabi;

namespace Aura.Channel.World.Entities
{
	public class Item : Entity
	{
		private const int DropRadius = 50;

		private static long _itemId = MabiId.TmpItems;

		public override DataType DataType { get { return DataType.Item; } }
		public override int RegionId
		{
			get { return this.Info.Region; }
			set { this.Info.Region = value; }
		}

		public ItemInfo Info;
		public ItemOptionInfo OptionInfo;
		public ItemData Data { get; protected set; }
		public MabiDictionary MetaData1 { get; protected set; }
		public MabiDictionary MetaData2 { get; protected set; }

		private bool _firstTimeAppear = true;
		/// <summary>
		/// Returns true once, and false afterwards, until it's set true again.
		/// Used to decide whether the appearing item should bounce.
		/// </summary>
		public bool FirstTimeAppear
		{
			get
			{
				var result = _firstTimeAppear;
				_firstTimeAppear = false;
				return result;
			}
		}

		public long QuestId { get; set; }

		/// <summary>
		/// Sets and returns the current amount (Info.Amount).
		/// Setting is restricted to a minimum of 0 and a maximum of StackMax.
		/// </summary>
		public int Amount
		{
			get { return this.Info.Amount; }
			set { this.Info.Amount = (ushort)Math2.Clamp(0, this.Data.StackMax, value); }
		}

		/// <summary>
		/// Returns ".OptionInfo.Balance / 100".
		/// </summary>
		public float Balance
		{
			get { return this.OptionInfo.Balance / 100f; }
		}

		/// <summary>
		/// Returns ".OptionInfo.Critical / 100".
		/// </summary>
		public float Critical
		{
			get { return this.OptionInfo.Critical / 100f; }
		}

		/// <summary>
		/// Returns true if tag contains "/pounch/bag/".
		/// </summary>
		public bool IsBag
		{
			get { return this.Data.HasTag("/pouch/bag/"); }
		}

		/// <summary>
		/// New item based on item id.
		/// </summary>
		/// <param name="itemId"></param>
		public Item(int itemId)
		{
			this.Init(itemId);
			this.SetNewEntityId();

			var script = ChannelServer.Instance.ScriptManager.GetItemScript(itemId);
			if (script != null)
				script.OnCreation(this);

			// Color of book seals
			var sealColor = this.MetaData1.GetString("MGCSEL");
			if (sealColor != null)
			{
				switch (sealColor)
				{
					case "yellow": this.Info.Color3 = 0xF4AE05; break;
					//case "blue": this.Info.Color3 = 0xF4AE05; break;
					//case "red": this.Info.Color3 = 0xF4AE05; break;
				}
			}
		}

		/// <summary>
		/// Item based on item and entity id.
		/// </summary>
		/// <param name="itemId"></param>
		/// <param name="entityId"></param>
		public Item(int itemId, long entityId)
		{
			this.Init(itemId);
			this.EntityId = entityId;
		}

		/// <summary>
		/// New item based on existing item.
		/// </summary>
		/// <param name="baseItem"></param>
		public Item(Item baseItem)
		{
			this.Info = baseItem.Info;
			this.OptionInfo = baseItem.OptionInfo;
			this.Data = baseItem.Data;
			this.MetaData1 = new MabiDictionary(baseItem.MetaData1.ToString());
			this.MetaData2 = new MabiDictionary(baseItem.MetaData2.ToString());
			this.QuestId = baseItem.QuestId;

			this.SetNewEntityId();
		}

		/// <summary>
		/// Sets item id, initializes item and loads defaults.
		/// </summary>
		/// <param name="itemId"></param>
		private void Init(int itemId)
		{
			this.Info.Id = itemId;
			this.MetaData1 = new MabiDictionary();
			this.MetaData2 = new MabiDictionary();

			this.LoadDefault();
		}

		public override Position GetPosition()
		{
			return new Position(this.Info.X, this.Info.Y);
		}

		// Modifies position in inventory.
		public void Move(Pocket pocket, int x, int y)
		{
			this.Info.Pocket = pocket;
			this.Info.Region = 0;
			this.Info.X = x;
			this.Info.Y = y;
		}

		/// <summary>
		/// Modifies pocket and entity location in world.
		/// </summary>
		/// <param name="region"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void Move(int region, int x, int y)
		{
			this.Info.Pocket = Pocket.None;
			this.Info.Region = region;
			this.Info.X = x;
			this.Info.Y = y;
		}

		/// <summary>
		/// Sets entity id to a new, unused one.
		/// </summary>
		public void SetNewEntityId()
		{
			this.EntityId = Interlocked.Increment(ref _itemId);
		}

		/// <summary>
		/// Drops item in location with a new entity id.
		/// </summary>
		/// <param name="region"></param>
		/// <param name="pos"></param>
		public void Drop(Region region, Position pos)
		{
			var rnd = RandomProvider.Get();

			// Get random drop position
			var x = rnd.Next(pos.X - DropRadius, pos.X + DropRadius + 1);
			var y = rnd.Next(pos.Y - DropRadius, pos.Y + DropRadius + 1);

			//this.SetNewEntityId();
			this.Move(region.Id, x, y);
			this.DisappearTime = DateTime.Now.AddSeconds(Math.Max(60, (this.OptionInfo.Price / 100) * 60));

			region.AddItem(this);
		}

		/// <summary>
		/// Loads data.
		/// </summary>
		public void LoadDefault()
		{
			this.Data = AuraData.ItemDb.Find(this.Info.Id);
			if (this.Data != null)
			{
				this.Info.KnockCount = this.Data.KnockCount;
				this.OptionInfo.KnockCount = this.Data.KnockCount;

				this.OptionInfo.Durability = this.Data.Durability;
				this.OptionInfo.DurabilityMax = this.Data.Durability;
				this.OptionInfo.DurabilityOriginal = this.Data.Durability;
				this.OptionInfo.AttackMin = this.Data.AttackMin;
				this.OptionInfo.AttackMax = this.Data.AttackMax;
				this.OptionInfo.Balance = this.Data.Balance;
				this.OptionInfo.Critical = this.Data.Critical;
				this.OptionInfo.Defense = this.Data.Defense;
				this.OptionInfo.Protection = this.Data.Protection;
				this.OptionInfo.Price = this.Data.Price;
				this.OptionInfo.SellingPrice = this.Data.SellingPrice;
				this.OptionInfo.WeaponType = this.Data.WeaponType;
				this.OptionInfo.AttackSpeed = (AttackSpeed)this.Data.AttackSpeed;
				this.OptionInfo.EffectiveRange = this.Data.Range;

				var rand = RandomProvider.Get();
				this.Info.Color1 = AuraData.ColorMapDb.GetRandom(this.Data.ColorMap1, rand);
				this.Info.Color2 = AuraData.ColorMapDb.GetRandom(this.Data.ColorMap2, rand);
				this.Info.Color3 = AuraData.ColorMapDb.GetRandom(this.Data.ColorMap3, rand);

				if (this.Data.StackType != StackType.Sac && this.Info.Amount < 1)
					this.Info.Amount = 1;
			}
			else
			{
				Log.Warning("Item.LoadDefault: Item '{0}' couldn't be found in database.", this.Info.Id);
			}

			this.OptionInfo.Flag = 1;
		}
	}
}
