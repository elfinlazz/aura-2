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
		/// <summary>
		/// Radius in which the item is dropped in Drop().
		/// </summary>
		private const int DropRadius = 50;

		/// <summary>
		/// Maximum item experience (proficiency).
		/// </summary>
		private const int MaxProficiency = 101000;

		/// <summary>
		/// Maximum item experience (proficiency).
		/// </summary>
		private const int UncappedMaxProficiency = 251000;

		/// <summary>
		/// Unique item id that is increased for every new item.
		/// </summary>
		private static long _itemId = MabiId.TmpItems;

		/// <summary>
		/// Returns entity data type "Item".
		/// </summary>
		public override DataType DataType { get { return DataType.Item; } }

		/// <summary>
		/// Gets or sets the item's region, forwarding to Info.Region.
		/// </summary>
		public override int RegionId
		{
			get { return this.Info.Region; }
			set { this.Info.Region = value; }
		}

		/// <summary>
		/// Public item information
		/// </summary>
		public ItemInfo Info;

		/// <summary>
		/// Private item information
		/// </summary>
		public ItemOptionInfo OptionInfo;

		/// <summary>
		/// Aura database item data
		/// </summary>
		public ItemData Data { get; protected set; }

		/// <summary>
		/// Meta information 1
		/// </summary>
		public MabiDictionary MetaData1 { get; protected set; }

		/// <summary>
		/// Meta information 2
		/// </summary>
		public MabiDictionary MetaData2 { get; protected set; }

		/// <summary>
		/// Ego weapon information
		/// </summary>
		public EgoInfo EgoInfo { get; protected set; }

		/// <summary>
		/// Bank at which the item is currently lying around.
		/// </summary>
		public string Bank { get; set; }

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

		/// <summary>
		/// Quest id, used for quest items.
		/// </summary>
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
		/// Gets or sets item's durability, capping it at 0~DuraMax.
		/// </summary>
		public int Durability
		{
			get { return this.OptionInfo.Durability; }
			set { this.OptionInfo.Durability = Math2.Clamp(0, this.OptionInfo.DurabilityMax, value); }
		}

		/// <summary>
		/// Gets or sets item's experience (proficiency), capping it at 0~100.
		/// </summary>
		/// <remarks>
		/// Officially two fields are used, Experience and EP, EP being the points
		/// and Exp the value in the parentheses. But using only one value
		/// seems much easier. Makes it work more like Durability. This
		/// requires some fixing for the client though.
		/// </remarks>
		public int Proficiency
		{
			get { return this.OptionInfo.Experience + this.OptionInfo.EP * 1000; }
			set
			{
				var max = MaxProficiency;
				if (ChannelServer.Instance.Conf.World.UncapProficiency)
					max = UncappedMaxProficiency;

				var newValue = Math2.Clamp(0, max, value);
				if (newValue == max)
				{
					this.OptionInfo.Experience = 1000;
					this.OptionInfo.EP = (byte)((newValue / 1000) - 1);
				}
				else
				{
					this.OptionInfo.Experience = (short)(newValue % 1000);
					this.OptionInfo.EP = (byte)(newValue / 1000);
				}
			}
		}

		/// <summary>
		/// Returns true if item is a able to receive proficiency.
		/// </summary>
		public bool IsTrainableWeapon
		{
			get { return this.Data.WeaponType != 0; }
		}

		/// <summary>
		/// Gets or sets whether the item is displayed as new in inv.
		/// [190100, NA200 (2015-01-15)]
		/// </summary>
		public bool IsNew { get; set; }

		/// <summary>
		/// Returns true if item has "Blessed" flag.
		/// </summary>
		public bool IsBlessed { get { return ((this.OptionInfo.Flags & ItemFlags.Blessed) != 0); } }

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
			this.EgoInfo = baseItem.EgoInfo.Copy();

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
			this.EgoInfo = new EgoInfo();

			this.LoadDefault();
		}

		/// <summary>
		/// Returns new ego weapon.
		/// </summary>
		/// <param name="itemId"></param>
		/// <param name="name"></param>
		/// <param name="egoRace"></param>
		/// <returns></returns>
		public static Item CreateEgo(int itemId, EgoRace egoRace, string name)
		{
			var item = new Item(itemId);
			item.EgoInfo.Race = egoRace;
			item.EgoInfo.Name = name;
			item.Info.FigureB = 1;

			return item;
		}

		/// <summary>
		/// Returns item's position, based on Info.X and Y.
		/// </summary>
		/// <returns></returns>
		public override Position GetPosition()
		{
			return new Position(this.Info.X, this.Info.Y);
		}

		/// <summary>
		/// Modifies position in inventory.
		/// </summary>
		/// <param name="pocket"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
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
		/// Loads default item information from data.
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
				this.OptionInfo.InjuryMin = this.Data.InjuryMin;
				this.OptionInfo.InjuryMax = this.Data.InjuryMax;
				this.OptionInfo.Balance = this.Data.Balance;
				this.OptionInfo.Critical = this.Data.Critical;
				this.OptionInfo.Defense = this.Data.Defense;
				this.OptionInfo.Protection = this.Data.Protection;
				this.OptionInfo.Price = this.Data.Price;
				this.OptionInfo.SellingPrice = this.Data.SellingPrice;
				this.OptionInfo.WeaponType = this.Data.WeaponType;
				this.OptionInfo.AttackSpeed = (AttackSpeed)this.Data.AttackSpeed;
				this.OptionInfo.EffectiveRange = this.Data.Range;
				this.OptionInfo.UpgradeMax = (byte)this.Data.MaxUpgrades;

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

			this.OptionInfo.Flags = ItemFlags.Unknown;
		}

		/// <summary>
		/// Returns repair cost for the given rate.
		/// </summary>
		/// <param name="repairRate">90~100%</param>
		/// <param name="points">Amount of points to repair, set to 0 to calculate the missing durability points.</param>
		/// <returns></returns>
		public int GetRepairCost(int repairRate, int points)
		{
			var price = 0f;
			var rate = 1f;
			var val = 1000000;

			if (points == 0)
				points = (int)Math.Floor((this.OptionInfo.DurabilityMax - this.OptionInfo.Durability) / 1000f);

			if (this.Data.HasTag("/weapon/edged/") ||
				this.Data.HasTag("/weapon/bow01/") ||
				this.Data.HasTag("/weapon/axe/") ||
				this.Data.HasTag("/weapon/bow/") ||
				this.Data.HasTag("/weapon/blunt/") ||
				this.Data.HasTag("/weapon/crossbow/") ||
				this.Data.HasTag("/weapon/wood/") ||
				this.Data.HasTag("/weapon/knuckle/") ||
				this.Data.HasTag("/weapon/atlatl/") ||
				this.Data.HasTag("/weapon/inverse_transmutator/") ||
				this.Data.HasTag("/weapon/cylinder_turret/") ||
				this.Data.HasTag("/weapon/lance/") ||
				this.Data.HasTag("/weapon/scythe/") ||
				this.Data.HasTag("/weapon/pillow/") ||
				this.Data.HasTag("/weapon/handle/") ||
				this.Data.HasTag("/weapon/dreamcatcher/") ||
				this.Data.HasTag("/weapon/gun/")
			)
			{
				switch (repairRate)
				{
					case 090: val = 100; rate = 0.005f; break;
					case 091: val = 150; rate = 0.010f; break;
					case 092: val = 200; rate = 0.015f; break;
					case 093: val = 250; rate = 0.020f; break;
					case 094: val = 300; rate = 0.025f; break;
					case 095: val = 350; rate = 0.050f; break;
					case 096: val = 400; rate = 0.070f; break;
					case 097: val = 450; rate = 0.100f; break;
					case 098: val = 500; rate = 0.130f; break;
					case 099: val = 550; rate = 0.300f; break;
					case 100: val = 700; rate = 1.000f; break;
				}
			}
			else if (
				this.Data.HasTag("/tool/") ||
				this.Data.HasTag("/shield/") ||
				this.Data.HasTag("/heulwen_tool/") ||
				this.Data.HasTag("/thunderstruck_oak_staff/")
			)
			{
				switch (repairRate)
				{
					case 090: val = 100; rate = 0.005f; break;
					case 091: val = 150; rate = 0.010f; break;
					case 092: val = 200; rate = 0.015f; break;
					case 093: val = 250; rate = 0.020f; break;
					case 094: val = 300; rate = 0.025f; break;
					case 095: val = 350; rate = 0.030f; break;
					case 096: val = 400; rate = 0.035f; break;
					case 097: val = 450; rate = 0.050f; break;
					case 098: val = 500; rate = 0.070f; break;
					case 099: val = 550; rate = 0.100f; break;
					case 100: val = 700; rate = 0.140f; break;
				}
			}
			else if (this.Data.HasTag("/weapon/") && (this.Data.HasTag("/wand/") || this.Data.HasTag("/staff/")))
			{
				switch (repairRate)
				{
					case 090: val = 0100; rate = 0.01f; break;
					case 091: val = 0200; rate = 0.02f; break;
					case 092: val = 0300; rate = 0.03f; break;
					case 093: val = 0400; rate = 0.04f; break;
					case 094: val = 0500; rate = 0.05f; break;
					case 095: val = 0600; rate = 0.06f; break;
					case 096: val = 0700; rate = 0.07f; break;
					case 097: val = 0800; rate = 0.08f; break;
					case 098: val = 1000; rate = 0.09f; break;
					case 099: val = 1200; rate = 0.10f; break;
					case 100: val = 1500; rate = 0.15f; break;
				}
			}
			else if (
				this.Data.HasTag("/armor/cloth/") ||
				this.Data.HasTag("/hand/glove/") ||
				this.Data.HasTag("/hand/bracelet/") ||
				this.Data.HasTag("/foot/shoes/") ||
				this.Data.HasTag("/head/headgear/") ||
				this.Data.HasTag("/robe/") ||
				this.Data.HasTag("/agelimit_robe/") ||
				this.Data.HasTag("/agelimit_cloth/") ||
				this.Data.HasTag("/pouch/bag/") ||
				this.Data.HasTag("/agelimit_glove/") ||
				this.Data.HasTag("/agelimit_shoes/") ||
				this.Data.HasTag("/wing/")
			)
			{
				switch (repairRate)
				{
					case 090: val = 100; rate = 0.0005f; break;
					case 091: val = 110; rate = 0.0010f; break;
					case 092: val = 120; rate = 0.0015f; break;
					case 093: val = 130; rate = 0.0020f; break;
					case 094: val = 140; rate = 0.0025f; break;
					case 095: val = 150; rate = 0.0030f; break;
					case 096: val = 160; rate = 0.0035f; break;
					case 097: val = 170; rate = 0.0040f; break;
					case 098: val = 200; rate = 0.0050f; break;
					case 099: val = 300; rate = 0.0060f; break;
					case 100: val = 500; rate = 0.0100f; break;
				}
			}
			else if (
				this.Data.HasTag("/hand/gauntlet/") ||
				this.Data.HasTag("/agelimit_gauntlet/")
			)
			{
				switch (repairRate)
				{
					case 090: val = 0200; rate = 0.0010f; break;
					case 091: val = 0300; rate = 0.0015f; break;
					case 092: val = 0400; rate = 0.0020f; break;
					case 093: val = 0500; rate = 0.0025f; break;
					case 094: val = 0600; rate = 0.0030f; break;
					case 095: val = 0700; rate = 0.0035f; break;
					case 096: val = 0800; rate = 0.0040f; break;
					case 097: val = 0900; rate = 0.0050f; break;
					case 098: val = 1000; rate = 0.0070f; break;
					case 099: val = 1500; rate = 0.0100f; break;
					case 100: val = 2000; rate = 0.0150f; break;
				}
			}
			else if (
				this.Data.HasTag("/foot/armorboots/") ||
				this.Data.HasTag("/head/helmet/") ||
				this.Data.HasTag("/agelimit_armorboots/")
			)
			{
				switch (repairRate)
				{
					case 090: val = 0400; rate = 0.0010f; break;
					case 091: val = 0600; rate = 0.0015f; break;
					case 092: val = 0800; rate = 0.0020f; break;
					case 093: val = 1000; rate = 0.0025f; break;
					case 094: val = 1500; rate = 0.0030f; break;
					case 095: val = 2000; rate = 0.0035f; break;
					case 096: val = 2500; rate = 0.0040f; break;
					case 097: val = 3000; rate = 0.0060f; break;
					case 098: val = 4000; rate = 0.0090f; break;
					case 099: val = 5000; rate = 0.0150f; break;
					case 100: val = 7000; rate = 0.0200f; break;
				}
			}
			else if (this.Data.HasTag("/armor/lightarmor/"))
			{
				switch (repairRate)
				{
					case 090: val = 0200; rate = 0.0005f; break;
					case 091: val = 0220; rate = 0.0010f; break;
					case 092: val = 0240; rate = 0.0015f; break;
					case 093: val = 0260; rate = 0.0020f; break;
					case 094: val = 0280; rate = 0.0025f; break;
					case 095: val = 0300; rate = 0.0030f; break;
					case 096: val = 0320; rate = 0.0035f; break;
					case 097: val = 0340; rate = 0.0040f; break;
					case 098: val = 0400; rate = 0.0050f; break;
					case 099: val = 0600; rate = 0.0060f; break;
					case 100: val = 1000; rate = 0.0100f; break;
				}
			}
			else if (this.Data.HasTag("/armor/heavyarmor/"))
			{
				switch (repairRate)
				{
					case 090: val = 0700; rate = 0.0005f; break;
					case 091: val = 0770; rate = 0.0010f; break;
					case 092: val = 0840; rate = 0.0015f; break;
					case 093: val = 0910; rate = 0.0020f; break;
					case 094: val = 0980; rate = 0.0025f; break;
					case 095: val = 1050; rate = 0.0030f; break;
					case 096: val = 1120; rate = 0.0035f; break;
					case 097: val = 1190; rate = 0.0040f; break;
					case 098: val = 1400; rate = 0.0050f; break;
					case 099: val = 2100; rate = 0.0060f; break;
					case 100: val = 3500; rate = 0.0100f; break;
				}
			}
			else if (this.Data.HasTag("/equip/accessary/") || this.Data.HasTag("/install_instrument/"))
			{
				switch (repairRate)
				{
					case 090: val = 0100; rate = 0.150f; break;
					case 091: val = 0200; rate = 0.152f; break;
					case 092: val = 0300; rate = 0.154f; break;
					case 093: val = 0400; rate = 0.156f; break;
					case 094: val = 0500; rate = 0.158f; break;
					case 095: val = 0600; rate = 0.160f; break;
					case 096: val = 0700; rate = 0.165f; break;
					case 097: val = 0800; rate = 0.170f; break;
					case 098: val = 0900; rate = 0.200f; break;
					case 099: val = 1000; rate = 0.250f; break;
					case 100: val = 1500; rate = 0.300f; break;
				}
			}
			else if (this.Data.HasTag("/falias_treasure/"))
				return 30000;

			price = this.OptionInfo.Price * rate;
			price += (price <= val ? price : val);

			var duraPoints = Math.Max(5, this.OptionInfo.DurabilityOriginal / 1000);

			var result = (int)(5.0 / Math.Sqrt(duraPoints) * price);
			if (result == 0)
				result = 1;

			// TODO: modifiers

			return result * points;
		}

		/// <summary>
		///  Returns true if item's data has the tag.
		/// </summary>
		/// <param name="tag"></param>
		/// <returns></returns>
		public override bool HasTag(string tag)
		{
			if (this.Data == null)
				return false;

			return this.Data.HasTag(tag);
		}
	}
}
