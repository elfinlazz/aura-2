//--- Aura Script -----------------------------------------------------------
// Rabbie Basic Dungeon
//--- Description -----------------------------------------------------------
// Script for Rabbie Basic.
//---------------------------------------------------------------------------

[DungeonScript("dunbarton_rabbie_low_dungeon")]
public class RabbieBasicDungeonScript : DungeonScript
{
	public override void OnBoss(Dungeon dungeon)
	{
		if (dungeon.CountPlayers() == 1)
		{
			dungeon.AddBoss(10302, 1); // Red Succubus
		}
		else
		{
			dungeon.AddBoss(170101, 1); // Lycanthrope
			dungeon.AddBoss(170102, 1); // Lycanthrope

			foreach (var member in dungeon.Party)
			{
				var cutscene = new Cutscene("bossroom_Lycanthrope2", member);
				cutscene.AddActor("me", member);
				cutscene.AddActor("#lycan", 170101);
				cutscene.Play();
			}
		}
	}

	public override void OnCleared(Dungeon dungeon)
	{
		var rnd = RandomProvider.Get();

		if (dungeon.CountPlayers() == 1)
		{
			var member = dungeon.Party[0];
			var treasureChest = new TreasureChest();

			// Cores' Healer Suit
			int prefix = 0, suffix = 0;
			switch (rnd.Next(3))
			{
				case 0: suffix = 30806; break; // Embroider
				case 1: suffix = 30805; break; // Falcon
				case 2: suffix = 30701; break; // Golem
			}
			treasureChest.Add(Item.CreateEnchanted(15030, prefix, suffix));

			treasureChest.Add(Item.Create(id: 2000, amountMin: 1140, amountMax: 5040)); // Gold
			treasureChest.Add(GetRandomTreasureItem(rnd)); // Random item

			dungeon.AddChest(treasureChest);

			member.GiveItemWithEffect(Item.CreateKey(70028, "chest"));
		}
		else
		{
			for (int i = 0; i < dungeon.Party.Count; ++i)
			{
				var member = dungeon.Party[i];
				var treasureChest = new TreasureChest();

				if (i == 0)
				{
					// Wave-patterned Long Boots
					int prefix = 20204, suffix = 0;
					switch (rnd.Next(3))
					{
						case 0: suffix = 30902; break; // The dawn
						case 1: suffix = 30602; break; // Healer
						case 2: suffix = 30505; break; // Direwolf
					}
					treasureChest.Add(Item.CreateEnchanted(17032, prefix, suffix));
				}

				treasureChest.Add(Item.Create(id: 2000, amountMin: 608, amountMax: 2688)); // Gold
				treasureChest.Add(GetRandomTreasureItem(rnd)); // Random item

				dungeon.AddChest(treasureChest);

				member.GiveItemWithEffect(Item.CreateKey(70028, "chest"));
			}
		}
	}

	List<DropData> drops;
	public Item GetRandomTreasureItem(Random rnd)
	{
		if (drops == null)
		{
			drops = new List<DropData>();
			drops.Add(new DropData(itemId: 62004, chance: 13, amountMin: 1, amountMax: 2)); // Magic Powder
			drops.Add(new DropData(itemId: 51102, chance: 13, amountMin: 1, amountMax: 2)); // Mana Herb
			drops.Add(new DropData(itemId: 51003, chance: 13, amountMin: 1, amountMax: 2)); // HP 50 Potion
			drops.Add(new DropData(itemId: 51008, chance: 13, amountMin: 1, amountMax: 2)); // MP 50 Potion
			drops.Add(new DropData(itemId: 51013, chance: 13, amountMin: 1, amountMax: 2)); // Stamina 50 Potion
			drops.Add(new DropData(itemId: 71006, chance: 2, amountMin: 3, amountMax: 4)); // Skeleton Fomor Scroll
			drops.Add(new DropData(itemId: 71007, chance: 2, amountMin: 1, amountMax: 2)); // Red Skeleton Fomor Scroll
			drops.Add(new DropData(itemId: 40031, chance: 1, amount: 1, color1: 0xA50000, durability: 0)); // Short Sword (red)
			drops.Add(new DropData(itemId: 63110, chance: 7, amount: 1, expires: 600)); // Rabbie Basic Fomor Pass
			drops.Add(new DropData(itemId: 60041, chance: 8, amountMin: 1, amountMax: 5)); // Magical Golden Thread

			if (IsEnabled("RabbieAdvanced"))
			{
				drops.Add(new DropData(itemId: 63141, chance: 5, amount: 1, expires: 360)); // Rabbie Adv. Fomor Pass for 2
				drops.Add(new DropData(itemId: 63142, chance: 7, amount: 1, expires: 360)); // Rabbie Adv. Fomor Pass for 3
				drops.Add(new DropData(itemId: 63143, chance: 3, amount: 1, expires: 360)); // Rabbie Adv. Fomor Pass
			}
		}

		return Item.GetRandomDrop(rnd, drops);
	}
}
