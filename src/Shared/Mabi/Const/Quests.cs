// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

namespace Aura.Shared.Mabi.Const
{
	public enum ObjectiveType : byte
	{
		Kill = 1,
		Collect = 2,
		Talk = 3,
		Deliver = 4,
		ReachRank = 9,
		ReachLevel = 15,
	}

	public enum RewardType : byte
	{
		Item = 1,
		Gold = 2,
		Exp = 3,
		ExplExp = 4,
		AP = 5,
		Skill = 8, // ?
	}

	public enum QuestType : byte
	{
		Deliver = 1,
		Normal = 2,
	}

	public enum PtjType
	{
		General = 0,
		GroceryStore = 1,
		Church = 2,
		CombatSchool = 3,
		MagicSchool = 4,
		HealersHouse = 5,
		Bank = 6,
		BlacksmithShop = 7,
		GeneralShop = 8,
		AdventurersAssociation = 9,
		ClothingShop = 10,
		Bookstore = 11,
		WeaponsShop = 12,
		Inn = 13,
		Stope = 14,
		Pub = 15,
		MusicShop = 16,
		FlowerShop = 17,
		TheAmazingRaceOfMabinogi = 18,
		Library = 19,
		//근면_어린이_대회 = 20,
		SecretWatchGuard = 21,
		SecretRoyalGuard = 22,
		SecretCombatInstructor = 23,
		SecretMagicInstructor = 24,
		SecretFlowerShop = 25,
		AlchemistHouse = 26,
		StreetArtist = 27,
		JoustingArena = 28,
		//타티스 = 29,
		//멘텀 = 30,
		//바스텟 = 31,
		//데이곤 = 32,
		PontiffsCourt = 33,
		Lighthouse = 34,
		PuppetShop = 35,
	}

	public enum RewardGroupType
	{
		Gold = 0,
		Exp = 1,
		Item = 2,
		Scroll = 3, // ?
	}

	/// <summary>
	/// Quest result, describes how much the player got or has to get done.
	/// </summary>
	public enum QuestResult : byte
	{
		Perfect = 0,
		Mid = 1,
		Low = 2,
		None = 99,
	}
}
