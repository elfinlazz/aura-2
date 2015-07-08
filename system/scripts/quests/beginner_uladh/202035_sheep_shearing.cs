//--- Aura Script -----------------------------------------------------------
// Sheep-shearing
//--- Description -----------------------------------------------------------
// Deian asks the player to sheer some sheep and gather wool.
//---------------------------------------------------------------------------

public class SheepShearingQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(202035);
		SetName("Sheep-shearing");
		SetDescription("Oh... this is really boring. Whoever sees this, please come up here and hang out with me. I'm a handsome, lonely shepherd boy killing time with these disgusting sheep in the grassland. I need to shear some wool, but it's really boring. Desperately in need of someone to do this for me! Oh... remember to bring something you can shear with. If you try plucking the sheep with bare hands, that'll be the end for all of us. - Deian the Shepherd Boy -");

		SetReceive(Receive.Automatically);
		AddPrerequisite(Completed(202033)); // Gathering Berries

		AddObjective("talk_deian1", "Talk with Deian", 1, 27953, 42287, Talk("deian"));
		AddObjective("talk_deian2", "Deliver 5 bundles of wool to Deian", 1, 27953, 42287, Talk("deian"));

		AddReward(Exp(1500));
		AddReward(Gold(300));
		AddReward(Item(63001)); // Wings of a Goddess

		AddHook("_deian", "after_intro", TalkNpc);
	}

	public async Task<HookResult> TalkNpc(NpcScript npc, params object[] args)
	{
		if (npc.QuestActive(this.Id, "talk_deian1"))
		{
			npc.FinishQuest(this.Id, "talk_deian1");

			npc.Msg("Oh thank you for coming, I was dying from boredom...");
			npc.Msg("Did you bring a gathering knife? I don't seem to<br/>have any extra around here. If you didn't you'll need<br/>to go see Ferghus about that!");
			npc.Msg("In any case, I could really use your help again shearing all my sheep.<br/>Just hold the knife gently in your one hand and grab a tuft of wool with<br/>the other. Easy right? Hehe, well why do you think I don't want to do it?<br/>Can you gather five bundles of wool for me?");

			return HookResult.Break;
		}
		else if (npc.QuestActive(this.Id, "talk_deian2") && npc.HasItem(60009, 5))
		{
			npc.FinishQuest(this.Id, "talk_deian2");

			npc.Msg("Thank you, thank you! You look like a natural with that knife, I must say.<br/>These bundles of wool will help me out the rest of the day.<br/>Come by again if you ever want to get more wool!");

			npc.RemoveItem(60009, 5); // Wool
			npc.CompleteQuest(this.Id);

			return HookResult.Break;
		}

		return HookResult.Continue;
	}
}
