//--- Aura Script -----------------------------------------------------------
// Barri Dungeon
//--- Description -----------------------------------------------------------
// Barri router and script for Barri Normal.
//---------------------------------------------------------------------------

[DungeonScript("bangor_barri_dungeon")]
public class BarriDungeonScript : DungeonScript
{
	public override bool Route(Creature creature, Item item, ref string dungeonName)
	{
		// Barri Basic
		if (item.Info.Id == 63113) // Barri Basic Fomor Pass
		{
			dungeonName = "bangor_barri_low_dungeon";
			return true;
		}

		// bangor_barri_dungeon
		return true;
	}

	public override void OnBoss(Dungeon dungeon)
	{
		dungeon.AddBoss(100003, 1); // Ogre Warrior
		dungeon.AddBoss(100004, 1); // Ogre Warrior
		dungeon.AddBoss(100005, 1); // Ogre Warrior
		dungeon.AddBoss(100006, 1); // Ogre Warrior
		dungeon.AddBoss(100007, 1); // Ogre Warrior

		foreach (var member in dungeon.Party)
		{
			var cutscene = new Cutscene("bossroom_OgreBros", member);
			cutscene.AddActor("me", member);
			cutscene.AddActor("#ogrebros1", 100003);
			cutscene.AddActor("#ogrebros2", 100004);
			cutscene.AddActor("#ogrebros3", 100005);
			cutscene.AddActor("#ogrebros4", 100006);
			cutscene.AddActor("#ogrebros5", 100007);
			cutscene.AddActor("#goblin", 10101);
			cutscene.Play();
		}
	}

	public override void OnCleared(Dungeon dungeon)
	{
		var rnd = RandomProvider.Get();

		for (int i = 0; i < dungeon.Party.Count; ++i)
		{
			var member = dungeon.Party[i];
			var treasureChest = new TreasureChest();

			// Warhammer
			int prefix = 0, suffix = 0;
			switch (rnd.Next(4))
			{
				case 0: prefix = 1704; break; // Imitation
				case 1: prefix = 1705; break; // Cheap
				case 2: prefix = 1706; break; // Good
				case 3: prefix = 305; break;  // Fine
			}
			treasureChest.Add(Item.CreateEnchanted(40016, prefix, suffix));

			treasureChest.AddGold(rnd.Next(688, 1728)); // Gold
			treasureChest.Add(GetRandomTreasureItem(rnd)); // Random item

			dungeon.AddChest(treasureChest);

			member.GiveItemWithEffect(Item.CreateKey(70028, "chest"));
		}
	}

	List<DropData> drops;
	public Item GetRandomTreasureItem(Random rnd)
	{
		if (drops == null)
		{
			drops = new List<DropData>();
			drops.Add(new DropData(itemId: 62004, chance: 14, amountMin: 1, amountMax: 2)); // Magic Powder
			drops.Add(new DropData(itemId: 51102, chance: 15, amountMin: 1, amountMax: 2)); // Mana Herb
			drops.Add(new DropData(itemId: 51003, chance: 10, amountMin: 1, amountMax: 2)); // HP 50 Potion
			drops.Add(new DropData(itemId: 51008, chance: 10, amountMin: 1, amountMax: 2)); // MP 50 Potion
			drops.Add(new DropData(itemId: 51013, chance: 10, amountMin: 1, amountMax: 2)); // Stamina 50 Potion
			drops.Add(new DropData(itemId: 60039, chance: 10, amountMin: 3, amountMax: 5));  // Fomor Token
			drops.Add(new DropData(itemId: 71044, chance: 5, amountMin: 2, amountMax: 5));  // Imp Fomor Scroll
			drops.Add(new DropData(itemId: 71037, chance: 5, amountMin: 2, amountMax: 4));  // Goblin Fomor Scroll (officially Imp duplicate #officialFix)
			drops.Add(new DropData(itemId: 40025, chance: 1, amount: 1, color1: 0xC00010, durability: 0)); // Pickaxe (bronze)
			drops.Add(new DropData(itemId: 63113, chance: 5, amount: 1, expires: 600)); // Barri Basic

			if (IsEnabled("BarriAdvanced"))
			{
				drops.Add(new DropData(itemId: 63133, chance: 5, amount: 1, expires: 360)); // Barri Adv. Fomor Pass for 2
				drops.Add(new DropData(itemId: 63134, chance: 5, amount: 1, expires: 360)); // Barri Adv. Fomor Pass for 3
				drops.Add(new DropData(itemId: 63135, chance: 5, amount: 1, expires: 360)); // Barri Adv. Fomor Pass
			}
		}

		return Item.GetRandomDrop(rnd, drops);
	}
}
