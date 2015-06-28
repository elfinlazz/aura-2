//--- Aura Script -----------------------------------------------------------
// Ranald's Armor Delivery
//--- Description -----------------------------------------------------------
// Ranald asks you to deliver an armor to Ferghus when you've got Defense,
// after which you get Windmill.
//---------------------------------------------------------------------------

public class RanaldsArmorDeliveryQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(20007);
		SetName("Ranald's Armor Delivery");
		SetDescription("If you deliver this armor to Ferghus, I'll give you help with the [Windmill skill]. - Ranald -");
		
		AddObjective("talk_ferghus", "Deliver Armor to Ferghus", 1, 18075, 29960, Talk("ferghus"));
		AddObjective("talk_ranald", "Talk to Ranald", 1, 4651, 32166, Talk("ranald"));

		AddReward(Exp(5));
		AddReward(Skill(SkillId.Windmill, SkillRank.RF));
		
		AddHook("_ranald", "before_keywords", RanaldKeywords);
		AddHook("_ferghus", "after_intro", TalkFerghus);
		AddHook("_ranald", "after_intro", TalkRanald);
	}
	
	public async Task<HookResult> RanaldKeywords(NpcScript npc, params object[] args)
	{
		var keyword = args[0] as string;
		
		if(keyword == "about_skill" && npc.HasSkill(SkillId.Defense) && !npc.HasSkill(SkillId.Windmill))
		{
			// Unofficial
			npc.Msg("You would like to learn more? That's the attitude!");
			npc.Msg("Unfortunately I'm a little busy right now, I've borrowed an armor from Ferghus that I have to get back to him. Unless...");
			npc.Msg("Could you deliver it to him for me? Afterwards I'll give you help with the Windmill skill.<button title='End Conversation' keyword='@end'/>");
			await npc.Select();
			npc.StartQuest(this.Id);
			//npc.GiveItem(70002); // Full Ring Mail to be Delivered (TODO: implement quest item handling)
			npc.Close();
			
			return HookResult.End;
		}
		
		return HookResult.Continue;
	}
	
	public async Task<HookResult> TalkFerghus(NpcScript npc, params object[] args)
	{
		if(npc.QuestActive(this.Id, "talk_ferghus"))
		{
			npc.FinishQuest(this.Id, "talk_ferghus");
			
			// Unofficial
			npc.Msg("Ah, my armor. Thank you, <username/>.");
			//npc.RemoveItem(70002); // Full Ring Mail to be Delivered
			
			return HookResult.Break;
		}
		
		return HookResult.Continue;
	}
	
	public async Task<HookResult> TalkRanald(NpcScript npc, params object[] args)
	{
		if(npc.QuestActive(this.Id, "talk_ranald"))
		{
			npc.FinishQuest(this.Id, "talk_ranald");
			npc.CompleteQuest(this.Id);
			
			// Unofficial
			npc.Msg("Thank you, <username/>. Now that that's out of the way, let me teach you all about the Windmill skill.");
			npc.Msg(Hide.Name, "... ... ..."); // He should probably say something about Windmill...
			npc.Msg("And that's it! Never stop training.");
			npc.End();
			
			return HookResult.End;
		}
		
		return HookResult.Continue;
	}
}
