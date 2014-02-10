//--- Aura Script -----------------------------------------------------------
// Bangor
//--- Description -----------------------------------------------------------
// Warp, prop, and spawn definitions for Bangor.
//---------------------------------------------------------------------------

public class BangorRegionScript : BaseScript
{
	public override void Load()
	{
		LoadWarps();
		LoadPropDrops();
	}
	
	public void LoadWarps()
	{
		// Gairech
		SetPropBehavior(45036129417756692, PropWarp(30, 39167, 17906));
		SetPropBehavior(45036125123248153, PropWarp(31, 13083, 23128));
		
		// Barri
		SetPropBehavior(45036129417756782, PropWarp(32, 3210, 2441));
		SetPropBehavior(45036133712723974, PropWarp(31, 13167, 15202));
		
		// Morva Aisle
		SetPropBehavior(45036129417756810, PropWarp(96, 18358, 35028));
		SetPropBehavior(45036408590565392, PropWarp(31, 12810, 5323));
	}
	
	public void LoadPropDrops()
	{
		SetPropBehavior(45036129417756673, PropDrop(1));
		SetPropBehavior(45036129417756675, PropDrop(1));
		SetPropBehavior(45036129417756676, PropDrop(2));
		SetPropBehavior(45036129417756678, PropDrop(1));
		SetPropBehavior(45036129417756681, PropDrop(1));
		SetPropBehavior(45036129417756682, PropDrop(1));
		SetPropBehavior(45036129417756683, PropDrop(1));
		SetPropBehavior(45036129417756685, PropDrop(1));
		SetPropBehavior(45036129417756687, PropDrop(1));
		SetPropBehavior(45036129417756691, PropDrop(2));
		SetPropBehavior(45036129417756693, PropDrop(1));
		SetPropBehavior(45036129417756696, PropDrop(1));
		SetPropBehavior(45036129417756699, PropDrop(1));
		SetPropBehavior(45036129417756702, PropDrop(2));
		SetPropBehavior(45036129417756704, PropDrop(1));
		SetPropBehavior(45036129417756707, PropDrop(1));
		SetPropBehavior(45036129417756708, PropDrop(2));
		SetPropBehavior(45036129417756710, PropDrop(2));
		SetPropBehavior(45036129417756711, PropDrop(1));
		SetPropBehavior(45036129417756717, PropDrop(2));
		SetPropBehavior(45036129417756718, PropDrop(2));
		SetPropBehavior(45036129417756719, PropDrop(2));
		SetPropBehavior(45036129417756720, PropDrop(2));
		SetPropBehavior(45036129417756721, PropDrop(1));
		SetPropBehavior(45036129417756722, PropDrop(2));
		SetPropBehavior(45036129417756725, PropDrop(2));
		SetPropBehavior(45036129417756728, PropDrop(1));
		SetPropBehavior(45036129417756732, PropDrop(2));
		SetPropBehavior(45036129417756733, PropDrop(1));
		SetPropBehavior(45036129417756734, PropDrop(2));
		SetPropBehavior(45036129417756739, PropDrop(2));
		SetPropBehavior(45036129417756740, PropDrop(2));
		SetPropBehavior(45036129417756742, PropDrop(2));
		SetPropBehavior(45036129417756743, PropDrop(1));
		SetPropBehavior(45036129417756744, PropDrop(2));
		SetPropBehavior(45036129417756745, PropDrop(2));
		SetPropBehavior(45036129417756748, PropDrop(1));
		SetPropBehavior(45036129417756750, PropDrop(2));
		SetPropBehavior(45036129417756751, PropDrop(2));
		SetPropBehavior(45036129417756752, PropDrop(1));
		SetPropBehavior(45036129417756756, PropDrop(2));
		SetPropBehavior(45036129417756758, PropDrop(1));
		SetPropBehavior(45036129417756760, PropDrop(1));
		SetPropBehavior(45036129417756767, PropDrop(1));
		SetPropBehavior(45036129417756771, PropDrop(1));
		SetPropBehavior(45036129417756773, PropDrop(1));
		SetPropBehavior(45036129417756774, PropDrop(1));
		SetPropBehavior(45036129417756775, PropDrop(2));
		SetPropBehavior(45036129417756777, PropDrop(1));
		SetPropBehavior(45036129417756779, PropDrop(1));
		SetPropBehavior(45036129417756780, PropDrop(1));
		SetPropBehavior(45036129417756783, PropDrop(1));
		SetPropBehavior(45036129417756786, PropDrop(1));
		SetPropBehavior(45036129417756787, PropDrop(1));
		SetPropBehavior(45036129417756788, PropDrop(2));
		SetPropBehavior(45036129417756790, PropDrop(1));
		SetPropBehavior(45036129417756793, PropDrop(2));
		SetPropBehavior(45036129417756795, PropDrop(2));
		SetPropBehavior(45036129417756797, PropDrop(1));
		SetPropBehavior(45036129417756800, PropDrop(2));
		SetPropBehavior(45036129417756802, PropDrop(1));
		SetPropBehavior(45036129417756804, PropDrop(1));
		SetPropBehavior(45036129417756819, PropDrop(2));
		SetPropBehavior(45036129417756820, PropDrop(2));
		SetPropBehavior(45036129417756822, PropDrop(2));
		SetPropBehavior(45036129417756823, PropDrop(2));
		SetPropBehavior(45036129417756824, PropDrop(2));
		SetPropBehavior(45036129417756825, PropDrop(1));
		SetPropBehavior(45036129417756829, PropDrop(1));
		SetPropBehavior(45036129417756878, PropDrop(1));
		SetPropBehavior(45036129417756894, PropDrop(1));
	}
}
