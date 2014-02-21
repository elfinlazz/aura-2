//--- Aura Script -----------------------------------------------------------
// Bangor (31)
//--- Description -----------------------------------------------------------
// Warp and spawn definitions for Bangor.
//---------------------------------------------------------------------------

public class BangorRegionScript : RegionScript
{
	public override void LoadWarps()
	{
		// Barri
		SetPropBehavior(0x00A0001F0001006E, PropWarp(31,13179,15447, 32,3210,2441));
		SetPropBehavior(0x00A0002000010006, PropWarp(32,3173,1551, 31,13167,15202));

		// Morva Aisle
		SetPropBehavior(0x00A0001F0001008A, PropWarp(31,12837,2891, 96,18358,35028));
		SetPropBehavior(0x00A0006000000010, PropWarp(96,18412,36408, 31,12810,5323));
	}
}
