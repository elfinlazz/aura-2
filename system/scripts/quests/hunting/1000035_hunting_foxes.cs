//--- Aura Script -----------------------------------------------------------
// Hunt 5 Gray Foxes
//--- Description -----------------------------------------------------------
// Second hunting quest beginner quest series, started automatically
// after reaching level 13 or completing Hunt 5 White Spiders.
//---------------------------------------------------------------------------

public class GrayFoxesQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(1000035);
		SetName("Hunt 5 Gray Foxes");
		SetDescription("How are you doing? I am a little concern that the gray foxes near the pasture will harm the sheep. If it's okay with you, will you please hunt 5 gray foxes? You don't have to come back and report. When you're done, just get your payment. - Duncan -");
		
		SetReceive(Receive.Automatically);
		AddPrerequisite(Or(Completed(1000034), ReachedTotalLevel(13)));

		AddObjective("kill_foxes", "Hunt 5 Gray Foxes", 1, 43003, 46253, Kill(5, "/grayfox/"));

		AddReward(Exp(440));
		AddReward(Item(51012, 3));
	}
}

