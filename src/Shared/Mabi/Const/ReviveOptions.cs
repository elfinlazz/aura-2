// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;

namespace Aura.Shared.Mabi.Const
{
	[Flags]
	public enum ReviveOptions : uint
	{
		None = 0x00,
		Town = 0x01,
		Here = 0x02,
		DungeonEntrance = 0x04,
		StatueOfGoddess = 0x08,
		ArenaSide = 0x10,
		ArenaLobby = 0x20,
		//
		NaoRevival1 = 0x80,
		WaitForRescue = 0x100,
		FeatherUp = 0x200,
		//
		BarriLobby = 0x800,
		//
		//
		//
		TirChonaill = 0x8000,
		//
		HereNoPenalty = 0x20000,
		HerePvP = 0x40000,
		InCamp = 0x80000,
		ArenaWaitingRoom = 0x100000,
		//
		//
		//
		//
		//
		NaoStone = 0x4000000,
	}
}
