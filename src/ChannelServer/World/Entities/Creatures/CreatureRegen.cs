// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Aura.Channel.Network.Sending;
using Aura.Shared.Mabi;
using Aura.Shared.Mabi.Const;

namespace Aura.Channel.World.Entities.Creatures
{
	/// <summary>
	/// Manages all regens for a creature.
	/// </summary>
	public class CreatureRegen : IDisposable
	{
		private Dictionary<int, StatRegen> _regens;
		private Dictionary<string, List<StatRegen>> _regenGroups;

		public Creature Creature { get; private set; }

		public CreatureRegen(Creature creature)
		{
			_regens = new Dictionary<int, StatRegen>();
			_regenGroups = new Dictionary<string, List<StatRegen>>();
			this.Creature = creature;

			ChannelServer.Instance.Events.SecondsTimeTick += this.OnSecondsTimeTick;
			ChannelServer.Instance.Events.ErinnDaytimeTick += this.OnErinnDaytimeTick;
		}

		public void Dispose()
		{
			ChannelServer.Instance.Events.SecondsTimeTick -= this.OnSecondsTimeTick;
			ChannelServer.Instance.Events.ErinnDaytimeTick -= this.OnErinnDaytimeTick;
		}

		/// <summary>
		/// Adds regen and returns the new object.
		/// Sends stat update.
		/// </summary>
		/// <param name="stat"></param>
		/// <param name="change"></param>
		/// <param name="max"></param>
		/// <param name="duration"></param>
		/// <returns></returns>
		public StatRegen Add(Stat stat, float change, float max, int duration = -1)
		{
			var regen = new StatRegen(stat, change, max, duration);

			lock (_regens)
				_regens.Add(regen.Id, regen);

			// Only send updates if the creature is actually in-game already.
			if (this.Creature.Region != null)
			{
				Send.NewRegens(this.Creature, StatUpdateType.Private, regen);
				if (regen.Stat >= Stat.Life && regen.Stat <= Stat.LifeMaxMod)
					Send.NewRegens(this.Creature, StatUpdateType.Public, regen);
			}

			return regen;
		}

		/// <summary>
		/// Adds regen, sends stat update, saves a reference in the group,
		/// and returns the new regen object.
		/// </summary>
		/// <param name="group"></param>
		/// <param name="stat"></param>
		/// <param name="change"></param>
		/// <param name="max"></param>
		/// <param name="duration"></param>
		/// <returns></returns>
		public StatRegen Add(string group, Stat stat, float change, float max, int duration = -1)
		{
			var regen = this.Add(stat, change, max, duration);

			lock (_regenGroups)
			{
				if (!_regenGroups.ContainsKey(group))
					_regenGroups[group] = new List<StatRegen>();

				_regenGroups[group].Add(regen);
			}

			return regen;
		}

		/// <summary>
		/// Removes regen by id, returns false if regen didn't exist.
		/// Sends stat update if successful.
		/// </summary>
		/// <param name="regen"></param>
		/// <returns></returns>
		public bool Remove(StatRegen regen)
		{
			return this.Remove(regen.Id);
		}

		/// <summary>
		/// Removes every regen in the group, returns false if the group
		/// doesn't exist.
		/// </summary>
		/// <param name="group"></param>
		/// <returns></returns>
		public bool Remove(string group)
		{
			lock (_regenGroups)
			{
				if (!_regenGroups.ContainsKey(group))
					return false;

				foreach (var regen in _regenGroups[group])
					this.Remove(regen);

				_regenGroups.Remove(group);
			}

			return true;
		}

		/// <summary>
		/// Removes regen by id, returns false if regen didn't exist.
		/// Sends stat update if successful.
		/// </summary>
		/// <param name="regenId"></param>
		/// <returns></returns>
		public bool Remove(int regenId)
		{
			StatRegen regen;

			lock (_regens)
			{
				_regens.TryGetValue(regenId, out regen);
				if (regen == null)
					return false;

				_regens.Remove(regenId);
			}

			// Always send private update, only send public if stat is
			// related to life.
			// TODO: When removing the regen, the stat should be updated.
			Send.RemoveRegens(this.Creature, StatUpdateType.Private, regen);
			if (regen.Stat >= Stat.Life && regen.Stat <= Stat.LifeMaxMod)
				Send.RemoveRegens(this.Creature, StatUpdateType.Public, regen);

			return true;
		}

