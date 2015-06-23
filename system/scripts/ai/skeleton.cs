//--- Aura Script -----------------------------------------------------------
// Skeleton AI
//--- Description -----------------------------------------------------------
// AI for skeletons.
//---------------------------------------------------------------------------

[AiScript("skeleton")]
public class SkeletonAi : AiScript
{
	public SkeletonAi()
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
		Do(Timeout(2000, KeepDistance(400)));
		Do(Circle(300, 1000, 1000));

		var num = Random(100);

		if (num < 40) // 40%
		{
			Do(Attack(3));
		}
		else if (num < 60) // 20%
		{
			Do(PrepareSkill(SkillId.Smash));
			Do(Attack(1, 4000));
		}
		else if (num < 80) // 20%
		{
			Do(PrepareSkill(SkillId.Defense));
			Do(Follow(600, true));
			Do(CancelSkill());
		}
		else // 20%
		{
			Do(PrepareSkill(SkillId.Counterattack));
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
