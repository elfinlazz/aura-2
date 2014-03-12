//--- Aura Script -----------------------------------------------------------
//  Sheep AI
//--- Description -----------------------------------------------------------
//  AI for sheeps.
//---------------------------------------------------------------------------

public class SheepAi : AiScript
{
	protected override IEnumerable Idle()
	{
		Do(Wander());
		Do(Wait(3000, 10000));
	}
	
	protected override IEnumerable Alert()
	{
		Return();
	}
	
	protected override IEnumerable Aggro()
	{
		Do(Attack(3));
		Do(Wait(3000, 8000));
	}
}
