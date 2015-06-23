//--- Aura Script -----------------------------------------------------------
// Rat Man AI
//--- Description -----------------------------------------------------------
// AI for Rat Man.
//---------------------------------------------------------------------------

[AiScript("ratman")]
public class RatManAi : AiScript
{
	public RatManAi()
	{
		Hates("/pc/", "/pet/");

		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
		On(AiState.Aggro, AiEvent.KnockDown, OnKnockDown);
	}

	protected override IEnumerable Idle()
	{
		Do(Wander());
		Do(Wait(2000, 5000));
	}

	protected override IEnumerable Aggro()
	{
		var num = Random(100);

		if (num < 30) // 30%
		{
			Do(Timeout(2000, KeepDistance(1000)));
			Do(Circle(600, 1000, 2000));
		}
		else if (num < 50) // 20%
		{
			Do(CancelSkill());
			Do(Attack(3));
		}
		else if (num < 70) // 20%
		{
			Do(PrepareSkill(SkillId.Smash));
			Do(CancelSkill());
			Do(Attack(3));
		}
		else if (num < 90) // 20%
		{
			Do(PrepareSkill(SkillId.Smash));
			Do(Attack(1, 4000));
		}
		else if (num < 95) // 5%
		{
			Do(PrepareSkill(SkillId.Defense));
			Do(Follow(600, true));
			Do(CancelSkill());
		}
		else // 5%
		{
			Do(PrepareSkill(SkillId.Counterattack));
			//Do(Follow(600, true));
			Do(CancelSkill());
		}
	}

	private IEnumerable OnDefenseHit()
	{
		Do(Attack(3));
		Do(Wait(3000));
	}

	private IEnumerable OnKnockDown()
	{
		Do(Attack(3));
		Do(Wait(3000));
	}
}
