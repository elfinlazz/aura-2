//--- Aura Script -----------------------------------------------------------
//  Hen AI
//--- Description -----------------------------------------------------------
//  AI for hens.
//---------------------------------------------------------------------------

public class HenAi : AiScript
{
	public HenAi()
	{
		SetAggroRadius(400);
		
		Hates("/fox/");
		Loves("/cock/");
	}
	
	protected override IEnumerable Idle()
	{
		Do(Wander());
		Do(Wait(8000));
		Do(StartSkill(SkillId.Rest));
		Do(Wait(2000, 30000));
		Do(StopSkill(SkillId.Rest));
	}
	
	protected override IEnumerable Aggro()
	{
		Do(Attack(3));
		Do(Wait(3000));
	}
	
	protected override IEnumerable Love()
	{
		Do(Follow(300, true));
		Do(Wait(5000, 10000));
		Do(StartSkill(SkillId.Rest));
		Do(Wait(10000));
		Do(StopSkill(SkillId.Rest));
	}
}
