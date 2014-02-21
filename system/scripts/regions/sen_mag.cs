//--- Aura Script -----------------------------------------------------------
// Sen Mag (53)
//--- Description -----------------------------------------------------------
// Warp and spawn definitions for Sen Mag.
// Region between Emain and Gairech.
//---------------------------------------------------------------------------

public class SenMagRegionScript : RegionScript
{
	public override void LoadWarps()
	{
		// Peaca
		SetPropBehavior(0x00A0003500070017, PropWarp(53,75599,118213, 74,3215,2114));
		SetPropBehavior(0x00A0004A00000011, PropWarp(74,3191,1710, 53,75606,117454));

		// Sen Mag Residential
		SetPropBehavior(0x00A00035000500CB, PropWarp(53,103137,78391, 202,54574,57302));
		//SetPropBehavior(0x00B000CA00030157, PropWarp(202,54835,57658, 53,103137,78391));
	}
	
	public override void LoadSpawns()
	{
		// ...
	}
}
