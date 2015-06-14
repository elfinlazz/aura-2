//--- Aura Script -----------------------------------------------------------
// Keychest Monster Puzzle
//--- Description -----------------------------------------------------------
// Used as 
//---------------------------------------------------------------------------

using Aura.Channel.Scripting.Scripts;
using Aura.Channel.World.Dungeons.Props;
using Aura.Channel.World.Dungeons.Puzzles;
using Aura.Channel.World.Entities;
using Aura.Shared.Util;

[PuzzleScript("switchdoor_switchmonster")]
public class SwitchdoorSwitchmonsterScript : PuzzleScript
{
	public override void OnPrepare(Puzzle puzzle)
	{
		var LockedPlace = puzzle.NewPlace("LockedPlace");

		LockedPlace.DeclareLockSelf();
		LockedPlace.ReservePlace();

		puzzle.Set("open", "Switch" + Random(1, 5));
	}

	public override void OnPuzzleCreate(Puzzle puzzle)
	{
		var LockedPlace = puzzle.GetPlace("LockedPlace");
		var color = LockedPlace.GetLockColor();

		for (int i = 1; i <= 4; ++i)
		{
			LockedPlace.AddProp(new Switch("Switch" + i, color), DungeonPropPositionType.Corner4);
			puzzle.Set("Switch" + i + "Activated", false);
		}

		LockedPlace.CloseAllDoors();
	}

	public override void OnPropEvent(Puzzle puzzle, Prop prop)
	{
		var Switch = prop as Switch;
		if (Switch == null)
			return;

		if (Switch.State == "on" && !puzzle.Get(Switch.InternalName + "Activated"))
		{
			puzzle.Set(Switch.InternalName + "Activated", true);

			var lockedPlace = puzzle.GetPlace("LockedPlace");

			if (Switch.InternalName == puzzle.Get("open"))
				puzzle.OpenPlace(lockedPlace);
			else
				lockedPlace.SpawnSingleMob(Switch.InternalName + "Mob", "Mob1");
		}
	}
}
