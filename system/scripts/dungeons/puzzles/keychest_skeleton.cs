//--- Aura Script -----------------------------------------------------------
// Keychest Skeleton Puzzle
//--- Description -----------------------------------------------------------
// Similar to keychest_monster, but you always get a chain and there's
// a 20% chance to get mini skeletons instead of the normal mobs.
//---------------------------------------------------------------------------

[PuzzleScript("keychest_skeleton")]
public class KeychestSkeletonScript : PuzzleScript
{
	public override void OnPrepare(Puzzle puzzle)
	{
		var lockedPlace = puzzle.NewPlace("LockedPlace");
		var chestPlace = puzzle.NewPlace("ChestPlace");

		lockedPlace.DeclareLock();
		chestPlace.DeclareUnlock(lockedPlace);
		chestPlace.ReservePlace();
		chestPlace.ReserveDoors();
		
		// 20% chance for small skeletons
		if (Random(100) < 20)
		{
			puzzle.GetMonsterData("Mob1")[0].RaceId = 11101; // Small Red Skeleton
			puzzle.GetMonsterData("Mob2")[0].RaceId = 11102; // Small Light Armor Skeleton
			puzzle.GetMonsterData("Mob3")[0].RaceId = 11103; // Small Heavy Armor Skeleton
		}

		puzzle.Set("ChestOpen", false);
	}

	public override void OnPuzzleCreate(Puzzle puzzle)
	{
		var lockedPlace = puzzle.GetPlace("LockedPlace");
		var chestPlace = puzzle.GetPlace("ChestPlace");

		puzzle.LockPlace(lockedPlace, "Lock");

		var chest = new Chest(puzzle, "KeyChest");
		AddChestDrops(chest);

		chestPlace.AddProp(chest, Placement.Random);
	}

	protected virtual void AddChestDrops(Chest chest)
	{
		switch (Random(5))
		{
			case 0: chest.Add(Item.Create(id: 63002, amountMin: 1, amountMax: 5)); break; // Firewood
			case 1: chest.Add(Item.Create(id: 60005, amountMin: 1, amountMax: 5)); break; // Bandage
			case 2: chest.Add(Item.Create(id: 51001, amountMin: 1, amountMax: 2)); break; // HP 10 Potion
			case 3: chest.Add(Item.Create(id: 52002, amountMin: 1, amountMax: 3)); break; // Small Gem
			case 4: chest.Add(Item.Create(id: 50009, amountMin: 1, amountMax: 3)); break; // Egg
		}
	}

	public override void OnPropEvent(Puzzle puzzle, Prop prop)
	{
		var chest = prop as Chest;
		if (chest != null)
		{
			if (chest.Name == "KeyChest" && !puzzle.Get("ChestOpen"))
			{
				puzzle.Set("ChestOpen", true);

				var chestPlace = puzzle.GetPlace("ChestPlace");
				chestPlace.CloseAllDoors();
				
				chestPlace.SpawnSingleMob("ChainMob1", "Mob1");
			}
		}
	}

	public override void OnMobAllocated(Puzzle puzzle, MonsterGroup group)
	{
		if (group.Name == "LastMob")
			group.AddKeyForLock(puzzle.GetPlace("LockedPlace"));
	}

	public override void OnMonsterDead(Puzzle puzzle, MonsterGroup group)
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
