// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

namespace Aura.Mabi.Const
{
	public enum EventType : int
	{
		/// <summary>
		/// ?
		/// </summary>
		Unk1 = 1,

		/// <summary>
		/// ? (texts, bgm change) (10)
		/// </summary>
		AreaChange = 10,

		/// <summary>
		/// Collision event, shape marks the collision lines. (14)
		/// </summary>
		Collision = 14,

		/// <summary>
		/// Confirmation message on props. (1100)
		/// </summary>
		Confirmation = 1100,

		/// <summary>
		/// Area in which creatures specified as group are spawned. (2000)
		/// </summary>
		CreatureSpawn = 2000,

		/// <summary>
		/// Dungeon altar (2110)
		/// </summary>
		Altar = 2110,

		/// <summary>
		/// Area at which LastTown is updated. (2610)
		/// </summary>
		SaveTown = 2610,
	}

	public enum SignalType : int
	{
		/// <summary>
		/// Triggered by entering event area. (101)
		/// </summary>
		Enter = 101,

		/// <summary>
		/// Triggered by leaving event area. (102)
		/// </summary>
		Leave = 102,

		/// <summary>
		/// Triggered when stepped on? Used for altars. (103)
		/// </summary>
		StepOn = 103,

		/// <summary>
		/// Triggered when touching prop. (202)
		/// </summary>
		Touch = 202,
	}

}
