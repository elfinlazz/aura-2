// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Data.Database;

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

		public bool InSoulStream;
	}
}
