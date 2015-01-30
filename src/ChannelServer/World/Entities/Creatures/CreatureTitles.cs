// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Aura.Channel.Network.Sending;
using Aura.Data;
using Aura.Shared.Util;
using Aura.Data.Database;
using Aura.Shared.Mabi.Const;

namespace Aura.Channel.World.Entities.Creatures
{
	public class CreatureTitles
	{
		private Creature _creature;

		private Dictionary<ushort, TitleState> _list;

		private TitleData _titleData, _optionTitleData;

		public ushort SelectedTitle { get; set; }
		public ushort SelectedOptionTitle { get; set; }
		public DateTime Applied { get; private set; }

		public CreatureTitles(Creature creature)
		{
			_creature = creature;
			_list = new Dictionary<ushort, TitleState>();
		}

		/// <summary>
		/// Returns amount of known titles.
		/// </summary>
		public int Count { get { lock (_list) return _list.Count; } }

		/// <summary>
		/// Adds title, returns true if title was added or state
		/// was changed.
		/// </summary>
		/// <param name="titleId"></param>
		/// <param name="state"></param>
		/// <returns></returns>
		public bool Add(ushort titleId, TitleState state)
		{
			lock (_list)
			{
				if (_list.ContainsKey(titleId) && _list[titleId] == state)
					return false;

				_list[titleId] = state;
			}
			return true;
		}

		/// <summary>
		/// Removes title.
		/// </summary>
		/// <param name="titleId"></param>
		/// <returns></returns>
		public bool Remove(ushort titleId)
		{
			lock (_list)
				return _list.Remove(titleId);
		}

		/// <summary>
		/// Adds title as "Known" and sends xyz.
		/// </summary>
		/// <param name="titleId"></param>
		public void Show(ushort titleId)
		{
			if (this.Add(titleId, TitleState.Known))
				Send.AddTitle(_creature, titleId, TitleState.Known);
		}

		/// <summary>
		/// Adds title as "Available" and sends xyz.
		/// </summary>
		/// <param name="titleId"></param>
		public void Enable(ushort titleId)
		{
			if (this.Add(titleId, TitleState.Usable))
				Send.AddTitle(_creature, titleId, TitleState.Usable);
		}

		/// <summary>
		/// Returns true if creature knows about title in any way.
		/// </summary>
		/// <param name="titleId"></param>
		/// <returns></returns>
		public bool Knows(ushort titleId)
		{
			lock (_list)
				return (_list.ContainsKey(titleId));
		}

		/// <summary>
		/// Returns true if creature is able to use title.
		/// </summary>
		/// <param name="titleId"></param>
		/// <returns></returns>
		public bool IsUsable(ushort titleId)
		{
			lock (_list)
				return (_list.ContainsKey(titleId) && _list[titleId] == TitleState.Usable);
		}

		/// <summary>
		/// Returns new list of all titles.
		/// </summary>
		/// <returns></returns>
		public ICollection<KeyValuePair<ushort, TitleState>> GetList()
		{
			lock (_list)
				return _list.ToArray();
		}

		/// <summary>
		/// Returns title or option title.
		/// </summary>
		/// <param name="option"></param>
		/// <returns></returns>
		private ushort GetTitle(bool option)
		{
			return (!option ? this.SelectedTitle : this.SelectedOptionTitle);
		}

		/// <summary>
		/// Sets title or option title.
		/// </summary>
		/// <param name="titleId"></param>
		/// <param name="option"></param>
		private void SetTitle(ushort titleId, bool option)
		{
			if (!option)
			{
				this.SelectedTitle = titleId;
				this.Applied = DateTime.Now;
			}
			else
				this.SelectedOptionTitle = titleId;
		}

		/// <summary>
		/// Tries to change title, returns false if anything goes wrong.
		/// </summary>
		/// <param name="titleId"></param>
		/// <param name="option"></param>
		/// <returns></returns>
		public bool ChangeTitle(ushort titleId, bool option)
		{
			if (titleId == 0 && this.GetTitle(option) == 0)
				return true;

			if (titleId != 0 && !this.IsUsable(titleId))
			{
				this.SetTitle(0, option);
				Log.Warning("Player '{0}' tried to use disabled title '{1}'.", _creature.Name, titleId);
				return false;
			}

			TitleData data = null;
			if (titleId != 0)
			{
				data = AuraData.TitleDb.Find(titleId);
				if (data == null)
				{
					this.SetTitle(0, option);
					Log.Warning("Player '{0}' tried to use unknown title '{1}'.", _creature.Name, titleId);
					return false;
				}
			}

			this.SwitchStatMods(data, option);
			this.SetTitle(titleId, option);

			if (_creature.Region != null)
				Send.TitleUpdate(_creature);

			return true;
		}

