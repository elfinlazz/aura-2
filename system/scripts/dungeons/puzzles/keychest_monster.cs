//--- Aura Script -----------------------------------------------------------
// Keychest Monster Puzzle
//--- Description -----------------------------------------------------------
// Used as 
//---------------------------------------------------------------------------

using Aura.Channel.Scripting.Scripts;
using Aura.Channel.World.Dungeons.Puzzles;

[PuzzleScript("keychest_monster")]
public class KeychestMonsterScript : PuzzleScript
{
	public override void OnPrepare(IPuzzle puzzle)
	{
		var LockedPlace = puzzle.NewPlace("LockedPlace");
		var ChestPlace = puzzle.NewPlace("ChestPlace");

		LockedPlace.DeclareLock();
		ChestPlace.DeclareUnlock(LockedPlace);
		ChestPlace.ReservePlace();
		ChestPlace.ReserveDoors();

		var chest = puzzle.NewChest(ChestPlace, "Key_Chest");

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
