//--- Aura Script -----------------------------------------------------------
// Rabbie Dungeon
//--- Description -----------------------------------------------------------
// Rabbie router and script for Rabbie Normal.
//---------------------------------------------------------------------------

[DungeonScript("dunbarton_rabbie_dungeon")]
public class RabbieDungeonScript : DungeonScript
{
	public override bool Route(Creature creature, Item item, ref string dungeonName)
	{
		// Rabbie Basic
		if (item.Info.Id == 63110) // Rabbie Basic Fomor Pass
		{
			dungeonName = "dunbarton_rabbie_low_dungeon";
			return true;
		}

		// dunbarton_rabbie_dungeon
		return true;
	}

	public override void OnBoss(Dungeon dungeon)
	{
		if (dungeon.CountPlayers() == 1)
		{
			dungeon.AddBoss(10301, 1); // Black Succubus
		}
		else
		{
			dungeon.AddBoss(10101, 1); // Goblin

			foreach (var member in dungeon.Party)
			{
				var cutscene = new Cutscene("bossroom_GoldGoblin", member);
				cutscene.AddActor("me", member);
				cutscene.AddActor("#gold_goblin", 10104);
				cutscene.AddActor("#goblin_archer", 10103);
				cutscene.Play();
			}
		}
	}

	public override void OnBossDeath(Dungeon dungeon, Creature boss, Creature killer)
	{
		if (boss.RaceId != 10101)
			return;

		dungeon.AddBoss(10104, 12); // Gold Goblin
		dungeon.AddBoss(10103, 6); // Goblin Archer
	}

	public override void OnCleared(Dungeon dungeon)
	{
		var rnd = RandomProvider.Get();

		if (dungeon.CountPlayers() == 1)
		{
			var member = dungeon.Party[0];
			var treasureChest = new TreasureChest();

			// Bracelet
			int prefix = 0, suffix = 0;
			switch (rnd.Next(3))
			{
				case 0: prefix = 206; break; // Snake
				case 1: prefix = 305; break; // Fine
				case 2: prefix = 303; break; // Rusty
			}
			switch (rnd.Next(3))
			{
				case 0: suffix = 10504; break; // Topaz
				case 1: suffix = 10605; break; // Soldier
				case 2: suffix = 11206; break; // Fountain
			}
			treasureChest.Add(Item.CreateEnchanted(16015, prefix, suffix));

			treasureChest.Add(Item.Create(id: 2000, amountMin: 570, amountMax: 2520)); // Gold
			treasureChest.Add(GetRandomTreasureItem(rnd)); // Random item

			dungeon.AddChest(treasureChest);

			member.GiveItemWithEffect(Item.CreateKey(70028, "chest"));
		}
		else
		{
			for (int i = 0; i < dungeon.Party.Count; ++i)
			{
				var member = dungeon.Party[i];
				var treasureChest = new TreasureChest();

				if (i == 0)
				{
					// Bracelet
					int prefix = 0, suffix = 0;
					switch (rnd.Next(3))
					{
						case 0: suffix = 10504; break; // Topaz
						case 1: suffix = 10605; break; // Soldier
						case 2: suffix = 11205; break; // Water
					}
					treasureChest.Add(Item.CreateEnchanted(16015, prefix, suffix));
				}

				treasureChest.Add(Item.Create(id: 2000, amountMin: 896, amountMax: 3600)); // Gold
				treasureChest.Add(GetRandomTreasureItem(rnd)); // Random item

				dungeon.AddChest(treasureChest);

				member.GiveItemWithEffect(Item.CreateKey(70028, "chest"));
			}
		}
	}

	List<DropData> drops;
	public Item GetRandomTreasureItem(Random rnd)
	{
		if (drops == null)
		{
			drops = new List<DropData>();
			drops.Add(new DropData(itemId: 62004, chance: 15, amountMin: 1, amountMax: 2)); // Magic Powder
			drops.Add(new DropData(itemId: 51102, chance: 15, amountMin: 1, amountMax: 2)); // Mana Herb
			drops.Add(new DropData(itemId: 51003, chance: 15, amountMin: 1, amountMax: 2)); // HP 50 Potion
			drops.Add(new DropData(itemId: 51008, chance: 15, amountMin: 1, amountMax: 2)); // MP 50 Potion
			drops.Add(new DropData(itemId: 51013, chance: 15, amountMin: 1, amountMax: 2)); // Stamina 50 Potion
			drops.Add(new DropData(itemId: 71006, chance: 1, amountMin: 1, amountMax: 2)); // Skeleton Fomor Scroll
			drops.Add(new DropData(itemId: 71007, chance: 1, amountMin: 1, amountMax: 1)); // Red Skeleton Fomor Scroll
			drops.Add(new DropData(itemId: 71008, chance: 1, amountMin: 1, amountMax: 1)); // Metal Skeleton Fomor Scroll
			drops.Add(new DropData(itemId: 71035, chance: 3, amountMin: 3, amountMax: 5)); // Gray Town Rat Fomor Scroll
			drops.Add(new DropData(itemId: 63110, chance: 8, amount: 1, expires: 600)); // Rabbie Basic Fomor Pass
			drops.Add(new DropData(itemId: 40005, chance: 1, amount: 1, color1: 0xFFE760, durability: 0)); // Short Sword (gold)

			if (IsEnabled("RabbieAdvanced"))
			{
				drops.Add(new DropData(itemId: 63141, chance: 3, amount: 1, expires: 360)); // Rabbie Adv. Fomor Pass for 2
				drops.Add(new DropData(itemId: 63142, chance: 5, amount: 1, expires: 360)); // Rabbie Adv. Fomor Pass for 3
				drops.Add(new DropData(itemId: 63143, chance: 2, amount: 1, expires: 360)); // Rabbie Adv. Fomor Pass
			}
		}

		return Item.GetRandomDrop(rnd, drops);
	}
}
