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
	public override void OnPrepare(IPuzzle puzzle)
	{
		var LockedPlace = puzzle.NewPlace("LockedPlace");

		LockedPlace.DeclareLockSelf();
		LockedPlace.ReservePlace();

		puzzle.Set("open", "Switch" + Random(1, 5));
		puzzle.Set("activated", "no");
	}

	public override void OnPuzzleCreate(IPuzzle puzzle)
	{
		var LockedPlace = puzzle.GetPlace("LockedPlace");

		uint color = LockedPlace.GetLockColor();
		var Switch1 = puzzle.NewSwitch(LockedPlace, "Switch1", DungeonPropPositionType.Corner4, color);
		var Switch2 = puzzle.NewSwitch(LockedPlace, "Switch2", DungeonPropPositionType.Corner4, color);
		var Switch3 = puzzle.NewSwitch(LockedPlace, "Switch3", DungeonPropPositionType.Corner4, color);
		var Switch4 = puzzle.NewSwitch(LockedPlace, "Switch4", DungeonPropPositionType.Corner4, color);

		LockedPlace.CloseAllDoors();
	}

	public override void OnPropEvent(IPuzzle puzzle, Prop prop)
	{
		var Switch = prop as Switch;
		if (Switch == null)
			return;

		var isActivated = puzzle.Get("activated") as string;
		if (Switch.State == "on" && isActivated != "yes")
		{
			var lockedPlace = puzzle.GetPlace("LockedPlace");
			var openSwitchName = puzzle.Get("open") as string;

			if (Switch.InternalName == openSwitchName)
			{
				puzzle.OpenPlace(lockedPlace);
				puzzle.Set("activated", "yes");
			}
			else
				lockedPlace.SpawnSingleMob(Switch.InternalName + "Mob", "Mob1");
		}
	}
}
