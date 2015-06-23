//--- Aura Script -----------------------------------------------------------
// Bat AI
//--- Description -----------------------------------------------------------
// AI for bats.
//---------------------------------------------------------------------------

[AiScript("bat")]
public class BatAi : AiScript
{
	public BatAi()
	{
		Doubts("/pc/", "/pet/");

		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
		On(AiState.Aggro, AiEvent.KnockDown, OnKnockDown);
	}

	protected override IEnumerable Idle()
	{
		Do(Wander());
		Do(Wait(2000, 5000));
	}

	protected override IEnumerable Alert()
	{
		if (Random(100) < 25)
			Do(PrepareSkill(SkillId.Defense));
		Do(Circle(300, 1000, 3000));
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

		if (num < 20) // 20%
			Do(Circle(600, 1000, 3000));
		else if (num < 70) // 50%
			Do(Timeout(2000, KeepDistance(400, Random(100) < 60)));
		else // 30%
			Do(Wait(3000, 3000));

		Do(CancelSkill());
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
