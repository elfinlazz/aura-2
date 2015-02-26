//--- Aura Script -----------------------------------------------------------
// Hunt 1 Red Bear
//--- Description -----------------------------------------------------------
// Sixteenth hunting quest beginner quest series, started automatically
// after completing Hunt 1 red Grizzly Cub or Reaching level 45.
//---------------------------------------------------------------------------

public class RedBearQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(1000049);
		SetName("Hunt 1 Red Bear");
		SetDescription("I am concerned about lots of the ferocious animals influenced by evil spirits. If it's okay with you, will you please hunt 1 red bear? You don't have to come back and report. When you're done, just get your payment. - Duncan -");
		
		SetReceive(Receive.Automatically);
		AddPrerequisite(Or(Completed(1000048), ReachedTotalLevel(45)));
		
		AddObjective("kill_bear", "Hunt 1 Red Bear", 16, 10222, 59148, Kill(1, "/redbear/"));

		AddReward(Exp(8000));
		AddReward(Item(51013, 3));
	}
}

