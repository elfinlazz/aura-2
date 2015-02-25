//--- Aura Script -----------------------------------------------------------
// Endelyon's Church Part-Time Job
//--- Description -----------------------------------------------------------
// Endelyon's int barley harvest quest.
//---------------------------------------------------------------------------

public class EndelyonBarleyIntPtjScript : QuestScript
{
	public override void Load()
	{
		SetId(502132);
		SetName("Church Part-Time Job");
		SetDescription("This task is to harvest grain from the farmland. Cut [15 bundles of barley] today. Barley can be harvested using a sickle on the farmlands around town.");
		
		SetType(QuestType.Deliver);
		SetPtjType(PtjType.Church);
		SetLevel(QuestLevel.Int);
		SetHours(start: 12, report: 16, deadline: 21);
		
		AddObjective("ptj", "Harvest 15 Bundles of Barley", 0, 0, 0, Collect(52028, 15));

		AddReward(1, RewardGroupType.Item, QuestResult.Perfect, Exp(300));
		AddReward(1, RewardGroupType.Item, QuestResult.Perfect, Item(63016, 6));
		AddReward(1, RewardGroupType.Item, QuestResult.Mid, Exp(150));
		AddReward(1, RewardGroupType.Item, QuestResult.Mid, Item(63016, 3));
		AddReward(1, RewardGroupType.Item, QuestResult.Low, Exp(60));
		AddReward(1, RewardGroupType.Item, QuestResult.Low, Item(63016, 1));
	}
}
