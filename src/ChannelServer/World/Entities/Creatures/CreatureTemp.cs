// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using Aura.Channel.Scripting.Scripts;
using Aura.Data.Database;
using Aura.Shared.Mabi.Structs;

namespace Aura.Channel.World.Entities.Creatures
{
	public class CreatureTemp
	{
		// Sitting
		public ChairData CurrentChairData;
		public Prop SittingProp;

		// Food cache
		public float WeightFoodChange, UpperFoodChange, LowerFoodChange;
		public float LifeFoodChange, ManaFoodChange, StaminaFoodChange;
		public float StrFoodChange, IntFoodChange, DexFoodChange, WillFoodChange, LuckFoodChange;

		// True while visiting Nao
		public bool InSoulStream;

		// Currently playing cutscene
		public Cutscene CurrentCutscene;

		// Last open shop
		public NpcShopScript CurrentShop;

		// Items temporarily used by skills
		public Item SkillItem1, SkillItem2;

		// Random dyeing cursors for regular dyes
		public DyePickers RegularDyePickers;

		// Final Hit training counters
		public int FinalHitKillCount, FinalHitKillCountStrong, FinalHitKillCountAwful, FinalHitKillCountBoss;

		// Backup of target's position when gathering, for run away check
		public Position GatheringTargetPosition;
	}
}
