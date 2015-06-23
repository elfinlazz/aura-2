//--- Aura Script -----------------------------------------------------------
// Fiodh Dungeon
//--- Description -----------------------------------------------------------
// Script for Fiodh Normal.
//---------------------------------------------------------------------------

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
			var member = dungeon.Party[i];
			var treasureChest = new TreasureChest();

			if (i == 0)
			{
				// Mongo's Fashion Cap
				int prefix = 0, suffix = 0;
				switch (rnd.Next(3))
				{
					case 0: prefix = 20401; break; // Smart
					case 1: prefix = 20601; break; // Blessing
					case 2: prefix = 20202; break; // Wild Dog
				}
				switch (rnd.Next(3))
				{
					case 0: suffix = 30307; break; // Red Bear
					case 1: suffix = 30601; break; // Thief
					case 2: suffix = 30503; break; // White Spider
				}
				treasureChest.Add(Item.CreateEnchanted(18004, prefix, suffix));
			}

			treasureChest.AddGold(rnd.Next(1500, 3600)); // Gold
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
			drops.Add(new DropData(itemId: 62004, chance: 17, amountMin: 1, amountMax: 2)); // Magic Powder
			drops.Add(new DropData(itemId: 51102, chance: 17, amountMin: 1, amountMax: 2)); // Mana Herb
			drops.Add(new DropData(itemId: 51003, chance: 17, amountMin: 1, amountMax: 2)); // HP 50 Potion
			drops.Add(new DropData(itemId: 51008, chance: 17, amountMin: 1, amountMax: 2)); // MP 50 Potion
			drops.Add(new DropData(itemId: 51013, chance: 17, amountMin: 1, amountMax: 2)); // Stamina 50 Potion
			drops.Add(new DropData(itemId: 71049, chance: 2, amountMin: 2, amountMax: 5)); // Snake Fomor Scroll
			drops.Add(new DropData(itemId: 71018, chance: 2, amountMin: 3, amountMax: 5)); // Black Spider Fomor Scroll
			drops.Add(new DropData(itemId: 71019, chance: 2, amountMin: 3, amountMax: 5)); // Red Spider Fomor Scroll (officially Black Spider duplicate #officialFix)
			drops.Add(new DropData(itemId: 71029, chance: 1, amount: 1)); // Red Grizzly Fomor Scroll (officially Jackal duplicate #officialFix)
			drops.Add(new DropData(itemId: 71052, chance: 2, amount: 5)); // Jackal Fomor Scroll
			drops.Add(new DropData(itemId: 63119, chance: 2, amount: 1, expires: 480)); // Fiodh Intermediate Fomor Pass for One
			drops.Add(new DropData(itemId: 63120, chance: 2, amount: 1, expires: 480)); // Fiodh Intermediate Fomor Pass for Two
			drops.Add(new DropData(itemId: 63121, chance: 2, amount: 1, expires: 480)); // Fiodh Intermediate Fomor Pass for Four
		}

		return Item.GetRandomDrop(rnd, drops);
	}
}
