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
		var Switch = puzzle.NewSwitch(LockedPlace, "Switch1", color);

		LockedPlace.CloseAllDoors();
	}

	public override void OnPropEvent(IPuzzle puzzle, IPuzzleProp prop, string propEvent)
	{
		var Switch = prop as IPuzzleSwitch;
		if (Switch != null)
		{
			if (Switch.GetName() == "Switch1" && propEvent == "on")
				puzzle.GetPlace("LockedPlace").OpenAllDoors();
		}
	}
}
