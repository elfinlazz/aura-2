//--- Aura Script -----------------------------------------------------------
// Endelyon's Church Part-Time Job
//--- Description -----------------------------------------------------------
// Endelyon's int egg gathering quest.
//---------------------------------------------------------------------------

public class EndelyonEggsIntPtjScript : QuestScript
{
	public override void Load()
	{
		SetId(502135);
		SetName("Church Part-Time Job");
		SetDescription("This job is to gather eggs from chickens. Please bring [20 eggs] today. Gather eggs from the chickens around town.");
		
		SetType(QuestType.Deliver);
		SetPtjType(PtjType.Church);
		SetLevel(QuestLevel.Int);
		SetHours(start: 12, report: 16, deadline: 21);
		
		AddObjective("ptj", "Collect 20 Eggs", 0, 0, 0, Collect(50009, 20));

		AddReward(1, RewardGroupType.Item, QuestResult.Perfect, Exp(300));
		AddReward(1, RewardGroupType.Item, QuestResult.Perfect, Item(63016, 6));
		AddReward(1, RewardGroupType.Item, QuestResult.Mid, Exp(150));
		AddReward(1, RewardGroupType.Item, QuestResult.Mid, Item(63016, 3));
		AddReward(1, RewardGroupType.Item, QuestResult.Low, Exp(60));
		AddReward(1, RewardGroupType.Item, QuestResult.Low, Item(63016, 1));
	}
}
