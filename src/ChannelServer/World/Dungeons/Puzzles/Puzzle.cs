// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using Aura.Channel.Scripting.Scripts;
using Aura.Channel.World.Dungeons.Generation;
using Aura.Channel.World.Dungeons.Props;
using Aura.Channel.World.Entities;
using Aura.Data.Database;
using Aura.Mabi;

namespace Aura.Channel.World.Dungeons.Puzzles
{
	/// <summary>
	/// Dungeon puzzle
	/// </summary>
	/// <remarks>
	/// Aka something that may or may not spawn props and monsters and may or
	/// may not create rooms.
	/// </remarks>
	public class Puzzle
	{
		private DungeonFloorSection _section;
		private Dictionary<string, Object> _variables;
		private Dictionary<string, PuzzlePlace> _places = new Dictionary<string, PuzzlePlace>();
		private Dictionary<string, DungeonMonsterGroupData> _monsterGroupData;
		private Dictionary<string, MonsterGroup> _monsterGroups;

		/// <summary>
		/// Name of the puzzle.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Puzzle's data from db.
		/// </summary>
		public DungeonPuzzleData Data { get; private set; }

		/// <summary>
		/// Data of the floor this puzzle is spawned on.
		/// </summary>
		public DungeonFloorData FloorData { get; private set; }

		/// <summary>
		/// Script that controls this puzzle.
		/// </summary>
		public PuzzleScript Script { get; private set; }

		/// <summary>
		/// Dungeon this puzzle is part of.
		/// </summary>
		public Dungeon Dungeon { get; private set; }

		/// <summary>
		/// Region this puzzle is in.
		/// </summary>
		public Region Region { get; private set; }

		/// <summary>
		/// List of props spawned by this puzzle.
		/// </summary>
		public Dictionary<string, Prop> Props { get; private set; }

		/// <summary>
		/// List of keys created for this puzzle.
		/// </summary>
		public Dictionary<string, Item> Keys { get; private set; }

		/// <summary>
		/// Creates new puzzle.
		/// </summary>
		/// <param name="dungeon"></param>
		/// <param name="section"></param>
		/// <param name="floorData"></param>
		/// <param name="puzzleData"></param>
		/// <param name="puzzleScript"></param>
		public Puzzle(Dungeon dungeon, DungeonFloorSection section, DungeonFloorData floorData, DungeonPuzzleData puzzleData, PuzzleScript puzzleScript)
		{
			_variables = new Dictionary<string, Object>();
			_monsterGroups = new Dictionary<string, MonsterGroup>();
			_monsterGroupData = new Dictionary<string, DungeonMonsterGroupData>();
			this.Props = new Dictionary<string, Prop>();
			this.Keys = new Dictionary<string, Item>();

			_section = section;
			this.Name = puzzleScript.Name;
			this.Data = puzzleData;
			this.Dungeon = dungeon;
			this.Script = puzzleScript;
			this.FloorData = floorData;

			for (int i = 1; i <= puzzleData.Groups.Count; ++i)
				_monsterGroupData["Mob" + i] = puzzleData.Groups[i - 1].Copy();
		}

		/// <summary>
		/// Creates doors for puzzle and calls OnPuzzleCreate.
		/// </summary>
		/// <param name="region"></param>
		public void OnCreate(Region region)
		{
			this.Region = region;
			foreach (var place in _places)
			{
				foreach (var door in place.Value.Doors.Where(x => x != null))
				{
					// Beware, some doors are shared between puzzles
					if (door.EntityId != 0)
						continue;
					door.Info.Region = region.Id;
					region.AddProp(door);
				}
			}

			this.Script.OnPuzzleCreate(this);
		}

		/// <summary>
		/// Creates a new place for the puzzle to use.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public PuzzlePlace NewPlace(string name)
		{
			_places[name] = new PuzzlePlace(_section, this, name);
			return _places[name];
		}

