//--- Aura Script -----------------------------------------------------------
// Tir Chonaill - Beginner Area (125)
//--- Description -----------------------------------------------------------
// Region you are warped to after talking to Nao/Tin.
//---------------------------------------------------------------------------

public class TirBeginnerRegionScript : RegionScript
{
	public override void LoadWarps()
	{
		// Tir
		SetPropBehavior(0x00A0007D00060018, PropWarp(125,27753,72762, 1, 15250, 38467));
		
		// Gargoyles
		SetPropBehavior(0x00A0007D0001003A, PropWarp(125,19971,69993, 125,17186,69763));
		SetPropBehavior(0x00A0007D0001003B, PropWarp(125,17641,69874, 125,20453,70023));
	}
	
	public override void LoadSpawns()
	{
		// ...
	}
}
