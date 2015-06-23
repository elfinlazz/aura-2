//--- Aura Script -----------------------------------------------------------
// Rat AI
//--- Description -----------------------------------------------------------
// AI for rats.
//---------------------------------------------------------------------------

[AiScript("rat")]
public class RatAi : AiScript
{
	public RatAi()
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
		Do(Attack(3));
		Do(Wait(3000));
	}

	private IEnumerable OnDefenseHit()
	{
		Do(Attack(3));
		Do(Wait(3000));
	}

	private IEnumerable OnKnockDown()
	{
		var num = Random(100);

		if (num < 30) // 30%
		{
			Do(PrepareSkill(SkillId.Defense));
			Do(Wait(4000, 8000));
			Do(CancelSkill());
		}
		else if (num < 70) // 40%
		{
			Do(Wait(7000, 8000));
		}
		else // 30%
		{
			Do(Attack(1, 4000));
		}
	}
}
