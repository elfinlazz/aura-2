//--- Aura Script -----------------------------------------------------------
// Hunt 5 Black Dire Wolves
//--- Description -----------------------------------------------------------
// Seventh hunting quest beginner quest series, started automatically
// after reaching level 19 or completing Hunt 5 Brown Dire Wolves .
//---------------------------------------------------------------------------

public class BlackDireWolvesQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(1000040);
		SetName("Hunt 5 Black Dire Wolves");
		SetDescription("Now you're turning into a warrior. Try hunting 5 black dire wolves. It is a mission you have to do far away from town and also against a tough animal, so please succeed. - Ranald-");
		
		SetReceive(Receive.Automatically);
		AddPrerequisite(Or(Completed(1000039), ReachedTotalLevel(19)));

		AddObjective("kill_wolves", "Hunt 5 Black Dire Wolves", 16, 15390, 23558, Kill(5, "/blackdirewolf/"));

		AddReward(Exp(1670));
		AddReward(Item(60005, 10));
	}
}

