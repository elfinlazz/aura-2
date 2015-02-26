//--- Aura Script -----------------------------------------------------------
// Hunt 5 Brown Dire Wolves
//--- Description -----------------------------------------------------------
// Sixth hunting quest beginner quest series, started automatically
// after reaching level 17? or completing Hunt 5 White Wolves .
//---------------------------------------------------------------------------

public class BrownDireWolvesQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(1000039);
		SetName("Hunt 5 Brown Dire Wolves");
		SetDescription("I think it's time you fight the beasts. Have you ever seen the brown dire wolf near Ciar Dungeon? Please hunt 5 brown dire wolves. Even without any reporting, I will pay you if you complete the mission. - Ranald-");
		
		SetReceive(Receive.Automatically);
		AddPrerequisite(Or(Completed(1000038), ReachedTotalLevel(17)));

		AddObjective("kill_wolves", "Hunt 5 Brown Dire Wolves", 1, 38800, 35066, Kill(5, "/browndirewolf/"));

		AddReward(Exp(1380));
		AddReward(Item(40013, 1));
	}
}

