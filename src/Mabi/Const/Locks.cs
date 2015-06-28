// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;

namespace Aura.Mabi.Const
{
	/// <summary>
	/// Locks to disable certain actions.
	/// </summary>
	/// <remarks>
	/// This list is based on vague information found in the client
	/// and some basic testing. Needs more research for unknown
	/// and missing values.
	/// </remarks>
	[Flags]
	public enum Locks : uint
	{
		// ? = 0x01,

		// ? = 0x02,

		/// <summary>
		/// Unable to change stance.
		/// </summary>
		/// <remarks>
		/// Since you can't change your stance you can't attack or hit
		/// props either.
		/// Was 0x02 in older clients?
		/// </remarks>
		ChanceStance = 0x04,

		/// <summary>
		/// Prevents walking entirely.
		/// </summary>
		Walk = 0x08,

		/// <summary>
		/// Prevents running, when walking is possible the client does that instead.
		/// </summary>
		Run = 0x10,

		// ? = 0x20, (hit? attack and prop hit works)

		/// <summary>
		/// Prevents changing equipment in any way.
		/// </summary>
		ChangeEquipment = 0x40,

		/// <summary>
		/// Prevents using items.
		/// </summary>
		UseItem = 0x80,

		// ? = 0x100, (usemagic? doesn't stop magic skills)

		/// <summary>
		/// Prevents preparing skills.
		/// </summary>
		/// <remarks>
		/// Does not affect Start or attacking (Combat Mastery).
		/// </remarks>
		PrepareSkills = 0x200,

		/// <summary>
		/// Prevents attacking and hitting props.
		/// </summary>
		/// <remarks>
		/// Does not affect stance changing, and character still runs
		/// towards the target (not for props).
		/// </remarks>
		Attack = 0x400,

		/// <summary>
		/// Prevents picking up from and dropping items to the floor.
		/// </summary>
		/// <remarks>
		/// Trying to drop something shows the message "Unable to discard item!".
		/// </remarks>
		PickUpAndDrop = 0x800,

		/// <summary>
		/// Prevents talking to NPCs.
		/// </summary>
		/// <remarks>
		/// Character still runs to the NPC.
		/// </remarks>
		TalkToNpc = 0x1000,

		// ? = 0x2000, (trade? )

		/// <summary>
		/// Prevents chat messages from the character.
		/// </summary>
		/// <remarks>
		/// As a result it also prevents GM commands.
		/// </remarks>
		Speak = 0x4000,

		// ? = 0x8000,

		/// <summary>
		/// Disables gesture buttons.
		/// </summary>
		/// <remarks>
		/// While this disables the gesture action buttons, you can still
		/// use them. For example, if they're in your hotkey bar.
		/// </remarks>
		Gesture = 0x10000,

		/// <summary>
		/// Prevents starting skills.
		/// </summary>
		/// <remarks>
		/// Does not affect Prepare or attacking.
		/// </remarks>
		StartSkills = 0x20000,

		// ?

		// ------------------------------------------------------------------

		/// <summary>
		/// Prevents running and walking.
		/// </summary>
		Move = Run | Walk,

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
