//--- Aura Script -----------------------------------------------------------
//  Chicken AI
//--- Description -----------------------------------------------------------
//  AI for chickens.
//---------------------------------------------------------------------------

public class ChickenAi : AiScript
{
	public ChickenAi()
	{
		SetAggroRadius(400);
		
		Hates("/fox/");
		Loves("/hen/");
	}
	
	protected override IEnumerable Idle()
	{
		Do(Wander());
		Do(Wait(5000, 30000));
	}
	
	protected override IEnumerable Aggro()
	{
		Do(Attack(3));
		Do(Wait(3000));
	}
	
	protected override IEnumerable Love()
	{
		Do(Follow(300));
		Do(Wait(5000, 10000));
	}
}
