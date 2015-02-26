//--- Aura Script -----------------------------------------------------------
// Hunt 1 Brown Grizzly Bear
//--- Description -----------------------------------------------------------
// Seventeenth hunting quest beginner quest series, started automatically
// after completing Hunt 1 Brown Grizzly Bear.
//---------------------------------------------------------------------------

public class BrownGrizzlyBearQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(1000050);
		SetName("Hunt 1 Brown Grizzly Bear");
		SetDescription("I'm Eavan from the Dunbarton Town Office. The brown grizzly bear from around Dunbarton is threatening the residents. Dunbarton Town will reward you for hunting the brown grizzly bear. - Eavan -");
		
		SetReceive(Receive.Automatically);
		AddPrerequisite(Completed(1000049));
		
		AddObjective("kill_bear", "Hunt 1 Brown Grizzly Bear", 14, 23394, 24548, Kill(1, "/browngrizzlybear/"));

		AddReward(Exp(9000));
		AddReward(Item(51158, 3));
	}
}