		/// <summary>
		/// Adds additional mana regen at night.
		/// </summary>
		/// <param name="time"></param>
		public void OnErinnDaytimeTick(ErinnTime time)
		{
			if (time.IsNight)
			{
				this.Add("NightMana", Stat.Mana, 0.1f, this.Creature.ManaMax);
			}
			else
			{
				this.Remove("NightMana");
			}
		}

		/// <summary>
		/// Applies regens to creature.
		/// </summary>
		/// <remarks>
		/// (Should be) called once a second.
		/// - Hunger doesn't go beyond 50% of max stamina.
		/// - Stamina regens at 20% efficiency from StaminaHunger onwards.
		/// </remarks>
		public void OnSecondsTimeTick(ErinnTime time)
		{
			if (this.Creature.Region == null || this.Creature.IsDead)
				return;

			lock (_regens)
			{
				var toRemove = new List<int>();

				foreach (var regen in _regens.Values)
				{
					if (regen.TimeLeft == 0)
					{
						toRemove.Add(regen.Id);
						continue;
					}

					switch (regen.Stat)
					{
						case Stat.Life: this.Creature.Life += regen.Change; break;
						case Stat.Mana: this.Creature.Mana += regen.Change; break;
						case Stat.Stamina:
							// Only positve regens are affected by the hunger multiplicator.
							this.Creature.Stamina += regen.Change * (regen.Change > 0 ? this.Creature.StaminaRegenMultiplicator : 1);
							break;
						case Stat.Hunger:
							// Regen can't lower hunger below a certain amount.
							this.Creature.Hunger += regen.Change;
							if (this.Creature.Hunger > this.Creature.StaminaMax / 2)
								this.Creature.Hunger = this.Creature.StaminaMax / 2;
							break;
						case Stat.LifeInjured: this.Creature.Injuries -= regen.Change; break;
					}
				}

				foreach (var id in toRemove)
					this.Remove(id);
			}
		}

		/// <summary>
		/// Returns new list of all active regens.
		/// </summary>
		public ICollection<StatRegen> GetList()
		{
			lock (_regens)
				return _regens.Values.ToArray();
		}

		/// <summary>
		/// Returns new list of all active regens available to the public.
		/// </summary>
		public ICollection<StatRegen> GetPublicList()
		{
			lock (_regens)
				return _regens.Values.Where(a => a.Stat == Stat.Life).ToArray();
		}
	}

	/// <summary>
	/// A regen changes a stat by a specified amount each second.
	/// </summary>
	public class StatRegen
	{
		private static int _id = 0;

		/// <summary>
		/// Unique id of the regen
		/// </summary>
		public int Id { get; protected set; }

		/// <summary>
		/// Stat to be modified
		/// </summary>
		public Stat Stat { get; protected set; }

		/// <summary>
		/// Change per second
		/// </summary>
		public float Change { get; set; }

		/// <summary>
		/// Max value of the stat.
		/// </summary>
		/// <remarks>
		/// This is always the max, negative regens don't need a 0 here.
		/// </remarks>
		public float Max { get; set; }

		/// <summary>
		/// When the regen was started
		/// </summary>
		public DateTime Started { get; protected set; }

		/// <summary>
		/// Duration in ms.
		/// </summary>
		public int Duration { get; protected set; }

		/// <summary>
		/// How much ms are left until the regen ends
		/// </summary>
		public int TimeLeft
		{
			get
			{
				if (this.Duration == -1)
					return -1;

				var passed = DateTime.Now - this.Started;

				if (passed.Milliseconds > this.Duration)
					return 0;
				else
					return this.Duration - passed.Milliseconds;
			}
		}

		public StatRegen(Stat stat, float change, float max, int duration = -1)
		{
			this.Id = Interlocked.Increment(ref _id);
			this.Stat = stat;
			this.Change = change;
			this.Max = max;
			this.Duration = duration;
			this.Started = DateTime.Now;
		}
	}
}
