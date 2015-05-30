using Aura.Channel.World.Dungeons;

[DungeonScript("tircho_alby_dungeon")]
public class AlbyDungeonScript : DungeonScript
{
	public override bool Route(Creature creature, Item item, ref string dungeonName)
	{
		//dungeonName = "TirCho_Alby_G15_Price_Of_Love_Mid_renewal";
		return true;
	}

	public override void OnCreation(Dungeon dungeon)
	{
	}

	public override void OnBoss(Dungeon dungeon)
	{
		dungeon.AddBoss(380001, 3); // Rabbit
	}

	public override void OnCleared(Dungeon dungeon)
	{
		var end = dungeon.Generator.Floors[0].MazeGenerator.EndPos;
		var endX = end.X * Dungeon.TileSize + Dungeon.TileSize / 2;
		var endY = end.Y * Dungeon.TileSize + Dungeon.TileSize / 2;

		var rnd = RandomProvider.Get();
		for (int i = 0; i < 500; ++i)
		{
			var item = new Item(2000);
			item.Info.Amount = 1000;
			item.Drop(dungeon.Regions[1], new Position(endX, endY + Dungeon.TileSize * 2).GetRandomInRange(1000, rnd));
		}

		foreach (var creature in dungeon.Party)
			Send.Notice(creature, "Congratulations!");
	}
}