		/// <summary>
		/// Removes previous stat mods and adds new ones.
		/// </summary>
		/// <param name="data"></param>
		/// <param name="option"></param>
		private void SwitchStatMods(TitleData data, bool option)
		{
			// Remove prev stat mods
			if (option && _optionTitleData != null)
				_creature.StatMods.Remove(StatModSource.Title, this.SelectedOptionTitle);
			else if (!option && _titleData != null)
				_creature.StatMods.Remove(StatModSource.Title, this.SelectedTitle);

			// Add new stat mods
			if (data != null)
			{
				foreach (var effect in data.Effects)
				{
					// Simply adding the bonuses allows to "recover" stats by
					// using different titles, eg first +40, then +120, to
					// add 160 Life, even though it should only be 120?
					// Not much of a problem with title apply delay.

					switch (effect.Key)
					{
						case "Life":
							_creature.StatMods.Add(Stat.LifeMaxMod, effect.Value, StatModSource.Title, data.Id);
							if (effect.Value > 0)
								_creature.Life += effect.Value; // Add value
							else
								_creature.Life = _creature.Life; // "Reset" stat (in case of reducation, stat = max)
							break;
						case "Mana":
							_creature.StatMods.Add(Stat.ManaMaxMod, effect.Value, StatModSource.Title, data.Id);
							if (effect.Value > 0)
								_creature.Mana += effect.Value;
							else
								_creature.Mana = _creature.Mana;
							break;
						case "Stamina":
							// Adjust hunger to new max value, so Food stays
							// at the same percentage.
							var hungerRate = (100 / _creature.StaminaMax * _creature.Hunger) / 100f;

							_creature.StatMods.Add(Stat.StaminaMaxMod, effect.Value, StatModSource.Title, data.Id);
							if (effect.Value > 0)
								_creature.Stamina += effect.Value;
							else
								_creature.Stamina = _creature.Stamina;
							_creature.Hunger = _creature.StaminaMax * hungerRate;
							break;
						case "Str": _creature.StatMods.Add(Stat.StrMod, effect.Value, StatModSource.Title, data.Id); break;
						case "Int": _creature.StatMods.Add(Stat.IntMod, effect.Value, StatModSource.Title, data.Id); break;
						case "Dex": _creature.StatMods.Add(Stat.DexMod, effect.Value, StatModSource.Title, data.Id); break;
						case "Will": _creature.StatMods.Add(Stat.WillMod, effect.Value, StatModSource.Title, data.Id); break;
						case "Luck": _creature.StatMods.Add(Stat.LuckMod, effect.Value, StatModSource.Title, data.Id); break;
						case "Defense": _creature.StatMods.Add(Stat.DefenseBaseMod, effect.Value, StatModSource.Title, data.Id); break;
						case "Protection": _creature.StatMods.Add(Stat.ProtectionBaseMod, effect.Value, StatModSource.Title, data.Id); break;
						case "MinAttack": _creature.StatMods.Add(Stat.AttackMinMod, effect.Value, StatModSource.Title, data.Id); break;
						case "MaxAttack": _creature.StatMods.Add(Stat.AttackMaxMod, effect.Value, StatModSource.Title, data.Id); break;
						default:
							Log.Warning("SwitchStatMods: Unknown title effect '{0}' in title {1}.", effect.Key, data.Id);
							break;
					}
				}
			}

			// Broadcast new stats if creature is in a region yet
			if (_creature.Region != null)
			{
				Send.StatUpdate(_creature, StatUpdateType.Private,
					Stat.LifeMaxMod, Stat.Life, Stat.LifeInjured, Stat.ManaMaxMod, Stat.Mana, Stat.StaminaMaxMod,
					Stat.Stamina, Stat.StrMod, Stat.IntMod, Stat.DexMod, Stat.WillMod, Stat.LuckMod,
					Stat.DefenseBaseMod, Stat.ProtectionBaseMod,
					Stat.AttackMinMod, Stat.AttackMaxMod
				);
				Send.StatUpdate(_creature, StatUpdateType.Public, Stat.Life, Stat.LifeMaxMod, Stat.LifeMax);
			}

			// Save data
			if (!option)
				_titleData = data;
			else
				_optionTitleData = data;
		}
	}

	public enum TitleState : byte { Known = 0, Usable = 1 }
}
