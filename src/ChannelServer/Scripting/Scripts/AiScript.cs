// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Channel.World.Entities;
using Aura.Shared.Mabi;
using Aura.Shared.Util;
using Aura.Channel.Network.Sending;
using Aura.Channel.World;
using System.Threading;

namespace Aura.Channel.Scripting.Scripts
{
	public abstract class AiScript : IDisposable
	{
		protected int MinHeartbeat = 50; // ms
		protected int IdleHeartbeat = 250; // ms
		protected int AggroHeartbeat = 50; // ms

		protected Timer _heartbeatTimer;
		protected int _heartbeat;
		protected AiState _state;
		protected double _timestamp;
		protected DateTime _lastBeat;
		protected bool _active;
		protected DateTime _minRunTime;

		protected Queue<IEnumerable> _actionQueue;
		protected IEnumerator _curAction;

		protected Random _rnd;

		public Creature Creature { get; set; }
		public List<string> Phrases { get; protected set; }

		public bool Active { get { return _active; } }

		public AiScript()
		{
			_actionQueue = new Queue<IEnumerable>();
			this.Phrases = new List<string>();

			_lastBeat = DateTime.MinValue;

			_heartbeat = IdleHeartbeat;
			_heartbeatTimer = new Timer(this.Heartbeat, null, -1, -1);

			_rnd = new Random(RandomProvider.Get().Next());
		}

		public void Dispose()
		{
			_heartbeatTimer.Change(-1, -1);
			_heartbeatTimer.Dispose();
			_heartbeatTimer = null;
		}

		/// <summary>
		/// Starts AI
		/// </summary>
		public void Activate(double minRunTime)
		{
			if (!_active && _heartbeatTimer != null)
			{
				_active = true;
				_minRunTime = DateTime.Now.AddMilliseconds(minRunTime);
				_heartbeatTimer.Change(_heartbeat, _heartbeat);
			}
		}

		/// <summary>
		/// Pauses AI
		/// </summary>
		public void Deactivate()
		{
			if (_active && _heartbeatTimer != null)
			{
				_active = false;
				_curAction = null;
				_actionQueue.Clear();
				_heartbeatTimer.Change(-1, -1);
			}
		}

		/// <summary>
		/// Changes the hearbeat interval.
		/// </summary>
		/// <param name="interval"></param>
		public void SetHeartbeat(int interval)
		{
			_heartbeat = Math.Max(MinHeartbeat, interval);
			_heartbeatTimer.Change(_heartbeat, _heartbeat);
		}

		/// <summary>
		/// Adds action to queue.
		/// </summary>
		/// <param name="aiAction"></param>
		protected void Add(IEnumerable aiAction)
		{
			_actionQueue.Enqueue(aiAction);
		}

		/// <summary>
		/// Main "loop"
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public void Heartbeat(object state)
		{
			var now = DateTime.Now;
			_timestamp += (now - _lastBeat).TotalMilliseconds;
			_lastBeat = now;

			var pos = this.Creature.GetPosition();

			// Stop if no players in range
			var players = this.Creature.Region.GetPlayersInRange(pos);
			if (players.Count == 0 && now > _minRunTime)
			{
				this.Deactivate();
				return;
			}

			// Stop and clear queue if stunned
			if (this.Creature.IsStunned)
			{
				if (_actionQueue.Count > 0)
					_actionQueue.Clear();
				return;
			}

			// Run existing actions
			if (CheckQueue())
				return;

			_state = AiState.Idle;

			// Run stat method
			switch (_state)
			{
				case AiState.Idle: Idle(); break;
				case AiState.Aggro: Aggro(); break;
			}

			// Run new actions, added in state methods
			CheckQueue();
		}

		/// <summary>
		/// Gets actions from the queue and runs them.
		/// Returns true as long as it got some action ôo
		/// </summary>
		/// <returns></returns>
		protected bool CheckQueue()
		{
			do
			{
				if (_curAction == null && _actionQueue.Count > 0)
				{
					_curAction = _actionQueue.Dequeue().GetEnumerator();
				}

				if (_curAction != null)
				{
					if (!_curAction.MoveNext())
						_curAction = null;
					return true;
				}
			}
			while (_actionQueue.Count > 0);

			return false;
		}

