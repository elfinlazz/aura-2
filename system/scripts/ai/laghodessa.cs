//--- Aura Script -----------------------------------------------------------
// Laghodessa AI
//--- Description -----------------------------------------------------------
// AI for Laghodessas.
// Official AI has WebSpinning because it's also used for some types
// of spiders.
//---------------------------------------------------------------------------

[AiScript("laghodessa")]
public class LaghodessaAi : AiScript
{
	public LaghodessaAi()
	{
		Hates("/pc/", "/pet/");

		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
	}

	protected override IEnumerable Idle()
	{
		Do(Wander());
		Do(Wait(2000, 5000));
	}

	protected override IEnumerable Alert()
	{
		Do(Follow(400, true));
	}

	protected override IEnumerable Aggro()
	{
		if (Random() < 33)
		{
			Do(PrepareSkill(SkillId.Defense));
			Do(Circle(400, 1000, 5000));
			Do(CancelSkill());
		}
		else
		{
			Do(Attack(3));
		}
	}

	private IEnumerable OnDefenseHit()
	{
		Do(Attack(3));
		Do(Wait(3000));
	}
}
