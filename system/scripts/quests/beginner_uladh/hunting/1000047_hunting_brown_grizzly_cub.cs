//--- Aura Script -----------------------------------------------------------
// Hunt 1 Brown Grizzly Cub
//--- Description -----------------------------------------------------------
// Fourteenth hunting quest beginner quest series, started automatically
// after completing Hunt 5 Skeletons.
//---------------------------------------------------------------------------

public class BrownGrizzlyCubQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(1000047);
		SetName("Hunt 1 Brown Grizzly Cub");
		SetDescription("I'm Eavan from the Dunbarton Town Office. The Brown Grizzly Cubs from around Dunbarton is threatening the residents. It's not like the mother bear but you can't help the aggression by evil spirits of Fomor. The town of Dunbarton will reward yu for hunting the Brown Grizzly Cub. - Eavan -");
		
		SetReceive(Receive.Automatically);
		AddPrerequisite(Completed(1000046));

		AddObjective("kill_bear", "Hunt 1 Brown Grizzly Cub", 14, 23314, 23798, Kill(1, "/brown/grizzlybearkid/"));

		AddReward(Exp(6000));
		AddReward(Item(51003, 3));
	}
}

