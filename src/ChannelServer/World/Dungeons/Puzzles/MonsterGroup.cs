// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.World.Entities;
using System;
using System.Linq;
using System.Collections.Generic;
using Aura.Data.Database;
using Aura.Shared.Util;
using Aura.Mabi.Const;
using System.Threading;
using Aura.Mabi;

namespace Aura.Channel.World.Dungeons.Puzzles
{
	public class MonsterGroup
	{
		private List<NPC> _monsters;

		/// <summary>
		/// Name of the group.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Puzzle this group is a part of.
		/// </summary>
		public Puzzle Puzzle { get; private set; }

		/// <summary>
		/// Place this group is a part of.
		/// </summary>
		public PuzzlePlace Place { get; private set; }

		/// <summary>
		/// Amount of monsters in this group.
		/// </summary>
		public int Count { get { return _monsters.Count; } }

		/// <summary>
		/// Amount of alive monsters in this group.
		/// </summary>
		public int Remaining { get { return _remaining; } }
		private int _remaining;

		private Placement _spawnPosition;

		/// <summary>
		/// Creates new monster group.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="puzzle"></param>
		/// <param name="place"></param>
		/// <param name="spawnPosition"></param>
		public MonsterGroup(string name, Puzzle puzzle, PuzzlePlace place, Placement spawnPosition = Placement.Random)
		{
			_monsters = new List<NPC>();

			this.Name = name;
			this.Puzzle = puzzle;
			this.Place = place;
			_spawnPosition = spawnPosition;
		}

		/// <summary>
		/// Creates monsters from group data and adds them to internal list.
		/// </summary>
		/// <param name="groupData"></param>
		public void Allocate(DungeonMonsterGroupData groupData)
		{
			foreach (var monsterData in groupData)
			{
				for (int i = 0; i < monsterData.Amount; ++i)
				{
					var monster = new NPC(monsterData.RaceId);
					monster.State |= CreatureStates.Spawned | CreatureStates.InstantNpc;
					monster.Death += this.OnDeath;

					_monsters.Add(monster);
				}
			}

			_remaining = this.Count;

			this.Puzzle.Script.OnMobAllocated(this.Puzzle, this);
		}

		/// <summary>
		/// Spawns monsters from internal list.
		/// </summary>
		public void Spawn()
		{
			var rnd = RandomProvider.Get();

			var region = this.Puzzle.Region;
			var worldPos = this.Place.GetWorldPosition();

			foreach (var monster in _monsters)
			{
				var pos = this.Place.GetPosition(_spawnPosition);
				monster.Direction = MabiMath.DegreeToByte(pos[2]);
				monster.Spawn(region.Id, pos[0], pos[1]);

				if (monster.AI != null)
					monster.AI.Activate(1000);
			}
		}

		/// <summary>
		/// Raised when one of the monsters dies.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="killer"></param>
		private void OnDeath(Creature creature, Creature killer)
		{
			Interlocked.Decrement(ref _remaining);
			this.Puzzle.Script.OnMonsterDead(this.Puzzle, this);
		}

		/// <summary>
		/// Adds key for lock place to a random monster of this group as a drop.
		/// </summary>
		/// <param name="lockPlace"></param>
		public void AddKeyForLock(PuzzlePlace lockPlace)
		{
			var place = lockPlace as PuzzlePlace;
			if (!place.IsLock)
			{
				Log.Warning("PuzzleChest.AddKeyForLock: This place isn't a Lock. ({0})", this.Puzzle.Name);
				return;
			}

			if (this.Count == 0)
			{
				Log.Warning("MonsterGroup.AddKeyForLock: No monsters in group.");
				return;
			}

			this.AddDrop(place.Key);
		}

		/// <summary>
		/// Adds item to the drops of one random monster in this group.
		/// </summary>
		/// <param name="item"></param>
		public void AddDrop(Item item)
		{
			var rnd = RandomProvider.Get();
			var rndMonster = _monsters[rnd.Next(_monsters.Count)];
			rndMonster.Drops.Add(item);
		}
	}
}
