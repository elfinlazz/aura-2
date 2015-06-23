//--- Aura Script -----------------------------------------------------------
// Switch Room Puzzle
//--- Description -----------------------------------------------------------
// Creates a room with 4 switches in the corners, of which one opens
// the locked door.
//---------------------------------------------------------------------------

using Aura.Channel.Scripting.Scripts;
using Aura.Channel.World.Dungeons.Props;
using Aura.Channel.World.Dungeons.Puzzles;
using Aura.Channel.World.Entities;
using Aura.Shared.Util;

[PuzzleScript("switchdoor_switchmonster")]
public class SwitchDoorSwitchMonsterScript : PuzzleScript
{
	public override void OnPrepare(Puzzle puzzle)
	{
		var lockedPlace = puzzle.NewPlace("LockedPlace");

		lockedPlace.DeclareLockSelf();
		lockedPlace.ReservePlace();

		puzzle.Set("open", "Switch" + Random(1, 5));
		puzzle.Set("activated", false);
	}

	public override void OnPuzzleCreate(Puzzle puzzle)
	{
		var lockedPlace = puzzle.GetPlace("LockedPlace");

		for (int i = 1; i <= 4; ++i)
		{
			lockedPlace.AddProp(new Switch("Switch" + i, lockedPlace.LockColor), Placement.Corner4);
			puzzle.Set("Switch" + i + "Activated", false);
		}

		lockedPlace.CloseAllDoors();
	}

	public override void OnPropEvent(Puzzle puzzle, Prop prop)
	{
		var Switch = prop as Switch;
		if (Switch == null)
			return;

		if (puzzle.Get("activated"))
			return;

		if (Switch.State == "on" && !puzzle.Get(Switch.Name + "Activated"))
		{
			puzzle.Set(Switch.Name + "Activated", true);

			var lockedPlace = puzzle.GetPlace("LockedPlace");

			if (Switch.Name == puzzle.Get("open"))
			{
				lockedPlace.Open();
				puzzle.Set("activated", true);
			}
			else
				lockedPlace.SpawnSingleMob(Switch.Name + "Mob", "Mob1");
		}
	}
}

// The monsters that spawn when you hit the wrong switch
// drop chest keys.
[PuzzleScript("chestkey_switchdoor_switchmonster")]
public class ChestKeySwitchDoorSwitchMonsterScript : SwitchDoorSwitchMonsterScript
{
	public override void OnMobAllocated(Puzzle puzzle, MonsterGroup group)
	{
		group.AddDrop(Item.CreateKey(70028, "lock1"));
	}
}
