//--- Aura Script -----------------------------------------------------------
// Hunt 1 Troll
//--- Description -----------------------------------------------------------
// Nineteenth hunting quest beginner quest series, started automatically
// after completing Hunt 1 Black Grizzly Bear or Reaching Level 50.
//---------------------------------------------------------------------------

public class TrollQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(1000052);
		SetName("Hunt 1 Troll");
		SetDescription("I know how ferocious the troll is by the stories adventures tell during their visit to the town office. Great courage is needed to face the troll. Dunbarton Town will reward you for hunting the troll that threatens the adventurers. - Eavan -");
		
		SetReceive(Receive.Automatically);
		AddPrerequisite(Or(Completed(1000051), ReachedTotalLevel(50)));
		
		AddObjective("kill_troll", "Hunt 1 troll", 30, 62550, 20232, Kill(1, "/normaltroll/"));

		AddReward(Exp(12000));
		AddReward(Gold(7000));
	}
}

