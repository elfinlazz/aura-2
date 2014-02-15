
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
		
		AddHook("_duncan", "after_intro", TalkDuncan);
		AddHook("_duncan", "before_keywords", TalkDuncan2);
	}
	
	public IEnumerable TalkDuncan(NpcScript npc, params object[] args)
	{
		npc.Msg("test");
		npc.Msg("Right?", npc.Button("Yes"));
		npc.Select();
		Return();
	}
	
	public IEnumerable TalkDuncan2(NpcScript npc, params object[] args)
	{
		var keyword = args[0] as string;
		if(keyword == "breast")
		{
			npc.Msg("*giggle*");
			Return("end");
		}
	}
}
