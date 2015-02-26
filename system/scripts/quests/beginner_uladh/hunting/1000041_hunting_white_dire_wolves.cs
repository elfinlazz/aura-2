//--- Aura Script -----------------------------------------------------------
// Hunt 5 White Dire Wolves
//--- Description -----------------------------------------------------------
// Eighth hunting quest beginner quest series, started automatically
// after reaching level 20 or completing Hunt 5 Black Dire Wolves .
//---------------------------------------------------------------------------

public class WhiteDireWolvesQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(1000041);
		SetName("Hunt 5 White Dire Wolves");
		SetDescription("It is not easy becoming a warrior. Try hunting 5 white dire wolves. I know it's not an easy mission but please succeed. - Ranald-");
		
		SetReceive(Receive.Automatically);
		AddPrerequisite(Or(Completed(1000040), ReachedTotalLevel(20)));

		AddObjective("kill_wolves", "Hunt 5 White Dire Wolves", 16, 37864, 21727, Kill(5, "/whitedirewolf/"));

		AddReward(Exp(2000));
		AddReward(Item(63001, 2));
	}
}

