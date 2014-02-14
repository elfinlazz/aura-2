
public class TestQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(1000000);
		SetName("Test");
		SetDescription("Do the test");
		SetReceive(Receive.Auto);
		
		AddObjective("talk_duncan", "Talk to Duncan", 1, 12000, 38000, Talk("duncan"));
		AddObjective("talk_duncan2", "Talk to Duncan", 1, 12000, 38000, Talk("duncan"));
		
		AddReward(Gold(1000));
	}
}
