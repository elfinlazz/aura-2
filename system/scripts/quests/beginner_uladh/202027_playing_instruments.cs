//--- Aura Script -----------------------------------------------------------
// Playing Instruments
//--- Description -----------------------------------------------------------
// Endelyon gifts the player a Lute to learn playing instruments.
//---------------------------------------------------------------------------

public class PlayingInstrumentsQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(202027);
		SetName("Playing Instruments");
		SetDescription("This is Endelyon of the Church. If you are interested in instruments, let me know. - Endelyon -");

		SetReceive(Receive.Automatically);
		AddPrerequisite(Completed(202004)); // Malcolm's Ring

		AddObjective("talk_endelyon", "Talk with Endelyon at the Church", 1, 5975, 36842, Talk("endelyon"));

		AddReward(Exp(900));
		if (IsEnabled("G1EasyOverseas"))
			AddReward(AP(2)); // 2 AP in EU and during "g1_easy_overseas" (212027)

		AddHook("_endelyon", "after_intro", TalkNpc);
	}

	public async Task<HookResult> TalkNpc(NpcScript npc, params object[] args)
	{
		if (npc.QuestActive(this.Id, "talk_endelyon"))
		{
			npc.FinishQuest(this.Id, "talk_endelyon");

			npc.Msg("Ah, you're here.<br/>May the blessings of Lymilark be with you in every step of the way. There are some people who think it to be difficult.<br/>Just hold on to the instrument, and you'll be able to play a semblance of music.<br/>Of course, it requires hours of practice if you plan on bringing tears to your special someone.<button title='End Conversatin' keyword='@end'/>");
			await npc.Select();

			npc.AcquireItem(40004); // Lute
			npc.End();

			return HookResult.End;
		}

		return HookResult.Continue;
	}
}
