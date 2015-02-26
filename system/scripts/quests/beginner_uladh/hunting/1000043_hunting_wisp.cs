//--- Aura Script -----------------------------------------------------------
// Hunt 1 Wisp
//--- Description -----------------------------------------------------------
// Tenth hunting quest beginner quest series, started automatically
// after reaching level 27 or completing Hunt 1 Brown Bear.
//---------------------------------------------------------------------------

public class WispQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(1000043);
		SetName("Hunt 1 Wisp");
		SetDescription("It's one thing to be short on hands, it's another to have a wisp appear around here and cause trouble. - Tracy -");
		
		SetReceive(Receive.Automatically);
		AddPrerequisite(Or(Completed(1000042), ReachedTotalLevel(27)));

		AddObjective("kill_wisp", "Hunt 1 wisp", 16, 9054, 58654, Kill(1, "/wisp/"));

		AddReward(Exp(3700));
		AddReward(Item(16006, 1));
	}
}

