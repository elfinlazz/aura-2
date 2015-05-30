using Aura.Channel.World.Dungeons;

[DungeonScript("gairech_fiodh_dungeon_eu")]
public class FiodhDungeonScript : DungeonScript
{
	public override void OnCreation(Dungeon dungeon)
	{
	}

	public override void OnBoss(Dungeon dungeon)
	{
		dungeon.AddBoss(170202, 1);
	}

	public override void OnCleared(Dungeon dungeon)
	{
	}
}
