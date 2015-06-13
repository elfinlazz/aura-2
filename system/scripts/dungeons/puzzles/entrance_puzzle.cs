//--- Aura Script -----------------------------------------------------------
// Entrance Puzzle
//--- Description -----------------------------------------------------------
// Used as first puzzle in every dungeon because <TODO: insert reason>.
//---------------------------------------------------------------------------

using Aura.Channel.Scripting.Scripts;
using Aura.Channel.World.Dungeons.Puzzles;

[PuzzleScript("entrance_puzzle")]
public class EntrancePuzzleScript : PuzzleScript
{
	public override void OnPrepare(IPuzzle puzzle)
	{
		var LockedPlace = puzzle.NewPlace("LockedPlace");
		var ChestPlace = puzzle.NewPlace("ChestPlace");

		LockedPlace.DeclareLock();
		ChestPlace.DeclareUnlock(LockedPlace);
		ChestPlace.ReservePlace();
		ChestPlace.ReserveDoors();

		var chest = puzzle.NewChest(ChestPlace, "Key_Chest", DungeonPropPositionType.Random);

		LockedPlace.CloseAllDoors();
		puzzle.LockPlace(LockedPlace, "Lock");

		chest.AddKeyForLock(LockedPlace);
	}

	public override void OnPropEvent(IPuzzle puzzle, IPuzzleProp prop, string propEvent)
	{
		var chest = prop as IPuzzleChest;
		if (chest != null)
		{
			if (chest.GetName() == "Key_Chest" && propEvent == "open")
				puzzle.GetPlace("ChestPlace").CloseAllDoors();
		}
	}
}
