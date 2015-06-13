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
		var lockedPlace = puzzle.NewPlace("LockedPlace");
		var chestPlace = puzzle.NewPlace("ChestPlace");

		lockedPlace.DeclareLock();
		chestPlace.DeclareUnlock(lockedPlace);
		chestPlace.ReservePlace();
		chestPlace.ReserveDoors();

		var chest = puzzle.NewChest(chestPlace, "KeyChest");

		lockedPlace.CloseAllDoors();
		puzzle.LockPlace(lockedPlace, "Lock");

		chest.AddKeyForLock(lockedPlace);
	}

	public override void OnPropEvent(IPuzzle puzzle, IPuzzleProp prop, string propEvent)
	{
		if (prop.GetName() == "KeyChest" && propEvent == "open")
		{
			var chestPlace = puzzle.GetPlace("ChestPlace");
			chestPlace.CloseAllDoors();
			chestPlace.SpawnSingleMob("SingleMob1");
		}
	}

	public override void OnMonsterDead(IPuzzle puzzle, MonsterGroup group)
	{
		if (group.Remaining != 0)
			return;

		puzzle.GetPlace("ChestPlace").OpenAllDoors();
	}
}
