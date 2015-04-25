//--- Aura Script -----------------------------------------------------------
// Reach Level 20
//--- Description -----------------------------------------------------------
// Automatically started upon reaching Level 15, finished once reaching
// Level 20.
//---------------------------------------------------------------------------

public class ReachLevl20QuestScript : QuestScript
{
	public override void Load()
	{
		SetId(1000028);
		SetName("Reach Level 20");
		SetDescription("I am Instructor Ranald. I heard you were training devotedly. Come to me when you reach level 20 and then I will give you something to help with your training.");
		
		SetReceive(Receive.Automatically);
		AddPrerequisite(ReachedLevel(15));
		
		AddObjective("reach_level", "Reach Level 20", 0, 0, 0, ReachLevel(20));
		AddObjective("talk_ranald", "Talk to Ranald", 1, 4651, 32166, Talk("ranald"));

		AddReward(Exp(5000));
		
		AddHook("_ranald", "after_intro", TalkRanald);
	}
	
	public async Task<HookResult> TalkRanald(NpcScript npc, params object[] args)
	{
		if(npc.QuestActive(this.Id, "talk_ranald"))
		{
			npc.FinishQuest(this.Id, "talk_ranald");
			
			// TODO: Get official dialog.
			npc.Msg("Good job reaching Level 20!");
			
			return HookResult.Break;
		}
		
		return HookResult.Continue;
	}
}
