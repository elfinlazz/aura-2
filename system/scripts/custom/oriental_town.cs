// --- Aura Script ----------------------------------------------------------
//  Oriental Town
// --- Description ----------------------------------------------------------
//  Creates portal near Ciar Dungeon, to reach the Oriental Town region.
// --- By -------------------------------------------------------------------
//  Miro, exec
// --------------------------------------------------------------------------

public class OrientalTownProps : GeneralScript
{
	public override void Load()
	{
		SpawnProp(42935, 1, 43851, 30349, 2, 1); // Stone entrance
		SpawnProp(41121, 1, 43851, 30349, 2, 0.5f, PropWarp(60207, 2503, 4929)); // Portal to Orient
		SetPropBehavior(0xA0EB2F00010009, PropWarp(1, 43583, 30820)); // Portal back
	}
}