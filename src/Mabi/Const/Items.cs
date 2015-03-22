// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;

namespace Aura.Mabi.Const
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

	/// <summary>
	/// Item's type
	/// </summary>
	/// <remarks>
	/// Basically the "Attr_Type" value found in the client, however,
	/// we made some adjustments in Aura. Officially, Gold Pouches have
	/// type 5. In Aura they have type 1000, we turn them into Sacs for
	/// Gold items. This way we can treat them like any other Sac.
	/// Also, all items that official have type 1000, but an id > 10000
	/// were changed to type 1001, because they aren't sacs.
	/// This difference was important in Aura before we had support for tags.
	/// TODO: Utilize tags more?
	/// </remarks>
	public enum ItemType
	{
		Armor = 0,
		Headgear = 1,
		Glove = 2,
		Shoe = 3,
		Book = 4,
		Currency = 5,
		ItemBag = 6,
		Weapon = 7,
		Weapon2H = 8, // 2H, bows, tools, etc
		Weapon2 = 9, // Ineffective Weapons? Signs, etc.
		Instrument = 10,
		Shield = 11,
		Robe = 12,
		Accessory = 13,
		SecondaryWeapon = 14,
		MusicScroll = 15,
		Manual = 16,
		EnchantScroll = 17,
		CollectionBook = 18,
		ShopLicense = 19,
		FaliasTreasure = 20,
		Kiosk = 21,
		StyleArmor = 22,
		StyleHeadgear = 23,
		StyleGlove = 24,
		StyleShoe = 25,
		ComboCard = 27,
		Unknown2 = 28,
		Hair = 100,
		Face = 101,
		Usable = 501,
		Quest = 502,
		Usable2 = 503,
		Unknown1 = 504,
		Sac = 1000,
		Misc = 1001,
	}

	/// <summary>
	/// The way an item is stacked.
	/// </summary>
	public enum StackType
	{
		/// <summary>
		/// Single item (1, e.g. equipment)
		/// </summary>
		None = 0,

		/// <summary>
		/// Stackable item (1+, e.g. potions)
		/// </summary>
		Stackable = 1,

		/// <summary>
		/// Sac, containing items (0+, e.g. Gold Pouches)
		/// </summary>
		Sac = 2,
	}

	/// <summary>
	/// Type of an instrument item, used to specify the sound.
	/// </summary>
	public enum InstrumentType
	{
		Lute = 0,
		Ukulele = 1,
		Mandolin = 2,
		Whistle = 3,
		Roncadora = 4,
		Flute = 5,
		Chalumeau = 6,

		ToneBottleC = 7,
		ToneBottleD = 8,
		ToneBottleE = 9,
		ToneBottleF = 10,
		ToneBottleG = 11,
		ToneBottleB = 12,
		ToneBottleA = 13,

		Tuba = 18,
		Lyra = 19,
		ElectricGuitar = 20,

		Piano = 21,
		Violin = 22,
		Cello = 23,

		BassDrum = 66,
		Drum = 67,
		Cymbals = 68,

		HandbellC = 69,
		HandbellD = 70,
		HandbellE = 71,
		HandbellF = 72,
		HandbellG = 73,
		HandbellB = 74,
		HandbellA = 75,
		HandbellHighC = 76,

		Xylophone = 77,

		MaleVoiceKr1 = 81,
		MaleVoiceKr2 = 82,
		MaleVoiceKr3 = 83,
		MaleVoiceKr4 = 84,
		FemaleVoiceKr1 = 90,
		FemaleVoiceKr2 = 91,
		FemaleVoiceKr3 = 92,
		FemaleVoiceKr4 = 93,
		FemaleVoiceKr5 = 94,

		MaleChorusVoice = 100,
		FemaleChorusVoice = 110,

		MaleVoiceJp = 120,
		FemaleVoiceJp = 121,
	}
}
