//--- Aura Script -----------------------------------------------------------
// Gathering Berries
//--- Description -----------------------------------------------------------
// Dilys asks the player to bring her a Berry.
//---------------------------------------------------------------------------

public class GatheringBerriesQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(202033);
		SetName("Gathering Berries");
		SetDescription("I heard berries are good for losing weight. Can you bring me 1 berry? - Dilys -");

		SetReceive(Receive.Automatically);
		AddPrerequisite(Completed(202030)); // Let's gather Green Gem

		// There seems to be a second, later version of this quest,
		// that doesn't require the first objective (204004).
		AddObjective("talk_dilys1", "Talk with Dilys", 1, 13650, 44611, Talk("dilys"));
		AddObjective("talk_dilys2", "Deliver a Berry to Dilys", 1, 13650, 44611, Talk("dilys"));

		AddReward(Exp(800));
		AddReward(Item(51012, 5)); // Stamina 30 Potion

		AddHook("_dilys", "after_intro", TalkNpc);
	}

	public async Task<HookResult> TalkNpc(NpcScript npc, params object[] args)
	{
		if (npc.QuestActive(this.Id, "talk_dilys1"))
		{
			npc.FinishQuest(this.Id, "talk_dilys1");

			npc.Msg("There's been talk recently about how healthy berries are.<br/>Their qualities have peaked my interest, even about the weight!");
			npc.Msg("Can you bring me one berry? I'd be very grateful.");

			return HookResult.Break;
		}
		else if (npc.QuestActive(this.Id, "talk_dilys2") && npc.HasItem(50007))
		{
			npc.FinishQuest(this.Id, "talk_dilys2");

			npc.Msg("Oh thank you so much! I can't wait to try it!<br/>Here, as an exchange, take these potions I've been working on.<button title='Continue' keyword='@continue'/>");
			await npc.Select();

			npc.RemoveItem(50007); // Berry
			npc.CompleteQuest(this.Id);

			return HookResult.Break;
		}

		return HookResult.Continue;
	}
}
