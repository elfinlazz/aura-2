//--- Aura Script -----------------------------------------------------------
// Endelyon's Church Part-Time Job
//--- Description -----------------------------------------------------------
// Endelyon's adv barley harvest quest.
//---------------------------------------------------------------------------

public class EndelyonBarleyAdvPtjScript : QuestScript
{
	public override void Load()
	{
		SetId(502162);
		SetName("Church Part-Time Job");
		SetDescription("This task is to harvest grain from the farmland. Cut [20 bundles of barley] today. Barley can be harvested using a sickle on the farmlands around town.");
		
		SetType(QuestType.Deliver);
		SetPtjType(PtjType.Church);
		SetLevel(QuestLevel.Adv);
		SetHours(start: 12, report: 16, deadline: 21);
		
		AddObjective("ptj", "Harvest 20 Bundles of Barley", 0, 0, 0, Collect(52028, 20));

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
