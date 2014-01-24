// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Collections.Generic;
using System.Linq;
using Aura.Shared.Mabi.Const;
using Aura.Shared.Util;

namespace Aura.Channel.World.Entities.Creatures
{
	public class CreatureStatMods
	{
		private readonly Creature _creature;
		private readonly Dictionary<Stat, List<StatMod>> _mods;
		private readonly Dictionary<Stat, float> _cache;

		public CreatureStatMods(Creature creature)
		{
			_creature = creature;
			_mods = new Dictionary<Stat, List<StatMod>>();
			_cache = new Dictionary<Stat, float>();
		}

		/// <summary>
		/// Adds stat mod.
		/// </summary>
		/// <param name="stat">Stat to change</param>
		/// <param name="value">Amount</param>
		/// <param name="source">What is changing the stat?</param>
		/// <param name="ident">Identificator for the source, eg skill or title id.</param>
		public void Add(Stat stat, float value, StatModSource source, SkillId ident)
		{
			this.Add(stat, value, source, (long)ident);
		}

		/// <summary>
		/// Adds stat mod.
		/// </summary>
		/// <param name="stat">Stat to change</param>
		/// <param name="value">Amount</param>
		/// <param name="source">What is changing the stat?</param>
		/// <param name="ident">Identificator for the source, eg skill or title id.</param>
		public void Add(Stat stat, float value, StatModSource source, long ident)
		{
			lock (_mods)
			{
				if (!_mods.ContainsKey(stat))
					_mods.Add(stat, new List<StatMod>(1));

				var mod = _mods[stat].FirstOrDefault(a => a.Source == source && a.Ident == ident);
				if (mod != null)
					Log.Warning("StatMods.Add: Double stat mod for '{0}:{1}'.", source, ident);

				_mods[stat].Add(new StatMod(stat, value, source, ident));
			}

			this.UpdateCache(stat);
		}

		/// <summary>
		/// Removes stat mod.
		/// </summary>
		/// <param name="stat"></param>
		/// <param name="source"></param>
		/// <param name="ident"></param>
		public void Remove(Stat stat, StatModSource source, SkillId ident)
		{
			this.Remove(stat, source, (long)ident);
		}

		/// <summary>
		/// Removes stat mod.
		/// </summary>
		/// <param name="stat"></param>
		/// <param name="source"></param>
		/// <param name="ident"></param>
		public void Remove(Stat stat, StatModSource source, long ident)
		{
			lock (_mods)
			{
				if (!_mods.ContainsKey(stat))
					return;

				_mods[stat].RemoveAll(a => a.Source == source && a.Ident == ident);
			}

			this.UpdateCache(stat);
		}

		/// <summary>
		/// Removes all stat mods for source and ident.
		/// </summary>
		/// <param name="stat"></param>
		/// <param name="source"></param>
		/// <param name="ident"></param>
		public void Remove(StatModSource source, long ident)
		{
			lock (_mods)
			{
				foreach (var mod in _mods)
				{
					mod.Value.RemoveAll(a => a.Source == source && a.Ident == ident);
					this.UpdateCache(mod.Key);
				}
			}
		}

		/// <summary>
		/// Returns total stat mod for stat.
		/// </summary>
		/// <param name="stat"></param>
		/// <returns></returns>
		public float Get(Stat stat)
		{
			lock (_mods)
			{
				if (!_cache.ContainsKey(stat))
					return 0;

				return _cache[stat];
			}
		}

		/// <summary>
		/// Recalculates cached value for stat.
		/// </summary>
		/// <param name="stat"></param>
		private void UpdateCache(Stat stat)
		{
			lock (_cache)
				_cache[stat] = _mods[stat].Sum(a => a.Value);
		}
	}

	public class StatMod
	{
		public Stat Stat { get; protected set; }
		public float Value { get; protected set; }
		public StatModSource Source { get; protected set; }
		public long Ident { get; protected set; }

		public StatMod(Stat stat, float value, StatModSource source, long ident)
		{
			this.Stat = stat;
			this.Value = value;
			this.Source = source;
			this.Ident = ident;
		}
	}

	public enum StatModSource
	{
		Skill,
		SkillRank,
		Title,
	}
}
