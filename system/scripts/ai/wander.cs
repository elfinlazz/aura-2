//--- Aura Script -----------------------------------------------------------
//  Wandering Creature AI
//--- Description -----------------------------------------------------------
//  Dummy AI for monsters, makes them wander around.
//---------------------------------------------------------------------------

[AiScript("wander")]
public class WanderingCreatureAI : AiScript
{
	protected override IEnumerable Idle()
	{
		Do(Wander());
		Do(Wait(5000, 10000));
	}
}
