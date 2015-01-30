// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;

namespace Aura.Shared.Mabi.Const
{
	[Flags]
	public enum CreatureStates : uint
	{
		Initialized = 0x00000001,
		Dead = 0x00000002,
		SitDown = 0x00000004,
		Spawned = 0x00000008,
		EverEnteredWorld = 0x00000010,
		NoRespawnPenalty = 0x00000020,
		InventoryViewDisabled = 0x00000080,
		PetFinishMode = 0x00000100,
		FreeRebirth = 0x00000200,
		JustRebirthed = 0x00000400,
		LevelResetRebirth = 0x00000800,
		EnableCommonPvp = 0x00001000,
		JournalVisible = 0x00002000,
		TransformCutsceneSkip = 0x00004000,
		EscortNpc = 0x02000000,
		UntouchableNpc = 0x08000000,
		InstantNpc = 0x10000000,

		/// <summary>
		/// Attackable, if not active.
		/// </summary>
		GoodNpc = 0x20000000,

		/// <summary>
		/// If not active, name == race name.
		/// Also required for conversation.
		/// </summary>
		NamedNpc = 0x40000000,

		/// <summary>
		/// Enables conversation and name lookup
		/// (in combination with NamedNpc).
		/// </summary>
		Npc = 0x80000000,
	}

	[Flags]
	public enum CreatureStatesEx : uint
	{
		DefenceBonus = 0x00000001,
		WhisperDisable = 0x00000002,
		FakeDeath = 0x00000004,
		Cloaking = 0x00000008,
		ThrowingStone = 0x00000010,
		SubRace = 0x00000020,
		NotDown = 0x00000040,
		NoHitDelay = 0x00000080,
		Hibernation = 0x00000100,
		SummonedByGiant = 0x00000200,
		DisableMeleeAttack = 0x00000400,
		SyncFrameworkEffect = 0x00000800,
		CloakingAfterRevival = 0x00001000,
		Bubble = 0x00002000,
		CaptureTheFlagMember = 0x00004000,
		CaptureTheFlagRedteam = 0x00008000,
		CaptureTheFlagHasflag = 0x00010000,
		CaptureTheFlagRedflag = 0x00020000,
		CaptureTheFlagFreezed = 0x00040000,
		WaterBalloonBattleMember = 0x00080000,
		WaterBalloonBattleRed = 0x00100000,
		WearShadowdungeonMask = 0x00400000,
		RoyalAlchemist = 0x00800000,
		TailingClaudius = 0x01000000,
		UseUmbrella = 0x02000000,
		ThrowingGold = 0x04000000,
		ZombieWalk = 0x08000000,
		// ? = 0x10000000,
		// ? = 0x20000000,
		RestR9 = 0x40000000,
		// ? = 0x80000000, // Rest related? Makes human sit like a giant Oo
	}
}
