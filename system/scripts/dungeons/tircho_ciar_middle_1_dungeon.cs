//--- Aura Script -----------------------------------------------------------
// Ciar Int 1 Dungeon
//--- Description -----------------------------------------------------------
// Script for Ciar Intermediate for One.
//---------------------------------------------------------------------------

using Aura.Channel.World.Dungeons;

[DungeonScript("tircho_ciar_middle_1_dungeon")]
public class CiarIntOneDungeonScript : DungeonScript
{
	public override void OnBoss(Dungeon dungeon)
	{
		dungeon.AddBoss(130007, 1); // Golem
		dungeon.AddBoss(11003, 6); // Metal Skeleton

		foreach (var member in dungeon.Party)
		{
			var cutscene = new Cutscene("bossroom_Metalskeleton_Golem3", member);
			cutscene.AddActor("player0", member);
			cutscene.AddActor("#golem3", 130007);
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

			treasureChest.AddGold(rnd.Next(1632, 4000)); // Gold
			treasureChest.Add(GetRandomTreasureItem(rnd)); // Random item

			dungeon.AddChest(treasureChest);

			member.GiveItemWithEffect(Item.CreateKey(70028, "chest"));
		}
	}

	DropData[] drops;
	public Item GetRandomTreasureItem(Random rnd)
	{
		if (drops == null)
		{
			drops = new DropData[]
			{
				new DropData(itemId: 62004, chance: 10, amountMin: 1, amountMax: 2), // Magic Powder
				new DropData(itemId: 51102, chance: 10, amountMin: 1, amountMax: 2), // Mana Herb
				new DropData(itemId: 51003, chance: 10, amountMin: 1, amountMax: 2), // HP 50 Potion
				new DropData(itemId: 51008, chance: 10, amountMin: 1, amountMax: 2), // MP 50 Potion
				new DropData(itemId: 51013, chance: 10, amountMin: 1, amountMax: 2), // Stamina 50 Potion
				new DropData(itemId: 71049, chance: 5, amountMin: 2, amountMax: 4), // Snake Fomor Scroll
				new DropData(itemId: 63123, chance: 10, amount: 1, expires: 480), // Ciar Intermediate Fomor Pass for One
				new DropData(itemId: 63124, chance: 10, amount: 1, expires: 480), // Ciar Intermediate Fomor Pass for Two
				new DropData(itemId: 63125, chance: 10, amount: 1, expires: 480), // Ciar Intermediate Fomor Pass for Four
				// advanced passes gX
			};
		}

		return Item.GetRandomDrop(rnd, drops);
	}
}
