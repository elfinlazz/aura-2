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
using Aura.Channel.Skills.Life;

namespace Aura.Channel.Scripting.Scripts
{
	// TODO: Rewrite into the new tree design before we make more
	//   of a mess out of this than necessary.
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

		protected Dictionary<AiState, Dictionary<AiEvent, Func<IEnumerable>>> _reactions;

		// Heartbeat cache
		protected IList<Creature> _playersInRange;

		// Settings
		protected int _aggroRadius, _aggroMaxRadius;
		protected TimeSpan _alertDelay;
		protected DateTime _awareTime, _alertTime;
		protected AggroLimit _aggroLimit;
		protected Dictionary<string, string> _hateTags, _loveTags, _doubtTags;
		protected bool _hatesBattleStance;
		protected int _maxDistanceFromSpawn;

		/// <summary>
		/// Creature controlled by AI.
		/// </summary>
		public Creature Creature { get; protected set; }

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
			_reactions = new Dictionary<AiState, Dictionary<AiEvent, Func<IEnumerable>>>();
			_reactions[AiState.Idle] = new Dictionary<AiEvent, Func<IEnumerable>>();
			_reactions[AiState.Alert] = new Dictionary<AiEvent, Func<IEnumerable>>();
			_reactions[AiState.Aggro] = new Dictionary<AiEvent, Func<IEnumerable>>();
			_reactions[AiState.Love] = new Dictionary<AiEvent, Func<IEnumerable>>();

			_state = AiState.Idle;
			_aggroRadius = 500;
			_aggroMaxRadius = 3000;
			_alertDelay = TimeSpan.FromMilliseconds(8000);
			_hateTags = new Dictionary<string, string>();
			_loveTags = new Dictionary<string, string>();
			_doubtTags = new Dictionary<string, string>();

			_maxDistanceFromSpawn = 3000;

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
		/// Sets AI's creature.
		/// </summary>
		/// <param name="creature"></param>
		public void Attach(Creature creature)
		{
			this.Creature = creature;
			this.Creature.Death += OnDeath;
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
				_playersInRange = this.Creature.Region.GetPlayersInRange(pos);
				if (_playersInRange.Count == 0 && now > _minRunTime)
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

				// Recover from knock back/down after stun ended
				if (this.Creature.WasKnockedBack)
				{
					Send.RiseFromTheDead(this.Creature);
					this.Creature.WasKnockedBack = false;
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
							case AiState.Love: this.SwitchAction(Love); break;
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
			var potentialTargets = this.Creature.Region.GetVisibleCreaturesInRange(this.Creature, _aggroRadius);

			// Stay in idle if there's no visible creature in aggro range
			if (potentialTargets.Count == 0 && this.Creature.Target == null)
			{
				if (_state != AiState.Idle)
					this.Reset();

				return;
			}

			// Find a new target
			if (this.Creature.Target == null)
			{
				// Get hated targets
				var hated = potentialTargets.Where(cr => this.DoesHate(cr) && !cr.Has(CreatureStates.NamedNpc));
				var hatedCount = hated.Count();

				// Get doubted targets
				var doubted = potentialTargets.Where(cr => this.DoesDoubt(cr) && !cr.Has(CreatureStates.NamedNpc));
				var doubtedCount = doubted.Count();

				// Get loved targets
				var loved = potentialTargets.Where(cr => this.DoesLove(cr));
				var lovedCount = loved.Count();

				// Handle hate and doubt
				if (hatedCount != 0 || doubtedCount != 0)
				{
					// Try to hate first, then doubt
					if (hatedCount != 0)
						this.Creature.Target = hated.ElementAt(this.Random(hatedCount));
					else
						this.Creature.Target = doubted.ElementAt(this.Random(doubtedCount));

					// Switch to aware
					_state = AiState.Aware;
					_awareTime = DateTime.Now;
				}
				// Handle love
				else if (lovedCount != 0)
				{
					this.Creature.Target = loved.ElementAt(this.Random(lovedCount));

					_state = AiState.Love;
				}
				// Stop if no targets were found
				else return;

				// Stop for this tick, the aware delay needs a moment anyway
				return;
			}

			// TODO: Monsters switch targets under certain circumstances,
			//   e.g. a wolf will aggro a player, even if it has already
			//   noticed a cow.

			// Reset on...
			if (this.Creature.Target.IsDead																 // target dead
			|| !this.Creature.GetPosition().InRange(this.Creature.Target.GetPosition(), _aggroMaxRadius) // out of aggro range
			|| this.Creature.Target.Client.State == ClientState.Dead									 // target disconnected
			|| (_state != AiState.Aggro && this.Creature.Target.Conditions.Has(ConditionsA.Invisible))	 // target hid before reaching aggro state
			)
			{
				this.Reset();
				return;
			}

			// Switch to alert from aware after the delay
			if (_state == AiState.Aware && DateTime.Now >= _awareTime + _alertDelay)
			{
				// Check if target is still in immediate range
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
					return;
				}
			}

			// Switch to aggro from alert
			if (_state == AiState.Alert && (this.DoesHate(this.Creature.Target) || (_hatesBattleStance && this.Creature.Target.IsInBattleStance)))
			{
				// Check aggro limit
				var aggroCount = this.Creature.Region.CountAggro(this.Creature.Target, this.Creature.Race);
				if (aggroCount >= (int)_aggroLimit) return;

				this.Clear();

				_state = AiState.Aggro;
				Send.SetCombatTarget(this.Creature, this.Creature.Target.EntityId, TargetMode.Aggro);
			}
		}

