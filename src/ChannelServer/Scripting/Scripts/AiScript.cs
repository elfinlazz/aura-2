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
using Aura.Shared.Network;
using Aura.Shared.Mabi.Const;

namespace Aura.Channel.Scripting.Scripts
{
	public abstract class AiScript : IDisposable
	{
		protected int MinHeartbeat = 50; // ms
		protected int IdleHeartbeat = 250; // ms
		protected int AggroHeartbeat = 50; // ms

		// Maintenance
		protected Timer _heartbeatTimer;
		protected int _heartbeat;
		protected double _timestamp;
		protected DateTime _lastBeat;
		protected bool _active;
		protected DateTime _minRunTime;
		private bool _inside = false;

		protected Random _rnd;
		protected AiState _state;
		protected IEnumerator _curAction;
		protected Creature _newAttackable;

		// Settings
		protected int _aggroRadius, _aggroMaxRadius;
		protected TimeSpan _alertDelay, _aggroDelay;
		protected DateTime _awareTime, _alertTime;
		protected AggroType _aggroType;

		/// <summary>
		/// Creature controlled by AI.
		/// </summary>
		public Creature Creature { get; set; }

		/// <summary>
		/// List of random phrases
		/// </summary>
		public List<string> Phrases { get; protected set; }

		/// <summary>
		/// Returns whether the AI is currently active.
		/// </summary>
		public bool Active { get { return _active; } }

		public AiScript()
		{
			this.Phrases = new List<string>();

			_lastBeat = DateTime.MinValue;
			_heartbeat = IdleHeartbeat;
			_heartbeatTimer = new Timer(this.Heartbeat, null, -1, -1);

			_rnd = new Random(RandomProvider.Get().Next());

			_state = AiState.Idle;
			_aggroRadius = 1000;
			_aggroMaxRadius = 3000;
			_alertDelay = TimeSpan.FromMilliseconds(8000);
			_aggroDelay = TimeSpan.FromMilliseconds(4000);

			_aggroType = AggroType.Neutral;
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
				_heartbeatTimer.Change(-1, -1);
			}
		}

		/// <summary>
		/// Main "loop"
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void Heartbeat(object state)
		{
			if (this.Creature == null || this.Creature.Region == null)
				return;

			if (_inside)
				Log.Warning("AI crash in '{0}'.", this.GetType().Name);

			_inside = true;
			try
			{
				var now = this.UpdateTimestamp();
				var pos = this.Creature.GetPosition();

				// Stop if no players in range
				var players = this.Creature.Region.GetPlayersInRange(pos);
				if (players.Count == 0 && now > _minRunTime)
				{
					this.Deactivate();
					this.Reset();
					return;
				}

				if (this.Creature.IsDead)
					return;

				this.SelectState();

				// Stop and clear if stunned
				if (this.Creature.IsStunned)
				{
					this.Clear();
					return;
				}

				// Select and run state
				var prevAction = _curAction;
				if (_curAction == null || !_curAction.MoveNext())
				{
					// If action is switched on the last iteration we end up
					// here, with a new action, which would be overwritten
					// with a default right away without this check.
					if (_curAction == prevAction)
					{
						switch (_state)
						{
							default:
							case AiState.Idle: this.SwitchAction(Idle); break;
							case AiState.Alert: this.SwitchAction(Alert); break;
							case AiState.Aggro: this.SwitchAction(Aggro); break;
						}

						_curAction.MoveNext();
					}
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex, "Exception in {0}", this.GetType().Name);
			}
			finally
			{
				_inside = false;
			}
		}

		/// <summary>
		/// Updates timestamp and returns DateTime.Now.
		/// </summary>
		/// <returns></returns>
		private DateTime UpdateTimestamp()
		{
			var now = DateTime.Now;
			_timestamp += (now - _lastBeat).TotalMilliseconds;
			return (_lastBeat = now);
		}

		/// <summary>
		/// Clears action, target, and stats state to Idle.
		/// </summary>
		private void Reset()
		{
			this.Clear();
			this.Creature.Target = null;
			_state = AiState.Idle;
		}

