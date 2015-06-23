//--- Aura Script -----------------------------------------------------------
// Ciar Dungeon
//--- Description -----------------------------------------------------------
// Ciar router and script for Ciar Normal.
//---------------------------------------------------------------------------

[DungeonScript("tircho_ciar_dungeon")]
public class CiarDungeonScript : DungeonScript
{
	public override bool Route(Creature creature, Item item, ref string dungeonName)
	{
		// Ciar Beginner
		if (item.Info.Id == 63139) // Ciar Beginner Pass
		{
			dungeonName = "tircho_ciar_beginner_1_dungeon";
			return true;
		}

		// Ciar Basic
		if (item.Info.Id == 63104) // Ciar Basic Fomor Pass
		{
			dungeonName = "tircho_ciar_low_dungeon";
			return true;
		}

		// Ciar Int 1
		if (item.Info.Id == 63123) // Ciar Intermediate Fomor Pass for One
		{
			dungeonName = "tircho_ciar_middle_1_dungeon";
			return true;
		}

		// Ciar Int 2
		// TODO: Party check
		if (item.Info.Id == 63124) // Ciar Intermediate Fomor Pass for Two
		{
			dungeonName = "tircho_ciar_middle_2_dungeon";
			return true;
		}

		// Ciar Int 4
		// TODO: Party check
		if (item.Info.Id == 63125) // Ciar Intermediate Fomor Pass for Four
		{
			dungeonName = "tircho_ciar_middle_4_dungeon";
			return true;
		}

		// tircho_ciar_dungeon
		return true;
	}

	public override void OnCreation(Dungeon dungeon)
	{
	}

	public override void OnBoss(Dungeon dungeon)
	{
		dungeon.AddBoss(130001, 1); // Golem
		dungeon.AddBoss(11003, 6); // Metal Skeleton

		foreach (var member in dungeon.Party)
		{
			var cutscene = new Cutscene("bossroom_Metalskeleton_Golem", member);
			cutscene.AddActor("player0", member);
			cutscene.AddActor("#golem", 130001);
			cutscene.AddActor("#metal_skeleton", 11003);
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
				// Broad Stick
				int prefix = 0, suffix = 0;
				switch (rnd.Next(4))
				{
					case 0: prefix = 207; break; // Fox
					case 1: prefix = 306; break; // Sharp
					case 2: prefix = 303; break; // Rusty
					case 3: prefix = 7; break; // Fox Hunter's
				}
				switch (rnd.Next(3))
				{
					case 0: suffix = 11106; break; // Blood
					case 1: suffix = 10806; break; // Understanding
					case 2: suffix = 10704; break; // Slug
				}
				treasureChest.Add(Item.CreateEnchanted(40019, prefix, suffix));
			}

			treasureChest.AddGold(rnd.Next(979, 2400)); // Gold
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
			drops.Add(new DropData(itemId: 62004, chance: 15, amountMin: 1, amountMax: 2)); // Magic Powder
			drops.Add(new DropData(itemId: 51102, chance: 15, amountMin: 1, amountMax: 2)); // Mana Herb
			drops.Add(new DropData(itemId: 51003, chance: 15, amountMin: 1, amountMax: 2)); // HP 50 Potion
			drops.Add(new DropData(itemId: 51008, chance: 15, amountMin: 1, amountMax: 2)); // MP 50 Potion
			drops.Add(new DropData(itemId: 51013, chance: 15, amountMin: 1, amountMax: 2)); // Stamina 50 Potion
			drops.Add(new DropData(itemId: 71037, chance: 4, amountMin: 2, amountMax: 4)); // Goblin Fomor Scroll
			drops.Add(new DropData(itemId: 71035, chance: 4, amountMin: 3, amountMax: 5)); // Gray Town Rat Fomor Scroll
			drops.Add(new DropData(itemId: 63104, chance: 3, amount: 1, expires: 480)); // Ciar Basic Fomor Pass
			drops.Add(new DropData(itemId: 63123, chance: 2, amount: 1, expires: 480)); // Ciar Intermediate Fomor Pass for One
			drops.Add(new DropData(itemId: 63124, chance: 2, amount: 1, expires: 480)); // Ciar Intermediate Fomor Pass for Two
			drops.Add(new DropData(itemId: 63125, chance: 2, amount: 1, expires: 480)); // Ciar Intermediate Fomor Pass for Four
			drops.Add(new DropData(itemId: 40006, chance: 2, amount: 1, color1: 0xFFDB60, durability: 0)); // Dagger (gold)

			if (IsEnabled("CiarAdvanced"))
			{
				drops.Add(new DropData(itemId: 63136, chance: 2, amount: 1, expires: 360)); // Ciar Adv. Fomor Pass for 2
				drops.Add(new DropData(itemId: 63137, chance: 2, amount: 1, expires: 360)); // Ciar Adv. Fomor Pass for 3
				drops.Add(new DropData(itemId: 63138, chance: 2, amount: 1, expires: 360)); // Ciar Adv. Fomor Pass
			}
		}

		return Item.GetRandomDrop(rnd, drops);
	}
}
