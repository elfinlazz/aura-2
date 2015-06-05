//--- Aura Script -----------------------------------------------------------
// Alby White Day Dungeon
//--- Description -----------------------------------------------------------
// Custom dungeon, involving a band of evil bunnies and their gold.
//---------------------------------------------------------------------------

using Aura.Channel.World.Dungeons;

[DungeonScript("tircho_alby_whiteday_dungeon")]
public class AlbyWhiteDayDungeonScript : DungeonScript
{
	public override void OnCreation(Dungeon dungeon)
	{
	}

	public override void OnBoss(Dungeon dungeon)
	{
		dungeon.AddBoss(380001, 3); // Rabbit
	}

	public override void OnCleared(Dungeon dungeon)
	{
		var rnd = RandomProvider.Get();
		var end = dungeon.Generator.Floors[0].MazeGenerator.EndPos;
		var endX = end.X * Dungeon.TileSize + Dungeon.TileSize / 2;
		var endY = end.Y * Dungeon.TileSize + Dungeon.TileSize / 2;

		for (int i = 0; i < 100; ++i)
		{
			var item = new Item(2000); // Gold
			item.Info.Amount = 1000;
			item.Drop(dungeon.Regions[1], new Position(endX, endY + Dungeon.TileSize * 2).GetRandomInRange(500, 1000, rnd));
		}

		for (int i = 0; i < 100; ++i)
		{
			var item = new Item(2004); // Check
			item.MetaData1.SetInt("EVALUE", 1000000);
			item.Drop(dungeon.Regions[1], new Position(endX, endY + Dungeon.TileSize * 2).GetRandomInRange(500, 1000, rnd));
		}
	}
}
