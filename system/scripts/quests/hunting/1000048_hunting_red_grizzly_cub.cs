//--- Aura Script -----------------------------------------------------------
// Hunt 1 Red Grizzly Cub
//--- Description -----------------------------------------------------------
// Fifteenth hunting quest beginner quest series, started automatically
// after completing Hunt 1 Brown Grizzly Cub.
//---------------------------------------------------------------------------

public class RedGrizzlyCubQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(1000048);
		SetName("Hunt 1 Red Grizzly Cub");
		SetDescription("I'm Eavan from the Dunbarton Town Office. The Red Grizzly Cubs from around Dunbarton is threatening travelers. Dunbarton Town will reward you for hunting 1 Red Grizzly Cub. - Eavan -");
		
		SetReceive(Receive.Automatically);
		AddPrerequisite(Completed(1000047));

		AddObjective("kill_bear", "Hunt 1 Red Grizzly Cub", 14, 21365, 24373, Kill(1, "/red/grizzlybearkid/"));

		AddReward(Exp(6600));
		AddReward(Item(51013, 3));
	}
}

