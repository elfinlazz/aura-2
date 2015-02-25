// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;

namespace Aura.Shared.Mabi.Const
{
	/// <summary>
	/// Inventory pockets
	/// </summary>
	/// <remarks>
	/// Every item is stored in a specific pocket.
	/// </remarks>
	public enum Pocket : byte
	{
		None = 0,
		Cursor = 1,
		Inventory = 2,
		Face = 3,
		Hair = 4,
		Armor = 5,
		Glove = 6,
		Shoe = 7,
		Head = 8,
		Robe = 9,

		// Actual RIGHT hand (left side in inv).
		RightHand1 = 10,
		RightHand2 = 11,

		// Actual LEFT hand (right side in inv).
		LeftHand1 = 12,
		LeftHand2 = 13,

		// Arrows go here, not in the left hand.
		Magazine1 = 14,
		Magazine2 = 15,

		Accessory1 = 16,
		Accessory2 = 17,

		Trade = 19,
		Temporary = 20,
		Quests = 23,
		Trash = 24,
		BattleReward = 28,
		EnchantReward = 29,
		ManaCrystalReward = 30,
		Falias1 = 32,
		Falias2 = 33,
		Falias3 = 34,
		Falias4 = 35,
		ComboCard = 41,
		ArmorStyle = 43,
		GloveStyle = 44,
		ShoeStyle = 45,
		HeadStyle = 46,
		RobeStyle = 47,
		PersonalInventory = 49,
		VIPInventory = 50,
		FarmStone = 81,
		ItemBags = 100,
		ItemBagsMax = 199,
	}

	[Flags]
	public enum BagTags
	{
		Equipment = 0x01,
		RecoveryPotion = 0x02,
		Artifact = 0x04,
		AlchemyCrystal = 0x08,
		Herb = 0x10,
		ThreadBall = 0x20,
		Cloth = 0x40,
		Ore = 0x80,
		Gem = 0x100,
		CullinStone = 0x200,
		Firewood = 0x400,
		Fish = 0x800,
		Food = 0x1000,
		Enchants = 0x2000,
		Pass = 0x4000,
		FomorScroll = 0x8000,
		AncientBook = 0x10000,
	}

	/// <summary>
	/// Extensions for Pocket enum.
	/// </summary>
	public static class PocketExtensions
	{
		/// <summary>
		/// Returns true if pocket is an equipment pocket (incl Face and Hair).
		/// </summary>
		/// <param name="pocket"></param>
		/// <returns></returns>
		public static bool IsEquip(this Pocket pocket)
		{
			if ((pocket >= Pocket.Face && pocket <= Pocket.Accessory2) || (pocket >= Pocket.ArmorStyle && pocket <= Pocket.RobeStyle))
				return true;
			return false;
		}

		/// <summary>
		/// Returns true if pocket is between min and max bag.
		/// </summary>
		/// <param name="pocket"></param>
		/// <returns></returns>
		public static bool IsBag(this Pocket pocket)
		{
			return (pocket >= Pocket.ItemBags && pocket <= Pocket.ItemBagsMax);
		}
	}

	/// <summary>
	/// Attack speed of a weapon.
	/// </summary>
	/// <remarks>
	/// Used in ItemOptionInfo.
	/// </remarks>
	public enum AttackSpeed : byte
	{
		VeryFast,
		Fast,
		Normal,
		Slow,
		VerySlow,
	}

	/// <summary>
	/// ?
	/// </summary>
	/// <remarks>
	/// Attr_ActionFlag in item db.
	/// </remarks>
	public enum ItemActionFlag
	{
		NormalItem = 0,
		StaticItem = 1,
		ImportantItem = 2,
		AccountPersonalItem = 3,
		DungeonItem = 4, // Special weapons for dungeons?
		CharacterPersonalItem = 5, // Elsinore/Training Short Sword
		RegionFixedItem = 6, // Keys?
		BankBlockedItem = 7, // Gems?
		NewBagItem = 8, // Events?
		BankBlockedCharacterPersonalItem = 9,
		GuildItem = 10, // Guild Robe
		// 11
		NotDealItem = 12,
		Important2Item = 13, // Brionac
		TradeLimitItem = 14,
		// 15
		LordKeyItem = 16,
	}

	public enum EgoRace : byte
	{
		None = 0,
		SwordM = 1,
		SwordF = 2,
		BluntM = 3,
		BluntF = 4,
		WandM = 5,
		WandF = 6,
		BowM = 7,
		BowF = 8,
		EirySword = 9,
		EiryBow = 10,
		EiryAxe = 11,
		EiryLute = 12,
		EiryCylinder = 13,
		EiryWind = 14, // ?
		CylinderM = 15,
		CylinderF = 16,
	}

	[Flags]
	public enum ItemFlags : byte
	{
		Unknown = 0x01,
		// ? = 0x02,
		Blessed = 0x04,
		Incomplete = 0x08,
		// ? = 0x10, (adds "-only Item" text, server side gender restriction?)
		// ? = 0x20, (removes "(Original)" text?)
		// ? = 0x40,
		// ? = 0x80,
	}
}
