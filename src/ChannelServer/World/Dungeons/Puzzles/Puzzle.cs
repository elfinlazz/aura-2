// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Aura.Channel.Scripting.Scripts;
using Aura.Channel.World.Dungeons.Generation;
using Aura.Channel.World.Dungeons.Props;
using Aura.Channel.World.Entities;
using Aura.Channel.World.Quests;
using Aura.Data.Database;

namespace Aura.Channel.World.Dungeons.Puzzles
{
	[Serializable]
	public class PuzzleException : Exception
	{
		public PuzzleException()
		{
		}

		public PuzzleException(string message)
			: base(message)
		{
		}

		public PuzzleException(string message, Exception inner)
			: base(message, inner)
		{
		}

		protected PuzzleException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}

	public class Puzzle
	{
		private DungeonFloorSection _section;
		private Dictionary<string, Object> _variables;
		private Dictionary<string, PuzzlePlace> _places = new Dictionary<string, PuzzlePlace>();
		private Dictionary<string, DungeonMonsterGroupData> _monsterGroupData;
		private Dictionary<string, MonsterGroup> _monsterGroups;

		public string Name { get; private set; }
		public PuzzleScript Script { get; private set; }
		public Dungeon Dungeon { get; private set; }
		public Region Region { get; private set; }
		public Dictionary<string, Prop> Props;
		public Dictionary<string, Item> Keys { get; private set; }
		public DungeonFloorData FloorData { get; private set; }

		public Puzzle(Dungeon dungeon, DungeonFloorSection section, DungeonFloorData floorData, PuzzleScript puzzleScript, List<DungeonMonsterGroupData> monsterGroups)
		{
			_variables = new Dictionary<string, Object>();
			_monsterGroups = new Dictionary<string, MonsterGroup>();
			_monsterGroupData = new Dictionary<string, DungeonMonsterGroupData>();
			this.Props = new Dictionary<string, Prop>();
			this.Keys = new Dictionary<string, Item>();

			_section = section;
			this.Name = puzzleScript.Name;
			this.Dungeon = dungeon;
			this.Script = puzzleScript;
			this.FloorData = floorData;

			for (int i = 1; i <= monsterGroups.Count; ++i)
				_monsterGroupData["Mob" + i] = monsterGroups[i - 1];
		}

		public void OnCreate(Region region)
		{
			this.Region = region;
			foreach (var place in _places)
			{
				foreach (var door in place.Value.Doors.Where(x => x != null))
				{
					// Beware, some doors are shared between puzzles
					if (door.EntityId != 0)
						return;
					door.Info.Region = region.Id;
					region.AddProp(door);
				}
			}

			this.Script.OnPuzzleCreate(this);
		}

		public IPuzzlePlace NewPlace(string name)
		{
			this._places[name] = new PuzzlePlace(_section, this, name);
			return this._places[name];
		}

		public IPuzzlePlace GetPlace(string name)
		{
			return this._places[name];
		}

		public Prop FindProp(string name)
		{
			if (this.Props.ContainsKey(name))
				return this.Props[name];
			return null;
		}

		public void Set(string name, Object value)
		{
			this._variables[name] = value;
		}

		public dynamic Get(string name)
		{
			if (this._variables.ContainsKey(name))
				return this._variables[name];
			return null;
		}

		public Item LockPlace(IPuzzlePlace lockPlace, string keyName)
		{
			var place = lockPlace as PuzzlePlace;
			if (place == null)
				throw new PuzzleException("tried to lock a non-existent place");

			if (!place.IsLock)
				throw new PuzzleException("tried to lock a place that isn't a Lock");

			var doorName = place.GetLockDoor().InternalName;

			Item key;
			if (place.IsBossLock)
			{
				key = Item.CreateKey(70030, doorName);
				key.Info.Color1 = 0xFF0000;
			}
			else
			{
				key = Item.CreateKey(70029, doorName);
				key.Info.Color1 = place.LockColor;
			}

			place.LockPlace(key);
			this.Keys[keyName] = key;

			return key;
		}

		public void OpenPlace(IPuzzlePlace lockPlace)
		{
			var place = lockPlace as PuzzlePlace;

			if (place == null)
				throw new PuzzleException("tried to open a non-existent place");

			place.OpenPlace();
		}

		public Item GetKey(string name, uint color)
		{
			if (this.Keys.ContainsKey(name))
				return this.Keys[name];
			return null;
		}

		public void AddProp(IPuzzlePlace place, DungeonProp prop, DungeonPropPositionType positionType)
		{
			if (this.Region == null)
				throw new PuzzleException("NewChest outside of OnPuzzleCreate.");

			var p = place as PuzzlePlace;
			var pos = p.GetPropPosition(positionType, 300);

			prop.RegionId = this.Region.Id;
			prop.Info.X = pos[0];
			prop.Info.Y = pos[1];
			prop.Info.Direction = pos[2];
			prop.Behavior += PuzzleEvent;

			this.Region.AddProp(prop);
			this.Props[prop.InternalName] = prop;
		}

		public void PuzzleEvent(Creature creature, Prop prop)
		{
			this.Script.OnPropEvent(this, prop);
		}

		public void AllocateAndSpawnMob(PuzzlePlace place, string name, DungeonMonsterGroupData group)
		{
			var mob = new MonsterGroup(name, this, place);
			_monsterGroups.Add(name, mob);

			mob.Allocate(group);
			mob.Spawn();
		}

		public MonsterGroup GetMonsterGroup(string name)
		{
			MonsterGroup result;
			_monsterGroups.TryGetValue(name, out result);
			return result;
		}

		public DungeonMonsterGroupData GetMonsterData(string name)
		{
			DungeonMonsterGroupData result;
			_monsterGroupData.TryGetValue(name, out result);
			return result;
		}
	}
}
