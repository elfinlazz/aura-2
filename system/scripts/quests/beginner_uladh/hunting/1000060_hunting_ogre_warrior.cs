//--- Aura Script -----------------------------------------------------------
// Hunt 1 Ogre Warrior
//--- Description -----------------------------------------------------------
// Sixth hunting quest Boss quest series, started automatically
// after Completing Hunt 1 Golem.
//---------------------------------------------------------------------------

public class OgreWarriorQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(1000060);
		SetName("Hunt 1 Ogre Warrior");
		SetDescription("I am Comgan, serving as a priest in Bangor. Evil creatures in Bangor Dungeon make it more difficult to gather minerals at the mine. Especially the ogre warrior, which is the biggest threat because it is the boss of the ogres. Can you please hunt 1 ogre warrior? - Comgan -");
		
		SetReceive(Receive.Automatically);
		AddPrerequisite(Completed(1000059));

		AddObjective("kill_ogre", "Hunt 1 Ogre Warrior", 32, 3192, 2891, Kill(1, "/ogrebros1/"));

		AddReward(Exp(50000));
		AddReward(Item(40030, 1));
		AddReward(Item(63016, 8));
	}
}

