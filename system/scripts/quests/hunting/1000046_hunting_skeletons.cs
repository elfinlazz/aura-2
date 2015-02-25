//--- Aura Script -----------------------------------------------------------
// Hunt 5 Skeletons
//--- Description -----------------------------------------------------------
// Thirteenth hunting quest beginner quest series, started automatically
// after completing Hunt 5 Skeleton Wolves.
//---------------------------------------------------------------------------

public class SkeletonsQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(1000046);
		SetName("Hunt 5 Skeletons");
		SetDescription("I am Comgan, serving as a priest at Bangor. Disturbances by evil creatures near Bangor make it difficult to transport ore from the mine. Can you please hunt 5 skeletons? - Comgan -");
		
		SetReceive(Receive.Automatically);
		AddPrerequisite(Completed(1000045));

		AddObjective("kill_skeletons", "Hunt 5 Skeletons", 30, 38314, 42319, Kill(5, "/skeleton/"));

		AddReward(Exp(4500));
		AddReward(Item(51000, 1));
	}
}

