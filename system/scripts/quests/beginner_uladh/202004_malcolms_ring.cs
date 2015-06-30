//--- Aura Script -----------------------------------------------------------
// Malcolm's Ring
//--- Description -----------------------------------------------------------
// The player is asked by Malcolm to go into a special version of Alby to
// to get a ring from the boss that he lost there.
//---------------------------------------------------------------------------

public class MalcolmsRingQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(202004);
		SetName("Malcolm's Ring");
		SetDescription("I'm Malcolm. I sell a variety of stuff at the General Shop near the Square. I happened to lose a ring in Alby Dungeon, but I cannot go find it myself. Can you help me find the ring? Come visit me first, though. - Malcolm -");
		
		SetReceive(Receive.Automatically);
		AddPrerequisite(Completed(202003)); // Save my Sheep

		AddObjective("talk_malcolm1", "Talk with Malcolm at the Tir Chonaill General Shop", 8, 1238, 1655, Talk("malcolm"));
		AddObjective("kill_spider", "Defeat the Giant Golden Spiderling in Alby Dungeon and get the Ring", 13, 3200, 3200, Kill(1, "/giantgoldenspiderkid/"));
		AddObjective("talk_malcolm2", "Give the Ring to Malcolm", 8, 1238, 1655, Talk("malcolm"));

		AddReward(Exp(1500));
		AddReward(Gold(1700));
		AddReward(Item(2001));
		AddReward(AP(3));

		AddHook("_malcolm", "after_intro", TalkMalcolm);
	}

	public async Task<HookResult> TalkMalcolm(NpcScript npc, params object[] args)
	{
		if (npc.QuestActive(this.Id, "talk_malcolm1"))
		{
			npc.FinishQuest(this.Id, "talk_malcolm1");
			
			npc.Msg("So, you received the quest I sent through the Owl.<br/>Thanks for coming.<br/>I think I lost my ring in Alby Dungeon,<br/>but I can't leave, because I have no one to take care of the General Shop.");
			npc.Msg("I know it's a lot to ask, but can you go find the ring for me?<br/>The dungeon is very dangerous so I suggest talking to Trefor first about the Counterattack skill.<br/><br/>Take this pass to enter the dungeon, and please find my ring.");
			npc.GiveItem(63181); // Malcolm's Pass
			npc.GiveKeyword("skill_counter_attack");

			return HookResult.End;
		}
		else if (npc.QuestActive(this.Id, "talk_malcolm2"))
		{
			npc.FinishQuest(this.Id, "talk_malcolm2");
			npc.GiveKeyword("Clear_Tutorial_Malcolm_Ring");
			npc.RemoveItem(75058); // Malcolm's Ring

			npc.Msg("You found my Ring!<br/>You have my thanks.");

			return HookResult.Break;
		}
		
		return HookResult.Continue;
	}
}
