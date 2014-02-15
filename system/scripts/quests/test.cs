
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
		AddHook("_duncan", "after_intro", TalkDuncan2);
	}
	
	public IEnumerable TalkDuncan(NpcScript npc, params object[] args)
	{
		if(npc.QuestActive(this.Id, "talk_duncan"))
		{
			npc.Msg("test");
			npc.Msg("Right?", npc.Button("Yes"));
			npc.Select();
			npc.FinishQuest(this.Id, "talk_duncan");
			Return("break_hook");
		}
		else if(npc.QuestActive(this.Id, "talk_duncan2"))
		{
			npc.Msg("Well done!");
			npc.FinishQuest(this.Id, "talk_duncan2");
			Return("break_hook");
		}
	}
	
	public IEnumerable TalkDuncan2(NpcScript npc, params object[] args)
	{
		npc.Msg("*giggle*");
		Return();
	}
}
