//--- Aura Script -----------------------------------------------------------
//  Spider AI
//--- Description -----------------------------------------------------------
//  AI for normal spiders.
//---------------------------------------------------------------------------

public class SpiderAi : AiScript
{
	public SpiderAi()
	{
		Doubts("/pc/", "/pet/");
		
		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
	}

	protected override IEnumerable Idle()
	{
		Do(Wander());
		if(Random() < 25)
			Do(PrepareSkill(SkillId.WebSpinning));
		Do(Wait(2000, 5000));
	}
	
	protected override IEnumerable Alert()
	{
		Do(Follow(400, true));
	}
	
	protected override IEnumerable Aggro()
	{
		if(Random() < 50)
		{
			Do(PrepareSkill(SkillId.Defense));
			Do(Circle(400, 1000, 5000));
			Do(CancelSkill());
		}
		else
		{
			Do(Attack());
		}
	}
	
	private IEnumerable OnDefenseHit()
	{
		Do(Attack());
		Do(Wait(3000));
	}
}
