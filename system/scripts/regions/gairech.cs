//--- Aura Script -----------------------------------------------------------
// Gairech (30)
//--- Description -----------------------------------------------------------
// Warp and spawn definitions for Gairech.
// Region between Dunbarton and Bangor.
//---------------------------------------------------------------------------

public class GairechRegionScript : RegionScript
{
	public override void LoadWarps()
	{
		// Gairech - Bangor
		SetPropBehavior(0x00A0001E00080019, PropWarp(30,39171,16618, 31,13083,23128));
		SetPropBehavior(0x00A0001F00010014, PropWarp(31,13103,24027, 30,39167,17906));

		// Gairech - Fiodh Altar
		SetPropBehavior(0x00A0001E00050039, PropWarp(30,10705,83742, 49,3516,5317));
		SetPropBehavior(0x00A0003100000003, PropWarp(49,3454,4430, 30,10707,82575));

		// Gairech - Sen Mag
		//SetPropBehavior(0x00B0001E000500E7, PropWarp(30,8230,72405, 53,139366,121883));
		SetPropBehavior(0x00A0003500030005, PropWarp(53,139366,121883, 30,9462,72387));
	}
	
	public override void LoadSpawns()
	{
		// ...
	}
}
