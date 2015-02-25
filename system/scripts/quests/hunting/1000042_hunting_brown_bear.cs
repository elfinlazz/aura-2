//--- Aura Script -----------------------------------------------------------
// Hunt 1 Brown Bear
//--- Description -----------------------------------------------------------
// Ninth hunting quest beginner quest series, started automatically
// after reaching level 25 or completing Hunt 5 White Dire Wolves .
//---------------------------------------------------------------------------

public class BrownBearQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(1000042);
		SetName("Hunt 1 Brown Bear");
		SetDescription("You ought to have seen a brown bear at least once when you were passing near Dunbarton of Dugald Aisle. If it's okay with you, will you please hunt 1 brown bear? You don't have to come back and report. When you're done just get your payment. - Duncan-");
		
		SetReceive(Receive.Automatically);
		AddPrerequisite(Or(Completed(1000041), ReachedTotalLevel(25)));

		AddObjective("kill_bear", "Hunt 1 Brown Bear", 16, 9910, 60016, Kill(1, "/brownbear/"));

		AddReward(Exp(3200));
		AddReward(Item(60025, 1));
	}
}

