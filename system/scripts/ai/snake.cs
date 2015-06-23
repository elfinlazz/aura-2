//--- Aura Script -----------------------------------------------------------
// Snake AI
//--- Description -----------------------------------------------------------
// AI for snakes.
//---------------------------------------------------------------------------

[AiScript("snake")]
public class SnakeAi : AiScript
{
	public SnakeAi()
	{
		Hates("/pc/", "/pet/");

		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
	}

	protected override IEnumerable Idle()
	{
		Do(Wait(2000, 10000));
	}

	protected override IEnumerable Alert()
	{
		if (Random(100) < 25)
			Do(PrepareSkill(SkillId.Defense));
		Do(Circle(600, 1000, 3000));
		Do(Wait(2000, 4000));
		Do(CancelSkill());
	}

	protected override IEnumerable Aggro()
	{
		if (Random(100) < 75)
			Do(Attack());
		else
			Do(PrepareSkill(SkillId.Defense));

		var num = Random(100);

		if (num < 40) // 40%
		{
			Do(Timeout(3000, KeepDistance(400, true)));
		}
		else if (num < 70) // 30%
		{
			Do(Timeout(3000, KeepDistance(700, false)));
		}
		else // 30%
		{
			Do(Wait(3000));
		}

		Do(CancelSkill());
	}

	private IEnumerable OnDefenseHit()
	{
		Do(Attack(3));
		Do(Wait(3000));
	}
}
