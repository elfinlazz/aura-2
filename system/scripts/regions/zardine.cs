//--- Aura Script -----------------------------------------------------------
// Zardine
//--- Description -----------------------------------------------------------
// Warp, prop, and spawn definitions for Zardine.
//---------------------------------------------------------------------------

using Aura.Channel.Scripting.Scripts;
using Aura.Channel.Network.Sending;

public class ZardineRegionScript : BaseScript
{
	public override void Load()
	{
		LoadWarps();
		LoadPropDrops();
	}
	
	public void LoadWarps()
	{
		SetPropBehavior(45036507375075384, PropWarp(3400, 239619, 256873));
	}

	public void LoadPropDrops()
	{
		SetPropBehavior(45050599165263965, PropDrop(1));
		SetPropBehavior(45050599165788253, PropDrop(1));
		SetPropBehavior(45050599165788254, PropDrop(1));
		SetPropBehavior(45050599165788255, PropDrop(1));
		SetPropBehavior(45050599165788256, PropDrop(1));
		SetPropBehavior(45050599166312468, PropDrop(2));
		SetPropBehavior(45050599166312469, PropDrop(2));
		SetPropBehavior(45050599166378011, PropDrop(2));
		SetPropBehavior(45050599166378012, PropDrop(2));
		SetPropBehavior(45050599166443527, PropDrop(2));
		SetPropBehavior(45050599166836739, PropDrop(1));
		SetPropBehavior(45050599166836805, PropDrop(2));
		SetPropBehavior(45050599166836806, PropDrop(2));
		SetPropBehavior(45050599166836873, PropDrop(2));
		SetPropBehavior(45050599166836874, PropDrop(2));
		SetPropBehavior(45050599166836924, PropDrop(2));
		SetPropBehavior(45050599166836925, PropDrop(2));
		SetPropBehavior(45050599166836926, PropDrop(2));
		SetPropBehavior(45050599166836932, PropDrop(1));
		SetPropBehavior(45050599166836933, PropDrop(1));
		SetPropBehavior(45050599166836934, PropDrop(1));
		SetPropBehavior(45050599166836935, PropDrop(1));
		SetPropBehavior(45050599166836936, PropDrop(1));
		SetPropBehavior(45050599166836938, PropDrop(1));
		SetPropBehavior(45050599166836939, PropDrop(1));
		SetPropBehavior(45050599166836940, PropDrop(1));
		SetPropBehavior(45050599166836941, PropDrop(1));
		SetPropBehavior(45050599166836942, PropDrop(1));
		SetPropBehavior(45050599166836943, PropDrop(1));
		SetPropBehavior(45050599166902281, PropDrop(2));
		SetPropBehavior(45050599166902282, PropDrop(2));
		SetPropBehavior(45050599166902361, PropDrop(2));
		SetPropBehavior(45050599166902363, PropDrop(2));
		SetPropBehavior(45050599166902366, PropDrop(2));
		SetPropBehavior(45050599166967823, PropDrop(2));
		SetPropBehavior(45050599166967872, PropDrop(2));
		SetPropBehavior(45050599166967883, PropDrop(2));
		SetPropBehavior(45050599166967884, PropDrop(2));
	}
}
