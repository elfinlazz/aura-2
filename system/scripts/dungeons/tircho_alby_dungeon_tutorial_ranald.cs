//--- Aura Script -----------------------------------------------------------
// Rescue Resident quest dungeon
//--- Description -----------------------------------------------------------
// Player has to make it to the boss room and fight a Giant Spiderling
// alongside a Tir Chonaill resident.
//---------------------------------------------------------------------------

using Aura.Channel.World.Dungeons;

[DungeonScript("tircho_alby_dungeon_tutorial_ranald")]
public class AlbyTutorialDungeonScript : DungeonScript
{
	public override void OnCreation(Dungeon dungeon)
	{
	}

	public override void OnBoss(Dungeon dungeon)
	{
		dungeon.AddBoss(30022, 1); // Giant Spiderling
		dungeon.AddBoss(1002, 1);  // Lost Resident

		foreach (var member in dungeon.Party)
		{
			var cutscene = new Cutscene("bossroom_tutorial_giantspider_kid", member);
			cutscene.AddActor("player0", member);
			cutscene.AddActor("#giant_spider_kid", 30022);
			cutscene.AddActor("#lostresident", 1002);
			cutscene.Play();
		}
	}

	public override void OnBossDeath(Dungeon dungeon, Creature deadBoss)
	{
		if (deadBoss.RaceId == 30022)
		{
			dungeon.Complete();
		}
		else
		{
			// Lost resident, as in "lost" lost
		}
	}

	public override void OnCleared(Dungeon dungeon)
	{
		foreach (var member in dungeon.Party)
			member.TalkToNpc("_dungeonlostresident", "Lost Resident");

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
			drops = new DropData[]
			{
				new DropData(itemId: 51001, chance: 54, amountMin: 1, amountMax: 4), // HP 10 Potion
				new DropData(itemId: 51102, chance: 20, amountMin: 1, amountMax: 2), // Mana Herb
				new DropData(itemId: 71017, chance: 15, amountMin: 1, amountMax: 2), // White Spider Fomor Scroll
				new DropData(itemId: 71019, chance: 10, amount: 1), // Red Spider Fomor Scroll
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
