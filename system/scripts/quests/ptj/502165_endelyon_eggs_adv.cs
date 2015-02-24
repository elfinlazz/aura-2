//--- Aura Script -----------------------------------------------------------
// Endelyon's Church Part-Time Job
//--- Description -----------------------------------------------------------
// Endelyon's adv egg gathering quest.
//---------------------------------------------------------------------------

public class EndelyonEggsAdvPtjScript : QuestScript
{
	public override void Load()
	{
		SetId(502165);
		SetName("Church Part-Time Job");
		SetDescription("This job is to gather eggs from chickens. Please bring [30 eggs] today. Gather eggs from the chickens around town.");
		
		SetType(QuestType.Deliver);
		SetPtjType(PtjType.Church);
		SetLevel(QuestLevel.Adv);
		SetHours(start: 12, report: 16, deadline: 21);
		
		AddObjective("ptj", "Collect 30 Eggs", 0, 0, 0, Collect(50009, 30));

		AddReward(1, RewardGroupType.Item, QuestResult.Perfect, Exp(500));
		AddReward(1, RewardGroupType.Item, QuestResult.Perfect, Item(63016, 10));
		AddReward(1, RewardGroupType.Item, QuestResult.Mid, Exp(250));
		AddReward(1, RewardGroupType.Item, QuestResult.Mid, Item(63016, 5));
		AddReward(1, RewardGroupType.Item, QuestResult.Low, Exp(100));
		AddReward(1, RewardGroupType.Item, QuestResult.Low, Item(63016, 2));
		AddReward(2, RewardGroupType.Item, QuestResult.Perfect, Item(40004, 1));
		AddReward(2, RewardGroupType.Item, QuestResult.Perfect, Item(19001, 1));
	}
}
