//--- Aura Script -----------------------------------------------------------
// Math Dungeon
//--- Description -----------------------------------------------------------
// Math router and script for Math Normal.
//---------------------------------------------------------------------------

[DungeonScript("dunbarton_math_dungeon")]
public class MathDungeonScript : DungeonScript
{
	public override bool Route(Creature creature, Item item, ref string dungeonName)
	{
		// dunbarton_math_dungeon
		return true;
	}

	public override void OnCreation(Dungeon dungeon)
	{
	}

	public override void OnBoss(Dungeon dungeon)
	{
		dungeon.AddBoss(20201, 3); // Hellhound

		foreach (var member in dungeon.Party)
		{
			var cutscene = new Cutscene("bossroom_HellHound", member);
			cutscene.AddActor("me", member);
			cutscene.AddActor("#hellhound", 20201);
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

			// Lute
			int prefix = 0, suffix = 0;
			switch (rnd.Next(3))
			{
				case 0: prefix = 4; break; // Donkey Hunter's
				case 1: prefix = 1506; break; // Swan Summoner's
				case 2: prefix = 1707; break; // Sturdy
			}
			switch (rnd.Next(3))
			{
				case 0: suffix = 10806; break; // Understanding
				case 1: suffix = 10504; break; // Topaz
				case 2: suffix = 10706; break; // Wind
			}
			switch (rnd.Next(3))
			{
				case 0: treasureChest.Add(Item.CreateEnchanted(40042, prefix, suffix)); break;
				case 1: treasureChest.Add(Item.CreateEnchanted(40042, prefix, 0)); break;
				case 2: treasureChest.Add(Item.CreateEnchanted(40042, 0, suffix)); break;
			}

			treasureChest.AddGold(rnd.Next(576, 2880)); // Gold
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
			drops.Add(new DropData(itemId: 62004, chance: 18, amountMin: 1, amountMax: 2)); // Magic Powder
			drops.Add(new DropData(itemId: 51102, chance: 18, amountMin: 1, amountMax: 2)); // Mana Herb
			drops.Add(new DropData(itemId: 51003, chance: 18, amountMin: 1, amountMax: 2)); // HP 50 Potion
			drops.Add(new DropData(itemId: 51008, chance: 18, amountMin: 1, amountMax: 2)); // MP 50 Potion
			drops.Add(new DropData(itemId: 51013, chance: 18, amountMin: 1, amountMax: 2)); // Stamina 50 Potion
			drops.Add(new DropData(itemId: 71018, chance: 3, amountMin: 3, amountMax: 5));  // Black Spider Fomor Scroll
			drops.Add(new DropData(itemId: 71035, chance: 3, amountMin: 3, amountMax: 5)); // Gray Town Rat Fomor Scroll
			drops.Add(new DropData(itemId: 40003, chance: 1, amount: 1, color1: 0x12644A)); // Short Bow (green)
			drops.Add(new DropData(itemId: 46001, chance: 2, amount: 1, color1: 0xEBBE21, durability: 0)); // Round Shield (gold)

			if (IsEnabled("MathAdvanced"))
				drops.Add(new DropData(itemId: 63131, chance: 1, amount: 1, expires: 480)); // Math Advanced
		}

		return Item.GetRandomDrop(rnd, drops);
	}
}
