//--- Aura Script -----------------------------------------------------------
// Alby White Day Dungeon
//--- Description -----------------------------------------------------------
// Custom dungeon, involving a band of evil bunnies and their gold.
//---------------------------------------------------------------------------

[DungeonScript("tircho_alby_whiteday_dungeon")]
public class AlbyWhiteDayDungeonScript : DungeonScript
{
	public override void OnCreation(Dungeon dungeon)
	{
	}

	public override void OnBoss(Dungeon dungeon)
	{
		dungeon.AddBoss(380001, 3); // Rabbit

		foreach (var member in dungeon.Party)
		{
			var cutscene = new Cutscene("bossroom_GiantSpider_kid", member);
			cutscene.AddActor("player0", member);
			cutscene.AddActor("#giant_spider_kid", 380001);
			cutscene.AddActor("#red_spider_kid", 380001);
			cutscene.Play();
		}
	}

	public override void OnCleared(Dungeon dungeon)
	{
		var rnd = RandomProvider.Get();
		var end = dungeon.Generator.Floors[0].MazeGenerator.EndPos;
		var endX = end.X * Dungeon.TileSize + Dungeon.TileSize / 2;
		var endY = end.Y * Dungeon.TileSize + Dungeon.TileSize / 2;
		var center = new Position(endX, endY + Dungeon.TileSize * 2);
		var region = dungeon.Regions.Last();

		for (int i = 0; i < 150; ++i)
		{
			var item = Item.CreateGold(rnd.Next(500, 1000 + 1));
			item.Drop(region, center.GetRandomInRange(500, 1000, rnd));
		}

		for (int i = 0; i < 50; ++i)
		{
			var item = Item.CreateCheck(rnd.Next(1, 4 + 1) * 10000);
			item.Drop(region, center.GetRandomInRange(500, 1000, rnd));
		}
	}
}
