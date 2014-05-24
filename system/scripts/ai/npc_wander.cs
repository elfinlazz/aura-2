//--- Aura Script -----------------------------------------------------------
//  Wander NPC AI
//--- Description -----------------------------------------------------------
//  AI for wandering NPCs, random phrases and random walking.
//---------------------------------------------------------------------------

public class WanderNpcAi : AiScript
{
	public WanderNpcAi()
	{
		SetHeartbeat(500);
		SetMaxDistanceFromSpawn(500);
	}

	protected override IEnumerable Idle()
	{
		if(Random() < 50)
			Do(SayRandomPhrase());
		else
			Do(Wander());
		Do(Wait(20000, 50000));
	}
}
