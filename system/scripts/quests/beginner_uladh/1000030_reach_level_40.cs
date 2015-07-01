//--- Aura Script -----------------------------------------------------------
// Reach Level 40
//--- Description -----------------------------------------------------------
// Automatically started upon reaching Level 15, finished once reaching
// Level 40.
//---------------------------------------------------------------------------

public class ReachLevl40QuestScript : QuestScript
{
	public override void Load()
	{
		SetId(1000030);
		SetName("Reach Level 40");
		SetDescription("I am Chief Duncan of Tir Chonaill. How are you? I've heard about your name and fame as a heroic adventurer recently around here. Come to me when you reach level 40 and then I'll tell you something to help you with your training.");
		
		SetReceive(Receive.Automatically);
		AddPrerequisite(ReachedLevel(35));
		
		AddObjective("reach_level", "Reach Level 40", 0, 0, 0, ReachLevel(40));
		AddObjective("talk_duncan", "Talk with Chief Duncan", 1, 15409, 38310, Talk("duncan"));

		AddReward(Exp(15000));
		
		AddHook("_duncan", "after_intro", TalkDuncan);
	}
	
	public async Task<HookResult> TalkDuncan(NpcScript npc, params object[] args)
	{
		if(npc.QuestActive(this.Id, "talk_duncan"))
		{
			npc.FinishQuest(this.Id, "talk_duncan");
			
			// TODO: Get official dialog.
			npc.Msg("Good job reaching Level 40!");
			
			return HookResult.Break;
		}
		
		return HookResult.Continue;
	}
}
