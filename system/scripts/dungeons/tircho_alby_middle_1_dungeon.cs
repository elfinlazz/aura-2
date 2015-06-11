//--- Aura Script -----------------------------------------------------------
// Alby Int 1
//--- Description -----------------------------------------------------------
// Script for Alby Intermediate for One.
//---------------------------------------------------------------------------

using Aura.Channel.World.Dungeons;

[DungeonScript("tircho_alby_middle_1_dungeon")]
public class AlbyIntOneDungeonScript : DungeonScript
{
	public override void OnBoss(Dungeon dungeon)
	{
		dungeon.AddBoss(30006, 1); // Giant Black Spider
		dungeon.AddBoss(30012, 6); // Burgundy Spider

		foreach (var member in dungeon.Party)
		{
			var cutscene = new Cutscene("bossroom_giant_spiderB", member);
			cutscene.AddActor("player0", member);
			cutscene.AddActor("#giant_black_spider", 30006);
			cutscene.AddActor("#darkred_spider", 30012);
			cutscene.Play();
		}
	}

	public override void OnCleared(Dungeon dungeon)
	{
		var rnd = RandomProvider.Get();

		for (int i = 0; i < dungeon.Party.Count; ++i)
		{
			var treasureChest = new TreasureChest();

			if (i == 0)
			{
				// Enchanted item
				Item item = null;
				switch (rnd.Next(2))
				{
					case 0: item = Item.CreateEnchanted(40004, prefix: 20105); break; // Artless Lute
					case 1: item = Item.CreateEnchanted(15022, prefix: 20612); break; // Splendit Popo's Skirt
				}
				treasureChest.Add(item);
			}

			treasureChest.AddGold(rnd.Next(2080, 4160)); // Gold
			treasureChest.Add(GetRandomTreasureItem(rnd)); // Random item

			dungeon.AddChest(treasureChest);
		}
	}

	DropData[] drops;
	public Item GetRandomTreasureItem(Random rnd)
	{
		if (drops == null)
		{
			drops = new DropData[]
			{
				new DropData(itemId: 62004, chance: 2, amountMin: 2, amountMax: 4), // Magic Powder
				new DropData(itemId: 51102, chance: 2, amountMin: 2, amountMax: 4), // Mana Herb
				new DropData(itemId: 51003, chance: 2, amountMin: 2, amountMax: 4), // HP 50 Potion
				new DropData(itemId: 51003, chance: 2, amountMin: 2, amountMax: 4), // Stamina 50 Potion
				new DropData(itemId: 51008, chance: 2, amountMin: 2, amountMax: 4), // MP 50 Potion
				new DropData(itemId: 63116, chance: 3, amount: 1, expires: 480), // Alby Intermediate Fomor Pass for One
				new DropData(itemId: 63117, chance: 2, amount: 1, expires: 480), // Alby Intermediate Fomor Pass for Two
				new DropData(itemId: 63118, chance: 2, amount: 1, expires: 480), // Alby Intermediate Fomor Pass for Four
			};
		}

		return Item.GetRandomDrop(rnd, drops);
	}
}
