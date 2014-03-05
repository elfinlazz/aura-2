//--- Aura Script -----------------------------------------------------------
//  Wolf AI
//--- Description -----------------------------------------------------------
//  AI for wolves.
//---------------------------------------------------------------------------

public class WolfAi : AiScript
{
	public WolfAi()
	{
		SetAggroType(AggroType.CarefulAggressive);
		Hates("/pc/", "/pet/");
		Hates("/cattle/");
	}

	protected override IEnumerable Idle()
	{
		Do(Wander());
		Do(Wait(3000, 10000));
	}
	
	protected override IEnumerable Alert()
	{
		// 30% chance to circle
		// 10% chance to active defense
		switch(Random(10))
		{
			case 0:
			case 1:  
			case 2:  Do(Circle(400, 1000, 5000)); break;
			case 9:  Do(Say("Counter!")); break;
			default: break;
		}
			
		Do(Wait(6000, 8000));
	}
	
	protected override IEnumerable Aggro()
	{
		Do(Attack(3));
		Do(Wait(3000, 8000));
	}
}
