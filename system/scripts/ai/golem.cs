//--- Aura Script -----------------------------------------------------------
//  Golem AI
//--- Description -----------------------------------------------------------
//  AI for golems.
//---------------------------------------------------------------------------

public class GolemAi : AiScript
{
	public GolemAi()
	{
		Hates("/pc/", "/pet/");
		
		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
	}

	protected override IEnumerable Idle()
	{
		Do(StartSkill(SkillId.Rest));
		Do(Wait(1000000000));
	}
	
	protected override IEnumerable Aggro()
	{
		var rndn = Random();
		if(rndn < 10) // 10%
		{
			Do(PrepareSkill(SkillId.Defense));
			Do(Follow(600, true));
			Do(CancelSkill());
		}
		else if(rndn < 20) // 10%
		{
			Do(PrepareSkill(SkillId.Smash));
			Do(Attack(1, 5000));
			Do(Wait(3000, 8000));
		}
		else if(rndn < 40) // 20%
		{
			Do(PrepareSkill(SkillId.Stomp));
			Do(Wait(2000));
			Do(Say("*STOMP*"));
			Do(CancelSkill());
			Do(Wait(2000));
		}
		else  // 60%
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
