//--- Aura Script -----------------------------------------------------------
// Ciar Beginner Dungeon
//--- Description -----------------------------------------------------------
// Script for Ciar Beginner.
//---------------------------------------------------------------------------

[DungeonScript("tircho_ciar_beginner_1_dungeon")]
public class CiarBeginnerDungeonScript : DungeonScript
{
	public override void OnBoss(Dungeon dungeon)
	{
		dungeon.AddBoss(130014, 1); // Small Golem

		foreach (var member in dungeon.Party)
		{
			var cutscene = new Cutscene("bossroom_small_golem_Ciar", member);
			cutscene.AddActor("me", member);
			cutscene.AddActor("#small_golem2", 130014);
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
				// Enchant
				var item = new Item(62005);
				switch (rnd.Next(6))
				{
					case 0: item.OptionInfo.Suffix = 11105; break; // Health
					case 1: item.OptionInfo.Suffix = 11106; break; // Blood
					case 2: item.OptionInfo.Suffix = 11205; break; // Water
					case 3: item.OptionInfo.Suffix = 11206; break; // Fountain
					case 4: item.OptionInfo.Suffix = 11304; break; // Patience
					case 5: item.OptionInfo.Suffix = 11305; break; // Sustainer
				}
				treasureChest.Add(item);
			}

			treasureChest.AddGold(rnd.Next(640, 960)); // Gold
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
			// advanced passes gX
		}

		return Item.GetRandomDrop(rnd, drops);
	}
}