		/// <summary>
		/// Returns the place with the given name, or null if it doesn't exist.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public PuzzlePlace GetPlace(string name)
		{
			if (_places.ContainsKey(name))
				return _places[name];
			return null;
		}

		/// <summary>
		/// Returns prop with the given name, or null if it doesn't exist.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public Prop FindProp(string name)
		{
			if (this.Props.ContainsKey(name))
				return this.Props[name];
			return null;
		}

		/// <summary>
		/// Sets temporary variable.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		public void Set(string name, object value)
		{
			_variables[name] = value;
		}

		/// <summary>
		/// Gets value of temporary variable, returns null if variable doesn't exist.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public dynamic Get(string name)
		{
			if (_variables.ContainsKey(name))
				return _variables[name];
			return null;
		}

		/// <summary>
		/// Locks place and creates and returns a key for it.
		/// </summary>
		/// <param name="place"></param>
		/// <param name="keyName"></param>
		/// <returns></returns>
		public Item LockPlace(PuzzlePlace place, string keyName)
		{
			if (!place.IsLock)
				throw new PuzzleException("Tried to lock a place that isn't a Lock");

			var doorName = place.GetLockDoor().Name;

			Item key;
			if (place.IsBossLock)
			{
				key = Item.CreateKey(70030, doorName); // Boss Room Key
				key.Info.Color1 = 0xFF0000; // Red
			}
			else
			{
				key = Item.CreateKey(70029, doorName); // Dungeon Room Key
				key.Info.Color1 = place.LockColor;
			}

			place.LockPlace(key);
			this.Keys[keyName] = key;

			return key;
		}

		/// <summary>
		/// Locks place without a key.
		/// </summary>
		/// <param name="place"></param>
		/// <returns></returns>
		public void LockPlace(PuzzlePlace place)
		{
			if (!place.IsLock)
				throw new PuzzleException("Tried to lock a place that isn't a Lock");

			place.LockPlace();
		}

		/// <summary>
		/// Adds prop to puzzle in place.
		/// </summary>
		/// <param name="place"></param>
		/// <param name="prop"></param>
		/// <param name="positionType"></param>
		public void AddProp(PuzzlePlace place, DungeonProp prop, Placement positionType)
		{
			if (this.Region == null)
				throw new PuzzleException("AddProp outside of OnPuzzleCreate.");

			var pos = place.GetPosition(positionType);

			prop.RegionId = this.Region.Id;
			prop.Info.X = pos[0];
			prop.Info.Y = pos[1];
			prop.UpdateShapes();
			prop.Info.Direction = MabiMath.DegreeToRadian(pos[2]);
			prop.Behavior += PuzzleEvent;

			this.Region.AddProp(prop);
			this.Props[prop.Name] = prop;
		}

		/// <summary>
		/// Calls OnPropEvent.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="prop"></param>
		public void PuzzleEvent(Creature creature, Prop prop)
		{
			this.Script.OnPropEvent(this, prop);
		}

		/// <summary>
		/// Spawns mob in place.
		/// </summary>
		/// <param name="place"></param>
		/// <param name="name"></param>
		/// <param name="group"></param>
		/// <param name="spawnPosition"></param>
		public void AllocateAndSpawnMob(PuzzlePlace place, string name, DungeonMonsterGroupData group, Placement spawnPosition)
		{
			var mob = new MonsterGroup(name, this, place, spawnPosition);
			_monsterGroups.Add(name, mob);

			mob.Allocate(group);
			mob.Spawn();
		}

		/// <summary>
		/// Returns monster group, or null if it doesn't exist.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public MonsterGroup GetMonsterGroup(string name)
		{
			MonsterGroup result;
			_monsterGroups.TryGetValue(name, out result);
			return result;
		}

		/// <summary>
		/// Returns monster data, or null if it doesn't exist.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public DungeonMonsterGroupData GetMonsterData(string name)
		{
			DungeonMonsterGroupData result;
			_monsterGroupData.TryGetValue(name, out result);
			return result;
		}
	}
}
