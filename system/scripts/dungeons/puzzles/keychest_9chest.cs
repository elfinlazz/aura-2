//--- Aura Script -----------------------------------------------------------
// Treasure Chest Room Puzzle
//--- Description -----------------------------------------------------------
// A room with 9 chests, that can be opened with keys dropped from mobs.
//---------------------------------------------------------------------------

public abstract class Keychest9ChestScript : PuzzleScript
{
	public override void OnPrepare(Puzzle puzzle)
	{
		var place = puzzle.NewPlace("Place");
		place.ReservePlace();
		place.ReserveDoors();

		puzzle.Set("MonsterI", 1);
	}

	public override void OnPuzzleCreate(Puzzle puzzle)
	{
		var place = puzzle.GetPlace("Place");
		var monsterChests = UniqueRnd(3, 1, 2, 3, 4, 5, 6, 7, 8, 9);

		for (int i = 1; i <= 9; ++i)
		{
			var monsterChest = monsterChests.Contains(i);

			var chest = new LockedChest(puzzle, "Chest" + i, "lock1");
			AddChestDrops(chest, i, monsterChest);
			place.AddProp(chest, Placement.Center9);

			puzzle.Set("Chest" + i + "Open", false);
			puzzle.Set("Chest" + i + "Monster", monsterChest);
		}
	}

	protected abstract void AddChestDrops(Chest chest, int chestNum, bool monsterChest);

	public override void OnPropEvent(Puzzle puzzle, Prop prop)
	{
		var chest = prop as LockedChest;
		if (chest == null || puzzle.Get(chest.Name + "Open") || !chest.IsOpen)
			return;

		puzzle.Set(chest.Name + "Open", true);

		if (puzzle.Get(chest.Name + "Monster"))
		{
			var place = puzzle.GetPlace("Place");
			place.CloseAllDoors();
			place.SpawnSingleMob(chest.Name + "Mob", "Mob" + puzzle.Get("MonsterI"));
			puzzle.Set("MonsterI", puzzle.Get("MonsterI") + 1);
		}
	}

	public override void OnMonsterDead(Puzzle puzzle, MonsterGroup group)
	{
		if (group.Remaining != 0)
			return;

		puzzle.GetPlace("Place").OpenAllDoors();
	}
}

[PuzzleScript("keychest_9chest_ciar")]
public class Keychest9ChestCiarScript : Keychest9ChestScript
{
	protected override void AddChestDrops(Chest chest, int chestNum, bool monsterChest)
	{
		if (!monsterChest)
			chest.Add(Item.Create(id: 2000, amountMin: 100, amountMax: 250));

		// Enchant
		int prefix = 0, suffix = 0;
		switch (Random(10))
		{
			case 0:
			case 1:
			case 2:
			case 3: prefix = 20205; break; // Restfull
			case 4:
			case 5:
			case 6: prefix = 20204; break; // Foggy
			case 7:
			case 8: suffix = 30501; break; // Giant
			case 9: suffix = 30602; break; // Healer
		}

		chest.Add(Item.CreateEnchanted(62005, prefix, suffix));
	}
}

[PuzzleScript("keychest_9chest_barri")]
public class Keychest9ChestBarriScript : Keychest9ChestScript
{
	protected override void AddChestDrops(Chest chest, int chestNum, bool monsterChest)
	{
		if (!monsterChest)
			chest.Add(Item.Create(id: 2000, amountMin: 250, amountMax: 400));

		// Enchant
		int prefix = 0, suffix = 0;
		switch (Random(10))
		{
			case 0:
			case 1:
			case 2: prefix = 20401; break; // Smart
			case 3:
			case 4:
			case 5: prefix = 20402; break; // Strong
			case 6:
			case 7: suffix = 30702; break; // Raven
			case 8: suffix = 30704; break; // Deadly
			case 9: suffix = 30805; break; // Falcon
		}

		chest.Add(Item.CreateEnchanted(62005, prefix, suffix));
	}
}

[PuzzleScript("keychest_9chest_rabbie")]
public class Keychest9ChestRabbieScript : Keychest9ChestScript
{
	protected override void AddChestDrops(Chest chest, int chestNum, bool monsterChest)
	{
		if (!monsterChest)
			chest.Add(Item.Create(id: 2000, amountMin: 150, amountMax: 350));

		// Enchant
		int prefix = 0, suffix = 0;
		switch (Random(10))
		{
			case 0:
			case 1:
			case 2:
			case 3: prefix = 20205; break; // Restfull
			case 4:
			case 5:
			case 6: prefix = 20204; break; // Mist
			case 7:
			case 8: suffix = 30501; break; // Giant
			case 9: suffix = 30602; break; // Healer
		}

		chest.Add(Item.CreateEnchanted(62005, prefix, suffix));
	}
}
