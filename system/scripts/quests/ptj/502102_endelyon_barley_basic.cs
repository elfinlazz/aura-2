//--- Aura Script -----------------------------------------------------------
// Endelyon's Church Part-Time Job
//--- Description -----------------------------------------------------------
// Endelyon's basic barley harvest quest.
//---------------------------------------------------------------------------

public class EndelyonBarleyBasicPtjScript : QuestScript
{
	public override void Load()
	{
		SetId(502102);
		SetName("Church Part-Time Job");
		SetDescription("This task is to harvest grain from the farmland. Cut [10 bundles of barley] today. Barley can be harvested using a sickle on the farmlands around town.");
		
		SetType(QuestType.Deliver);
		SetPtjType(PtjType.Church);
		SetLevel(QuestLevel.Basic);
		SetHours(start: 12, report: 16, deadline: 21);
		
		AddObjective("ptj", "Harvest 10 Bundles of Barley", 0, 0, 0, Collect(52028, 10));

		AddReward(1, RewardGroupType.Item, QuestResult.Perfect, Exp(200));
		AddReward(1, RewardGroupType.Item, QuestResult.Perfect, Item(63016, 4));
		AddReward(1, RewardGroupType.Item, QuestResult.Mid, Exp(100));
		AddReward(1, RewardGroupType.Item, QuestResult.Mid, Item(63016, 2));
		AddReward(1, RewardGroupType.Item, QuestResult.Low, Exp(40));
		AddReward(1, RewardGroupType.Item, QuestResult.Low, Item(63016, 1));
	}
}