		/// <summary>
		/// Idle state
		/// </summary>
		protected virtual IEnumerable Idle()
		{
			yield break;
		}

		/// <summary>
		/// Alert state
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

		/// <summary>
		/// Love state
		/// </summary>
		protected virtual IEnumerable Love()
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
			//_aggroDelay = TimeSpan.FromMilliseconds(time);
			Log.Warning("{0}: SetAggroDelay is obsolete.", this.GetType().Name);
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
			//_aggroType = type;
			Log.Warning("{0}: SetAggroType is obsolete, use 'Doubts' and 'HatesBattleStance' instead.", this.GetType().Name);
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
		/// Adds a race tag that the AI doubts.
		/// </summary>
		/// <param name="tags"></param>
		protected void Doubts(params string[] tags)
		{
			foreach (var tag in tags)
			{
				var key = tag.Trim(' ', '/');
				if (_hateTags.ContainsKey(key))
					return;

				_doubtTags.Add(key, tag);
			}
		}

		/// <summary>
		/// Specifies that the AI will go from alert into aggro when enemy
		/// changes into battle mode.
		/// </summary>
		protected void HatesBattleStance()
		{
			_hatesBattleStance = true;
		}

		/// <summary>
		/// Sets the max distance an NPC can wander away from its spawn.
		/// </summary>
		/// <param name="distance"></param>
		protected void SetMaxDistanceFromSpawn(int distance)
		{
			_maxDistanceFromSpawn = distance;
		}

