//--- Aura Script -----------------------------------------------------------
// Ore Deposit Room
//--- Description -----------------------------------------------------------
// Spawns ore deposits in a room.
//---------------------------------------------------------------------------

[PuzzleScript("collectprop_test")]
public class CollectPropTestScript : PuzzleScript
{
	public override void OnPrepare(Puzzle puzzle)
	{
		var propPlace = puzzle.NewPlace("PropPlace");
		propPlace.ReservePlace();
		propPlace.ReserveDoors();
	}

	public override void OnPuzzleCreate(Puzzle puzzle)
	{
		var propPlace = puzzle.GetPlace("PropPlace");

		for (int i = 1; i <= 3; ++i)
		{
			var oreDeposit = new OreDeposit(22000, "Deposit" + i);
			propPlace.AddProp(oreDeposit, Placement.Ore);
		}
	}
}