		/// <summary>
		/// Changes state based on (potential) targets.
		/// </summary>
		private void SelectState()
		{
			if (_aggroType == AggroType.Neutral)
			{
				_state = AiState.Idle;
				return;
			}

			if (this.Creature.Target == null)
			{
				// Try to find a target
				this.Creature.Target = this.SelectTarget(this.Creature.Region.GetCreaturesInRange(this.Creature, _aggroRadius));
				if (this.Creature.Target != null)
				{
					_state = AiState.Aware;
					_awareTime = DateTime.Now;
				}
			}
			else
			{
				// Untarget on death, out of range, or disconnect
				if (this.Creature.Target.IsDead || !this.Creature.GetPosition().InRange(this.Creature.Target.GetPosition(), _aggroMaxRadius) || this.Creature.Target.Client.State == ClientState.Dead)
				{
					this.Reset();

					this.Creature.BattleStance = BattleStance.Idle;

					Send.SetCombatTarget(this.Creature, 0, 0);

					return;
				}

				// Switch to alert from aware after the delay
				if (_state == AiState.Aware && DateTime.Now >= _awareTime + _alertDelay)
				{
					this.Clear();

					_state = AiState.Alert;
					_alertTime = DateTime.Now;
					this.Creature.BattleStance = BattleStance.Ready;

					Send.SetCombatTarget(this.Creature, this.Creature.Target.EntityId, TargetMode.Alert);

					return;
				}


				// Switch to aggro from alert after the delay
				if ((_aggroType == AggroType.Aggressive || (_aggroType == AggroType.CarefulAggressive && this.Creature.Target.BattleStance == BattleStance.Ready)) && _state == AiState.Alert && DateTime.Now >= _alertTime + _aggroDelay)
				{
					this.Clear();

					_state = AiState.Aggro;

					Send.SetCombatTarget(this.Creature, this.Creature.Target.EntityId, TargetMode.Aggro);

					return;
				}
			}
		}

		/// <summary>
		/// Returns a valid target or null.
		/// </summary>
		/// <param name="creatures"></param>
		/// <returns></returns>
		private Creature SelectTarget(ICollection<Creature> creatures)
		{
			if (creatures == null || creatures.Count == 0)
				return null;

			// Random targetable creature
			var potentialTargets = creatures.Where(a => this.Creature.CanTarget(a)).ToList();
			if (potentialTargets.Count == 0)
				return null;

			return potentialTargets[Random(potentialTargets.Count)];
		}

		/// <summary>
		/// Idle state
		/// </summary>
		protected virtual IEnumerable Idle()
		{
			yield break;
		}

		/// <summary>
		/// Aware state
		/// </summary>
		protected virtual IEnumerable Alert()
		{
			yield break;
		}

		/// <summary>
		/// Aggro state
		/// </summary>
		protected virtual IEnumerable Aggro()
		{
			yield break;
		}

		// ------------------------------------------------------------------

		/// <summary>
		/// Changes the hearbeat interval.
		/// </summary>
		/// <param name="interval"></param>
		protected void SetHeartbeat(int interval)
		{
			_heartbeat = Math.Max(MinHeartbeat, interval);
			_heartbeatTimer.Change(_heartbeat, _heartbeat);
		}

		/// <summary>
		/// Milliseconds before creature notices.
		/// </summary>
		/// <param name="time"></param>
		protected void SetAlertDelay(int time)
		{
			_alertDelay = TimeSpan.FromMilliseconds(time);
		}

		/// <summary>
		/// Milliseconds before creature attacks.
		/// </summary>
		/// <param name="time"></param>
		protected void SetAggroDelay(int time)
		{
			_aggroDelay = TimeSpan.FromMilliseconds(time);
		}

		/// <summary>
		/// Milliseconds before creature attacks.
		/// </summary>
		/// <param name="time"></param>
		protected void SetAggroRadius(int radius)
		{
			_aggroRadius = radius;
		}

		/// <summary>
		/// Milliseconds before creature attacks.
		/// </summary>
		/// <param name="time"></param>
		protected void SetAggroType(AggroType type)
		{
			_aggroType = type;
		}

		/// <summary>
		/// Clears AI and sets new current action.
		/// </summary>
		/// <param name="action"></param>
		protected void SwitchAction(Func<IEnumerable> action)
		{
			_curAction = action().GetEnumerator();
		}

		/// <summary>
		/// Creates enumerator and runs it once.
		/// </summary>
		/// <param name="action"></param>
		protected void ExecuteOnce(IEnumerable action)
		{
			action.GetEnumerator().MoveNext();
		}

		// ------------------------------------------------------------------

		/// <summary>
		/// Cleares action queue.
		/// </summary>
		protected void Clear()
		{
			_curAction = null;
		}

