//--- Aura Script -----------------------------------------------------------
// Fountain Puzzle
//--- Description -----------------------------------------------------------
// Spawns a blue or red fountain by a random chance, either in a room
// or in an alley.
//---------------------------------------------------------------------------

[PuzzleScript("eventprop_fountain")]
public class EventPropFountainScript : PuzzleScript
{
	public override void OnPrepare(Puzzle puzzle)
	{
		var propPlace = puzzle.NewPlace("PropPlace");
		propPlace.ReservePlace();

		if (Random(3) == 0)
			propPlace.ReserveDoors();
	}

	public override void OnPuzzleCreate(Puzzle puzzle)
	{
		var propPlace = puzzle.GetPlace("PropPlace");

		Fountain fountain = null;

		if (Random(3) < 2)
		{
			if (CheckSpawn(puzzle, 10, 20, 30, 50))
				fountain = new Fountain("Fountain");
		}
		else
		{
			if (CheckSpawn(puzzle, 0, 10, 15, 30))
				fountain = new RedFountain("Fountain");
		}

		if (fountain != null)
			propPlace.AddProp(fountain, Placement.Center);
	}

	private bool CheckSpawn(Puzzle puzzle, int def, int low, int middle, int high)
	{
		var chance = def;

		if (puzzle.Dungeon.Name.Contains("_low_"))
			chance = low;
		else if (puzzle.Dungeon.Name.Contains("_middle_"))
			chance = middle;
		else if (puzzle.Dungeon.Name.Contains("_high_"))
			chance = high;

		return (Random() < chance);
	}
}
