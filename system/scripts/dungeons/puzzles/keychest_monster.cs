//--- Aura Script -----------------------------------------------------------
// Keychest Monster Puzzle
//--- Description -----------------------------------------------------------
// Spawns a chest in a room with either a single mob or a chain.
// The last mob drops a key to a locked door.
//---------------------------------------------------------------------------

[PuzzleScript("keychest_monster")]
public class KeychestMonsterScript : PuzzleScript
{
	public override void OnPrepare(Puzzle puzzle)
	{
		var lockedPlace = puzzle.NewPlace("LockedPlace");
		var chestPlace = puzzle.NewPlace("ChestPlace");

		lockedPlace.DeclareLock();
		chestPlace.DeclareUnlock(lockedPlace);
		chestPlace.ReservePlace();
		chestPlace.ReserveDoors();

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
		if (Random(100) != 0)
		{
			for (int i = 0; i < 2; ++i)
				chest.Add(Item.Create(id: 2000, amountMin: 10, amountMax: 50));
		}
		else
		{
			for (int i = 0; i < 5; ++i)
				chest.Add(Item.Create(id: 2000, amountMin: 100, amountMax: 200));
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

				if (Random() < 50)
					chestPlace.SpawnSingleMob("LastMob");
				else
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

[PuzzleScript("keychest_monster2")]
public class KeychestMonster2Script : KeychestMonsterScript
{
	protected override void AddChestDrops(Chest chest)
	{
		if (Random(100) != 0)
		{
			switch (Random(5))
			{
				case 0: chest.Add(Item.Create(id: 63002, amountMin: 1, amountMax: 5)); break;
				case 1: chest.Add(Item.Create(id: 60005, amountMin: 1, amountMax: 5)); break;
				case 2: chest.Add(Item.Create(id: 51001, amountMin: 1, amountMax: 2)); break;
				case 3: chest.Add(Item.Create(id: 52002, amountMin: 1, amountMax: 3)); break;
				case 4: chest.Add(Item.Create(id: 50009, amountMin: 1, amountMax: 3)); break;
			}
		}
		else
		{
			for (int i = 0; i < 5; ++i)
				chest.Add(Item.Create(id: 2000, amountMin: 100, amountMax: 200));
		}
	}
}
