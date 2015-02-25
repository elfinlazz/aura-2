//--- Aura Script -----------------------------------------------------------
// Endelyon's Church Part-Time Job
//--- Description -----------------------------------------------------------
// Endelyon's basic wheat harvest quest.
//---------------------------------------------------------------------------

public class EndelyonWheatBasicPtjScript : QuestScript
{
	public override void Load()
	{
		SetId(502103);
		SetName("Church Part-Time Job");
		SetDescription("This task is to harvest grain from the farmland. Cut [10 bundles of wheat] today. Use a sickle to harvest wheat from farmlands around town.");
		
		SetType(QuestType.Deliver);
		SetPtjType(PtjType.Church);
		SetLevel(QuestLevel.Basic);
		SetHours(start: 12, report: 16, deadline: 21);
		
		AddObjective("ptj", "Harvest 10 Bundles of Wheat", 0, 0, 0, Collect(52027, 10));

		AddReward(1, RewardGroupType.Item, QuestResult.Perfect, Exp(200));
		AddReward(1, RewardGroupType.Item, QuestResult.Perfect, Item(63016, 4));
		AddReward(1, RewardGroupType.Item, QuestResult.Mid, Exp(100));
		AddReward(1, RewardGroupType.Item, QuestResult.Mid, Item(63016, 2));
		AddReward(1, RewardGroupType.Item, QuestResult.Low, Exp(40));
		AddReward(1, RewardGroupType.Item, QuestResult.Low, Item(63016, 1));
	}
}
