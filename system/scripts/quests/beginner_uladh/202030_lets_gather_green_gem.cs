//--- Aura Script -----------------------------------------------------------
// Let's gather Green Gem
//--- Description -----------------------------------------------------------
// Bebhinn asks the player to bring her a Small Green Gem.
//---------------------------------------------------------------------------

public class LetsGatherGreenGemQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(202030);
		SetName("Let's gather Green Gem");
		SetDescription("Hello, this is Bebhinn from the Bank in Tir Chonaill. These days I've taken a hobby of making accessories, and I was making necklaces when I realized I'm short on Small Green Gems. Can you get me 1? - Bebhinn -");

		SetReceive(Receive.Automatically);
		AddPrerequisite(ReachedLevel(6));

		AddObjective("talk_bebhinn", "Deliver 1 Small Green Gem to Bebhinn", 2, 1364, 1785, Talk("bebhinn"));

		AddReward(Exp(500));
		AddReward(Item(1602)); // Quest Guidebook Vol. 2

		AddHook("_bebhinn", "after_intro", TalkNpc);
	}

	public async Task<HookResult> TalkNpc(NpcScript npc, params object[] args)
	{
		if (npc.QuestActive(this.Id, "talk_bebhinn") && npc.HasItem(52004))
		{
			npc.FinishQuest(this.Id, "talk_bebhinn");

			npc.Msg("Oh hello! Looks like you got my message.");
			npc.Msg("Is that a Small Green Gem for me? Thank you!<br/>Now I can finish my necklace!<br/>I found this old guide book while waiting for your<br/>arrival. It's probably better in your hands.<button title='Continue' keyword='@continue'/>");
			await npc.Select();

			npc.RemoveItem(52004); // Small Green Gem
			npc.CompleteQuest(this.Id);

			return HookResult.Break;
		}

		return HookResult.Continue;
	}
}
