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

		if (!HasKeyword("TirChonaill_Tutorial_Judging") && !HasKeyword("TirChonaill_Tutorial_Perceiving"))
		{
			if (reward == "@gold")
			{
				Msg("Some money? Of course, here you go.");
				GiveGold(1000);
				GiveKeyword("TirChonaill_Tutorial_Judging");
			}
			else if (reward == "@item")
			{
				Msg("Please take this, may it bring you luck.");
				GiveItem(16009);
				GiveKeyword("TirChonaill_Tutorial_Perceiving");
			}
		}

		GiveKeyword("Clear_Tutorial_Alby_Dungeon");
		Msg("Thank you so much, now let's leave this horrible place...", Button("End Conversation"));
		await Select();
		Close2();

		var cutscene = new Cutscene("tuto_result", Player);
		cutscene.AddActor("me", Player);
		cutscene.AddActor("#lostresident", 1002);
		cutscene.Play(_ => WarpToRewardRoom());
	}

	// There's no reason for this function to be in the core if this is the
	// only place we ever need it at. It's literally used in *two* places
	// in official scripts.
	private void WarpToRewardRoom()
	{
		var dungeonRegion = Player.Region as Aura.Channel.World.Dungeons.DungeonRegion;
		if (dungeonRegion == null || dungeonRegion.Dungeon == null)
			return;

		var tileSize = Aura.Channel.World.Dungeons.Dungeon.TileSize;
		var lastFoor = dungeonRegion.Dungeon.Generator.Floors.Last();

		var x = lastFoor.MazeGenerator.EndPos.X * tileSize + tileSize / 2;
		var y = lastFoor.MazeGenerator.EndPos.Y * tileSize + tileSize * 2 + tileSize / 3;

		Player.Warp(dungeonRegion.Id, x, y);
	}
}
