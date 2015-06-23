//--- Aura Script -----------------------------------------------------------
// Barri Basic Dungeon
//--- Description -----------------------------------------------------------
// Script for Barri Basic.
//---------------------------------------------------------------------------

[DungeonScript("bangor_barri_low_dungeon")]
public class BarriBasicDungeonScript : DungeonScript
{
	public override void OnBoss(Dungeon dungeon)
	{
		dungeon.AddBoss(170201, 5); // Werewolf
		dungeon.AddBoss(160101, 5); // Gray Gremlin

		foreach (var member in dungeon.Party)
		{
			var cutscene = new Cutscene("bossroom_WereWolf", member);
			cutscene.AddActor("me", member);
			cutscene.AddActor("#werewolf", 170201);
			cutscene.AddActor("#gray_gremlin", 160101);
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

			// Fluted Short Sword
			int prefix = 0, suffix = 0;
			switch (rnd.Next(4))
			{
				case 0: suffix = 30702; break; // Raven
				case 1: suffix = 30602; break; // Healer
				case 2: suffix = 30504; break; // Gold Goblin
				case 3: suffix = 30501; break; // Giant
			}
			treasureChest.Add(Item.CreateEnchanted(40015, prefix, suffix));

			treasureChest.AddGold(rnd.Next(2384, 2992)); // Gold
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
			drops.Add(new DropData(itemId: 40010, chance: 1, amount: 1, color1: 0x6383CA, durability: 0)); // Longsword (purple)
			drops.Add(new DropData(itemId: 63113, chance: 9, amount: 1, expires: 600)); // Barri Basic

			if (IsEnabled("BarriAdvanced"))
			{
				drops.Add(new DropData(itemId: 63133, chance: 5, amount: 1, expires: 360)); // Barri Adv. Fomor Pass for 2
				drops.Add(new DropData(itemId: 63134, chance: 5, amount: 1, expires: 360)); // Barri Adv. Fomor Pass for 3
				drops.Add(new DropData(itemId: 63135, chance: 5, amount: 1, expires: 360)); // Barri Adv. Fomor Pass
			}
		}

		return Item.GetRandomDrop(rnd, drops);
	}
}
