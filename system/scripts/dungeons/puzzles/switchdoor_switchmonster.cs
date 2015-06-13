//--- Aura Script -----------------------------------------------------------
// Keychest Monster Puzzle
//--- Description -----------------------------------------------------------
// Used as 
//---------------------------------------------------------------------------

using Aura.Channel.Scripting.Scripts;
using Aura.Channel.World.Dungeons.Puzzles;

[PuzzleScript("switchdoor_switchmonster")]
public class SwitchdoorSwitchmonsterScript : PuzzleScript
{
	public override void OnPrepare(IPuzzle puzzle)
	{
		var LockedPlace = puzzle.NewPlace("LockedPlace");

		LockedPlace.DeclareLockSelf();
		LockedPlace.ReservePlace();

		uint color = LockedPlace.GetLockColor();
		var Switch1 = puzzle.NewSwitch(LockedPlace, "Switch1", DungeonPropPositionType.Corner4, color);
		var Switch2 = puzzle.NewSwitch(LockedPlace, "Switch2", DungeonPropPositionType.Corner4, color);
		var Switch3 = puzzle.NewSwitch(LockedPlace, "Switch3", DungeonPropPositionType.Corner4, color);
		var Switch4 = puzzle.NewSwitch(LockedPlace, "Switch4", DungeonPropPositionType.Corner4, color);

		LockedPlace.CloseAllDoors();
	}

	public override void OnPropEvent(IPuzzle puzzle, IPuzzleProp prop, string propEvent)
	{
		if (!(prop is IPuzzleSwitch))
			return;

		if (propEvent == "on")
		{
			var lockedPlace = puzzle.GetPlace("LockedPlace");

			if (prop.GetName() == "Switch1")
				puzzle.OpenPlace(lockedPlace);
			else
				lockedPlace.SpawnSingleMob(prop.GetName() + "Mob", "Mob1");
		}
	}
}
