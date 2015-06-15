// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Collections.Generic;
using Aura.Channel.World.Dungeons.Generation;
using Aura.Channel.World.Entities;
using Aura.Mabi.Const;
using System;
using Aura.Channel.World.Dungeons.Props;
using Aura.Data.Database;

namespace Aura.Channel.World.Dungeons.Puzzles
{
	/// <summary>
	/// Place on a tile in a dungeon that contains a part of a puzzle.
	/// </summary>
	public class PuzzlePlace
	{
		private DungeonFloorSection _section;
		private string _name;
		private LinkedListNode<RoomTrait> _placeNode;
		private PlacementProvider[] _placementProviders;

		/// <summary>
		/// X world coordinate of this place.
		/// </summary>
		public int X { get; private set; }

		/// <summary>
		/// Y world coordinate of this place.
		/// </summary>
		public int Y { get; private set; }

		/// <summary>
		/// Direction in which the place is locked.
		/// </summary>
		public int DoorDirection { get; private set; }

		/// <summary>
		/// Returns true if place contains a locked door.
		/// </summary>
		public bool IsLock { get; private set; }

		/// <summary>
		/// Color of the lock.
		/// </summary>
		public uint LockColor { get; private set; }

		/// <summary>
		/// Returns true if place contains measures to unlock a place.
		/// </summary>
		public bool IsUnlock { get; private set; }

		/// <summary>
		/// Returns true if this place contains the boss lock.
		/// </summary>
		public bool IsBossLock { get; private set; }

		/// <summary>
		/// The key this place's door is locked with.
		/// </summary>
		public Item Key { get; private set; }

		/// <summary>
		/// Doors between this place, alleys, and other rooms.
		/// </summary>
		public Door[] Doors { get; private set; }

		/// <summary>
		/// The puzzle this place is a part of.
		/// </summary>
		public Puzzle Puzzle { get; private set; }

		/// <summary>
		/// Creates new puzzle place.
		/// </summary>
		/// <param name="section"></param>
		/// <param name="puzzle"></param>
		/// <param name="name"></param>
		public PuzzlePlace(DungeonFloorSection section, Puzzle puzzle, string name)
		{
			this.Doors = new Door[] { null, null, null, null };

			var placementTypesCount = Enum.GetValues(typeof(Placement)).Length;
			_placementProviders = new PlacementProvider[placementTypesCount];

			_section = section;
			_name = name;
			this.Puzzle = puzzle;
		}

		/// <summary>
		/// Updates world position of place.
		/// </summary>
		private void UpdatePosition()
		{
			this.X = _placeNode.Value.X * Dungeon.TileSize + Dungeon.TileSize / 2;
			this.Y = _placeNode.Value.Y * Dungeon.TileSize + Dungeon.TileSize / 2;
		}

		/// <summary>
		/// Adds door to place.
		/// </summary>
		/// <param name="direction"></param>
		/// <param name="doorType"></param>
		private void AddDoor(int direction, DungeonBlockType doorType)
		{
			var room = _placeNode.Value;
			var door = room.GetPuzzleDoor(direction);
			if (door != null)
			{
				this.Doors[direction] = door;
				return;
			}

			// Create new door
			var floorData = this.Puzzle.FloorData;
			var doorBlock = this.Puzzle.Dungeon.Data.Style.Get(doorType, direction);
			var doorName = string.Format("{0}_door_{1}{2}_{3}", _name, this.X, this.Y, direction);

			door = new Door(doorBlock.PropId, 0, this.X, this.Y, doorBlock.Rotation, doorType, doorName);
			door.Info.Color1 = floorData.Color1;
			door.Info.Color2 = floorData.Color2;
			door.Info.Color3 = this.LockColor;
			if (doorType == DungeonBlockType.BossDoor)
				door.Behavior += this.Puzzle.Dungeon.BossDoorBehavior;
			door.Behavior += this.Puzzle.PuzzleEvent;

			this.Doors[direction] = door;
			this.Puzzle.Props[doorName] = door;
			room.SetPuzzleDoor(door, direction);
			room.SetDoorType(direction, (int)doorType);
		}

		/// <summary>
		/// Adds prop to place.
		/// </summary>
		/// <param name="prop"></param>
		/// <param name="positionType"></param>
		public void AddProp(DungeonProp prop, Placement positionType)
		{
			this.Puzzle.AddProp(this, prop, positionType);
		}

		/// <summary>
		/// Makes it a locked place with a locked door.
		/// </summary>
		public void DeclareLock()
		{
			var doorElement = _section.GetLock();
			if (doorElement == null)
				return;

			_placeNode = doorElement.placeNode;
			this.UpdatePosition();
			this.IsLock = true;
			this.LockColor = _section.GetLockColor();
			this.DoorDirection = doorElement.direction;

			var room = _placeNode.Value;
			room.ReserveDoor(this.DoorDirection);
			room.isLocked = true;
			if (room.RoomType != RoomType.End || room.RoomType != RoomType.Start)
				room.RoomType = RoomType.Room;

			// Boss door - special case
			if ((DungeonBlockType)room.DoorType[this.DoorDirection] == DungeonBlockType.BossDoor)
			{
				this.IsBossLock = true;
				this.AddDoor(this.DoorDirection, DungeonBlockType.BossDoor);
			}
			else
				this.AddDoor(this.DoorDirection, DungeonBlockType.DoorWithLock);
		}

		/// <summary>
		/// Makes it a locked place with a locked door that doesn't need an unlock place.
		/// </summary>
		public void DeclareLockSelf()
		{
			this.DeclareLock();
			this.GetLockDoor().IsLocked = true;
		}

