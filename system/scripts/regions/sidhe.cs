//--- Aura Script -----------------------------------------------------------
// Sidhe Sneachta (47/48)
//--- Description -----------------------------------------------------------
// Warp and spawn definitions for Sidhe, two fields north of Tir.
//---------------------------------------------------------------------------

public class SidheRegionScript : RegionScript
{
	public override void LoadWarps()
	{
		// Tir
		SetPropBehavior(0x00A0000100080067, PropWarp(1,1516,59999, 47,9985,6522));
		SetPropBehavior(0x00A0002F000100C9, PropWarp(47,10007,5680, 1,1748,59187));
		
		// Sidhe North - Sidhe South
		SetPropBehavior(0x00A0002F000100C1, PropWarp(47,9593,19201, 48,12514,7711));
		SetPropBehavior(0x00A00030000100FD, PropWarp(48,12802,6898, 47,9558,18288));
	}
	
	public override void LoadSpawns()
	{
		// ...
	}
}
