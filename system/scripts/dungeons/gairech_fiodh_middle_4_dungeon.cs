//--- Aura Script -----------------------------------------------------------
// Fiodh Int 4 Dungeon
//--- Description -----------------------------------------------------------
// Script for Fiodh Intermediate for Four.
//---------------------------------------------------------------------------

[DungeonScript("gairech_fiodh_middle_4_dungeon")]
public class FiodhIntFourDungeonScript : DungeonScript
{
	public override void OnBoss(Dungeon dungeon)
	{
		dungeon.AddBoss(10602, 3); // Giant Imp

		foreach (var member in dungeon.Party)
		{
			var cutscene = new Cutscene("bossroom_imp", member);
			cutscene.AddActor("player0", member);
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
				switch (rnd.Next(3))
				{
					case 0: treasureChest.Add(Item.CreateEnchanted(40023, 20611, 0)); break; // Strict Gathering Knife
					case 1: treasureChest.Add(Item.CreateEnchanted(18006, 0, 30807)); break; // Prophet Wizard Hat
					case 2: treasureChest.Add(Item.CreateEnchanted(18012, 0, 30521)); break; // Fancy Tork Merchant Cap
				}
			}

			treasureChest.AddGold(rnd.Next(6630, 8670)); // Gold
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
			drops.Add(new DropData(itemId: 62004, chance: 4, amountMin: 2, amountMax: 4)); // Magic Powder
			drops.Add(new DropData(itemId: 51102, chance: 4, amountMin: 2, amountMax: 4)); // Mana Herb
			drops.Add(new DropData(itemId: 51003, chance: 5, amountMin: 2, amountMax: 4)); // HP 50 Potion
			drops.Add(new DropData(itemId: 51008, chance: 5, amountMin: 2, amountMax: 4)); // MP 50 Potion
			drops.Add(new DropData(itemId: 51013, chance: 5, amountMin: 2, amountMax: 4)); // Stamina 50 Potion
			drops.Add(new DropData(itemId: 63119, chance: 6, amount: 1, expires: 480)); // Fiodh Intermediate Fomor Pass for One
			drops.Add(new DropData(itemId: 63120, chance: 4, amount: 1, expires: 480)); // Fiodh Intermediate Fomor Pass for Two
			drops.Add(new DropData(itemId: 63121, chance: 3, amount: 1, expires: 480)); // Fiodh Intermediate Fomor Pass for Four

			if (IsEnabled("FiodhAdvanced"))
				drops.Add(new DropData(itemId: 63253, chance: 4, amount: 1, expires: 480)); // Fiodh Advanced Fomor Pass
		}

		return Item.GetRandomDrop(rnd, drops);
	}
}