		/// <summary>
		/// Reigsters a reaction.
		/// </summary>
		/// <param name="ev">The event on which func should be executed.</param>
		/// <param name="func">The reaction to the event.</param>
		protected void On(AiState state, AiEvent ev, Func<IEnumerable> func)
		{
			lock (_reactions)
				_reactions[state][ev] = func;
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
		/// Returns true if AI doubts target creature.
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		protected bool DoesDoubt(Creature target)
		{
			return _doubtTags.Values.Any(tag => target.RaceData.HasTag(tag));
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

		/// <summary>
		/// Sends SharpMind to all applicable creatures.
		/// </summary>
		/// <remarks>
		/// The Wiki is speaking of a passive Sharp Mind skill, but it doesn't
		/// seem to be a skill at all anymore.
		/// 
		/// TODO: Implement old Sharp Mind (optional).
		/// 
		/// TODO: To implement the old Sharp Mind we have to figure out how
		///   to display a failed Sharp Mind (X). "?" is shown for skill id 0.
		///   Older logs make use of status 3 and 4, but the current NA client
		///   doesn't seem to react to them.
		///   If we can't get X to work we could use ? for both.
		/// </remarks>
		/// <param name="skillId"></param>
		/// <param name="status"></param>
		protected void SharpMind(SkillId skillId, SharpMindStatus status)
		{
			foreach (var creature in _playersInRange)
			{
				if (status == SharpMindStatus.Cancelling || status == SharpMindStatus.None)
				{
					Send.SharpMind(this.Creature, creature, skillId, SharpMindStatus.Cancelling);
					Send.SharpMind(this.Creature, creature, skillId, SharpMindStatus.None);
				}
				else
				{
					Send.SharpMind(this.Creature, creature, skillId, status);
				}
			}
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
			this.ExecuteOnce(this.CancelSkill());

			// Cancel rest
			if (this.Creature.Has(CreatureStates.SitDown))
			{
				var restHandler = ChannelServer.Instance.SkillManager.GetHandler<Rest>(SkillId.Rest);
				if (restHandler != null)
					restHandler.Stop(this.Creature, this.Creature.Skills.Get(SkillId.Rest));
			}

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
			this.Clear();
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
		/// <param name="walk"></param>
		/// <param name="timeout"></param>
		/// <returns></returns>
		protected IEnumerable Follow(int maxDistance, bool walk = false, int timeout = 5000)
		{
			var until = _timestamp + Math.Max(0, timeout);

			while (_timestamp < until)
			{
				var pos = this.Creature.GetPosition();
				var targetPos = this.Creature.Target.GetPosition();

				if (!pos.InRange(targetPos, maxDistance))
				{
					// Walk up to distance-50 (a buffer so it really walks into range)
					this.ExecuteOnce(this.MoveTo(pos.GetRelative(targetPos, -maxDistance + 50), walk));
				}

				yield return true;
			}
		}

		/// <summary>
		/// Creature tries to get away from target.
		/// </summary>
		/// <param name="minDistance"></param>
		/// <returns></returns>
		protected IEnumerable KeepDistance(int minDistance, bool walk = false)
		{
			Position pos, targetPos;

			while ((pos = this.Creature.GetPosition()).InRange((targetPos = this.Creature.Target.GetPosition()), minDistance))
			{
				foreach (var action in this.MoveTo(pos.GetRelative(targetPos, minDistance + 50), walk))
					yield return action;
			}

			yield return true;
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
		protected IEnumerable Attack(int count, int timeout = 300000)
		{
			if (this.Creature.Target == null)
			{
				this.Reset();
				yield break;
			}

			timeout = Math2.Clamp(0, 300000, timeout);
			var timeoutDt = DateTime.Now.AddMilliseconds(timeout);

			// Get skill
			var skill = this.Creature.Skills.ActiveSkill;
			if (skill == null && (skill = this.Creature.Skills.Get(SkillId.CombatMastery)) == null)
			{
				Log.Warning("AI.Attack: Creature '{0}' doesn't have Combat Mastery.", this.Creature.Race);
				yield break;
			}

			// Get skill handler
			var skillHandler = ChannelServer.Instance.SkillManager.GetHandler<ICombatSkill>(skill.Info.Id);
			if (skillHandler == null)
			{
				Log.Error("AI.Attack: Skill handler not found for '{0}'.", skill.Info.Id);
				yield break;
			}

			var attackRange = this.Creature.AttackRangeFor(this.Creature.Target);

			// Each successful hit counts, attack until count or timeout is reached.
			for (int i = 0; ; )
			{
				// Stop timeout was reached
				if (DateTime.Now >= timeoutDt)
					break;

				// Attack
				var result = skillHandler.Use(this.Creature, skill, this.Creature.Target.EntityId);
				if (result == CombatSkillResult.Okay)
				{
					// Stop when max attack count is reached
					if (++i >= count)
						break;

					yield return true;
				}
				else if (result == CombatSkillResult.OutOfRange)
				{
					// Run to target if out of range
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

			// Handle completing of skill, if it hasn't been canceled
			if (skill.Info.Id != SkillId.CombatMastery && this.Creature.Skills.ActiveSkill != null)
			{
				// Get handler
				var completeHandler = skillHandler as ICompletable;
				if (completeHandler == null)
				{
					Log.Error("AI.Attack: Missing complete handler for {0}.", skill.Info.Id);
				}
				else
				{
					// Try completing
					try
					{
						completeHandler.Complete(this.Creature, skill, null);
					}
					catch (NullReferenceException)
					{
						Log.Warning("AI.Attack: Null ref exception while completing '{0}', skill might have parameters.", skill.Info.Id);
					}
					catch (NotImplementedException)
					{
						Log.Unimplemented("AI.Attack: Skill complete method for '{0}'.", skill.Info.Id);
					}
				}

				this.SharpMind(this.Creature.Skills.ActiveSkill.Info.Id, SharpMindStatus.Cancelling);

				// Reset active skill in any case.
				this.Creature.Skills.ActiveSkill = null;
			}
		}

		/// <summary>
		/// Makes creature prepare given skill.
		/// </summary>
		/// <param name="skillId"></param>
		/// <returns></returns>
		protected IEnumerable PrepareSkill(SkillId skillId)
		{
			// Get skill
			var skill = this.Creature.Skills.Get(skillId);
			if (skill == null)
			{
				Log.Warning("AI.PrepareSkill: AI '{0}' tried to preapre skill that its creature '{1}' doesn't have.", this.GetType().Name, this.Creature.Race);
				yield break;
			}

			skill.State = SkillState.None;

			// Cancel previous skill
			if (this.Creature.Skills.ActiveSkill != null)
				this.ExecuteOnce(this.CancelSkill());

			// Explicit handling
			if (skillId == SkillId.WebSpinning)
			{
				var skillHandler = ChannelServer.Instance.SkillManager.GetHandler<WebSpinning>(skillId);
				skillHandler.Prepare(this.Creature, skill, null);
				this.Creature.Skills.ActiveSkill = skill;
				skillHandler.Complete(this.Creature, skill, null);
			}
			// Try to handle implicitly
			else
			{
				// Get preparable handler
				var skillHandler = ChannelServer.Instance.SkillManager.GetHandler<IPreparable>(skillId);
				if (skillHandler == null)
				{
					Log.Unimplemented("AI.PrepareSkill: Missing handler or IPreparable for '{0}'.", skillId);
					yield break;
				}

				// Get readyable handler.
				// TODO: There are skills that don't have ready, but go right to
				//   use from Prepare. Handle somehow.
				var readyHandler = skillHandler as IReadyable;
				if (readyHandler == null)
				{
					Log.Unimplemented("AI.PrepareSkill: Missing IReadyable for '{0}'.", skillId);
					yield break;
				}

				this.SharpMind(skillId, SharpMindStatus.Loading);

				// Prepare skill
				try
				{
					if (!skillHandler.Prepare(this.Creature, skill, null))
						yield break;

					this.Creature.Skills.ActiveSkill = skill;
					skill.State = SkillState.Prepared;
				}
				catch (NullReferenceException)
				{
					Log.Warning("AI.PrepareSkill: Null ref exception while preparing '{0}', skill might have parameters.", skillId);
				}
				catch (NotImplementedException)
				{
					Log.Unimplemented("AI.PrepareSkill: Skill prepare method for '{0}'.", skillId);
				}

				// Wait for loading to be done
				foreach (var action in this.Wait(skill.RankData.LoadTime))
					yield return action;

				// Call ready
				readyHandler.Ready(this.Creature, skill, null);
				skill.State = SkillState.Ready;

				this.SharpMind(skillId, SharpMindStatus.Loaded);
			}
		}

		/// <summary>
		/// Makes creature cancel currently loaded skill.
		/// </summary>
		/// <returns></returns>
		protected IEnumerable CancelSkill()
		{
			if (this.Creature.Skills.ActiveSkill != null)
			{
				this.SharpMind(this.Creature.Skills.ActiveSkill.Info.Id, SharpMindStatus.Cancelling);
				this.Creature.Skills.CancelActiveSkill();
			}

			yield break;
		}

		/// <summary>
		/// Makes creature cancel currently loaded skill.
		/// </summary>
		/// <returns></returns>
		protected IEnumerable CompleteSkill()
		{
			if (this.Creature.Skills.ActiveSkill == null)
				yield break;

			var skill = this.Creature.Skills.ActiveSkill;
			var skillId = this.Creature.Skills.ActiveSkill.Info.Id;

			this.Creature.Skills.ActiveSkill = null;
			skill.State = SkillState.Completed;

			this.SharpMind(skillId, SharpMindStatus.Cancelling);

			var skillHandler = ChannelServer.Instance.SkillManager.GetHandler<ICompletable>(skillId);
			if (skillHandler == null)
			{
				Log.Unimplemented("AI.CompleteSkill: Missing handler or ICompletable for '{0}'.", skillId);
				yield break;
			}

			try
			{
				skillHandler.Complete(this.Creature, skill, null);
				skill.State = SkillState.Completed;
			}
			catch (NullReferenceException)
			{
				Log.Warning("AI.CompleteSkill: Null ref exception while preparing '{0}', skill might have parameters.", skillId);
			}
			catch (NotImplementedException)
			{
				Log.Unimplemented("AI.CompleteSkill: Skill complete method for '{0}'.", skillId);
			}
		}

		/// <summary>
		/// Makes creature start given skill.
		/// </summary>
		/// <param name="skillId"></param>
		/// <returns></returns>
		protected IEnumerable StartSkill(SkillId skillId)
		{
			// Get skill
			var skill = this.Creature.Skills.Get(skillId);
			if (skill == null)
			{
				Log.Warning("AI.StartSkill: AI '{0}' tried to preapre skill that its creature '{1}' doesn't have.", this.GetType().Name, this.Creature.Race);
				yield break;
			}

			// Get handler
			var skillHandler = ChannelServer.Instance.SkillManager.GetHandler<IStartable>(skillId);
			if (skillHandler == null)
			{
				Log.Unimplemented("AI.StartSkill: Missing handler or interface for '{0}'.", skillId);
				yield break;
			}

			// Run handler
			try
			{
				if (skillHandler is Rest)
				{
					var restHandler = (Rest)skillHandler;
					restHandler.Start(this.Creature, skill, MabiDictionary.Empty);
				}
				else
				{
					skillHandler.Start(this.Creature, skill, null);
				}
			}
			catch (NullReferenceException)
			{
				Log.Warning("AI.StartSkill: Null ref exception while starting '{0}', skill might have parameters.", skillId);
			}
			catch (NotImplementedException)
			{
				Log.Unimplemented("AI.StartSkill: Skill start method for '{0}'.", skillId);
			}
		}

		/// <summary>
		/// Makes creature stop given skill.
		/// </summary>
		/// <param name="skillId"></param>
		/// <returns></returns>
		protected IEnumerable StopSkill(SkillId skillId)
		{
			// Get skill
			var skill = this.Creature.Skills.Get(skillId);
			if (skill == null)
			{
				Log.Warning("AI.StopSkill: AI '{0}' tried to preapre skill that its creature '{1}' doesn't have.", this.GetType().Name, this.Creature.Race);
				yield break;
			}

			// Get handler
			var skillHandler = ChannelServer.Instance.SkillManager.GetHandler<IStoppable>(skillId);
			if (skillHandler == null)
			{
				Log.Unimplemented("AI.StopSkill: Missing handler or interface for '{0}'.", skillId);
				yield break;
			}

			// Run handler
			try
			{
				if (skillHandler is Rest)
				{
					var restHandler = (Rest)skillHandler;
					restHandler.Stop(this.Creature, skill, MabiDictionary.Empty);
				}
				else
				{
					skillHandler.Stop(this.Creature, skill, null);
				}
			}
			catch (NullReferenceException)
			{
				Log.Warning("AI.StopSkill: Null ref exception while stopping '{0}', skill might have parameters.", skillId);
			}
			catch (NotImplementedException)
			{
				Log.Unimplemented("AI.StopSkill: Skill stop method for '{0}'.", skillId);
			}
		}

		// ------------------------------------------------------------------

		/// <summary>
		/// Called when creature is hit.
		/// </summary>
		/// <param name="action"></param>
		public virtual void OnHit(TargetAction action)
		{
			// Aggro attacker if there is not current target,
			// or if there is a target but it's not a player, and the attacker is one,
			// or if the current target is not aggroed yet.
			if (this.Creature.Target == null || (this.Creature.Target != null && action.Attacker != null && !this.Creature.Target.IsPlayer && action.Attacker.IsPlayer) || _state != AiState.Aggro)
			{
				this.AggroCreature(action.Attacker);
			}

			var activeSkillWas = SkillId.None;

			if (this.Creature.Skills.ActiveSkill != null)
			{
				activeSkillWas = this.Creature.Skills.ActiveSkill.Info.Id;
				this.SharpMind(this.Creature.Skills.ActiveSkill.Info.Id, SharpMindStatus.Cancelling);
			}

			lock (_reactions)
			{
				if (activeSkillWas == SkillId.Defense && _reactions[_state].ContainsKey(AiEvent.DefenseHit))
				{
					this.SwitchAction(_reactions[_state][AiEvent.DefenseHit]);
				}
				else if (action.Has(TargetOptions.KnockDown) && _reactions[_state].ContainsKey(AiEvent.KnockDown))
				{
					this.SwitchAction(_reactions[_state][AiEvent.KnockDown]);
				}
				else if (_reactions[_state].ContainsKey(AiEvent.Hit))
				{
					this.SwitchAction(_reactions[_state][AiEvent.Hit]);
				}
			}
		}

		/// <summary>
		/// Raised from Creature.Kill when creature died,
		/// before active skill is canceled.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="killer"></param>
		private void OnDeath(Creature creature, Creature killer)
		{
			if (this.Creature.Skills.ActiveSkill != null)
				this.SharpMind(this.Creature.Skills.ActiveSkill.Info.Id, SharpMindStatus.Cancelling);
		}

		/// <summary>
		/// Called when the AI hit someone with a skill.
		/// </summary>
		/// <param name="aAction"></param>
		public void OnUsedSkill(AttackerAction aAction)
		{
			if (this.Creature.Skills.ActiveSkill != null)
				this.ExecuteOnce(this.CompleteSkill());
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

			/// <summary>
			/// Likes target
			/// </summary>
			Love,
		}

		public enum AiEvent
		{
			Hit,
			DefenseHit,
			KnockDown,
		}
	}
}
