//--- Aura Script -----------------------------------------------------------
//  Mimic AI
//--- Description -----------------------------------------------------------
//  AI for mimics.
//---------------------------------------------------------------------------

[AiScript("mimic")]
public class MimicAi : AiScript
{
	public MimicAi()
	{
		//Doubts("/pc/");
		
		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
	}

	protected override IEnumerable Idle()
	{
		Do(Wait(1000000000));
	}
	
	protected override IEnumerable Aggro()
	{
		if(Random() < 75)
			Do(Attack(3, 4000));
		else
			Do(PrepareSkill(SkillId.Defense));
			
		if(Random() < 50)
			Do(Circle(500, 3000, 5000, false, false));
		else
			Do(Circle(500, 3000, 5000, true, false));
		
		Do(CancelSkill());
	}
	
	private IEnumerable OnDefenseHit()
	{
		Do(Attack());
	}
}
