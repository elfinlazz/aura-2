//--- Aura Script -----------------------------------------------------------
// Keychest Monster Puzzle
//--- Description -----------------------------------------------------------
// Spawns a chest in a room with either a single mob or a chain.
// The last mob drops a key to a locked door.
//---------------------------------------------------------------------------

using Aura.Channel.Scripting.Scripts;
using Aura.Channel.World.Dungeons.Props;
using Aura.Channel.World.Dungeons.Puzzles;
using Aura.Channel.World.Entities;
using Aura.Shared.Util;

[PuzzleScript("keychest_monster")]
public class KeychestMonsterScript : PuzzleScript
{
	public override void OnPrepare(IPuzzle puzzle)
	{
		var lockedPlace = puzzle.NewPlace("LockedPlace");
		var chestPlace = puzzle.NewPlace("ChestPlace");

		lockedPlace.DeclareLock();
		chestPlace.DeclareUnlock(lockedPlace);
		chestPlace.ReservePlace();
		chestPlace.ReserveDoors();
	}

	public override void OnPuzzleCreate(IPuzzle puzzle)
	{
		var lockedPlace = puzzle.GetPlace("LockedPlace");
		var chestPlace = puzzle.GetPlace("ChestPlace");

		var chest = puzzle.NewChest(chestPlace, "KeyChest", DungeonPropPositionType.Random);

		lockedPlace.CloseAllDoors();
		puzzle.LockPlace(lockedPlace, "Lock");
	}

	public override void OnPropEvent(IPuzzle puzzle, Prop prop)
	{
		var chest = prop as Chest;
		if (chest != null)
		{
			if (chest.InternalName == "KeyChest" && chest.State == "open")
			{
				var chestPlace = puzzle.GetPlace("ChestPlace");
				chestPlace.CloseAllDoors();

				var rnd = RandomProvider.Get();
				if (rnd.NextDouble() < 0.5)
					chestPlace.SpawnSingleMob("LastMob");
				else
					chestPlace.SpawnSingleMob("ChainMob1", "Mob1");
			}
		}
	}

	public override void OnMobAllocated(IPuzzle puzzle, MonsterGroup group)
	{
		if (group.Name == "LastMob")
			group.AddKeyForLock(puzzle.GetPlace("LockedPlace"));
	}

	public override void OnMonsterDead(IPuzzle puzzle, MonsterGroup group)
	{
		if (group.Remaining != 0)
			return;

		var chestPlace = puzzle.GetPlace("ChestPlace");

		if (group.Name == "ChainMob1")
		{
			chestPlace.SpawnSingleMob("ChainMob2", "Mob2");
		}
		else if (group.Name == "ChainMob2")
		{
			chestPlace.SpawnSingleMob("LastMob", "Mob3");
		}
		else if (group.Name == "LastMob")
		{
			puzzle.GetPlace("ChestPlace").OpenAllDoors();
		}
	}
}
