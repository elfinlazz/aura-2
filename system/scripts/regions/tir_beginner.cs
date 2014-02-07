//--- Aura Script -----------------------------------------------------------
// Tir Chonaill - Beginner Area (125)
//--- Description -----------------------------------------------------------
// Region you are warped to after talking to Nao/Tin.
//---------------------------------------------------------------------------

using Aura.Channel.Scripting.Scripts;
using Aura.Channel.Network.Sending;

public class TirBeginnerRegionScript : BaseScript
{
	public override void Load()
	{
		LoadWarps();
		LoadSpawns();
		LoadPropDrops();
	}
	
	public void LoadWarps()
	{
		// Warp to Tir
		SetPropBehavior(0x00A0007D00060018, PropWarp(1, 15250, 38467));
	}
	
	public void LoadSpawns()
	{
		// ...
	}
	
	public void LoadPropDrops()
	{
		SetPropBehavior(0x00A0007D00000014, PropDrop(2));
		SetPropBehavior(0x00A0007D0000001B, PropDrop(1));
		SetPropBehavior(0x00A0007D00000020, PropDrop(2));
		SetPropBehavior(0x00A0007D00010009, PropDrop(1));
		SetPropBehavior(0x00A0007D0001000A, PropDrop(1));
		SetPropBehavior(0x00A0007D0001000B, PropDrop(1));
		SetPropBehavior(0x00A0007D0001000C, PropDrop(1));
		SetPropBehavior(0x00A0007D0001000D, PropDrop(1));
		SetPropBehavior(0x00A0007D0001000E, PropDrop(1));
		SetPropBehavior(0x00A0007D0001000F, PropDrop(1));
		SetPropBehavior(0x00A0007D00010010, PropDrop(1));
		SetPropBehavior(0x00A0007D00010011, PropDrop(1));
		SetPropBehavior(0x00A0007D00010026, PropDrop(1));
		SetPropBehavior(0x00A0007D00010027, PropDrop(1));
		SetPropBehavior(0x00A0007D00010030, PropDrop(1));
		SetPropBehavior(0x00A0007D00010032, PropDrop(1));
		SetPropBehavior(0x00A0007D00010033, PropDrop(1));
		SetPropBehavior(0x00A0007D0001003C, PropDrop(2));
		SetPropBehavior(0x00A0007D00010043, PropDrop(2));
		SetPropBehavior(0x00A0007D00010044, PropDrop(2));
		SetPropBehavior(0x00A0007D00010045, PropDrop(2));
		SetPropBehavior(0x00A0007D00010046, PropDrop(2));
		SetPropBehavior(0x00A0007D00010047, PropDrop(2));
		SetPropBehavior(0x00A0007D0001005E, PropDrop(2));
		SetPropBehavior(0x00A0007D0001005F, PropDrop(2));
		SetPropBehavior(0x00A0007D00010060, PropDrop(2));
		SetPropBehavior(0x00A0007D00010061, PropDrop(2));
		SetPropBehavior(0x00A0007D00010070, PropDrop(2));
		SetPropBehavior(0x00A0007D000100A0, PropDrop(54));
		SetPropBehavior(0x00A0007D000100A1, PropDrop(54));
		SetPropBehavior(0x00A0007D000100F0, PropDrop(2));
		SetPropBehavior(0x00A0007D000100F3, PropDrop(2));
		SetPropBehavior(0x00A0007D0001012F, PropDrop(2));
		SetPropBehavior(0x00A0007D0001013D, PropDrop(2));
		SetPropBehavior(0x00A0007D0001013E, PropDrop(2));
		SetPropBehavior(0x00A0007D0001013F, PropDrop(2));
		SetPropBehavior(0x00A0007D00010140, PropDrop(2));
		SetPropBehavior(0x00A0007D00010141, PropDrop(2));
		SetPropBehavior(0x00A0007D00010142, PropDrop(2));
		SetPropBehavior(0x00A0007D00010143, PropDrop(2));
		SetPropBehavior(0x00A0007D00010144, PropDrop(54));
		SetPropBehavior(0x00A0007D00010145, PropDrop(2));
		SetPropBehavior(0x00A0007D00010152, PropDrop(2));
		SetPropBehavior(0x00A0007D00010153, PropDrop(2));
		SetPropBehavior(0x00A0007D00010155, PropDrop(2));
		SetPropBehavior(0x00A0007D00010157, PropDrop(2));
		SetPropBehavior(0x00A0007D000101CD, PropDrop(1));
		SetPropBehavior(0x00A0007D000101E4, PropDrop(1));
		SetPropBehavior(0x00A0007D00020017, PropDrop(1));
		SetPropBehavior(0x00A0007D0004001C, PropDrop(2));
		SetPropBehavior(0x00A0007D00060002, PropDrop(2));
		SetPropBehavior(0x00A0007D00060006, PropDrop(2));
		SetPropBehavior(0x00A0007D0006000F, PropDrop(2));
		SetPropBehavior(0x00A0007D00060014, PropDrop(1));
		SetPropBehavior(0x00A0007D00060015, PropDrop(1));
		SetPropBehavior(0x00A0007D00060016, PropDrop(1));
		SetPropBehavior(0x00A0007D0006001C, PropDrop(2));
		SetPropBehavior(0x00A0007D00060027, PropDrop(2));
		SetPropBehavior(0x00A0007D00060036, PropDrop(2));
		SetPropBehavior(0x00A0007D0006003C, PropDrop(2));
		SetPropBehavior(0x00A0007D00060042, PropDrop(2));
		SetPropBehavior(0x00A0007D00060055, PropDrop(2));
		SetPropBehavior(0x00A0007D00060057, PropDrop(2));
		SetPropBehavior(0x00A0007D00060058, PropDrop(1));
		SetPropBehavior(0x00A0007D0006006F, PropDrop(2));
		SetPropBehavior(0x00A0007D00060076, PropDrop(1));
		SetPropBehavior(0x00A0007D00060078, PropDrop(2));
		SetPropBehavior(0x00A0007D0006008C, PropDrop(2));
		SetPropBehavior(0x00A0007D0006009F, PropDrop(2));
		SetPropBehavior(0x00A0007D000600A8, PropDrop(2));
		SetPropBehavior(0x00A0007D000600B1, PropDrop(2));
		SetPropBehavior(0x00A0007D000600B2, PropDrop(2));
		SetPropBehavior(0x00A0007D000600B6, PropDrop(2));
		SetPropBehavior(0x00A0007D000600BB, PropDrop(1));
		SetPropBehavior(0x00A0007D000600C8, PropDrop(2));
		SetPropBehavior(0x00A0007D000600D1, PropDrop(2));
		SetPropBehavior(0x00A0007D000600D9, PropDrop(2));
		SetPropBehavior(0x00A0007D000600DE, PropDrop(2));
		SetPropBehavior(0x00A0007D000600ED, PropDrop(1));
		SetPropBehavior(0x00A0007D000600F5, PropDrop(2));
		SetPropBehavior(0x00A0007D000600FA, PropDrop(1));
		SetPropBehavior(0x00A0007D000600FE, PropDrop(2));
		SetPropBehavior(0x00A0007D00060102, PropDrop(1));
		SetPropBehavior(0x00A0007D00060104, PropDrop(2));
		SetPropBehavior(0x00A0007D0006010D, PropDrop(2));
		SetPropBehavior(0x00A0007D0006010E, PropDrop(2));
		SetPropBehavior(0x00A0007D00060130, PropDrop(2));
		SetPropBehavior(0x00A0007D00060144, PropDrop(1));
		SetPropBehavior(0x00A0007D00060155, PropDrop(2));
		SetPropBehavior(0x00A0007D00060157, PropDrop(1));
		SetPropBehavior(0x00A0007D0006015C, PropDrop(1));
		SetPropBehavior(0x00A0007D00060162, PropDrop(1));
		SetPropBehavior(0x00A0007D00060166, PropDrop(1));
		SetPropBehavior(0x00A0007D00060167, PropDrop(2));
		SetPropBehavior(0x00A0007D0006017D, PropDrop(1));
		SetPropBehavior(0x00A0007D0006019D, PropDrop(1));
		SetPropBehavior(0x00A0007D000601A3, PropDrop(2));
		SetPropBehavior(0x00A0007D000601A4, PropDrop(2));
		SetPropBehavior(0x00A0007D000601B3, PropDrop(1));
		SetPropBehavior(0x00A0007D000601C5, PropDrop(2));
		SetPropBehavior(0x00A0007D000601CE, PropDrop(2));
		SetPropBehavior(0x00A0007D000601D1, PropDrop(2));
		SetPropBehavior(0x00A0007D000601E5, PropDrop(2));
		SetPropBehavior(0x00A0007D000601EB, PropDrop(1));
		SetPropBehavior(0x00A0007D000601FF, PropDrop(1));
		SetPropBehavior(0x00A0007D00060201, PropDrop(2));
		SetPropBehavior(0x00A0007D00060202, PropDrop(2));
		SetPropBehavior(0x00A0007D00060212, PropDrop(1));
		SetPropBehavior(0x00A0007D00060214, PropDrop(2));
		SetPropBehavior(0x00A0007D0006021A, PropDrop(2));
		SetPropBehavior(0x00A0007D00060241, PropDrop(2));
		SetPropBehavior(0x00A0007D00060242, PropDrop(2));
		SetPropBehavior(0x00A0007D0006024B, PropDrop(2));
		SetPropBehavior(0x00A0007D00060250, PropDrop(2));
		SetPropBehavior(0x00A0007D00060251, PropDrop(2));
		SetPropBehavior(0x00A0007D0006025B, PropDrop(1));
		SetPropBehavior(0x00A0007D0006025C, PropDrop(1));
		SetPropBehavior(0x00A0007D0006025D, PropDrop(1));
		SetPropBehavior(0x00A0007D00060269, PropDrop(1));
		SetPropBehavior(0x00A0007D000602AF, PropDrop(1));
		SetPropBehavior(0x00A0007D0006030F, PropDrop(2));
	}
}
