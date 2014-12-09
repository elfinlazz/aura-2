//--- Aura Script -----------------------------------------------------------
// Let's Learn the Resting Skill
//--- Description -----------------------------------------------------------
// Started automatically after finishing "Rescue Resident".
//---------------------------------------------------------------------------

public class BeginnerUladhRestQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(1000003);
		SetName("Let's Learn the Resting Skill");
		SetDescription("This is Nora, working for the Inn. Welcome to Tir Chonaill. All Travelers seem to sleep outdoors. They just show up and vanish. How can we make money at this rate!? Teee Heee... Well, it's no secret.... I'll let you know the Resting Skill. - Nora -");
		
		SetReceive(Receive.Automatically);
		AddPrerequisite(Completed(202002));
		AddPrerequisite(NotSkill(SkillId.Rest));

		AddObjective("get_rest_rf", "Acquire Resting Skill Rank F", 1, 15933, 33363, ReachRank(SkillId.Rest, SkillRank.RF));
		
		AddReward(Exp(700));
		AddReward(Item(1601)); // Skill Guide Book
		
		AddHook("_nora", "after_intro", TalkNora);
		AddHook("_nora", "before_keywords", NoraKeywords);
	}
	
	public async Task<HookResult> TalkNora(NpcScript npc, params object[] args)
	{
		if(npc.QuestActive(this.Id) && !npc.Player.Skills.Has(SkillId.Rest))
		{
			npc.Msg("If you came here because of the mail you received about the Resting skill,<br/>we need to talk about it first.<br/>You know you should ask me with the 'Skills' keyword, right?");
			return HookResult.Break;
		}
		
		return HookResult.Continue;
	}
	
	public async Task<HookResult> NoraKeywords(NpcScript npc, params object[] args)
	{
		var keyword = args[0] as string;
		
		if(keyword == "about_skill" && !npc.Player.Skills.Has(SkillId.Rest))
		{
			npc.Player.Skills.Give(SkillId.Rest, SkillRank.Novice);
			npc.Player.Keywords.Give("skill_rest");
			
			npc.Msg("You are at the Inn. This is where weary travelers rest.<br/>It's important to rejuvenate yourself both mentally and physically by resting.<br/>Do you know about the Resting skill?<br/>If not, I'll tell you about it.");
			npc.Msg("Now, open the Skill window. Press the 'Skills' button at the bottom of the screen.<br/>Or, just press 'S'.");
			npc.Msg("Do you see the Resting skill? You can't use it now because you're talking to me.<br/>You can activate it from the Skill window or drag-and-drop it at the top of the screen and use the Function keys as hotkeys.<br/>In my case, I use F1 to activate it.");
			npc.Msg("You usually use the Resting skill when you need to fill your Stamina.<br/>But it is also useful to recover HP,<br/>and it's good to heal wounds, although it does take longer.", npc.Image("skill_rest"));
			
			npc.Msg("Anyway, I don't understand why so many people ask me about skills.");
			await npc.Conversation();
		}
		
		return HookResult.Continue;
	}
}
