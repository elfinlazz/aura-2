//--- Aura Script -----------------------------------------------------------
//  Normal NPC AI
//--- Description -----------------------------------------------------------
//  AI for normal NPCs, that do nothing but saying random phrases.
//---------------------------------------------------------------------------

public class NormalNpcAi : AiScript
{
	public NormalNpcAi()
	{
		SetHeartbeat(500);
	}

	protected override IEnumerable Idle()
	{
		Do(SayRandomPhrase());
		Do(Wait(30000, 40000));
	}
}
