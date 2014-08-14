// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Aura.Channel.Network.Sending;
using Aura.Channel.Skills;
using Aura.Channel.Skills.Base;
using Aura.Channel.World;
using Aura.Channel.World.Entities;
using Aura.Shared.Mabi;
using Aura.Shared.Mabi.Const;
using Aura.Shared.Network;
using Aura.Shared.Util;

namespace Aura.Channel.Scripting.Scripts
{
	public abstract class AiScript : IDisposable
	{
		// Official heartbeat while following a target seems
		// to be about 100-200ms?

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
		protected AggroLimit _aggroLimit;
		protected Dictionary<string, string> _hateTags, _loveTags;
		protected int _maxDistanceFromSpawn;

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

		/// <summary>
		/// Returns state of the AI
		/// </summary>
		public AiState State { get { return _state; } }

		protected AiScript()
		{
			this.Phrases = new List<string>();

			_lastBeat = DateTime.MinValue;
			_heartbeat = IdleHeartbeat;
			_heartbeatTimer = new Timer(this.Heartbeat, null, -1, -1);

			_rnd = new Random(RandomProvider.Get().Next());

			_state = AiState.Idle;
			_aggroRadius = 500;
			_aggroMaxRadius = 3000;
			_alertDelay = TimeSpan.FromMilliseconds(8000);
			_aggroDelay = TimeSpan.FromMilliseconds(4000);
			_hateTags = new Dictionary<string, string>();
			_loveTags = new Dictionary<string, string>();

			_maxDistanceFromSpawn = 3000;

			_aggroType = AggroType.Passive;
			_aggroLimit = AggroLimit.One;
		}

