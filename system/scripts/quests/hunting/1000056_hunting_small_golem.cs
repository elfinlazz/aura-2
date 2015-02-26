//--- Aura Script -----------------------------------------------------------
// Hunt 1 Small Golem
//--- Description -----------------------------------------------------------
// Second hunting quest Boss quest series, started automatically
// after Completing Hunt 1 Giant Spider.
//---------------------------------------------------------------------------

public class SmallGolemQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(1000056);
		SetName("Hunt 1 Small Golem");
		SetDescription("The golems normally are a pile of stones but turns into monsters when somebody goes near. It's not easy to face a golem but you look like you could face a small golem. Give away the Ciar Beginner Dungeon pass to the dungeon altar and then go in and hunt 1 small golem. Come back and let me know if you need another pass. - Ranald -");
		
		SetReceive(Receive.Automatically);
		AddPrerequisite(Completed(1000055));

		AddObjective("kill_golem", "Hunt 1 Small Golem", 11, 3213, 3209, Kill(1, "/SmallGolem3/"));

		AddReward(Exp(10000));
		AddReward(Item(16012, 1));
	}
}

