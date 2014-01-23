// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using Aura.Channel.World.Entities;
using Aura.Shared.Mabi;
using Aura.Shared.Util;
using Aura.Channel.Network.Sending;
using Aura.Channel.World;

namespace Aura.Channel.Scripting.Scripts
{
	public abstract class AiScript : IDisposable
	{
		protected int DefaultHeartbeat = 50; // ms

		protected Timer _heartbeatTimer;
		protected AiState _state;
		protected double _timestamp;
		protected bool _active;

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

			_heartbeatTimer = new Timer();
			_heartbeatTimer.AutoReset = true;
			_heartbeatTimer.Interval = DefaultHeartbeat;
			_heartbeatTimer.Elapsed += this.Heartbeat;

			_rnd = new Random(RandomProvider.Get().Next());
		}

		public void Dispose()
		{
			_heartbeatTimer.Stop();
			_heartbeatTimer = null;
		}

		/// <summary>
		/// Starts AI
		/// </summary>
		public void Activate()
		{
			if (!_active && _heartbeatTimer != null)
			{
				_active = true;
				_curAction = null;
				_actionQueue.Clear();
				_heartbeatTimer.Start();
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
				_heartbeatTimer.Stop();
			}
		}

		/// <summary>
		/// Changes the hearbeat interval.
		/// </summary>
		/// <param name="interval"></param>
		public void SetHeartbeat(int interval)
		{
			_heartbeatTimer.Interval = Math.Max(50, interval);
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
		protected void Heartbeat(object sender, EventArgs args)
		{
			_timestamp += _heartbeatTimer.Interval;

			// Stop if no players in range
			var players = this.Creature.Region.GetPlayersInRange(this.Creature.GetPosition());
			if (players.Count == 0)
			{
				this.Deactivate();
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
				yield return true;
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

			var pos = this.Creature.GetPosition();
			var rnd = RandomProvider.Get();
			var distance = rnd.Next(minDistance, maxDistance);

			var angle = rnd.NextDouble() * Math.PI * 2;
			var x = pos.X + distance * Math.Cos(angle);
			var y = pos.Y + distance * Math.Sin(angle);
			var destination = new Position((int)x, (int)y);

			this.Creature.Move(destination, true);
			while (this.Creature.GetPosition() != destination)
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
