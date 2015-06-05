//--- Aura Script -----------------------------------------------------------
// Alby Dungeon
//--- Description -----------------------------------------------------------
// Alby router and script for Alby normal.
//---------------------------------------------------------------------------

using Aura.Channel.World.Dungeons;

[DungeonScript("tircho_alby_dungeon")]
public class AlbyDungeonScript : DungeonScript
{
	public override bool Route(Creature creature, Item item, ref string dungeonName)
	{
		//dungeonName = "TirCho_Alby_Whiteday_Dungeon";
		return true;
	}

	public override void OnCreation(Dungeon dungeon)
	{
	}

	public override void OnBoss(Dungeon dungeon)
	{
		dungeon.AddBoss(30003, 1); // Red Spider
	}

	public override void OnCleared(Dungeon dungeon)
	{
		var rnd = RandomProvider.Get();

		for (int i = 0; i < dungeon.Party.Count; ++i)
		{
			var treasureChest = new TreasureChest();

			// Gold
			var gold = new Item(2000);
			gold.Amount = rnd.Next(153, 768);
			treasureChest.Add(gold);

			// Enchant
			var enchant = new Item(62005);
			switch (rnd.Next(3))
			{
				case 0: enchant.OptionInfo.Prefix = 1506; break; // Swan Summoner's
				case 1: enchant.OptionInfo.Prefix = 1706; break; // Good
				case 2: enchant.OptionInfo.Prefix = 305; break;  // Fine
			}
			treasureChest.Add(enchant);

			// Random item
			treasureChest.Add(GetRandomTreasureItem(rnd));

			dungeon.AddChest(treasureChest);
		}
	}

	DropData[] drops;
	public Item GetRandomTreasureItem(Random rnd)
	{
		var num = rnd.NextDouble() * 100;

		if (drops == null)
		{
			// Magic Powder 44%, 62004, 1~2
			// Mana Herb 44%, 51102, 1~2
			// White Spider Fomor Scroll 2%, 71017, 1~2
			// Red Spider Fomor Scroll 2%, 71019, 1
			// Alby Int 1 1%, 63116, expires 480
			// Alby Int 2 1%, 63117, expires 480
			// Alby Int 4 1%, 63118, expires 480
			// Alby Basic 2%, 63101, expires 480
			// Wooden Blade 1%, 40002, col1 0, dura 0
			// Alby Advanced 3 1%, 63160, expires 360
			// Alby Advanced 1%, 63161, expires 360

			drops = new DropData[]
			{
				new DropData(itemId: 62004, chance: 44, amountMin: 1, amountMax: 2), // Magic Powder
				new DropData(itemId: 51102, chance: 44, amountMin: 1, amountMax: 2), // Mana Herb
				new DropData(itemId: 71017, chance: 2, amountMin: 1, amountMax: 2),  // White Spider Fomor Scroll
				new DropData(itemId: 71019, chance: 2, amountMin: 1, amountMax: 1), // Red Spider Fomor Scroll
				new DropData(itemId: 63116, chance: 1, amount: 1), // Alby Int 1
				new DropData(itemId: 63117, chance: 1, amount: 1), // Alby Int 2
				new DropData(itemId: 63118, chance: 1, amount: 1), // Alby Int 4
				new DropData(itemId: 63101, chance: 2, amount: 1), // Alby Basic
				new DropData(itemId: 63160, chance: 1, amount: 1), // Alby Advanced 3
				new DropData(itemId: 63161, chance: 1, amount: 1), // Alby Advanced
				new DropData(itemId: 40002, chance: 1, amount: 1, color1: 0x000000), // Wooden Blade (black)
			};
		}

		var n = 0.0;
		DropData data = null;
		foreach (var drop in drops)
		{
			n += drop.Chance;
			if (num <= n)
			{
				data = drop;
				break;
			}
		}

		return new Item(data);
	}
}
