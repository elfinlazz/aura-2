//--- Aura Script -----------------------------------------------------------
// Collect Prop Puzzle
//--- Description -----------------------------------------------------------
// Spawns herb patches in a room.
//---------------------------------------------------------------------------

using Aura.Channel.Scripting.Scripts;
using Aura.Channel.World.Dungeons.Props;
using Aura.Channel.World.Dungeons.Puzzles;
using Aura.Channel.World.Entities;

[PuzzleScript("eventprop_fountain")]
public class EventPropFountainScript : PuzzleScript
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

		Fountain fountain = null;

		if (Random(3) < 2)
		{
			if (CheckSpawn(puzzle, 10, 20, 30, 50))
				fountain = new Fountain("Fountain");
		}
		else
		{
			if (CheckSpawn(puzzle, 10, 15, 30, 0))
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
