//--- Aura Script -----------------------------------------------------------
// Ciar Int 4 Dungeon
//--- Description -----------------------------------------------------------
// Script for Ciar Intermediate for Four.
//---------------------------------------------------------------------------

[DungeonScript("tircho_ciar_middle_4_dungeon")]
public class CiarIntFourDungeonScript : DungeonScript
{
	public override void OnBoss(Dungeon dungeon)
	{
		dungeon.AddBoss(130007, 1); // Golem
		dungeon.AddBoss(130008, 1); // Golem
		dungeon.AddBoss(11010, 6); // Metal Skeleton

		foreach (var member in dungeon.Party)
		{
			var cutscene = new Cutscene("bossroom_Golem34", member);
			cutscene.AddActor("player0", member);
			cutscene.AddActor("#golem3", 130007);
			cutscene.AddActor("#golem4", 130008);
			cutscene.AddActor("#metalskeleton_armora", 11010);
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
				// Mace
				int prefix = 0, suffix = 0;
				switch (rnd.Next(2))
				{
					case 0: prefix = 20701; break; // Stiff
					case 1: prefix = 21003; break; // Fatal
				}
				switch (rnd.Next(2))
				{
					case 0: suffix = 30506; break; // Belligerent
					case 1: suffix = 10807; break; // Considerate
				}
				treasureChest.Add(Item.CreateEnchanted(40079, prefix, suffix));
			}

			treasureChest.AddGold(rnd.Next(2608, 6400)); // Gold
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
			drops.Add(new DropData(itemId: 62004, chance: 10, amountMin: 1, amountMax: 2)); // Magic Powder
			drops.Add(new DropData(itemId: 51102, chance: 10, amountMin: 1, amountMax: 2)); // Mana Herb
			drops.Add(new DropData(itemId: 51003, chance: 10, amountMin: 1, amountMax: 2)); // HP 50 Potion
			drops.Add(new DropData(itemId: 51008, chance: 10, amountMin: 1, amountMax: 2)); // MP 50 Potion
			drops.Add(new DropData(itemId: 51013, chance: 10, amountMin: 1, amountMax: 2)); // Stamina 50 Potion
			drops.Add(new DropData(itemId: 71049, chance: 5, amountMin: 2, amountMax: 4)); // Snake Fomor Scroll
			drops.Add(new DropData(itemId: 63123, chance: 10, amount: 1, expires: 480)); // Ciar Intermediate Fomor Pass for One
			drops.Add(new DropData(itemId: 63124, chance: 10, amount: 1, expires: 480)); // Ciar Intermediate Fomor Pass for Two
			drops.Add(new DropData(itemId: 63125, chance: 10, amount: 1, expires: 480)); // Ciar Intermediate Fomor Pass for Four

			if (IsEnabled("CiarAdvanced"))
			{
				drops.Add(new DropData(itemId: 63136, chance: 5, amount: 1, expires: 360)); // Ciar Adv. Fomor Pass for 2
				drops.Add(new DropData(itemId: 63137, chance: 5, amount: 1, expires: 360)); // Ciar Adv. Fomor Pass for 3
				drops.Add(new DropData(itemId: 63138, chance: 5, amount: 1, expires: 360)); // Ciar Adv. Fomor Pass
			}
		}

		return Item.GetRandomDrop(rnd, drops);
	}
}
