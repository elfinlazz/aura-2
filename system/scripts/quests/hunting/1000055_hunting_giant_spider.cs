//--- Aura Script -----------------------------------------------------------
// Hunt 1 Giant Spider
//--- Description -----------------------------------------------------------
// First hunting quest Boss quest series, started automatically
// after ?
//---------------------------------------------------------------------------

public class GiantSpiderQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(1000055);
		SetName("Hunt 1 Giant Spider");
		SetDescription("I am Trefor, the guard. Have you ever heard about the Giant Spider? Why don't you drop a Common Item on the Alby dungeon altar to enter and hunt 1 Giant Spider? I think it's a good chance to test your courage. - Trefor -");
		
		SetReceive(Receive.Automatically);
		AddPrerequisite(ReachedTotalLevel(21));

		AddObjective("kill_spider", "Hunt 1 Giant Spider", 13, 3210, 3209, Kill(1, "/giantspider/"));

		AddReward(Exp(4000));
		AddReward(Item(17018, 1));
	}
}

