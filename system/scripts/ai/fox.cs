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
		Hates("/pc/", "/pet/");
		Hates("/chicken/");
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
			case 1:  Do(Circle(400, 1000, 5000)); break;
			case 3:  Do(Circle(400, 1000, 5000)); break;
			case 9:  Do(Say("Defense!")); break;
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
