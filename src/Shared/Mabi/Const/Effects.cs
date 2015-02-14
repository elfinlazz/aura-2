// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aura.Shared.Mabi.Const
{
	// No enum because we don't need type safety and string conversion,
	// but something that can be passed to a function. (No casts ftw.)
	public static class Effect
	{
		public static readonly int Revive = 4;

		/// <summary>
		/// Used when picking up a dungeon key.
		/// int:itemId, int:?, int:?, int:?
		/// </summary>
		public static readonly int PickUpKey = 8;

		/// <summary>
		/// Logged values: "", "healing", "flashing", "thunder", "icespear"
		/// string:?
		/// </summary>
		public static readonly int SkillInit = 11;

		/// <summary>
		/// Logged values: "healing", "thunder", "icespear"
		/// string:?
		/// </summary>
		public static readonly int HoldMagic = 12;

		/// <summary>
		/// Logged values: "healing_stack"
		/// string:?, byte:amount?, byte:0
		/// </summary>
		public static readonly int StackUpdate = 13;

		/// <summary>
		/// Logged values: "healing_firstaid", "healing", "healing_phoenix", "thunder"
		/// string:?, long:targetCreatureId
		/// Healing Effects?
		/// </summary>
		public static readonly int UseMagic = 14;

		/// <summary>
		/// b:type, i|s:song, i:?, si:?, i:?, b:quality?, b:instrument, b:?, b:?, b:loops
		/// </summary>
		public static readonly int PlayMusic = 17;

		/// <summary>
		/// On music complete
		/// </summary>
		public static readonly int StopMusic = 18;

		/// <summary>
		/// Used for various pet actions, like dancing, admiring, etc.
		/// long:masterId?, byte:0~?, byte:0
		/// </summary>
		public static readonly int PetAction = 19;

		/// <summary>
		/// White flash.
		/// int:duration, int:0
		/// </summary>
		public static readonly int ScreenFlash = 27;

		/// <summary>
		/// int:region, float:x, float:y, byte:type (0=monster,1=pet,2=pet_despawn,3=monster_despawn,4=golem,5=golem_despawn)
		/// </summary>
		public static readonly int Spawn = 29;

		/// <summary>
		/// Sent by thunder
		/// </summary>
		public static readonly int Lightningbolt = 30;

		/// <summary>
		/// Fireball in the air. int:Region, float:fromx, float:fromy, float:tox, float:toy, int:time, byte:0
		/// </summary>
		public static readonly int FireballFly = 39;

		/// <summary>
		/// The frozen effect of Ice Spear
		/// </summary>
		public static readonly int IcespearFreeze = 65;

		public static readonly int IcespearBoom = 66;

		// [190100, NA201 (2015-02-14)] Something was added somewhere,
		// which increased all following values by 1. Yay enums. Spawn
		// and Flash still worked though, it was afterwards.

		/// <summary>
		/// The teleport effect for Silent Move and Final Hit
		/// </summary>
		public static readonly int SilentMoveTeleport = 68;

		/// <summary>
		/// Effect shown while Final Hit is active.
		/// </summary>
		public static readonly int FinalHit = 70;

		/// <summary>
		/// Chef Owl
		/// </summary>
		public static readonly int ChefOwl = 122;

		/// <summary>
		/// Blue Aura used when activating Mana Shield
		/// </summary>
		/// <remarks>
		/// According to older logs, this should've been 121,
		/// something had been added when we got to implement it.
		/// </remarks>
		public static readonly int ManaShield = 123;

		/// <summary>
		/// Parameters: None
		/// </summary>
		public static readonly int AwakeningOfLight1 = 174;

		/// <summary>
		/// Parameters: None
		/// </summary>
		public static readonly int AwakeningOfLight2 = 177;

		public static readonly int SupportShot = 241;

		/// <summary>
		/// ?
		/// </summary>
		public static readonly int Casting = 248;

		/// <summary>
		/// Shadow Bunshin casting, clones, etc.
		/// </summary>
		public static readonly int ShadowBunshin = 263;

		/// <summary>
		/// Used in thunder's final stage
		/// </summary>
		public static readonly int Thunderbolt = 298;

		/// <summary>
		/// Cherry blossoms falling onto the character.
		/// byte:1|0 (on/off)
		/// </summary>
		public static readonly int CherryBlossoms = 346;

		/// <summary>
		/// Used for Outfit Action.
		/// byte:1|0 (on/off)
		/// </summary>
		public static readonly int OutfitAction = 366;
	}

	public enum SpawnEffect : byte
	{
		Monster = 0,
		Pet = 1,
		PetDespawn = 2,
		MonsterDespawn = 3,
		Golem = 4,
		GolemDespawn = 5,
		//GolemDespawn = 6, // ?
		//Demi? = 7, // ?
		//Demi? = 8, // ?
	}
}
