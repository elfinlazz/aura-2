//--- Aura Script -----------------------------------------------------------
// Fiodh Dungeon
//--- Description -----------------------------------------------------------
// Script for Fiodh Normal.
//---------------------------------------------------------------------------

using Aura.Channel.World.Dungeons;

[DungeonScript("gairech_fiodh_dungeon")]
public class FiodhDungeonScript : DungeonScript
{
	public override void OnBoss(Dungeon dungeon)
	{
		dungeon.AddBoss(130003, 1); // Small Golem
		dungeon.AddBoss(190001, 3); // Flying Sword

		foreach (var member in dungeon.Party)
		{
			var cutscene = new Cutscene("bossroom_SmallGolem_FlyingSword", member);
			cutscene.AddActor("me", member);
			cutscene.AddActor("#small_golem", 130003);
			cutscene.AddActor("#flying_sword", 190001);
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
				// Mongo's Fashion Cap
				var cap = new Item(18004);
				switch (rnd.Next(3))
				{
					case 0: cap.OptionInfo.Prefix = 20401; break; // Smart
					case 1: cap.OptionInfo.Prefix = 20601; break; // Blessing
					case 2: cap.OptionInfo.Prefix = 20202; break; // Hyena's
				}
				switch (rnd.Next(3))
				{
					case 0: cap.OptionInfo.Suffix = 30307; break; // Red Bear
					case 1: cap.OptionInfo.Suffix = 30601; break; // Thief
					case 2: cap.OptionInfo.Suffix = 30503; break; // White Spider
				}
				treasureChest.Add(cap);
			}

			treasureChest.Add(Item.CreateGold(rnd.Next(1500, 3600))); // Gold
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
				new DropData(itemId: 62004, chance: 17, amountMin: 1, amountMax: 2), // Magic Powder
				new DropData(itemId: 51102, chance: 17, amountMin: 1, amountMax: 2), // Mana Herb
				new DropData(itemId: 51003, chance: 17, amountMin: 1, amountMax: 2), // HP 50 Potion
				new DropData(itemId: 51008, chance: 17, amountMin: 1, amountMax: 2), // MP 50 Potion
				new DropData(itemId: 51013, chance: 17, amountMin: 1, amountMax: 2), // Stamina 50 Potion
				new DropData(itemId: 71049, chance: 2, amountMin: 2, amountMax: 5), // Snake Fomor Scroll
				new DropData(itemId: 71018, chance: 2, amountMin: 3, amountMax: 5), // Black Spider Fomor Scroll
				new DropData(itemId: 71018, chance: 2, amountMin: 3, amountMax: 5), // Black Spider Fomor Scroll (supposed to be Red, devCAT error)
				new DropData(itemId: 71052, chance: 1, amount: 1), // Jackal Fomor Scroll (supposed to be Red Grizzley, devCAT error)
				new DropData(itemId: 71052, chance: 2, amount: 5), // Jackal Fomor Scroll
				new DropData(itemId: 63119, chance: 2, amount: 1, expires: 480), // Fiodh Intermediate Fomor Pass for One
				new DropData(itemId: 63120, chance: 2, amount: 1, expires: 480), // Fiodh Intermediate Fomor Pass for Two
				new DropData(itemId: 63121, chance: 2, amount: 1, expires: 480), // Fiodh Intermediate Fomor Pass for Four
			};
		}

		return Item.GetRandomDrop(rnd, drops);
	}
}
