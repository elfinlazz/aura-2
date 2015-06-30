//--- Aura Script -----------------------------------------------------------
// Malcolm's Ring quest dungeon
//--- Description -----------------------------------------------------------
// Player has to make it to the boss room and fight a Giant Golden
// Spiderling.
//---------------------------------------------------------------------------

[DungeonScript("tircho_alby_dungeon_tutorial_malcolm")]
public class AlbyTutorialMalcolmDungeonScript : DungeonScript
{
	public override void OnBoss(Dungeon dungeon)
	{
		dungeon.AddBoss(30802, 1); // Giant Golden Spiderling

		foreach (var member in dungeon.Party)
		{
			var cutscene = new Cutscene("bossroom_tutorial_giantgoldenspiderkid", member);
			cutscene.AddActor("player0", member);
			cutscene.AddActor("#giantgoldenspiderkid", 30802);
			cutscene.Play();
		}
	}

	public override void OnBossDeath(Dungeon dungeon, Creature boss, Creature killer)
	{
		killer.AcquireItem(Item.Create(75058)); // Malcolm's Ring
	}

	public override void OnCleared(Dungeon dungeon)
	{
		var rnd = RandomProvider.Get();

		for (int i = 0; i < dungeon.Party.Count; ++i)
		{
			var member = dungeon.Party[i];
			var treasureChest = new TreasureChest();

			treasureChest.AddGold(rnd.Next(153, 768)); // Gold
			treasureChest.Add(GetRandomTreasureItem(rnd)); // Random item

			dungeon.AddChest(treasureChest);

			member.GiveItemWithEffect(Item.CreateKey(70028, "chest"));
		}
	}

	public override void OnLeftEarly(Dungeon dungeon, Creature creature)
	{
		// Give pass again if dungeon is being left before the spider is killed
		if (creature.Quests.IsActive(202004, "kill_spider"))
			creature.Inventory.Add(63181); // Malcolm's Pass
	}

	List<DropData> drops;
	public Item GetRandomTreasureItem(Random rnd)
	{
		if (drops == null)
		{
			drops = new List<DropData>();
			drops.Add(new DropData(itemId: 51001, chance: 54, amountMin: 2, amountMax: 8)); // HP 10 Potion
			drops.Add(new DropData(itemId: 51102, chance: 20, amountMin: 1, amountMax: 2)); // Mana Herb
			drops.Add(new DropData(itemId: 71017, chance: 15, amountMin: 1, amountMax: 2)); // White Spider Fomor Scroll
			drops.Add(new DropData(itemId: 71019, chance: 10, amount: 1)); // Red Spider Fomor Scroll
			drops.Add(new DropData(itemId: 40002, chance: 1, amount: 1, color1: 0x000000, durability: 0)); // Wooden Blade (black)
		}

		return Item.GetRandomDrop(rnd, drops);
	}
}