		/// <summary>
		/// Returns random number between 0.0 and 100.0.
		/// </summary>
		/// <returns></returns>
		protected double Random()
		{
			lock (_rnd)
				return (100 * _rnd.NextDouble());
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
		protected IEnumerable Say(string msg)
		{
			Send.Chat(this.Creature, msg);
			yield break;
		}

		/// <summary>
		/// Makes creature say a random phrase in public chat.
		/// </summary>
		/// <returns></returns>
		protected IEnumerable SayRandomPhrase()
		{
			if (this.Phrases.Count > 0)
				Send.Chat(this.Creature, this.Phrases[this.Random(this.Phrases.Count)]);
			else
				Send.Chat(this.Creature, "...");
			yield break;
		}

		/// <summary>
		/// Makes AI wait for a random amount of ms, between min and max.
		/// </summary>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		protected IEnumerable Wait(int min, int max = 0)
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
		protected IEnumerable Wander(int minDistance = 100, int maxDistance = 600)
		{
			if (maxDistance < minDistance)
				maxDistance = minDistance;

			var rnd = RandomProvider.Get();
			var pos = this.Creature.GetPosition();
			var destination = pos.GetRandomInRange(minDistance, maxDistance, rnd);

			foreach (var action in this.WalkTo(destination))
				yield return action;
		}

		/// <summary>
		/// Runs action till it's done or the timeout is reached.
		/// </summary>
		/// <param name="timeout"></param>
		/// <param name="action"></param>
		/// <returns></returns>
		protected IEnumerable Timeout(double timeout, IEnumerable action)
		{
			timeout += _timestamp;

			foreach (var a in action)
			{
				if (_timestamp >= timeout)
					yield break;
				yield return true;
			}
		}

		/// <summary>
		/// Creature runs to destination.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		protected IEnumerable RunTo(Position destination)
		{
			return this.MoveTo(destination, false);
		}

		/// <summary>
		/// Creature walks to destination.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		protected IEnumerable WalkTo(Position destination)
		{
			return this.MoveTo(destination, true);
		}

		/// <summary>
		/// Creature moves to destination.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		protected IEnumerable MoveTo(Position destination, bool walk)
		{
			var pos = this.Creature.GetPosition();

			// Check for collision
			Position intersection;
			if (this.Creature.Region.Collissions.Find(pos, destination, out intersection))
				destination = pos.GetRelative(intersection, -10);

			this.Creature.Move(destination, walk);

			var time = this.Creature.MoveDuration * 1000;
			var walkTime = _timestamp + time;

			do
			{
				// Yield at least once, even if it took 0 time,
				// to avoid unexpected problems, like infinite outer loops,
				// because an action expected the walk to yield at least once.
				yield return true;
			}
			while (_timestamp < walkTime);
		}

		/// <summary>
		/// Creature circles around target.
		/// </summary>
		/// <param name="distance"></param>
		/// <param name="timeMin"></param>
		/// <param name="timeMax"></param>
		/// <returns></returns>
		protected IEnumerable Circle(int radius, int timeMin = 1000, int timeMax = 5000)
		{
			return this.Circle(radius, timeMin, timeMax, this.Random() < 50);
		}

		/// <summary>
		/// Creature circles around target.
		/// </summary>
		/// <param name="distance"></param>
		/// <param name="timeMin"></param>
		/// <param name="timeMax"></param>
		/// <returns></returns>
		protected IEnumerable Circle(int radius, int timeMin, int timeMax, bool clockwise)
		{
			if (timeMin < 500)
				timeMin = 500;
			if (timeMax < timeMin)
				timeMax = timeMin;

			var time = (timeMin == timeMax ? timeMin : this.Random(timeMin, timeMax + 1));
			var until = _timestamp + time;

			for (int i = 0; _timestamp < until || i == 0; ++i)
			{
				var targetPos = this.Creature.Target.GetPosition();
				var pos = this.Creature.GetPosition();

				var deltaX = pos.X - targetPos.X;
				var deltaY = pos.Y - targetPos.Y;
				var angle = Math.Atan2(deltaY, deltaX) + (Math.PI / 8 * 2) * (clockwise ? -1 : 1);
				var x = targetPos.X + (Math.Cos(angle) * radius);
				var y = targetPos.Y + (Math.Sin(angle) * radius);

				foreach (var action in this.WalkTo(new Position((int)x, (int)y)))
					yield return action;
			}
		}

		/// <summary>
		/// Creature follows its target.
		/// </summary>
		/// <param name="maxDistance"></param>
		/// <returns></returns>
		protected IEnumerable Follow(int maxDistance)
		{
			while (true)
			{
				var pos = this.Creature.GetPosition();
				var targetPos = this.Creature.Target.GetPosition();

				if (!pos.InRange(targetPos, maxDistance))
				{
					// Walk up to distance-50 (a buffer so it really walks into range)
					this.ExecuteOnce(this.WalkTo(pos.GetRelative(targetPos, -maxDistance + 50)));
				}

				yield return true;
			}
		}

		// ------------------------------------------------------------------

		protected enum AiState { Idle, Aware, Alert, Aggro }
		protected enum AggroType
		{
			/// <summary>
			/// Stays in Idle unless provoked
			/// </summary>
			Neutral,

			/// <summary>
			/// Goes into alert, but doesn't attack unprovoked.
			/// </summary>
			Careful,

			/// <summary>
			/// Goes into alert and attacks if target is in battle mode.
			/// </summary>
			CarefulAggressive,

			/// <summary>
			/// Goes straight into alert and aggro.
			/// </summary>
			Aggressive,
		}
	}
}
