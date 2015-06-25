// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Aura.Mabi.Const;

namespace Aura.Data.Database
{
	[Serializable]
	public class ItemData : TaggableData
	{
		public int Id { get; set; }

		public string Name { get; set; }
		public string KorName { get; set; }

		public ItemType Type { get; set; }

		/// <summary>
		/// Specifies whether an item is consumed upon use.
		/// </summary>
		public bool Consumed { get; set; }

		public byte Width { get; set; }
		public byte Height { get; set; }

		public byte ColorMap1 { get; set; }
		public byte ColorMap2 { get; set; }
		public byte ColorMap3 { get; set; }

		public StackType StackType { get; set; }
		public ushort StackMax { get; set; }
		public int StackItem { get; set; }
		public int Price { get; set; }
		public int SellingPrice { get; set; }
		public int Durability;

		public int Defense { get; set; }
		public short Protection { get; set; }

		public byte WeaponType { get; set; }
		public InstrumentType InstrumentType { get; set; }
		public int MaxUpgrades { get; set; }

		public short Range { get; set; }
		public ushort AttackMin { get; set; }
		public ushort AttackMax { get; set; }
		public byte InjuryMin { get; set; }
		public byte InjuryMax { get; set; }
		public sbyte Critical { get; set; }
		public byte Balance { get; set; }
		public AttackSpeed AttackSpeed { get; set; }
		public byte KnockCount { get; set; }
		public float SplashRadius { get; set; }
		public float SplashAngle { get; set; }
		public float SplashDamage { get; set; }
		public float StaminaUsage { get; set; }

		public int BagWidth { get; set; }
		public int BagHeight { get; set; }

		public int BaseSize { get; set; }

		public ItemDataTaste Taste { get; set; }

		public string OnUse { get; set; }
		public string OnEquip { get; set; }
		public string OnUnequip { get; set; }
		public string OnCreation { get; set; }
	}

	public class ItemDataTaste
	{
		public int Beauty { get; set; }
		public int Individuality { get; set; }
		public int Luxury { get; set; }
		public int Toughness { get; set; }
		public int Utility { get; set; }
		public int Rarity { get; set; }
		public int Meaning { get; set; }
		public int Adult { get; set; }
		public int Maniac { get; set; }
		public int Anime { get; set; }
		public int Sexy { get; set; }
	}

	/// <summary>
	/// Item database, indexed by item id.
	/// </summary>
	public class ItemDb : DatabaseJsonIndexed<int, ItemData>
	{
		public ItemData Find(string name)
		{
			name = name.ToLower();
			return this.Entries.FirstOrDefault(a => a.Value.Name.ToLower() == name).Value;
		}

		public List<ItemData> FindAll(string name)
		{
			name = name.ToLower();
			return this.Entries.FindAll(a => a.Value.Name.ToLower().Contains(name));
		}

		protected override void ReadEntry(JObject entry)
		{
			entry.AssertNotMissing("id", "name", "originalName", "tags", "type", "width", "height", "price");

			var info = new ItemData();
			info.Id = entry.ReadInt("id");

			info.Name = entry.ReadString("name");
			info.KorName = entry.ReadString("originalName");
			info.Tags = entry.ReadString("tags");
			info.Type = (ItemType)entry.ReadInt("type");
			info.StackType = (StackType)entry.ReadInt("stackType");
			info.StackMax = entry.ReadUShort("stackMax", 1);

			if (info.StackMax < 1)
				info.StackMax = 1;

			info.StackItem = entry.ReadInt("stackItem");

			info.Consumed = entry.ReadBool("consumed");
			info.Width = entry.ReadByte("width");
			info.Height = entry.ReadByte("height");
			info.ColorMap1 = entry.ReadByte("colorMap1");
			info.ColorMap2 = entry.ReadByte("colorMap2");
			info.ColorMap3 = entry.ReadByte("colorMap3");
			info.Price = entry.ReadInt("price");
			info.SellingPrice = (info.Id != 2000 ? (int)(info.Price * 0.1f) : 1000);
			info.Durability = entry.ReadInt("durability");
			info.Defense = entry.ReadInt("defense");
			info.Protection = entry.ReadShort("protection");
			info.InstrumentType = (InstrumentType)entry.ReadInt("instrumentType");
			info.MaxUpgrades = entry.ReadInt("maxUpgrades");

			info.WeaponType = entry.ReadByte("weaponType");
			if (info.WeaponType != 0)
			{
				info.Range = entry.ReadShort("range");
				info.AttackMin = entry.ReadUShort("attackMin");
				info.AttackMax = entry.ReadUShort("attackMax");
				info.InjuryMin = entry.ReadByte("injuryMin");
				info.InjuryMax = entry.ReadByte("injuryMax");
				info.Critical = entry.ReadSByte("critical");
				info.Balance = entry.ReadByte("balance");
				info.AttackSpeed = (AttackSpeed)entry.ReadByte("attackSpeed");
				info.KnockCount = entry.ReadByte("knockCount");
				info.SplashRadius = entry.ReadFloat("splashRadius");
				info.SplashAngle = entry.ReadFloat("splashAngle");
				info.SplashDamage = entry.ReadFloat("splashDamage");
				info.StaminaUsage = entry.ReadFloat("splashUsage");
			}

			info.BagWidth = entry.ReadInt("bagWidth");
			info.BagHeight = entry.ReadInt("bagHeight");

			info.BaseSize = entry.ReadInt("baseSize");

			info.Taste = new ItemDataTaste();
			if (entry.ContainsKeys("taste"))
			{
				var taste = entry["taste"] as JObject;

				info.Taste.Beauty = taste.ReadInt("beauty");
				info.Taste.Individuality = taste.ReadInt("individuality");
				info.Taste.Luxury = taste.ReadInt("luxury");
				info.Taste.Toughness = taste.ReadInt("toughness");
				info.Taste.Utility = taste.ReadInt("utility");
				info.Taste.Rarity = taste.ReadInt("rarity");
				info.Taste.Meaning = taste.ReadInt("meaning");
				info.Taste.Adult = taste.ReadInt("adult");
				info.Taste.Maniac = taste.ReadInt("maniac");
				info.Taste.Anime = taste.ReadInt("anime");
				info.Taste.Sexy = taste.ReadInt("sexy");
			}

			info.OnUse = entry.ReadString("onUse");
			info.OnEquip = entry.ReadString("onEquip");
			info.OnUnequip = entry.ReadString("onUnequip");
			info.OnCreation = entry.ReadString("onCreation");

			this.Entries[info.Id] = info;
		}
	}
}
