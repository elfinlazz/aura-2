// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Aura.Channel.Scripting.Scripts;
using Aura.Channel.World.Dungeons.Generation;
using Aura.Channel.World.Entities;
using Aura.Data.Database;
using Aura.Shared.Util;
using Aura.Mabi.Const;

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
		IPuzzleProp FindProp(string name);
		void Set(string name, Object value);
		Object Get(string name);

		/// <summary>
		/// Lock this place with a key.
		/// </summary>
		/// <param name="lockPlace"></param>
		/// <param name="keyName"></param>
		void LockPlace(IPuzzlePlace lockPlace, string keyName);

		/// <summary>
		/// Open locked place.
		/// </summary>
		/// <param name="lockPlace"></param>
		void OpenPlace(IPuzzlePlace lockPlace);

		IPuzzleChest NewChest(IPuzzlePlace place, string name);
		IPuzzleSwitch NewSwitch(IPuzzlePlace place, string name, uint color);
		//IMonsterGroup AllocateMonsterGroup(IPuzzlePlace place, string name, int group);
		//IMonsterGroup GetMonsterGroup(string name);
	}

	public class Puzzle : IPuzzle
	{
		private Dungeon _dungeon;
		private DungeonFloorSection _section;
		private Region _region;
		private PuzzleScript _puzzleScript;
		public Dictionary<string, PuzzleProp> Props;
		public Dictionary<string, Item> Keys { get; private set; }
		public string Name { get; private set; }
		private Dictionary<string, Object> _variables;
		private Dictionary<string, PuzzlePlace> _places = new Dictionary<string, PuzzlePlace>();
		private Dictionary<string, DungeonMonsterGroupData> _monsterGroupData;
		private Dictionary<string, MonsterGroup> _monsterGroups;

		public PuzzleScript Script { get { return _puzzleScript; } }

		public Puzzle(Dungeon dungeon, DungeonFloorSection section, Region region, PuzzleScript puzzleScript, List<DungeonMonsterGroupData> monsterGroups)
		{
			this._dungeon = dungeon;
			this._section = section;
			this._puzzleScript = puzzleScript;
			this.Props = new Dictionary<string, PuzzleProp>();
			this.Keys = new Dictionary<string, Item>();
			this.Name = puzzleScript.Name;
			this._region = region;

			_monsterGroups = new Dictionary<string, MonsterGroup>();
			_monsterGroupData = new Dictionary<string, DungeonMonsterGroupData>();
			for (int i = 1; i <= monsterGroups.Count; ++i)
				_monsterGroupData["Mob" + i] = monsterGroups[i - i];
		}

		public IPuzzlePlace NewPlace(string name)
		{
			this._places[name] = new PuzzlePlace(_section, this, name);
			return this._places[name];
		}

		public Region GetRegion()
		{
			return this._region;
		}


		public Dungeon GetDungeon()
		{
			return this._dungeon;
		}

		public IPuzzlePlace GetPlace(string name)
		{
			return this._places[name];
		}

		public IPuzzleProp FindProp(string name)
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

		public void LockPlace(IPuzzlePlace lockPlace, string keyName)
		{
			var place = lockPlace as PuzzlePlace;
			if (place == null) throw new CPuzzleException("tried to lock a non-existent place");
			if (!place.IsLock)
			{
				throw new CPuzzleException("tried to lock a place that isn't a Lock");
			}
			if (place.IsBossLock)
				place.LockPlace(this.GetBoosKey(keyName));
			else
				place.LockPlace(this.GetKey(keyName, place.LockColor));
		}

		public void OpenPlace(IPuzzlePlace lockPlace)
		{
			var place = lockPlace as PuzzlePlace;
			if (place != null) place.OpenPlace();
			else throw new CPuzzleException("tried to open a non-existent place");
		}

		public Item GetBoosKey(string name)
		{
			if (this.Keys.ContainsKey(name)) return this.Keys[name];
			var key = new Item(70030);
			key.Info.Color1 = 0xFF0000;
			this.Keys[name] = key;
			return key;
		}

		public Item GetKey(string name, uint color)
		{
			if (this.Keys.ContainsKey(name)) return this.Keys[name];
			var key = new Item(70029);
			key.Info.Color1 = color;
			this.Keys[name] = key;
			return key;
		}

		public IPuzzleChest NewChest(IPuzzlePlace place, string name)
		{
			this.Props[name] = new PuzzleChest(place as PuzzlePlace, name);
			return (IPuzzleChest)this.Props[name];
		}

		public IPuzzleSwitch NewSwitch(IPuzzlePlace place, string name, uint color)
		{
			this.Props[name] = new PuzzleSwitch(place as PuzzlePlace, name, color);
			return (IPuzzleSwitch)this.Props[name];
		}

		public void PuzzleEvent(PuzzleProp prop, string propEvent)
		{
			this._puzzleScript.OnPropEvent(this, prop, propEvent);
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
