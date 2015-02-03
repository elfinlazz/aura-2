//--- Aura Script -----------------------------------------------------------
// Duncan's Call (aka Getting rid of Eiry)
//--- Description -----------------------------------------------------------
// The quest in which you trade Eiry for Fluted Short Sword,
// received automatically at total level 26.
//---------------------------------------------------------------------------

public class DuncansCallQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(202006);
		SetName("Duncan's Call");
		SetDescription("Looks like you're living a good life. Come visit me when you have the time. - Duncan -");
		
		SetReceive(Receive.Automatically);
		AddPrerequisite(ReachedTotalLevel(26));
		
		AddObjective("talk_duncan", "Talk to Chief Duncan.", 1, 15409, 38310, Talk("duncan"));

		AddReward(Exp(500));
		AddReward(Gold(1400));
		
		AddHook("_duncan", "after_intro", TalkDuncan);
	}
	
	public async Task<HookResult> TalkDuncan(NpcScript npc, params object[] args)
	{
		if(npc.QuestActive(this.Id, "talk_duncan"))
		{
			npc.FinishQuest(this.Id, "talk_duncan");
			
			Send.Effect(npc.NPC, Effect.ScreenFlash, 3000, 0);
			
			// Remove Eiry
			var eiry = npc.Player.Inventory.Items.FirstOrDefault(item => item.EgoInfo.Race == EgoRace.EirySword);
			if(eiry != null)
				npc.Player.Inventory.Remove(eiry);
			
			// Give sword
			npc.GiveItem(40015);
			npc.Notice("Received Fluted Short Sword from Duncan.");
			
			npc.Msg("Welcome to Tir Chonaill.");
			npc.Msg("Oh, you are finally here, <username/>.<br/>I've heard a lot about you from the villagers.<br/>You've leveled up quite a lot.");
			npc.Msg("This may be a small town,<br/>but Tir Chonaill has long boasted a tradition<br/>of growing resolute and sturdy warriors<br/>that don't have to worry about the kingdom's control.");
			npc.Msg("You have now completed all our missions<br/>and have earned the right to join the rank of Tir Chonaill's warriors.<br/>Well done, and congratulations.");
			npc.Msg("Wherever you go, don't forget who you are and what you have become.<br/>Since you have become strong enough to survive alone,<br/>your ego guide must have already parted ways with you.<br/>There is no one to look after you now, so befriend as many people as you can.");
			npc.Msg("If you have anything else to ask, let me know.");
			await npc.Conversation();
			
			npc.Close2();
			
			var cutscene = new Cutscene("etc_event_ego_goodbye", npc.Player);
			cutscene.AddActor("me", npc.Player);
			cutscene.Play();
			
			return HookResult.Break;
		}
		
		return HookResult.Continue;
	}
}
