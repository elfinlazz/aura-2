// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.World.Entities;
using Aura.Mabi.Const;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aura.Channel.World.Dungeons.Props
{
	/// <summary>
	/// Fountain prop, as found in dungeons.
	/// </summary>
	public class Fountain : DungeonProp
	{
		/// <summary>
		/// Maximum amount of times the fountain can be used before it's empty.
		/// </summary>
		private const int MaxTouch = 8;

		private HashSet<long> _touchedBy;
		private int count;

		/// <summary>
		/// Returns true if the fountain is on.
		/// </summary>
		public bool IsOn { get { return (this.State == "on"); } }

		/// <summary>
		/// Creates new fountain.
		/// </summary>
		/// <param name="name"></param>
		public Fountain(string name)
			: base(311, name, "on")
		{
			_touchedBy = new HashSet<long>();

			this.Info.Color1 = 0xE4DFCB;
			this.Info.Color2 = 0x8EFCFF;
			this.Behavior = this.DefaultBehavior;
		}

		/// <summary>
		/// Turns fountain on.
		/// </summary>
		public void TurnOn()
		{
			this.SetState("on");
		}

		/// <summary>
		/// Turns fountain off.
		/// </summary>
		public void TurnOff()
		{
			this.SetState("off");
		}

		/// <summary>
		/// Fountain's default behavior, checking state, who and how many times
		/// they used the fountain, and calling Touch.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="prop"></param>
		protected virtual void DefaultBehavior(Creature creature, Prop prop)
		{
			if (!this.IsOn)
				return;

			lock (_touchedBy)
			{
				if (_touchedBy.Contains(creature.EntityId))
					return;
				_touchedBy.Add(creature.EntityId);
			}

			this.Touch(creature);

			Interlocked.Increment(ref count);
			if (count >= MaxTouch)
				this.TurnOff();
		}

		/// <summary>
		/// Applies random fountain effect to creature.
		/// </summary>
		/// <param name="creature"></param>
		protected virtual void Touch(Creature creature)
		{
			var rnd = RandomProvider.Get();

			// All notices are unofficial

			switch (rnd.Next(15))
			{
				case 0: // Full Life
					{
						creature.FullLifeHeal();
						Send.Notice(creature, Localization.Get("Full Life"));
						break;
					}

				case 1: // 0 Injuries
					{
						creature.Injuries = 0;
						Send.StatUpdateDefault(creature);
						Send.Notice(creature, Localization.Get("No Injuries"));
						break;
					}

				case 2: // Full Stamina
					{
						creature.Stamina = creature.StaminaMax;
						Send.StatUpdateDefault(creature);
						Send.Notice(creature, Localization.Get("Full Stamina"));
						break;
					}

				case 3: // Full Mana
					{
						creature.Mana = creature.ManaMax;
						Send.StatUpdateDefault(creature);
						Send.Notice(creature, Localization.Get("Full Mana"));
						break;
					}

				case 4: // No Hunger
					{
						creature.Hunger = 0;
						Send.StatUpdateDefault(creature);
						Send.Notice(creature, Localization.Get("No Hunger"));
						break;
					}

				case 5: // Gold
					{
						creature.GiveItem(Item.CreateGold(rnd.Next(100, 200 + 1)));
						Send.Notice(creature, Localization.Get("Gold"));
						break;
					}

				case 6: // Exp
					{
						creature.GiveExp(1000);
						Send.Notice(creature, Localization.Get("Exp"));
						break;
					}

				case 7: // Bless All
					{
						foreach (var item in creature.Inventory.ActualEquipment)
						{
							item.OptionInfo.Flags |= ItemFlags.Blessed;
							Send.ItemBlessed(creature, item);
						}
						Send.Notice(creature, Localization.Get("Blessed All"));
						break;
					}

				case 8: // Bless One
					{
						var equip = creature.Inventory.ActualEquipment.Where(a => !a.IsBlessed);
						var count = equip.Count();

						if (count == 0)
							break;

						var item = equip.ElementAt(rnd.Next(count));
						item.OptionInfo.Flags |= ItemFlags.Blessed;
						Send.ItemBlessed(creature, item);

						Send.Notice(creature, Localization.Get("Blessed {0}"), item.Data.Name);
						break;
					}

				case 9: // Repair One
					{
						var equip = creature.Inventory.ActualEquipment.Where(a => a.OptionInfo.Durability != a.OptionInfo.DurabilityMax);
						var count = equip.Count();

						if (count == 0)
							break;

						var item = equip.ElementAt(rnd.Next(count));
						item.OptionInfo.Durability = item.OptionInfo.DurabilityMax;
						Send.ItemDurabilityUpdate(creature, item);

						Send.Notice(creature, Localization.Get("Repaired {0}"), item.Data.Name);
						break;
					}

				case 10: // No Stamina and Hungry
					{
						creature.Stamina = 0;
						creature.Hunger = creature.StaminaMax;
						Send.StatUpdateDefault(creature);
						Send.Notice(creature, Localization.Get("No Stamina and Hungry"));
						break;
					}

				case 11: // Lose one blessing
					{
						var equip = creature.Inventory.ActualEquipment.Where(a => a.IsBlessed);
						var count = equip.Count();

						if (count == 0)
							break;

						var item = equip.ElementAt(rnd.Next(count));
						item.OptionInfo.Flags &= ~ItemFlags.Blessed;
						Send.ItemBlessed(creature, item);

						Send.Notice(creature, Localization.Get("Lost blessing on {0}"), item.Data.Name);
						break;
					}

				case 12: // No Stamina
					{
						creature.Stamina = 0;
						Send.StatUpdateDefault(creature);
						Send.Notice(creature, Localization.Get("No Stamina"));
						break;
					}

				case 13: // Random Injuries
					{
						creature.Injuries = rnd.Next((int)(creature.Life * 0.9f) + 1);
						Send.StatUpdateDefault(creature);
						Send.Notice(creature, Localization.Get("Random Injuries"));
						break;
					}

				case 14: // Lose one durability on random equip
					{
						var equip = creature.Inventory.ActualEquipment.Where(a => a.OptionInfo.Durability >= 1000);
						var count = equip.Count();

						if (count == 0)
							break;

						var item = equip.ElementAt(rnd.Next(count));
						item.OptionInfo.Durability -= 1000;
						Send.ItemDurabilityUpdate(creature, item);

						Send.Notice(creature, Localization.Get("Repaired {0}"), item.Data.Name);
						break;
					}
			}
		}
	}

	/// <summary>
	/// Red Fountain, as found in dungeons.
	/// </summary>
	public class RedFountain : Fountain
	{
		/// <summary>
		/// Creates new red fountain.
		/// </summary>
		/// <param name="name"></param>
		public RedFountain(string name)
			: base(name)
		{
			this.Info.Color2 = 0xA21515;
		}

		/// <summary>
		/// Applies random fountain effect to creature.
		/// </summary>
		/// <remarks>
		/// We're currently missing time limited status affects, which are
		/// needed for the red fountain's effects.
		/// </remarks>
		/// <param name="creature"></param>
		protected override void Touch(Creature creature)
		{
			var rnd = RandomProvider.Get();

			// All notices are unofficial

			// Good
			if (rnd.Next(100) < 60)
			{
			}
			// Bad
			else
			{
			}

			Send.Notice(creature, "Unimplemented.");
		}
	}
}
