// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;

namespace Aura.Mabi.Const
{
	/// <summary>
	/// Options for attacker options.
	/// </summary>
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
		/// ?
		/// </summary>
		/// <remarks>
		/// Charge?
		/// </remarks>
		Dashed = 0x10,

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

		/// <summary>
		/// Missed? (0x800)
		/// </summary>
		Missed = 0x800,

		/// <summary>
		/// ?
		/// </summary>
		/// <remarks>
		/// Attack Phase, used in Pummel?
		/// </remarks>
		PhaseAttack = 0x1000,
	}

	/// <summary>
	/// Options for target options.
	/// </summary>
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
		/// 
		/// Required for the target to target the attacker automatically.
		/// (Makes the client send SetTarget.)
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

		/// <summary>
		/// ?
		/// </summary>
		MultiHit = 0x2000000,

		// ??? = 0x4000000 // logged on a counter hit / using mana shield

		/// <summary>
		/// Finished | KnockDownFinish | FinishingHit = 0x1110
		/// </summary>
		/// <remarks>
		/// Always active when creature dies.
		/// </remarks>
		FinishingKnockDown = Finished | KnockDownFinish | FinishingHit,

		/// <summary>
		/// Combined flags for knock back/downs?
		/// </summary>
		Downed = 0x7CF00,
	}

	/// <summary>
	/// Type of the combat action pack.
	/// </summary>
	public enum CombatActionPackType : byte
	{
		/// <summary>
		/// A normal hit.
		/// </summary>
		NormalAttack = 1,

		/// <summary>
		/// Dual wield attack, consisting of 2 packs in sequence.
		/// </summary>
		TwinSwordAttack = 2,

		/// <summary>
		/// Dual arrow attack? Elf ranged.
		/// </summary>
		ChainRangeAttack = 3
	}

	/// <summary>
	/// Flags of a combat action.
	/// </summary>
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
		Attacker = 0x02,

		/// <summary>
		/// ? (0x04)
		/// </summary>
		Unknown = 0x04,

		/// <summary>
		/// ? (0x10)
		/// </summary>
		/// <remarks>
		/// Skill was active?
		/// </remarks>
		SkillActive = 0x10,

		/// <summary>
		/// ? (0x20)
		/// </summary>
		/// <remarks>
		/// Skill was used successfully?
		/// </remarks>
		SkillSuccess = 0x20,

		/// <summary>
		/// ? (0x40)
		/// </summary>
		PlayerCharacter = 0x40,

		/// <summary>
		/// Both hit at the same time (0x06)
		/// </summary>
		SimultaneousHit = Attacker | Unknown,

		/// <summary>
		/// Alternative target type for Counter? (0x13)
		/// </summary>
		CounteredHit2 = TakeHit | Attacker | SkillActive,

		/// <summary>
		/// Smash/Counter (0x32)
		/// </summary>
		HardHit = SkillSuccess | SkillActive | Attacker,

		/// <summary>
		/// Target type Defense (0x33)
		/// </summary>
		Defended = SkillSuccess | SkillActive | Attacker | TakeHit,

		// Target type with Mana Shield
		// ??? = 0x41,

		/// <summary>
		/// Passive Damage, Shadow Bunshin/Fireball (0x42)
		/// </summary>
		SpecialHit = PlayerCharacter | Attacker,

		/// <summary>
		/// Target type for Counter (0x53)
		/// </summary>
		CounteredHit = PlayerCharacter | SkillActive | Attacker | TakeHit,

		/// <summary>
		/// Magicbolt, range, doing Counter? (0x72)
		/// </summary>
		RangeHit = PlayerCharacter | SkillSuccess | SkillActive | Attacker,

		//DefendedHit = 0x73, // ?
	}

	public enum TargetMode : byte { Normal = 0, Alert = 1, Aggro = 2 }
}
