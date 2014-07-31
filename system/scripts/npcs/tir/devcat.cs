//--- Aura Script -----------------------------------------------------------
// devCAT in Tir Chonaill
//--- Description -----------------------------------------------------------
// Mysterious "weird cat" in Chief Duncan's house
//---------------------------------------------------------------------------

public class devCATScript : NpcScript
{
    public override void Load()
    {
        SetName("_devcat");
        SetRace(100);
        SetColor(0x0072672E, 0x00808080, 0x00808080);
        SetLocation(3, 2198, 1243, 31);
    }

    protected override async Task Talk()
    {
        SetBgm("NPC_devCAT.mp3");

        Msg("Meeeoow.");
    }
}
