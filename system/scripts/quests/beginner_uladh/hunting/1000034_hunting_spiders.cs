//--- Aura Script -----------------------------------------------------------
// Hunt 5 White Spiders
//--- Description -----------------------------------------------------------
// First hunting quest beginner quest series, started automatically
// after reaching level 12.
//---------------------------------------------------------------------------

public class WhiteSpiderQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(1000034);
		SetName("Hunt 5 White Spiders");
		SetDescription("You can find the white spider near the graveyard easily. The graveyard is often damaged by the white spider that have increased suddenly. Can you take the lead and banish the white spider? First hunt 5 white spiders. You don't have to come back and report. When you're done, just get your payment. - Duncan -");
		
		SetReceive(Receive.Automatically);
		AddPrerequisite(ReachedTotalLevel(12));

		AddObjective("kill_spiders", "Hunt 5 White spiders", 1, 17910, 42893, Kill(5, "/whitespider/"));

		AddReward(Exp(330));
		AddReward(Item(51002, 3));
	}
}

