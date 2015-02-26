//--- Aura Script -----------------------------------------------------------
// Hunt 5 Skeleton Wolves
//--- Description -----------------------------------------------------------
// Twelfth hunting quest beginner quest series, started automatically
// after completing Hunt 5 Kobold Bandits.
//---------------------------------------------------------------------------

public class SkeletonWolvesQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(1000045);
		SetName("Hunt 5 Skeleton Wolves");
		SetDescription("I am Comgan, serving as a priest at Bangor. Evil creatures near Bangor make it difficult for travelers to travel. Can you please hunt 5 skeleton wolves? - Comgan -");
		
		SetReceive(Receive.Automatically);
		AddPrerequisite(Completed(1000044));

		AddObjective("kill_wolves", "Hunt 5 Skeleton Wolves", 30, 32006, 49139, Kill(5, "/skeletonwolf/"));

		AddReward(Exp(4200));
		AddReward(Item(19003, 1));
	}
}

