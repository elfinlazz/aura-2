//--- Aura Script -----------------------------------------------------------
// Osna Snail (53)
//--- Description -----------------------------------------------------------
// Warp and spawn definitions for Osna Snail.
// Region between Emain and Dunbarton.
//---------------------------------------------------------------------------

public class OsnaSnailRegionScript : RegionScript
{
	public override void LoadWarps()
	{
		// Dunbarton - Osna Sail
		SetPropBehavior(0x00A0000E0006000F, PropWarp(14,16050,33339, 70,44316,19980));
		SetPropBehavior(0x00A0004600010001, PropWarp(70,44700,20000, 14,18169,33718));
		
		// Emain Macha - Osna Sail
		SetPropBehavior(0x00A00034001700A3, PropWarp(52,53323,75511, 70,6920,13157));
		SetPropBehavior(0x00A0004600020001, PropWarp(70,6400,12800, 52,52344,74261));
	}
	
	public override void LoadSpawns()
	{
		// ...
	}
}
