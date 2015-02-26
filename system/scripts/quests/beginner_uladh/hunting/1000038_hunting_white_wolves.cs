//--- Aura Script -----------------------------------------------------------
// Hunt 5 White Wolves
//--- Description -----------------------------------------------------------
// Fifth hunting quest beginner quest series, started automatically
// after reaching level 16 or completing Hunt 5 Black Wolves .
//---------------------------------------------------------------------------

public class WhiteWolvesQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(1000038);
		SetName("Hunt 5 White Wolves");
		SetDescription("I am Deian from the pasture east of Tir Chonaill. I'm fed up with the white wolves, that roams near the plains, eating up my sheep. How about hunting them down for me?");
		
		SetReceive(Receive.Automatically);
		AddPrerequisite(Or(Completed(1000037), ReachedTotalLevel(16)));

		AddObjective("kill_wolves", "Hunt 5 White Wolves", 1, 9650, 18276, Kill(5, "/whitewolf/"));

		AddReward(Exp(920));
		AddReward(Item(63000, 3));
	}
}