		/// <summary>
		/// Disables heartbeat timer.
		/// </summary>
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
		/// <param name="state"></param>
		private void Heartbeat(object state)
		{
			if (this.Creature == null || this.Creature.Region == null)
				return;

			if (_inside)
				Log.Debug("AI crash in '{0}'.", this.GetType().Name);

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
					// Clearing causes it to run aggro from beginning again
					// and again, this should probably be moved to the take
					// damage "event"?
					//this.Clear();
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
		/// Clears action, target, and sets state to Idle.
		/// </summary>
		private void Reset()
		{
			this.Clear();
			_state = AiState.Idle;

			if (this.Creature.IsInBattleStance)
				this.Creature.IsInBattleStance = false;

			if (this.Creature.Target != null)
			{
				this.Creature.Target = null;
				Send.SetCombatTarget(this.Creature, 0, 0);
			}
		}

		/// <summary>
		/// Changes state based on (potential) targets.
		/// </summary>
		private void SelectState()
		{
			// Always goes back into idle if there's no target.
			// Continue if neutral has a target, for the reset checks.
			if (_aggroType == AggroType.Passive && this.Creature.Target == null)
			{
				_state = AiState.Idle;
				return;
			}

			// Find a target if you don't have one.
			if (this.Creature.Target == null)
			{
				// Try to find a target
				this.Creature.Target = this.SelectRandomTarget(this.Creature.Region.GetVisibleCreaturesInRange(this.Creature, _aggroRadius));
				if (this.Creature.Target != null)
				{
					_state = AiState.Aware;
					_awareTime = DateTime.Now;
				}
			}
			// Got target.
			else
			{
				// Untarget on death, out of range, or disconnect
				if (this.Creature.Target.IsDead || !this.Creature.GetPosition().InRange(this.Creature.Target.GetPosition(), _aggroMaxRadius) || this.Creature.Target.Client.State == ClientState.Dead || (_state != AiState.Aggro && this.Creature.Target.Conditions.Has(ConditionsA.Invisible)))
				{
					this.Reset();
					return;
				}

				// Switch to alert from aware after the delay
				if (_state == AiState.Aware && DateTime.Now >= _awareTime + _alertDelay)
				{
					// Check if target is still in range
					if (this.Creature.GetPosition().InRange(this.Creature.Target.GetPosition(), _aggroRadius))
					{
						this.Clear();

						_state = AiState.Alert;
						_alertTime = DateTime.Now;
						this.Creature.IsInBattleStance = true;

						Send.SetCombatTarget(this.Creature, this.Creature.Target.EntityId, TargetMode.Alert);
					}
					// Reset if target ran away like a coward.
					else
					{
						this.Reset();
					}

					return;
				}

				// Switch to aggro from alert after the delay
				if (_state == AiState.Alert && (_aggroType == AggroType.Aggressive || (_aggroType == AggroType.CarefulAggressive && this.Creature.Target.IsInBattleStance) || (_aggroType > AggroType.Passive && !this.Creature.Target.IsPlayer)) && DateTime.Now >= _alertTime + _aggroDelay)
				{
					// Check aggro limit
					var aggroCount = this.Creature.Region.CountAggro(this.Creature.Target, this.Creature.Race);
					if (aggroCount >= (int)_aggroLimit) return;

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
		private Creature SelectRandomTarget(ICollection<Creature> creatures)
		{
			if (creatures == null || creatures.Count == 0)
				return null;

			// Random targetable creature
			var potentialTargets = creatures.Where(target => this.Creature.CanTarget(target) &&
															 this.DoesHate(target) &&
															 !this.DoesLove(target)).ToList();

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

		// Setup
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
		/// Radius in which creature become potential targets.
		/// </summary>
		/// <param name="radius"></param>
		protected void SetAggroRadius(int radius)
		{
			_aggroRadius = radius;
		}

		/// <summary>
		/// The way the AI decides whether to go into Alert/Aggro.
		/// </summary>
		/// <param name="type"></param>
		protected void SetAggroType(AggroType type)
		{
			_aggroType = type;
		}

		/// <summary>
		/// Milliseconds before creature attacks.
		/// </summary>
		/// <param name="limit"></param>
		protected void SetAggroLimit(AggroLimit limit)
		{
			_aggroLimit = limit;
		}

		/// <summary>
		/// Adds a race tag that the AI hates and will target.
		/// </summary>
		/// <param name="tags"></param>
		protected void Hates(params string[] tags)
		{
			foreach (var tag in tags)
			{
				var key = tag.Trim(' ', '/');
				if (_hateTags.ContainsKey(key))
					return;

				_hateTags.Add(key, tag);
			}
		}

		/// <summary>
		/// Adds a race tag that the AI likes and will not target unless
		/// provoked.
		/// </summary>
		/// <remarks>
		/// By default the AI will only target hated races. Loved races are
		/// a white-list, to filter some races out. For example, when creating
		/// an AI that hates everybody (*), but isn't supposed to target
		/// players (/pc/).
		/// </remarks>
		/// <param name="tags"></param>
		protected void Loves(params string[] tags)
		{
			foreach (var tag in tags)
			{
				var key = tag.Trim(' ', '/');
				if (_loveTags.ContainsKey(key))
					return;

				_loveTags.Add(key, tag);
			}
		}

		/// <summary>
		/// Sets the max distance an NPC can wander away from its spawn.
		/// </summary>
		/// <param name="distance"></param>
		protected void SetMaxDistanceFromSpawn(int distance)
		{
			_maxDistanceFromSpawn = distance;
		}

		// Functions
		// ------------------------------------------------------------------

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

		/// <summary>
		/// Returns true if AI hates target creature.
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		protected bool DoesHate(Creature target)
		{
			return _hateTags.Values.Any(tag => target.RaceData.HasTag(tag));
		}

		/// <summary>
		/// Returns true if AI loves target creature.
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		protected bool DoesLove(Creature target)
		{
			return _loveTags.Values.Any(tag => target.RaceData.HasTag(tag));
		}

		/// <summary>
		/// Returns true if there are collisions between the two positions.
		/// </summary>
		/// <param name="pos1"></param>
		/// <param name="pos2"></param>
		/// <returns></returns>
		protected bool AnyCollisions(Position pos1, Position pos2)
		{
			Position intersection;
			return this.Creature.Region.Collisions.Find(pos1, pos2, out intersection);
		}

		// Flow control
		// ------------------------------------------------------------------

		/// <summary>
		/// Cleares action queue.
		/// </summary>
		protected void Clear()
		{
			_curAction = null;
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
		/// <remarks>
		/// Useful if you want to make a creature go somewhere, but you don't
		/// want to wait for it to arrive there. Effectively running the action
		/// with a 0 timeout.
		/// </remarks>
		/// <param name="action"></param>
		protected void ExecuteOnce(IEnumerable action)
		{
			action.GetEnumerator().MoveNext();
		}

		/// <summary>
		/// Sets target and puts creature in battle mode.
		/// </summary>
		/// <param name="creature"></param>
		protected void AggroCreature(Creature creature)
		{
			_state = AiState.Aggro;
			this.Creature.IsInBattleStance = true;
			this.Creature.Target = creature;
			Send.SetCombatTarget(this.Creature, this.Creature.Target.EntityId, TargetMode.Aggro);
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

			// Make sure NPCs don't wander off
			var npc = this.Creature as NPC;
			if (npc != null && destination.GetDistance(npc.SpawnLocation.Position) > _maxDistanceFromSpawn)
				destination = pos.GetRelative(npc.SpawnLocation.Position, (minDistance + maxDistance) / 2);

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
		/// <param name="destination"></param>
		/// <returns></returns>
		protected IEnumerable RunTo(Position destination)
		{
			return this.MoveTo(destination, false);
		}

		/// <summary>
		/// Creature walks to destination.
		/// </summary>
		/// <param name="destination"></param>
		/// <returns></returns>
		protected IEnumerable WalkTo(Position destination)
		{
			return this.MoveTo(destination, true);
		}

		/// <summary>
		/// Creature moves to destination.
		/// </summary>
		/// <param name="destination"></param>
		/// <param name="walk"></param>
		/// <returns></returns>
		protected IEnumerable MoveTo(Position destination, bool walk)
		{
			var pos = this.Creature.GetPosition();

			// Check for collision
			Position intersection;
			if (this.Creature.Region.Collisions.Find(pos, destination, out intersection))
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
		/// <param name="radius"></param>
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
		/// <param name="radius"></param>
		/// <param name="timeMin"></param>
		/// <param name="timeMax"></param>
		/// <param name="clockwise"></param>
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

		/// <summary>
		/// Attacks target creature "KnockCount" times.
		/// </summary>
		/// <returns></returns>
		protected IEnumerable Attack()
		{
			return this.Attack(this.Creature.RaceData.KnockCount);
		}

		/// <summary>
		/// Attacks target creature x times.
		/// </summary>
		/// <returns></returns>
		protected IEnumerable Attack(int count)
		{
			if (this.Creature.Target == null)
			{
				this.Reset();
				yield break;
			}

			var skillId = SkillId.CombatMastery;

			// Get skill
			var skill = this.Creature.Skills.Get(skillId);
			if (skill == null)
			{
				Log.Error("AI.Attack: Skill '{0}' not found for '{1}'.", skillId, this.Creature.Race);
				yield break;
			}

			// Get skill handler
			var skillHandler = ChannelServer.Instance.SkillManager.GetHandler<ICombatSkill>(skillId);
			if (skillHandler == null)
			{
				Log.Error("AI.Attack: Skill handler not found for '{0}'.", skillId);
				yield break;
			}

			var attackRange = this.Creature.AttackRangeFor(this.Creature.Target);

			// Each successful hit counts, attack until count is reached.
			for (int i = 0; ; )
			{
				var result = skillHandler.Use(this.Creature, skill, this.Creature.Target.EntityId);
				if (result == CombatSkillResult.Okay)
				{
					if (++i >= count)
						yield break;
					else
						yield return true;
				}
				else if (result == CombatSkillResult.OutOfRange)
				{
					var pos = this.Creature.GetPosition();
					var targetPos = this.Creature.Target.GetPosition();

					//this.ExecuteOnce(this.RunTo(pos.GetRelative(targetPos, -attackRange + 50)));
					this.ExecuteOnce(this.RunTo(targetPos));

					yield return true;
				}
				else
				{
					Log.Error("AI.Attack: Unhandled combat skill result ({0}).", result);
					yield break;
				}
			}
		}

		// ------------------------------------------------------------------

		/// <summary>
		/// Called when creature is hit.
		/// </summary>
		/// <param name="action"></param>
		public virtual void OnHit(TargetAction action)
		{
			if (this.Creature.Target == null || (this.Creature.Target != null && action.Attacker != null && !this.Creature.Target.IsPlayer && action.Attacker.IsPlayer) || _state != AiState.Aggro)
			{
				this.AggroCreature(action.Attacker);
			}
		}

		// ------------------------------------------------------------------

		protected enum AggroType
		{
			/// <summary>
			/// Stays in Idle unless provoked
			/// </summary>
			Passive,

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

		protected enum AggroLimit
		{
			/// <summary>
			/// Only auto aggroes if no other creature of the same race
			/// aggroed target yet.
			/// </summary>
			One = 1,

			/// <summary>
			/// Only auto aggroes if at most one other creature of the same
			/// race aggroed target.
			/// </summary>
			Two,

			/// <summary>
			/// Only auto aggroes if at most two other creatures of the same
			/// race aggroed target.
			/// </summary>
			Three,

			/// <summary>
			/// Auto aggroes regardless of other enemies.
			/// </summary>
			None = int.MaxValue,
		}

		public enum AiState
		{
			/// <summary>
			/// Doing nothing
			/// </summary>
			Idle,

			/// <summary>
			/// Doing nothing, but noticed a potential target
			/// </summary>
			Aware,

			/// <summary>
			/// Watching target (!)
			/// </summary>
			Alert,

			/// <summary>
			/// Aggroing target (!!)
			/// </summary>
			Aggro,
		}
	}
}
