//--- Aura Script -----------------------------------------------------------
// Nao's Letter of Introduction
//--- Description -----------------------------------------------------------
// First quest in the Uladh beginner quest series, started when talking
// to Nao for the first time.
//---------------------------------------------------------------------------

public class NaosLetterQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(202001);
		SetName("Nao's Letter of Introduction");
		SetDescription("Dear [Chief Duncan],\r\nI am directing someone to you. This person is from another world. Please help them adjust to life in Erinn. Thank you, and I hope I will be able to visit you soon. - Nao Pryderi -");
		
		AddObjective("talk_duncan", "Go to Tir Chonaill and deliver the Letter to Chief Duncan.", 1, 15409, 38310, Talk("duncan"));

		AddReward(Exp(100));
		
		AddHook("_duncan", "after_intro", TalkDuncan);
	}
	
	public async Task<HookResult> TalkDuncan(NpcScript npc, params object[] args)
	{
		if(npc.QuestActive(this.Id, "talk_duncan"))
		{
			npc.FinishQuest(this.Id, "talk_duncan");
			
			npc.Msg(Hide.Name, "(You hand Nao's Letter of Introduction to Duncan.)");
			npc.Msg("Ah, a letter from Nao.<br/>Hard to believe that little<br/>tomboy's all grown up...");
			npc.Msg(Hide.Name, "(Duncan folds the letter in half and puts it in his pocket.)");
			npc.Msg("So, you're <username/>.<br/>I'm Duncan, the chief of this town.<br/>Welcome to Tir Chonaill.");
			npc.Msg("Would you like to learn how to complete quests?");
			npc.Msg(npc.Image("npctalk_questwindow", true, 272, 235), npc.Text("Press the "), npc.Hotkey("QuestView"), npc.Text(" key or<br/>press the Quest button at the bottom of your screen.<br>The quest window will appear and display your current quests."));

			while (true)
			{
				npc.Msg(npc.Text("Press the "), npc.Hotkey("QuestView"), npc.Text(" key or<br/>press the Quest button at the bottom of your screen."), npc.Button("I pressed the Quest button", "@pressed"), npc.Button("$hidden", "@quest_btn_clicked", "autoclick_QuestView"));
				if (await npc.Select() != "@pressed")
					break;
				npc.Msg("Hmm... Are you sure you pressed the Quest button?<br/>It's possible that the Quest Window was already open, so<br/>try pressing it again.");
			}

			npc.Msg("Well done. See the list of quests?<br/>Clicking on a quest brings up the quest's details.<br/>Quests will show a yellow Complete button<br/>next to their names when you finish them.");
			npc.Msg("Try pressing the Complete button now.<br/>As important as it is to complete quests,<br/>it's just as important to press the \"Complete\" button<br/>afterwards to recieve your rewards.");
			npc.Msg("(Duncan looks at you with his benevolent hazel eyes.)");
			npc.Msg("You've just learned one very basic skill<br/>to survive in Erinn.");
			npc.Msg("Soon, you will recieve a quest from an owl.<br/>Then, you will be able to start your training for real.");

			return HookResult.Break;
		}
		
		return HookResult.Continue;
	}
}
