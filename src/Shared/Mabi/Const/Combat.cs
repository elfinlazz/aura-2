// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aura.Shared.Mabi.Const
{
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
		KnockBackHit2 = 0x04,

		/// <summary>
		/// Prop effect? Thunderbolts and fireball explosion (0x08)
		/// </summary>
		UseEffect = 0x08,

		/// <summary>
		/// Req for some skills, didn't show for fireball or thunder (0x20)
		/// </summary>
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

	public enum TargetOptions : uint
	{
		None = 0x00,
		Critical = 0x01,
		CleanHit = 0x04,
		OneShotFinish = 0x08,
		Finished = 0x10,
		Result = 0x20,
		KnockDownFinish = 0x100,
		Smash = 0x200, // Seems be used with Smash sometimes?
		KnockBack = 0x400,
		KnockDown = 0x800,
		FinishingHit = 0x1000,
		// ??? = 0x100000 // logged using mana shield
		// ??? = 0x4000000 // logged on a counter hit / using mana shield

		/// <summary>
		/// Finished | KnockDownFinish | FinishingHit
		/// </summary>
		FinishingKnockDown = 0x1110,
	}

	// Most likely flags
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
