//--- Aura Script -----------------------------------------------------------
// Reach Level 30
//--- Description -----------------------------------------------------------
// Automatically started upon reaching Level 25, finished once reaching
// Level 30.
//---------------------------------------------------------------------------

public class ReachLevl30QuestScript : QuestScript
{
	public override void Load()
	{
		SetId(1000029);
		SetName("Reach Level 30");
		SetDescription("I am Aranwen from the Dunbarton School. I heard you have been training very hard lately. Come to me when you reach Level 30 and then I will tell you something that will be of help to you.");
		
		SetReceive(Receive.Automatically);
		AddPrerequisite(ReachedLevel(25));
		
		AddObjective("reach_level", "Reach Level 30", 0, 0, 0, ReachLevel(30));
		AddObjective("talk_aranwen", "Talk with Aranwen from the Dunbarton School", 14, 43378, 40048, Talk("aranwen"));

		AddReward(Exp(10000));
		
		AddHook("_aranwen", "after_intro", TalkAranwen);
	}
	
	public async Task<HookResult> TalkAranwen(NpcScript npc, params object[] args)
	{
		if(npc.QuestActive(this.Id, "talk_aranwen"))
		{
			npc.FinishQuest(this.Id, "talk_aranwen");
			
			// TODO: Get official dialog.
			npc.Msg("Good job reaching Level 30!");
			
			return HookResult.Break;
		}
		
		return HookResult.Continue;
	}
}
