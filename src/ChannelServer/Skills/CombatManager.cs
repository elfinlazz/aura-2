// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Aura.Channel.World.Entities;

namespace Aura.Channel.Skills
{
	public class CombatManager
	{
		private ReaderWriterLockSlim _aggroingRWLS = new ReaderWriterLockSlim();

		// aggroer : target
		private Dictionary<Creature, Creature> _aggroing;

		public CombatManager()
		{
			_aggroing = new Dictionary<Creature, Creature>();
		}

		/// <summary>
		/// Notifies manager about an aggro change.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="target"></param>
		public void AggroChange(Creature creature, Creature target)
		{
			_aggroingRWLS.EnterWriteLock();
			try
			{
				_aggroing[creature] = target;
			}
			finally
			{
				_aggroingRWLS.ExitWriteLock();
			}
		}

		/// <summary>
		/// Returns amount of creatures that have target aggroed and have
		/// the same race id.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="raceId"></param>
		/// <returns></returns>
		public int GetAggroCount(Creature target, int raceId)
		{
			_aggroingRWLS.EnterReadLock();
			try
			{
				return _aggroing.Where(a => a.Key.Race == raceId && a.Value == target).Count();
			}
			finally
			{
				_aggroingRWLS.ExitReadLock();
			}
		}
	}
}
