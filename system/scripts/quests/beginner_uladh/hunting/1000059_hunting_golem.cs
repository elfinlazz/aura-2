//--- Aura Script -----------------------------------------------------------
// Hunt 1 Golem
//--- Description -----------------------------------------------------------
// Fifth hunting quest Boss quest series, started automatically
// after Completing Hunt 1 Black Succubus.
//---------------------------------------------------------------------------

public class GolemQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(1000059);
		SetName("Hunt 1 Golem");
		SetDescription("The golems normally are a pile of stones but turns into monsters when somebody goes near. It's not easy to face a Golem but you look like you could face a Golem - Ranald -");
		
		SetReceive(Receive.Automatically);
		AddPrerequisite(Completed(1000058));

		AddObjective("kill_golem", "Hunt 1 Golem", 11, 3213, 3209, Kill(1, "/golem/"));

		AddReward(Exp(40000));
		AddReward(Item(40262, 1));
	}
}

