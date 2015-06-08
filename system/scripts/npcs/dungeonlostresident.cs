//--- Aura Script -----------------------------------------------------------
// Lost Resident
//--- Description -----------------------------------------------------------
// The NPC you rescue in the quest "Rescue Resident".
//---------------------------------------------------------------------------

public class DungeonLostResidentNpcScript : NpcScript
{
	public override void Load()
	{
		SetName("_dungeonlostresident");
		SetRace(1002);
		SetLocation(22, 6313, 5712);
	}

	protected override async Task Talk()
	{
		// Unofficial
		Msg("My hero! How can I ever repay you for this... How about a reward?", Button("Some gold maybe?", "@gold"), Button("An item!", "@item"));
		var reward = await Select();

		if (reward == "@gold")
		{
			Msg("Some money? Of course, here you go.");
			GiveGold(1000);
			GiveKeyword("TirChonaill_Tutorial_Judging");
		}
		else if(reward=="@item")
		{
			Msg("Please take this, may it bring you luck.");
			GiveItem(16009);
			GiveKeyword("TirChonaill_Tutorial_Perceiving");
		}

		GiveKeyword("Clear_Tutorial_Alby_Dungeon");
		Close2(Hide.None, "Thank you so much, now let's leave this horrible place...");

		// TODO: Cutscene "tuto_result"
		// TODO: Warp to reward room

		Exit();
	}
}
