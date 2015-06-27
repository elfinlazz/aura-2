//--- Aura Script -----------------------------------------------------------
// Mimic Pit Puzzle
//--- Description -----------------------------------------------------------
// Spawns a chest in an alley, with 3 mimics and mobs.
//---------------------------------------------------------------------------

[PuzzleScript("keychest_3mimic")]
internal class Keychest3Mimic : PuzzleScript
{
	public override void OnPrepare(Puzzle puzzle)
	{
		var lockedPlace = puzzle.NewPlace("LockedPlace");
		var chestPlace = puzzle.NewPlace("ChestPlace");

		lockedPlace.DeclareLock();
		chestPlace.DeclareUnlock(lockedPlace);
		chestPlace.ReservePlace();
	}

	public override void OnPuzzleCreate(Puzzle puzzle)
	{
		var lockedPlace = puzzle.GetPlace("LockedPlace");
		var chestPlace = puzzle.GetPlace("ChestPlace");

		var key = puzzle.LockPlace(lockedPlace, "Lock");

		var chest = new Chest(puzzle, "KeyChest");
		chest.Add(Item.Create(id: 2000, amountMin: 10, amountMax: 30));
		chest.Add(key);
		chestPlace.AddProp(chest, Placement.Corner4);

		chestPlace.SpawnSingleMob("Trap", "Mob3", Placement.Corner4);
		chestPlace.SpawnSingleMob("ChestMob1", "Mob1");
		lockedPlace.SpawnSingleMob("ChestMob2", "Mob2");
	}
}