//--- Aura Script -----------------------------------------------------------
// Hunt 5 Black Wolves
//--- Description -----------------------------------------------------------
// Fourth hunting quest beginner quest series, started automatically
// after reaching level 15 or completing Hunt 5 Gray Wolves .
//---------------------------------------------------------------------------

public class BlackWolvesQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(1000037);
		SetName("Hunt 5 Black Wolves");
		SetDescription("Looks like every day is so peaceful but...... I'm nervous... These days I am a little concerned that the black wolves near Ciar Dungeon may harm the travelers. If it's okay with you, will you please hunt 5 black wolves? You don't have to come back and report. When you're done, just get your payment. - Duncan -");
		
		SetReceive(Receive.Automatically);
		AddPrerequisite(Or(Completed(1000036), ReachedTotalLevel(15)));

		AddObjective("kill_wolves", "Hunt 5 Black Wolves", 1, 47439, 43990, Kill(5, "/blackwolf/"));

		AddReward(Exp(730));
		AddReward(Item(1008, 1));
	}
}