		/// <summary>
		/// This place will precede locked place and contain some means to unlock it.
		/// </summary>
		/// <param name="lockPlace"></param>
		public void DeclareUnlock(PuzzlePlace lockPlace)
		{
			var place = lockPlace as PuzzlePlace;
			if (place == null || place._placeNode == null)
				throw new PuzzleException("We can't declare unlock");

			_placeNode = _section.GetUnlock(place._placeNode);
			if (_placeNode != null)
				this.IsUnlock = true;

			this.UpdatePosition();
		}

		/// <summary>
		/// Declares that this place is not to be used by any other puzzles.
		/// If we didn't declare this place to be something, reserve random place.
		/// </summary>
		public void ReservePlace()
		{
			if (this.IsUnlock || this.IsLock || this.IsBossLock)
				_section.ReservePlace(_placeNode);
			else
				_placeNode = _section.ReservePlace();

			this.UpdatePosition();
		}

		/// <summary>
		/// Declares this place to be a room.
		/// Doors of this place won't be locked with a key.
		/// </summary>
		public void ReserveDoors()
		{
			var room = _placeNode.Value;
			room.ReserveDoors();

			_section.CleanLockedDoorCandidates();

			for (var dir = 0; dir < 4; ++dir)
			{
				if (room.Links[dir] == LinkType.From || room.Links[dir] == LinkType.To)
					this.AddDoor(dir, DungeonBlockType.Door);
			}

			this.OpenAllDoors();
		}

		/// <summary>
		/// Closes all doors at this place.
		/// </summary>
		public void CloseAllDoors()
		{
			foreach (var door in Doors)
			{
				if (door != null)
					door.Close();
			}
		}

		/// <summary>
		/// Opens all doors at this place.
		/// </summary>
		public void OpenAllDoors()
		{
			foreach (var door in Doors)
			{
				if (door != null)
					door.Open();
			}
		}

		/// <summary>
		/// Locks this place with the given key.
		/// </summary>
		/// <param name="key"></param>
		public void LockPlace(Item key)
		{
			this.GetLockDoor().IsLocked = true;
			this.Key = key;
		}

		/// <summary>
		/// Returns locked door of this place.
		/// </summary>
		/// <returns></returns>
		/// <exception cref="PuzzleException">Thrown if there is no lock or no door.</exception>
		public Door GetLockDoor()
		{
			var door = Doors[this.DoorDirection];

			if (!this.IsLock)
				throw new PuzzleException("Place isn't a lock.");
			if (door == null)
				throw new PuzzleException("No door found.");

			return door;
		}

		/// <summary>
		/// Opens locked place.
		/// </summary>
		public void OpenPlace()
		{
			if (!IsLock)
				return;

			this.Doors[this.DoorDirection].IsLocked = false;
			this.Doors[this.DoorDirection].Open();

			if (_placeNode.Value.DoorType[this.DoorDirection] == (int)DungeonBlockType.BossDoor)
				this.Puzzle.Dungeon.BossDoorBehavior(null, Doors[this.DoorDirection]);
		}

		/// <summary>
		/// Returns prop position and direction for placement.
		/// </summary>
		/// <param name="placement"></param>
		/// <param name="border"></param>
		/// <returns>3 values, X, Y, and Direction (in degree).</returns>
		public int[] GetPropPosition(Placement placement, int border = -1)
		{
			if (_placeNode == null)
				throw new PuzzleException("Place hasn't been declared anything or it wasn't reserved.");

			// todo: check those values
			var radius = 0;
			if (border >= 0)
			{
				radius = (_placeNode.Value.RoomType == RoomType.Alley ? 200 - border : 800 - border);
				if (radius < 0)
					radius = 0;
			}
			else
				radius = (_placeNode.Value.RoomType == RoomType.Alley ? 200 : 800);

			if (_placementProviders[(int)placement] == null)
				_placementProviders[(int)placement] = new PlacementProvider(placement, radius);

			var pos = _placementProviders[(int)placement].GetPosition();
			if (pos == null)
				throw new PuzzleException(String.Format("We out of positions of type {0}", placement));

			pos[0] += this.X;
			pos[1] += this.Y;

			return pos;
		}

		/// <summary>
		/// Returns the room's position.
		/// </summary>
		/// <returns></returns>
		public Position GetRoomPosition()
		{
			return new Position(_placeNode.Value.X, _placeNode.Value.Y);
		}

		/// <summary>
		/// Returns the position of the place in world coordinates.
		/// </summary>
		/// <returns></returns>
		public Position GetWorldPosition()
		{
			return new Position(this.X, this.Y);
		}

		/// <summary>
		/// Creates mob in puzzle, in this place.
		/// </summary>
		/// <param name="mobGroupName">Name of the mob, for reference.</param>
		/// <param name="mobToSpawn">Mob to spawn (Mob1-3), leave as null for auto select.</param>
		public void SpawnSingleMob(string mobGroupName, string mobToSpawn = null, Placement placement = Placement.Random)
		{
			DungeonMonsterGroupData data;
			if (mobToSpawn == null)
				data = this.Puzzle.GetMonsterData("Mob3") ?? this.Puzzle.GetMonsterData("Mob2") ?? this.Puzzle.GetMonsterData("Mob1");
			else
				data = this.Puzzle.GetMonsterData(mobToSpawn);

			if (data == null)
				throw new Exception("No monster data found.");

			this.Puzzle.AllocateAndSpawnMob(this, mobGroupName, data, placement);
		}
	}
}
