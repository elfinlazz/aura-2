// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.World.Entities;
using Aura.Shared.Mabi;

namespace Aura.Channel.Scripting.Scripts
{
	/// <summary>
	/// Item script base
	/// </summary>
	/// <remarks>
	/// Stat updates are done automatically, after running the scripts.
	/// </remarks>
	public abstract class ItemScript : BaseScript
	{
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
			creature.Stamina += (float)stamina * creature.StaminaHungryMultiplicator;
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
		/// <param name="creature"></param>
		/// <param name="hunger"></param>
		/// <param name="weight"></param>
		/// <param name="upper"></param>
		/// <param name="lower"></param>
		/// <param name="str"></param>
		/// <param name="int_"></param>
		/// <param name="dex"></param>
		/// <param name="will"></param>
		/// <param name="luck"></param>
		/// <param name="life"></param>
		/// <param name="mana"></param>
		/// <param name="stm"></param>
		protected void Feed(Creature creature, double hunger, double weight = 0, double upper = 0, double lower = 0, double str = 0, double int_ = 0, double dex = 0, double will = 0, double luck = 0, double life = 0, double mana = 0, double stm = 0)
		{
			creature.Hunger -= (float)hunger;

			// handle weight
			// handle stats
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
	}
}
