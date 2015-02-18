// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aura.Shared.Mabi.Const
{
	[Flags]
	public enum AttackerOptions : uint
	{
		None = 0x00,

		/// <summary>
		/// Difference between KBH 1 and 2 unknown. (0x02)
		/// </summary>
		KnockBackHit1 = 0x02,

		/// <summary>
		/// Difference between KBH 1 and 2 unknown. (0x04)
		/// </summary>
		/// <remarks>
		/// Required for knock back hit animation.
		/// </remarks>
		KnockBackHit2 = 0x04,

		/// <summary>
		/// Prop effect? Thunderbolts and fireball explosion (0x08)
		/// </summary>
		UseEffect = 0x08,

		/// <summary>
		/// Req for some skills (0x20)
		/// </summary>
		/// <remarks>
		/// Didn't show for fireball or thunder.
		/// Seems to be required for proper knock back.
		/// </remarks>
		Result = 0x20,

		/// <summary>
		/// Set when using two weapons (0x40)
		/// </summary>
		DualWield = 0x40,

		/// <summary>
		/// Shows "First Hit"? (0x400)
		/// </summary>
		FirstHit = 0x400,
	}

	[Flags]
	public enum TargetOptions : uint
	{
		None = 0x00,

		/// <summary>
		/// Displays crit effect
		/// </summary>
		Critical = 0x01,

		/// <summary>
		/// Displays "Clean Hit"
		/// </summary>
		CleanHit = 0x04,

		/// <summary>
		/// Displays "First Hit"
		/// </summary>
		FirstHit = 0x08,

		/// <summary>
		/// Displays "Finish"
		/// </summary>
		/// <remarks>
		/// This alone removes any delays between dmg and animation.
		/// </remarks>
		Finished = 0x10,

		/// <summary>
		/// ?
		/// </summary>
		/// <remarks>
		/// Some have it, some don't.
		/// </remarks>
		Result = 0x20,

		/// <summary>
		/// Knocks target down
		/// </summary>
		KnockDownFinish = 0x100,

		/// <summary>
		/// ?
		/// </summary>
		/// <remarks>
		/// Used by Smash sometimes, looks like a knock down.
		/// </remarks>
		Smash = 0x200,

		/// <summary>
		/// Sliding back animation
		/// </summary>
		KnockBack = 0x400,

		/// <summary>
		/// Just another knock down
		/// </summary>
		KnockDown = 0x800,

		/// <summary>
		/// Displays "Last Blow"
		/// </summary>
		FinishingHit = 0x1000,

		/// <summary>
		/// For blue numbers
		/// </summary>
		/// <remarks>
		/// Damage has to be 0 for this to work, otherweise the client
		/// displays that value instead, a little delayed.
		/// </remarks>
		ManaShield = 0x100000,

		// ??? = 0x4000000 // logged on a counter hit / using mana shield

		/// <summary>
		/// Finished | KnockDownFinish | FinishingHit = 0x1110
		/// </summary>
		/// <remarks>
		/// Always active when creature dies.
		/// </remarks>
		FinishingKnockDown = Finished | KnockDownFinish | FinishingHit,
	}

	// Most likely flags
	[Flags]
	public enum CombatActionType : byte
	{
		None = 0x00,

		/// <summary>
		/// Target takes simple hit (0x01)
		/// </summary>
		TakeHit = 0x01,

		/// <summary>
		/// Simple hit by Source (0x02)
		/// </summary>
		Hit = 0x02,

		/// <summary>
		/// Both hit at the same time (0x06)
		/// </summary>
		SimultaneousHit = 0x06,

		/// <summary>
		/// Alternative target type for Counter? (0x13)
		/// </summary>
		CounteredHit2 = 0x13,

		/// <summary>
		/// Smash/Counter (0x32)
		/// </summary>
		HardHit = 0x32,

		/// <summary>
		/// Target type Defense (0x33)
		/// </summary>
		Defended = 0x33,

		// Target type with Mana Shield
		// ??? = 0x41,

		/// <summary>
		/// Passive Damage, Shadow Bunshin/Fireball (0x42)
		/// </summary>
		SpecialHit = 0x42,

		/// <summary>
		/// Target type for Counter (0x53)
		/// </summary>
		CounteredHit = 0x53,

		/// <summary>
		/// Magicbolt, range, doing Counter? (0x72)
		/// </summary>
		RangeHit = 0x72,

		//DefendedHit = 0x73, // ?
	}
}
