// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Aura.Channel.Network.Sending;
using Aura.Channel.World.Entities;
using Aura.Data.Database;
using Aura.Mabi;
using Aura.Mabi.Const;
using Aura.Shared.Util;
using Boo.Lang.Compiler.TypeSystem;

namespace Aura.Channel.World.Dungeons.Puzzles
{
	public interface IPuzzleProp
	{
		string GetName();
		string GetState();
		void SetState(string state);
	}

	public class PuzzleProp : IPuzzleProp
	{
		protected Puzzle _puzzle;
		protected PuzzlePlace _place;
		protected Prop _prop;
		protected string _state;
		protected DungeonData _data;
		protected Region _region;
		public string Name { get; private set; }

		public PuzzleProp(PuzzlePlace place, string name)
		{
			this._puzzle = place.GetPuzzle() as Puzzle;
			this._place = place;
			this.Name = name;
			this._prop = null;
			this._data = this._puzzle.GetDungeon().Data;
			this._region = this._puzzle.GetRegion();
		}

		public string GetName()
		{
			return Name;
		}

		public string GetState()
		{
			return this._state;
		}

		public void SetState(string state)
		{
			this._state = state;
			if (this._prop != null)
				this._prop.SetState(this._state);
			if (this._puzzle != null)
				this._puzzle.PuzzleEvent(this, this._state);
		}
	}

	public interface IPuzzleChest : IPuzzleProp
	{
		void Add(Item item);
		void AddKeyForLock(IPuzzlePlace lockPlace);
	}

	public class PuzzleChest : PuzzleProp, IPuzzleChest
	{
		public const int ChestPropId = 10201;

		public List<Item> Items { get; private set; }

		public PuzzleChest(PuzzlePlace place, string name)
			: base(place, name)
		{
			this.Items = new List<Item>();
			this.Spawn();
		}

		private void Spawn()
		{
			var position = this._place.GetPosition();
			var x = position.X * Dungeon.TileSize + Dungeon.TileSize / 2;
			var y = position.Y * Dungeon.TileSize + Dungeon.TileSize / 2;

			this._prop = new Prop(ChestPropId, this._region.Id, x, y, 0.0f);
			this._prop.Extensions.Add(new ConfirmationPropExtension("", Localization.Get("Do you wish to open this chest?")));
			this._prop.Behavior = (creature, prop) =>
			{
				var rnd = RandomProvider.Get();
				foreach (var item in this.Items)
				{
					var pos = new Position(x, y).GetRandomInRange(50, rnd);
					item.Drop(this._region, pos);
				}
				this.SetState("open");
			};
			this._region.AddProp(this._prop);
		}

		public void Add(Item item)
		{
			this.Items.Add(item);
		}

		public void AddKeyForLock(IPuzzlePlace lockPlace)
		{
			var place = lockPlace as PuzzlePlace;
			if (!place.IsLock)
			{
				Log.Warning("PuzzleChest.AddKeyForLock: This place isn't a Lock. ({0})", this._puzzle.Name);
				return;
			}
			this.Add(place.Key);
		}
	}

	public interface IPuzzleSwitch : IPuzzleProp
	{
		void SetColor(uint color);
		void TurnOn();
		void TurnOff();
		bool IsTurnedOn();
	}

	public class PuzzleSwitch : PuzzleProp, IPuzzleSwitch
	{
		public const int SwitchPropId = 10202;
		private uint _color;

		public PuzzleSwitch(PuzzlePlace place, string name, uint color)
			: base(place, name)
		{
			this._color = color;
			this._state = "off";
			this.Spawn();
		}

		public void Spawn()
		{
			var position = this._place.GetPosition();
			var x = position.X * Dungeon.TileSize + Dungeon.TileSize / 2;
			var y = position.Y * Dungeon.TileSize + Dungeon.TileSize / 2;

			this._prop = new Prop(SwitchPropId, this._region.Id, x, y, 0.0f, state: this._state);
			this._prop.Info.Color2 = this._color;

			this._prop.Behavior = (creature, prop) =>
			{
				this.SetState("on");
			};
			this._region.AddProp(this._prop);
		}

		public void SetColor(uint color)
		{
			this._color = color;
			this._region.RemoveProp(this._prop);
			this.Spawn();
		}

		public void TurnOn()
		{
			this.SetState("on");
		}

		public void TurnOff()
		{
			this.SetState("off");
		}

		public bool IsTurnedOn()
		{
			return this._state == "on";
		}

	}

	public class PuzzleDoor : PuzzleProp
	{
		private int _direction;
		private DungeonBlockType _doorType;
		private Item _key;

		public PuzzleDoor(PuzzlePlace place, string name, int direction, DungeonBlockType type)
			: base(place, name)
		{
			this._direction = direction;
			this._doorType = type;
			this._state = "open";
			this._prop = null;
			this._key = null;
			this.Spawn();
		}

		public void Spawn()
		{
			var position = this._place.GetPosition();
			var x = position.X * Dungeon.TileSize + Dungeon.TileSize / 2;
			var y = position.Y * Dungeon.TileSize + Dungeon.TileSize / 2;
			var doorBlock = this._data.Style.Get(this._doorType, this._direction);
			var rotation = MabiMath.DegreeToRadian(doorBlock.Rotation);
			if (this._doorType == DungeonBlockType.BossDoor)
			{
				rotation = MabiMath.DirectionToRadian(0, 1);
				y += Dungeon.TileSize + Dungeon.TileSize / 2;
			}

			this._prop = new Prop(doorBlock.PropId, this._region.Id, x, y, rotation, state: this._state);
			this._prop.Info.Color1 = 0xFFFFFF;
			this._prop.Info.Color2 = 0xFFFFFF;
			if (this._key != null)
				this._prop.Info.Color3 = this._key.Info.Color1;
			this._prop.Behavior = (creature, prop) =>
			{
				switch (this._doorType)
				{
					case DungeonBlockType.BossDoor:
						if (this._key != null)
						{
							if (this.openWithKey(creature))
								this._puzzle.GetDungeon().BossDoorBehavior(creature, prop);
							else Send.Notice(creature, NoticeType.MiddleSystem, Localization.Get("There is no matching key."));
						}
						else this._puzzle.GetDungeon().BossDoorBehavior(creature, prop);
						break;
					case DungeonBlockType.DoorWithLock:
						if (this._key != null)
						{
							if (this.openWithKey(creature))
							{
								this.SetState("open");
								Send.Notice(creature, NoticeType.MiddleSystem,
									Localization.Get("You have opened the door with the key."));
							}
							else Send.Notice(creature, NoticeType.MiddleSystem, Localization.Get("There is no matching key."));
						}
						else
							this.SetState("open");
						break;
					// TODO: allow to teleport into closed room. Don't open 
					case DungeonBlockType.Door:
						this.SetState("open");
						break;
				}
			};
			this._region.AddProp(this._prop);
		}

		private bool openWithKey(Creature character)
		{
			foreach (var item in character.Inventory.Items)
				if (item.Info.Id == 70029 || item.Info.Id == 70030)
					if (item.Info.Color1 == this._key.Info.Color1) // Really?
					{
						character.Inventory.Remove(item);
						return true;
					}
			return false;
		}

		public void Close()
		{
			this.SetState("closed");
		}

		public void Open()
		{
			this.SetState("open");
		}

		public void SetKey(Item key)
		{
			this._key = key;
			this._region.RemoveProp(this._prop);
			this.Spawn();
		}

		public Prop GetDoorProp()
		{
			return this._prop;
		}
	}
}