		/// <summary>
		/// Idle state
		/// </summary>
		protected virtual void Idle()
		{
		}

		/// <summary>
		/// Aggro state
		/// </summary>
		protected virtual void Aggro()
		{
		}

		// ------------------------------------------------------------------

		/// <summary>
		/// Returns random number between 0 and 99.
		/// </summary>
		/// <returns></returns>
		protected int Random()
		{
			return Random(0, 100);
		}

		/// <summary>
		/// Returns random number between 0 and max-1.
		/// </summary>
		/// <param name="max">Exclusive upper bound</param>
		/// <returns></returns>
		protected int Random(int max)
		{
			lock (_rnd)
				return _rnd.Next(max);
		}

		/// <summary>
		/// Returns random number between min and max-1.
		/// </summary>
		/// <param name="min">Inclusive lower bound</param>
		/// <param name="max">Exclusive upper bound</param>
		/// <returns></returns>
		protected int Random(int min, int max)
		{
			lock (_rnd)
				return _rnd.Next(min, max);
		}

		// Actions
		// ------------------------------------------------------------------

		/// <summary>
		/// Makes creature say something in public chat.
		/// </summary>
		/// <param name="msg"></param>
		/// <returns></returns>
		protected IEnumerable ActionSay(string msg)
		{
			Send.Chat(this.Creature, msg);
			yield break;
		}

		/// <summary>
		/// Makes creature say a random phrase in public chat.
		/// </summary>
		/// <returns></returns>
		protected IEnumerable ActionSayRandomPhrase()
		{
			if (this.Phrases.Count != 0)
				Send.Chat(this.Creature, this.Phrases[this.Random(this.Phrases.Count)]);
			yield break;
		}

		/// <summary>
		/// Makes AI wait for a random amount of ms, between min and max.
		/// </summary>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		protected IEnumerable ActionWait(int min, int max = 0)
		{
			if (max < min)
				max = min;

			var duration = (min == max ? min : this.Random(min, max + 1));
			var target = _timestamp + duration;

			while (_timestamp < target)
			{
				yield return true;
			}
		}

		/// <summary>
		/// Makes creature walk to a random position in range.
		/// </summary>
		/// <param name="minDistance"></param>
		/// <param name="maxDistance"></param>
		/// <returns></returns>
		protected IEnumerable ActionWander(int minDistance = 100, int maxDistance = 600)
		{
			if (maxDistance < minDistance)
				maxDistance = minDistance;

			var rnd = RandomProvider.Get();
			var pos = this.Creature.GetPosition();
			var destination = pos.GetRandomInRange(minDistance, maxDistance, rnd);

			// Check for collision, take a break if there is one.
			Position intersection;
			if (this.Creature.Region.Collissions.Find(pos, destination, out intersection))
				//destination = pos.GetRelative(intersection, -10);
				yield break;

			var time = Math.Ceiling(pos.GetDistance(destination) / this.Creature.GetSpeed());
			var targetTime = _timestamp + time;

			this.Creature.Move(destination, true);

			//while (this.Creature.GetPosition() != destination)
			while (_timestamp < targetTime)
				yield return true;
		}

		/// <summary>
		/// Runs action till it's done or the timeout is reached.
		/// </summary>
		/// <param name="timeout"></param>
		/// <param name="action"></param>
		/// <returns></returns>
		protected IEnumerable ActionTimeout(double timeout, IEnumerable action)
		{
			timeout += _timestamp;

			foreach (var a in action)
			{
				if (_timestamp >= timeout)
					yield break;
				yield return true;
			}
		}

		// Action shortcuts
		// ------------------------------------------------------------------

		protected void Say(string msg) { Add(ActionSay(msg)); }
		protected void SayRandomPhrase() { Add(ActionSayRandomPhrase()); }
		protected void Wait(int min, int max = 0) { Add(ActionWait(min, max)); }
		protected void Wander(int minDistance = 100, int maxDistance = 600) { Add(ActionWander(minDistance, maxDistance)); }
		protected void Timeout(double timeout, IEnumerable action) { Add(ActionTimeout(timeout, action)); }

		// ------------------------------------------------------------------

		protected enum AiState { Idle, Aggro }
	}
}
