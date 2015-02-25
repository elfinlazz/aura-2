//--- Aura Script -----------------------------------------------------------
// Hunt 1 Black Grizzly Bear
//--- Description -----------------------------------------------------------
// Eighteenth hunting quest beginner quest series, started automatically
// after completing Hunt 1 Brown Grizzly Bear.
//---------------------------------------------------------------------------

public class BlackGrizzlyBearQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(1000051);
		SetName("Hunt 1 Black Grizzly Bear");
		SetDescription("I'm Eavan from the Dunbarton Town Office. The Black Grizzly Bear from around Dunbarton is threatening the residents. Dunbarton Town will reward you for hunting the black grizzly bear. - Eavan -");
		
		SetReceive(Receive.Automatically);
		AddPrerequisite(Completed(1000050));
		
		AddObjective("kill_bear", "Hunt 1 Black Grizzly Bear", 14, 41632, 20951, Kill(1, "/blackgrizzlybear/"));

		AddReward(Exp(10000));
		AddReward(Item(17005, 1));
	}
}

