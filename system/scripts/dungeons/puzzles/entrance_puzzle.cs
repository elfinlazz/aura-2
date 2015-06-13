//--- Aura Script -----------------------------------------------------------
// Entrance Puzzle
//--- Description -----------------------------------------------------------
// Used as first puzzle in every dungeon because <TODO: insert reason>.
//---------------------------------------------------------------------------

using Aura.Channel.Scripting.Scripts;
using Aura.Channel.World.Dungeons.Props;
using Aura.Channel.World.Dungeons.Puzzles;
using Aura.Channel.World.Entities;

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

		var chest = puzzle.NewChest(chestPlace, "KeyChest", DungeonPropPositionType.Random);

		lockedPlace.CloseAllDoors();
		puzzle.LockPlace(lockedPlace, "Lock");
	}

	public override void OnPropEvent(IPuzzle puzzle, Prop prop)
	{
		var chest = prop as Chest;
		if (chest != null && chest.InternalName == "KeyChest" && chest.State == "open")
		{
			var chestPlace = puzzle.GetPlace("ChestPlace");
			chestPlace.CloseAllDoors();
			chestPlace.SpawnSingleMob("SingleMob1");
		}
	}

	public override void OnMobAllocated(IPuzzle puzzle, MonsterGroup group)
	{
		if (group.Name == "SingleMob1")
			group.AddKeyForLock(puzzle.GetPlace("LockedPlace"));
	}

	public override void OnMonsterDead(IPuzzle puzzle, MonsterGroup group)
	{
		if (group.Remaining != 0)
			return;

		puzzle.GetPlace("ChestPlace").OpenAllDoors();
	}
}
