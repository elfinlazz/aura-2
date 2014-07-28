//--- Aura Script -----------------------------------------------------------
// Nao's Letter of Introduction
//--- Description -----------------------------------------------------------
// First quest in the Uladh beginner quest series, started when talking
// to Nao for the first time.
//---------------------------------------------------------------------------

public class BeginnerUladh1QuestScript : QuestScript
{
	public override void Load()
	{
		SetId(1000001);
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
			
			npc.Msg("(Duncan reads over Nao's letter.)<br/>Another Milletian! I'm always glad to meet a friend of Nao's.");
			npc.Msg("(Duncan folds the letter in half and puts it in his pocket.)<br/>So, you're <username/>?<br/>I'm Duncan, the chief of this town.<br/>Welcome to Tir Chonaill.");
			npc.Msg("Ah, it appears you've got a quest that needs to be completed.<br/>Let me show you how.");
			npc.Msg(npc.Image("npctalk_questwindow", true, 272, 235), npc.Text("Press the "), npc.Hotkey("QuestView"), npc.Text(" key or<br/>press the Quest button at the bottom of your screen.<br>The quest window will appear and display your current quests."));

			while (true)
			{
				npc.Msg(npc.Text("Press the "), npc.Hotkey("QuestView"), npc.Text(" key or the button that looks like a scroll<br/>at the bottom of your screen. This will bring up the main Quest window."), npc.Button("I pressed the Quest button", "@pressed"), npc.Button("$hidden", "@quest_btn_clicked", "autoclick_QuestView"));
				if (await npc.Select() != "@pressed")
					break;
				npc.Msg("Hmm... Are you sure you pressed the Quest button?<br/>It's possible that the Quest Window was already open, so<br/>try pressing it again.");
			}

            npc.Msg("There we are! THis window lets you check your progress and view<br/>the details on any quests you've accepted."); //Should show image of quest window, but I was having trouble finding it
			npc.Msg("When you complete a quest, you need to press the Complete button<br/>to receive your rewards.<p/>You must do this for every quest! Otherwise, it'll sit in your Quest window forever.");
			npc.Msg("Now go on, press the Complete button for the quest you have!");

			return HookResult.Break;
		}
		
		return HookResult.Continue;
	}
}
