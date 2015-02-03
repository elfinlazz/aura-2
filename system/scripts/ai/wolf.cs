//--- Aura Script -----------------------------------------------------------
//  Wolf AI
//--- Description -----------------------------------------------------------
//  AI for wolves.
//---------------------------------------------------------------------------

public class WolfAi : AiScript
{
	public WolfAi()
	{
		SetAggroRadius(650);
		
		Doubts("/pc/", "/pet/");
		Doubts("/cow/");
		Hates("/sheep/");
		Hates("/dog/");
		HatesBattleStance();
		
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
		{
			if(Random() < 50)
			{
				Do(PrepareSkill(SkillId.Defense));
				Do(Circle(500, 1000, 5000));
				Do(CancelSkill());
			}
			else
			{
				Do(PrepareSkill(SkillId.Counterattack));
				Do(Wait(5000));
				Do(CancelSkill());
			}
		}
		else
		{
			Do(Circle(400, 1000, 5000));
			Do(Wait(1000, 5000));
		}
	}
	
	protected override IEnumerable Aggro()
	{
		if(Random() < 50)
		{
			var rndnum = Random();
			if(rndnum < 20) // 20%
			{
				Do(PrepareSkill(SkillId.Defense));
				Do(Circle(500, 1000, 5000));
				Do(CancelSkill());
			}
			else if(rndnum < 60) // 40%
			{
				Do(PrepareSkill(SkillId.Smash));
				Do(Attack(1, 5000));
				Do(Wait(3000, 8000));
			}
			else // 40%
			{
				Do(PrepareSkill(SkillId.Counterattack));
				Do(Wait(5000));
				Do(CancelSkill());
			}
		}
		else
		{
			Do(Attack(3, 5000));
		}
	}
	
	private IEnumerable OnDefenseHit()
	{
		Do(Attack());
		Do(Wait(3000));
	}
}
