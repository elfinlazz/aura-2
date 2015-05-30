using Aura.Channel.World.Dungeons;

[DungeonScript("danu_dungeon")]
public class FiodhDungeonRouteScript : DungeonScript
{
	public override bool Route(Creature creature, Item item, ref string dungeonName)
	{
		dungeonName = "gairech_fiodh_dungeon_eu";
		return true;
	}
}
