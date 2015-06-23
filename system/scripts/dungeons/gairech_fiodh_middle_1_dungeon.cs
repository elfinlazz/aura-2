//--- Aura Script -----------------------------------------------------------
// Fiodh Int 1 Dungeon
//--- Description -----------------------------------------------------------
// Script for Fiodh Intermediate for One.
//---------------------------------------------------------------------------

[DungeonScript("gairech_fiodh_middle_1_dungeon")]
public class FiodhIntOneDungeonScript : DungeonScript
{
	public override void OnBoss(Dungeon dungeon)
	{
		dungeon.AddBoss(130004, 1); // Small Golem
		dungeon.AddBoss(160102, 6); // Green Gremlin

		foreach (var member in dungeon.Party)
		{
			var cutscene = new Cutscene("bossroom_small_golem", member);
			cutscene.AddActor("player0", member);
			cutscene.AddActor("#small_golem", 130004);
			cutscene.AddActor("#imp", 10601);
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
				// Cooking Knife
				int prefix = 0, suffix = 0;
				switch (rnd.Next(2))
				{
					case 0: prefix = 20206; break; // Nervous
					case 1: prefix = 20711; break; // Famous
				}
				treasureChest.Add(Item.CreateEnchanted(40042, prefix, suffix));
			}

			treasureChest.AddGold(rnd.Next(2880, 4380)); // Gold
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
			drops.Add(new DropData(itemId: 62004, chance: 2, amountMin: 2, amountMax: 4)); // Magic Powder
			drops.Add(new DropData(itemId: 51102, chance: 2, amountMin: 2, amountMax: 4)); // Mana Herb
			drops.Add(new DropData(itemId: 51003, chance: 2, amountMin: 2, amountMax: 4)); // HP 50 Potion
			drops.Add(new DropData(itemId: 51008, chance: 2, amountMin: 2, amountMax: 4)); // MP 50 Potion
			drops.Add(new DropData(itemId: 51013, chance: 1, amountMin: 2, amountMax: 4)); // Stamina 50 Potion
			drops.Add(new DropData(itemId: 63119, chance: 4, amount: 1, expires: 480)); // Fiodh Intermediate Fomor Pass for One
			drops.Add(new DropData(itemId: 63120, chance: 3, amount: 1, expires: 480)); // Fiodh Intermediate Fomor Pass for Two
			drops.Add(new DropData(itemId: 63121, chance: 2, amount: 1, expires: 480)); // Fiodh Intermediate Fomor Pass for Four

			if (IsEnabled("FiodhAdvanced"))
				drops.Add(new DropData(itemId: 63253, chance: 2, amount: 1, expires: 480)); // Fiodh Advanced Fomor Pass
		}

		return Item.GetRandomDrop(rnd, drops);
	}
}
