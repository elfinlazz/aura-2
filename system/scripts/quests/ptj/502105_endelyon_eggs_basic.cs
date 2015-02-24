//--- Aura Script -----------------------------------------------------------
// Endelyon's Church Part-Time Job
//--- Description -----------------------------------------------------------
// Endelyon's basic egg gathering quest.
//---------------------------------------------------------------------------

public class EndelyonEggsBasicPtjScript : QuestScript
{
	public override void Load()
	{
		SetId(502105);
		SetName("Church Part-Time Job");
		SetDescription("This job is to gather eggs from chickens. Please bring [15 eggs] today. Gather eggs from the chickens around town.");
		
		SetType(QuestType.Deliver);
		SetPtjType(PtjType.Church);
		SetLevel(QuestLevel.Basic);
		SetHours(start: 12, report: 16, deadline: 21);
		
		AddObjective("ptj", "Collect 15 Eggs", 0, 0, 0, Collect(50009, 15));

		AddReward(1, RewardGroupType.Item, QuestResult.Perfect, Exp(200));
		AddReward(1, RewardGroupType.Item, QuestResult.Perfect, Item(63016, 4));
		AddReward(1, RewardGroupType.Item, QuestResult.Mid, Exp(100));
		AddReward(1, RewardGroupType.Item, QuestResult.Mid, Item(63016, 2));
		AddReward(1, RewardGroupType.Item, QuestResult.Low, Exp(40));
		AddReward(1, RewardGroupType.Item, QuestResult.Low, Item(63016, 1));
	}
}
