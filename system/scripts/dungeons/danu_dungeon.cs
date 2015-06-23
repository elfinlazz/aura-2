//--- Aura Script -----------------------------------------------------------
// Fiodh Router
//--- Description -----------------------------------------------------------
// Danu was renamed to Fiodh, but the name of the altar is still "Danu",
// which is why we route from here to to the Fiodh scripts, unlike other
// dungeons, where the script for the normal version is also the router.
//---------------------------------------------------------------------------

[DungeonScript("danu_dungeon")]
public class FiodhDungeonRouteScript : DungeonScript
{
	public override bool Route(Creature creature, Item item, ref string dungeonName)
	{
		// Fiodh Int 1
		if (item.Info.Id == 63119) // Fiodh Intermediate Fomor Pass for One
		{
			dungeonName = "gairech_fiodh_middle_1_dungeon";
			return true;
		}

		// Fiodh Int 2
		// TODO: Party check
		if (item.Info.Id == 63120) // Fiodh Intermediate Fomor Pass for Two
		{
			dungeonName = "gairech_fiodh_middle_2_dungeon";
			return true;
		}

		// Fiodh Int 4
		// TODO: Party check
		if (item.Info.Id == 63121) // Fiodh Intermediate Fomor Pass for Four
		{
			dungeonName = "gairech_fiodh_middle_4_dungeon";
			return true;
		}

		dungeonName = "gairech_fiodh_dungeon";
		return true;
	}
}
