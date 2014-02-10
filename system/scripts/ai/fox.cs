//--- Aura Script -----------------------------------------------------------
//  Fox AI
//--- Description -----------------------------------------------------------
//  AI for foxes and fox-like creatures.
//---------------------------------------------------------------------------

public class FoxAi : AiScript
{
	public FoxAi()
	{
		SetAggroType(AggroType.Careful);
	}

	protected override IEnumerable Idle()
	{
		Do(Wander());
		Do(Wait(3000, 10000));
	}
	
	protected override IEnumerable Alert()
	{
		switch(Random(10))
		{
			case 0:  Do(Circle(400, 1000, 5000)); break;
			case 1:  Do(Say("Defense!")); break;
			default: break;
		}
			
		Do(Wait(6000, 8000));
	}
	
	protected override IEnumerable Aggro()
	{
		Do(Say("Attack!"));
		Do(Wait(3000, 6000));
	}
}
