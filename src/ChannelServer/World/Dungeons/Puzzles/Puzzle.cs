// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
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
	public class CPuzzleException : Exception
	{
		public CPuzzleException()
		{
		}

		public CPuzzleException(string message)
			: base(message)
		{
		}

		public CPuzzleException(string message, Exception inner)
			: base(message, inner)
		{
		}

		protected CPuzzleException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}

	/// <summary>
	/// Provides interface to make new places, props for this puzzle.
	/// </summary>
	public interface IPuzzle
	{
		IPuzzlePlace NewPlace(string name);
		IPuzzlePlace GetPlace(string name);
		Region GetRegion();
		Dungeon GetDungeon();
		Prop FindProp(string name);
		void Set(string name, Object value);
		Object Get(string name);

		/// <summary>
		/// Lock this place with a key.
		/// </summary>
		/// <param name="lockPlace"></param>
		/// <param name="keyName"></param>
		Item LockPlace(IPuzzlePlace lockPlace, string keyName);

		/// <summary>
		/// Open locked place.
		/// </summary>
		/// <param name="lockPlace"></param>
		void OpenPlace(IPuzzlePlace lockPlace);

		Chest NewChest(IPuzzlePlace place, string name, DungeonPropPositionType positionType);
		Switch NewSwitch(IPuzzlePlace place, string name, DungeonPropPositionType positionType, uint color);
		//IMonsterGroup AllocateMonsterGroup(IPuzzlePlace place, string name, int group);
		//IMonsterGroup GetMonsterGroup(string name);
	}

	public class Puzzle : IPuzzle
	{
		private Dungeon _dungeon;
		private DungeonFloorSection _section;
		public DungeonFloorData FloorData { get; private set; }
		private Region _region;
		private PuzzleScript _puzzleScript;
		public Dictionary<string, Prop> Props;
		public Dictionary<string, Item> Keys { get; private set; }
		public string Name { get; private set; }
		private Dictionary<string, Object> _variables;
		private Dictionary<string, PuzzlePlace> _places = new Dictionary<string, PuzzlePlace>();
		private Dictionary<string, DungeonMonsterGroupData> _monsterGroupData;
		private Dictionary<string, MonsterGroup> _monsterGroups;

		public PuzzleScript Script { get { return _puzzleScript; } }

		public Puzzle(Dungeon dungeon, DungeonFloorSection section, DungeonFloorData floorData, PuzzleScript puzzleScript, List<DungeonMonsterGroupData> monsterGroups)
		{
			this._dungeon = dungeon;
			this._section = section;
			this._puzzleScript = puzzleScript;
			this.Props = new Dictionary<string, Prop>();
			this.Keys = new Dictionary<string, Item>();
			_variables = new Dictionary<string, Object>();
			this.Name = puzzleScript.Name;
			_region = null;
			this.FloorData = floorData;

			_monsterGroups = new Dictionary<string, MonsterGroup>();
			_monsterGroupData = new Dictionary<string, DungeonMonsterGroupData>();
			for (int i = 1; i <= monsterGroups.Count; ++i)
				_monsterGroupData["Mob" + i] = monsterGroups[i - 1];
		}

		public void OnCreate(Region region)
		{
			_region = region;
			foreach (var place in _places)
				Array.ForEach(Array.FindAll(place.Value.Doors, x => x!= null), (door) =>
				{
					// Beware, some doors are shared between puzzles
					if (door.EntityId != 0) 
						return;
					door.Info.Region = region.Id;
					region.AddProp(door);
				});
			Script.OnPuzzleCreate(this);
		}

		public IPuzzlePlace NewPlace(string name)
		{
			this._places[name] = new PuzzlePlace(_section, this, name);
			return this._places[name];
		}

		public Region GetRegion()
		{
			return _region;
		}


		public Dungeon GetDungeon()
		{
			return this._dungeon;
		}

		public IPuzzlePlace GetPlace(string name)
		{
			return this._places[name];
		}

		public Prop FindProp(string name)
		{
			if (this.Props.ContainsKey(name)) return this.Props[name];
			return null;
		}

		public void Set(string name, Object value)
		{
			this._variables[name] = value;
		}

		public Object Get(string name)
		{
			if (this._variables.ContainsKey(name)) return this._variables[name];
			return null;
		}

		public Item LockPlace(IPuzzlePlace lockPlace, string keyName)
		{
			var place = lockPlace as PuzzlePlace;
			if (place == null) throw new CPuzzleException("tried to lock a non-existent place");
			if (!place.IsLock)
			{
				throw new CPuzzleException("tried to lock a place that isn't a Lock");
			}
			Item key;
			string doorName = place.GetLockDoor().InternalName;
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
			if (place != null) place.OpenPlace();
			else throw new CPuzzleException("tried to open a non-existent place");
		}

		public Item GetKey(string name, uint color)
		{
			if (this.Keys.ContainsKey(name)) return this.Keys[name];
			return null;
		}

		public Chest NewChest(IPuzzlePlace place, string name, DungeonPropPositionType positionType)
		{
			if (_region == null)
				throw new CPuzzleException("NewChest outside of OnPuzzleCreate.");
			var p = place as PuzzlePlace;
			var pos = p.GetPropPosition(positionType, 300);
			var chest = Chest.CreateChest(pos[0], pos[1], pos[2], regionId: _region.Id, name: name);
			chest.Behavior += PuzzleEvent;
			_region.AddProp(chest);
			this.Props[name] = chest;
			return chest;
		}

		public Switch NewSwitch(IPuzzlePlace place, string name, DungeonPropPositionType positionType,  uint color)
		{
			if (_region == null)
				throw new CPuzzleException("NewSwitch outside of OnPuzzleCreate.");
			var p = place as PuzzlePlace;
			var pos = p.GetPropPosition(positionType);
			var s = Switch.CreateSwitch(pos[0], pos[1], pos[2], color, regionId: _region.Id, name: name);
			s.Behavior += PuzzleEvent;
			_region.AddProp(s);
			this.Props[name] = s;
			return s;
		}

		public void PuzzleEvent(Creature creature, Prop prop)
		{
			this._puzzleScript.OnPropEvent(this, prop);
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
