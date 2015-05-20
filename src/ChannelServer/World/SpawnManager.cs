// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.World.Entities;
using Aura.Mabi.Const;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aura.Channel.World
{
	public class SpawnManager
	{
		private Dictionary<int, CreatureSpawner> _spawners;

		/// <summary>
		/// Creates new spawn manager.
		/// </summary>
		public SpawnManager()
		{
			_spawners = new Dictionary<int, CreatureSpawner>();
		}

		/// <summary>
		/// Adds spawner to manager.
		/// </summary>
		/// <param name="spawner"></param>
		public void Add(CreatureSpawner spawner)
		{
			lock (_spawners)
				_spawners[spawner.Id] = spawner;
		}

		/// <summary>
		/// Returns spawn with given id or null if it doesn't exist.
		/// </summary>
		/// <param name="spawnerId"></param>
		/// <returns></returns>
		public CreatureSpawner Get(int spawnerId)
		{
			CreatureSpawner result;

			lock (_spawners)
				_spawners.TryGetValue(spawnerId, out result);

			return result;
		}

		/// <summary>
		/// Disposes and removes all spawners.
		/// </summary>
		public void Clear()
		{
			lock (_spawners)
			{
				foreach (var spawner in _spawners.Values)
					spawner.Dispose();
				_spawners.Clear();
			}
		}

		/// <summary>
		/// Calls Spawn on all of the manager's spawners.
		/// </summary>
		public void SpawnAll()
		{
			Log.Info("Spawning creatures...");

			var count = 0;

			foreach (var spawner in _spawners.Values)
				count += spawner.Spawn().Count;

			Log.Info("Done spawning {0} creatures.", count);
		}

		/// <summary>
		/// Spawns a creature.
		/// </summary>
		/// <param name="raceId"></param>
		/// <param name="regionId"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="spawnId"></param>
		/// <param name="active"></param>
		/// <param name="effect"></param>
		/// <returns></returns>
		public NPC Spawn(int raceId, int regionId, int x, int y, bool active, bool effect)
		{
			// Create NPC
			var npc = new NPC(raceId);

			// Warp to spawn point
			if (!npc.Warp(regionId, x, y))
			{
				Log.Error("Failed to spawn '{0}'s, region '{1}' doesn't exist.", raceId, regionId);
				return null;
			}

			// Save spawn location
			npc.SpawnLocation = new Location(regionId, x, y);

			// Activate AI at least once
			if (npc.AI != null && active)
				npc.AI.Activate(0);

			// Spawn effect
			if (effect)
				Send.SpawnEffect(SpawnEffect.Monster, npc.RegionId, x, y, npc, npc);

			return npc;
		}
	}

	/// <summary>
	/// Defines area in which certain creatures are spawned, up to a certain amount.
	/// </summary>
	public class CreatureSpawner : IDisposable
	{
		private static int _id;

		private Point[] _points;
		private int _minX = int.MaxValue, _minY = int.MaxValue, _maxX = 0, _maxY = 0;

		private int[] _titles;

		private List<NPC> _creatures;

		/// <summary>
		/// Unique id for the spawn.
		/// </summary>
		public int Id { get; private set; }

		/// <summary>
		/// Race spawned
		/// </summary>
		public int RaceId { get; private set; }

		/// <summary>
		/// Number of creatures spawned
		/// </summary>
		public int Amount { get; private set; }

		/// <summary>
		/// Region in which the creatures are spawned.
		/// </summary>
		public int RegionId { get; private set; }

		/// <summary>
		/// Creatures new CreatureSpawn
		/// </summary>
		/// <param name="raceId"></param>
		/// <param name="amount"></param>
		/// <param name="regionId"></param>
		/// <param name="coordinates"></param>
		public CreatureSpawner(int raceId, int amount, int[] titles, int regionId, int[] coordinates)
		{
			this.Id = Interlocked.Increment(ref _id);
			this.RaceId = raceId;
			this.Amount = amount;
			this.RegionId = regionId;

			_titles = titles;

			if (coordinates.Length < 2 || coordinates.Length % 2 != 0)
				throw new Exception("CreatureSpawn: Invalid amount of coordinates.");

			_points = new Point[coordinates.Length / 2];
			for (int i = 0, j = 0; i < coordinates.Length; ++j, i += 2)
			{
				_points[j] = new Point(coordinates[i], coordinates[i + 1]);
				if (coordinates[i] < _minX) _minX = coordinates[i];
				if (coordinates[i] > _maxX) _maxX = coordinates[i];
				if (coordinates[i + 1] < _minY) _minY = coordinates[i + 1];
				if (coordinates[i + 1] > _maxY) _maxY = coordinates[i + 1];
			}

			_creatures = new List<NPC>();
		}

		/// <summary>
		/// Unsubscribes from spawned creature's disappear events.
		/// </summary>
		public void Dispose()
		{
			lock (_creatures)
			{
				foreach (var creature in _creatures)
					creature.Disappears -= this.OnDisappears;
				_creatures.Clear();
			}
		}

		/// <summary>
		/// Returns random spawn position.
		/// </summary>
		/// <returns></returns>
		public Point GetRandomPosition()
		{
			// Single position
			if (_points.Length == 1)
				return _points[0];

			var rnd = RandomProvider.Get();

			// Line
			if (_points.Length == 2)
			{
				var d = rnd.NextDouble();
				var x = _points[0].X + (_points[1].X - _points[0].X) * d;
				var y = _points[0].Y + (_points[1].Y - _points[0].Y) * d;
				return new Point((int)x, (int)y);
			}

			// Polygon
			var result = new Point();
			while (!this.IsPointInside(result = new Point(rnd.Next(_minX, _maxX), rnd.Next(_minY, _maxY))))
			{ }

			return result;
		}

		/// <summary>
		/// Returns true if point is within the spawn points.
		/// </summary>
		/// <param name="point"></param>
		/// <returns></returns>
		private bool IsPointInside(Point point)
		{
			var result = false;

			for (int i = 0, j = _points.Length - 1; i < _points.Length; j = i++)
			{
				if (((_points[i].Y > point.Y) != (_points[j].Y > point.Y)) && (point.X < (_points[j].X - _points[i].X) * (point.Y - _points[i].Y) / (_points[j].Y - _points[i].Y) + _points[i].X))
					result = !result;
			}

			return result;
		}

		/// <summary>
		/// Spawns as many creatures as necessary to reach this spawn's amount.
		/// </summary>
		public ICollection<NPC> Spawn()
		{
			var result = new List<NPC>();
			var spawnAmount = Math.Max(0, this.Amount - _creatures.Count);

			for (int i = 0; i < spawnAmount; ++i)
			{
				var creature = this.SpawnOne();
				if (creature != null)
					result.Add(creature);
			}

			return result;
		}

		/// <summary>
		/// Spawns one creature based on this spawners settings.
		/// </summary>
		/// <returns></returns>
		private NPC SpawnOne()
		{
			var pos = this.GetRandomPosition();
			var creature = ChannelServer.Instance.World.SpawnManager.Spawn(this.RaceId, this.RegionId, pos.X, pos.Y, false, false);

			if (creature != null)
			{
				// Random title
				if (_titles != null && _titles.Length != 0)
				{
					var title = (ushort)(_titles[RandomProvider.Get().Next(_titles.Length)]);
					if (title != 0)
					{
						creature.Titles.Enable(title);
						creature.Titles.ChangeTitle(title, false);
					}
				}

				creature.SpawnId = this.Id;
				creature.Disappears += this.OnDisappears;

				lock (_creatures)
					_creatures.Add(creature);
			}

			return creature;
		}

		/// <summary>
		/// Called when an NPC created by this spawner disappears,
		/// after it was killed.
		/// </summary>
		/// <param name="entity"></param>
		private void OnDisappears(Entity entity)
		{
			var npc = entity as NPC;
			if (npc == null)
				return;

			lock (_creatures)
				_creatures.Remove(npc);

			this.SpawnOne();
		}
	}
}
