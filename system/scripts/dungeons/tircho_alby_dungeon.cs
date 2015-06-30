//--- Aura Script -----------------------------------------------------------
// Alby Dungeon
//--- Description -----------------------------------------------------------
// Alby router and script for Alby Normal.
//---------------------------------------------------------------------------

[DungeonScript("tircho_alby_dungeon")]
public class AlbyDungeonScript : DungeonScript
{
	public override bool Route(Creature creature, Item item, ref string dungeonName)
	{
		// Access to bunny dungeon with a check worth at least 1m
		if (item.Info.Id == 2004 && item.MetaData1.GetInt("EVALUE") >= 1000000)
		{
			dungeonName = "tircho_alby_whiteday_dungeon";
			return true;
		}

		// Rescue Resident quest dungeon
		if (item.Info.Id == 63180) // Trefor's Pass
		{
			dungeonName = "tircho_alby_dungeon_tutorial_ranald";
			return true;
		}

		// Malcolm's Ring quest dungeon
		if (item.Info.Id == 63181) // Malcolm's Pass
		{
			dungeonName = "tircho_alby_dungeon_tutorial_malcolm";
			return true;
		}

		// Alby Beginner
		if (item.Info.Id == 63140) // Alby Beginner Pass
		{
			dungeonName = "tircho_alby_beginner_1_dungeon";
			return true;
		}

		// Alby Basic
		if (item.Info.Id == 63101) // Alby Basic Fomor Pass
		{
			dungeonName = "tircho_alby_low_dungeon";
			return true;
		}

		// Alby Int 1
		if (item.Info.Id == 63116) // Alby Intermediate Fomor Pass for One
		{
			dungeonName = "tircho_alby_middle_1_dungeon";
			return true;
		}

		// Alby Int 2
		// TODO: Party check
		if (item.Info.Id == 63117) // Alby Intermediate Fomor Pass for Two
		{
			dungeonName = "tircho_alby_middle_2_dungeon";
			return true;
		}

		// Alby Int 4
		// TODO: Party check
		if (item.Info.Id == 63118) // Alby Intermediate Fomor Pass for Four
		{
			dungeonName = "tircho_alby_middle_4_dungeon";
			return true;
		}

		// tircho_alby_dungeon
		return true;
	}

	public override void OnCreation(Dungeon dungeon)
	{
	}

	public override void OnBoss(Dungeon dungeon)
	{
		dungeon.AddBoss(30004, 1); // Giant Spider
		dungeon.AddBoss(30003, 6); // Red Spider

		foreach (var member in dungeon.Party)
		{
			var cutscene = new Cutscene("bossroom_GiantSpider", member);
			cutscene.AddActor("player0", member);
			cutscene.AddActor("#giant_spider", 30004);
			cutscene.AddActor("#darkred_spider", 30003);
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

			if (i == 0)
			{
				// Enchant
				var enchant = new Item(62005);
				switch (rnd.Next(3))
				{
					case 0: enchant.OptionInfo.Prefix = 1506; break; // Swan Summoner's
					case 1: enchant.OptionInfo.Prefix = 1706; break; // Good
					case 2: enchant.OptionInfo.Prefix = 305; break;  // Fine
				}
				treasureChest.Add(enchant);
			}

			treasureChest.AddGold(rnd.Next(153, 768)); // Gold
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
			drops.Add(new DropData(itemId: 62004, chance: 44, amountMin: 1, amountMax: 2)); // Magic Powder
			drops.Add(new DropData(itemId: 51102, chance: 44, amountMin: 1, amountMax: 2)); // Mana Herb
			drops.Add(new DropData(itemId: 71017, chance: 2, amountMin: 1, amountMax: 2));  // White Spider Fomor Scroll
			drops.Add(new DropData(itemId: 71019, chance: 2, amountMin: 1, amountMax: 1)); // Red Spider Fomor Scroll
			drops.Add(new DropData(itemId: 63116, chance: 1, amount: 1, expires: 480)); // Alby Int 1
			drops.Add(new DropData(itemId: 63117, chance: 1, amount: 1, expires: 480)); // Alby Int 2
			drops.Add(new DropData(itemId: 63118, chance: 1, amount: 1, expires: 480)); // Alby Int 4
			drops.Add(new DropData(itemId: 63101, chance: 2, amount: 1, expires: 480)); // Alby Basic
			drops.Add(new DropData(itemId: 40002, chance: 1, amount: 1, color1: 0x000000, durability: 0)); // Wooden Blade (black)

			if (IsEnabled("AlbyAdvanced"))
			{
				drops.Add(new DropData(itemId: 63160, chance: 1, amount: 1, expires: 360)); // Alby Advanced 3-person Fomor Pass
				drops.Add(new DropData(itemId: 63161, chance: 1, amount: 1, expires: 360)); // Alby Advanced Fomor Pass
			}
		}

		return Item.GetRandomDrop(rnd, drops);
	}
}
