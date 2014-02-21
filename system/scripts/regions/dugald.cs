//--- Aura Script -----------------------------------------------------------
// Dugald Aisle (16)
//--- Description -----------------------------------------------------------
// Warp and spawn definitions for Dugald.
// Region between Tir and Dunbarton.
//---------------------------------------------------------------------------

public class DugaldRegionScript : RegionScript
{
	public override void LoadWarps()
	{
		// Residential
		SetPropBehavior(0x00A00010000600BA, PropWarp(16,43104,55693, 200,7615,14099));
		SetPropBehavior(0x00A000C8000F029B, PropWarp(200,7233,14023, 16,42571,55698));
	}
	
	public override void LoadSpawns()
	{
		// ...
	}
}
