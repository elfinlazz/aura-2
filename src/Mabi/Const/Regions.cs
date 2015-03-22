// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

namespace Aura.Mabi.Const
{
	public enum EventType : int
	{
		Unk1 = 1,
		AreaChange = 10, // ? (texts, bgm change)
		Collision = 14,
		CreatureSpawn = 2000,
	}

	public enum SignalType : int
	{
		/// <summary>
		/// Triggered by entering event area.
		/// </summary>
		Enter = 101,

		/// <summary>
		/// Triggered by leaving event area.
		/// </summary>
		Leave = 102,
	}

}
