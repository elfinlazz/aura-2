//--- Aura Script -----------------------------------------------------------
// Seal Stones
//--- Description -----------------------------------------------------------
// Restrict access to specific areas through placement of shapely rocks.
//---------------------------------------------------------------------------

// North Emain Macha
// --------------------------------------------------------------------------

[Override("NorthEmainSealStoneScript")]
public class NorthEmainSealStoneG2Script : NorthEmainSealStoneScript
{
	public override void Setup()
	{
		base.Setup();
		SetLock(false);
	}
}

// South Emain Macha
// --------------------------------------------------------------------------

[Override("SouthEmainSealStoneScript")]
public class SouthEmainSealStoneG2Script : SouthEmainSealStoneScript
{
	public override void Setup()
	{
		base.Setup();
		SetLock(false);
	}
}
