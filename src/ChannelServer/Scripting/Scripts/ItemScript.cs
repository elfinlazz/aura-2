// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using Aura.Channel.Network.Sending;
using Aura.Channel.World.Entities;
using Aura.Shared.Mabi;
using Aura.Shared.Mabi.Const;
using Aura.Shared.Util;

namespace Aura.Channel.Scripting.Scripts
{
	/// <summary>
	/// Item script base
	/// </summary>
	/// <remarks>
	/// Stat updates are done automatically, after running the scripts.
	/// </remarks>
	public abstract class ItemScript : GeneralScript
	{
		private const float WeightChangePlus = 0.0015f;
		private const float WeightChangeMinus = 0.000375f;

		/// <summary>
		/// Executed when item is used.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="item"></param>
		public virtual void OnUse(Creature creature, Item item)
		{ }

		/// <summary>
		/// Executed when item is equipped.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="item"></param>
		public virtual void OnEquip(Creature creature, Item item)
		{ }

		/// <summary>
		/// Executed when item is unequipped.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="item"></param>
		public virtual void OnUnequip(Creature creature, Item item)
		{ }

		/// <summary>
		/// Executed when item is first created.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="item"></param>
		public virtual void OnCreation(Item item)
		{ }

		// Functions
		// ------------------------------------------------------------------

		/// <summary>
		/// Heals a certain amount of life, mana, and stamina.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="life"></param>
		/// <param name="mana"></param>
		/// <param name="stamina"></param>
		protected void Heal(Creature creature, double life, double mana, double stamina)
		{
			creature.Life += (float)life;
			creature.Mana += (float)mana;
			creature.Stamina += (float)stamina * creature.StaminaRegenMultiplicator;
		}

		/// <summary>
		/// Heals a certain percentage of life, mana, and stamina.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="life"></param>
		/// <param name="mana"></param>
		/// <param name="stamina"></param>
		protected void HealRate(Creature creature, double life, double mana, double stamina)
		{
			if (life != 0)
				creature.Life += (float)(creature.LifeMax / 100f * life);
			if (mana != 0)
				creature.Mana += (float)(creature.ManaMax / 100f * life);
			if (stamina != 0)
				creature.Stamina += (float)(creature.StaminaMax / 100f * life);
		}

		/// <summary>
		/// Heals life, mana, and stamina completely.
		/// </summary>
		/// <param name="creature"></param>
		protected void HealFull(Creature creature)
		{
			creature.Injuries = 0;
			creature.Hunger = 0;
			this.HealRate(creature, 100, 100, 100);
		}

		/// <summary>
		/// Adds to pot poisoning.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="foodPoison"></param>
		protected void Poison(Creature creature, double foodPoison)
		{
			//creature.X += (float)foodPoison;
		}

		/// <summary>
		/// Reduces hunger by amount and handles weight gain/loss
		/// and stat bonuses.
		/// </summary>
		/// <remarks>
		/// Body and stat changes are applied inside Creature,
		/// on MabiTick (every 5 minutes).
		/// </remarks>
		protected void Feed(Creature creature, double hunger, double weight = 0, double upper = 0, double lower = 0, double str = 0, double int_ = 0, double dex = 0, double will = 0, double luck = 0, double life = 0, double mana = 0, double stm = 0)
		{
			// Hunger
			var diff = creature.Hunger;
			creature.Hunger -= (float)hunger;
			diff -= creature.Hunger;

			// Weight (multiplicators guessed, based on packets)
			// Only increase weight if you eat above 0% Hunger?
			if (diff < hunger)
			{
				creature.Temp.WeightFoodChange += (float)weight * (weight >= 0 ? WeightChangePlus : WeightChangeMinus);
				creature.Temp.UpperFoodChange += (float)upper * (upper >= 0 ? WeightChangePlus : WeightChangeMinus);
				creature.Temp.LowerFoodChange += (float)lower * (lower >= 0 ? WeightChangePlus : WeightChangeMinus);
			}

			// Stats
			creature.Temp.StrFoodChange += MabiMath.FoodStatBonus(str, hunger, diff, creature.Age);
			creature.Temp.IntFoodChange += MabiMath.FoodStatBonus(int_, hunger, diff, creature.Age);
			creature.Temp.DexFoodChange += MabiMath.FoodStatBonus(dex, hunger, diff, creature.Age);
			creature.Temp.WillFoodChange += MabiMath.FoodStatBonus(will, hunger, diff, creature.Age);
			creature.Temp.LuckFoodChange += MabiMath.FoodStatBonus(luck, hunger, diff, creature.Age);
		}

		/// <summary>
		/// Reduces injuries by amount.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="injuries"></param>
		protected void Treat(Creature creature, double injuries)
		{
			creature.Injuries -= (float)injuries;
		}

		/// <summary>
		/// Adds gesture by keyword.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="keyword"></param>
		/// <param name="name"></param>
		protected void AddGesture(Creature creature, string keyword, string name)
		{
			creature.Keywords.Give(keyword);
			Send.Notice(creature, Localization.Get("The {0} Gesture has been added. Check your gestures window."), name);
		}

		/// <summary>
		/// Adds magic seal meta data to item.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="color"></param>
		/// <param name="script"></param>
		protected void MagicSeal(Item item, string color, string script = null)
		{
			item.MetaData1.SetString("MGCSEL", color);
			if (script != null)
				item.MetaData1.SetString("MGCWRD", script);
		}
	}
}
