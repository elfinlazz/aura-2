// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;

namespace Aura.Shared.Mabi.Const
{
	/// <summary>
	/// This list is based on vague information found in the client,
	/// aside from the values at the end. Testing needed.
	/// </summary>
	[Flags]
	public enum Locks : uint
	{
		MoveRun = 0x00000001,
		MoveWalk = 0x00000002,
		MoveAll = 0x00000003,

		SkillAll = 0x00000040,
		// ...

		Summon = 0x00020000,
		Ride = 0x00040000,

		AviationTakeoff = 0x00100000,
		AviationLand = 0x00200000,
		AviationAll = 0x00F00000,

		PvpShowdown = 0x01000000,
		PvpAll = 0x0F000000,

		ChatSend = 0x00000F00,
		ChatRecv = 0x0000F000,
		// &
		ChatNormal = 0x00001100,
		ChatParty = 0x00002200,
		ChatWhipser = 0x00004400,
		ChatAll = 0x0000FF00,

		// ------------------------------------------------------------------

		/// <summary>
		/// Chat, Moving, [...] (0xEFFFFFFE)
		/// </summary>
		/// <remarks>
		/// Set on login, warping, etc.
		/// </remarks>
		Default = 0xEFFFFFFE,

		/// <summary>
		/// Set on take off and landing
		/// </summary>
		Flying = 0xFFFFBDDF,
	}
}
