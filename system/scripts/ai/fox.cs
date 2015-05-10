//--- Aura Script -----------------------------------------------------------
//  Fox AI
//--- Description -----------------------------------------------------------
//  AI for foxes and fox-like creatures.
//---------------------------------------------------------------------------

[AiScript("fox")]
public class FoxAi : AiScript
{
	public FoxAi()
	{
		Doubts("/pc/", "/pet/");
		Hates("/chicken/");
		
		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
	}

	protected override IEnumerable Idle()
	{
		Do(Wander());
		Do(Wait(2000, 5000));
	}
	
	protected override IEnumerable Alert()
	{
		if(Random() < 50)
			Do(PrepareSkill(SkillId.Defense));
		Do(Circle(400, 1000, 5000));
		Do(Wait(2000, 5000));
		Do(CancelSkill());
	}
	
	protected override IEnumerable Aggro()
	{
		if(Random() < 50)
			Do(PrepareSkill(SkillId.Defense));
		else
			Do(Attack());
			
		if(Random() < 50)
			Do(Circle(400, 1000, 5000));
		else
			Do(Wait(3000));
		
		Do(CancelSkill());
	}
	
	private IEnumerable OnDefenseHit()
	{
		Do(Attack());
		Do(Wait(3000));
	}
}
