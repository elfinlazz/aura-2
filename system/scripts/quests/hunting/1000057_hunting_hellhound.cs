//--- Aura Script -----------------------------------------------------------
// Hunt 1 Hellhound
//--- Description -----------------------------------------------------------
// Third hunting quest Boss quest series, started automatically
// after Completing Hunt 1 Small Golem.
//---------------------------------------------------------------------------

public class HellhoundQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(1000057);
		SetName("Hunt 1 Hellhound");
		SetDescription("The hellhound of Math Dungeon is not easy to fight. They are strong creatures that don't allow you to make any mistakes. But don't you think that's why it's worth challenging yourself? - Aranwen -");
		
		SetReceive(Receive.Automatically);
		AddPrerequisite(Completed(1000056));

		AddObjective("kill_hound", "Hunt 1 Hellhound", 25, 3200, 3426, Kill(1, "/hellhound/"));

		AddReward(Exp(20000));
		AddReward(Item(40012, 1));
	}
}

