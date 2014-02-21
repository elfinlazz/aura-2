//--- Aura Script -----------------------------------------------------------
// Ceo Island (56)
//--- Description -----------------------------------------------------------
// Warp and spawn definitions for Ceo.
//---------------------------------------------------------------------------

public class CeoRegionScript : RegionScript
{
	public override void LoadWarps()
	{
		// Ceo Cellar
		SetPropBehavior(0x00A0003800020069, PropWarp(56,18800,16600, 68,5600,4284));
		SetPropBehavior(0x00A0004400000001, PropWarp(68,5616,3885, 56,18799,15876));
	}
	
	public override void LoadSpawns()
	{
		// ...
	}
}
