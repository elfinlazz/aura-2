// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Collections.Generic;
using Aura.Channel.World.Dungeons.Generation;
using Aura.Channel.World.Entities;
using Aura.Mabi.Const;
using Aura.Shared.Util;
using System;
using Aura.Channel.Network.Sending;
using Aura.Data.Database;

namespace Aura.Channel.World.Dungeons.Puzzles
{

	/// <summary>
	/// Room for a puzzle.
	/// </summary>
	public interface IPuzzlePlace
	{
		IPuzzle GetPuzzle();

		/// <summary>
		/// Makes it a locked place with a locked door.
		/// </summary>
		void DeclareLock();

		/// <summary>
		/// Makes it a locked place with a locked door that don't need an unlock place.
		/// </summary>
		void DeclareLockSelf();

		/// <summary>
		/// This place will precede locked place and contain some means to unlock it.
		/// </summary>
		void DeclareUnlock(IPuzzlePlace lockPlace);

		/// <summary>
		/// This place won't be used for any other puzzle. If we didn't declared this place to be something - reserve random place
		/// </summary>
		void ReservePlace();

		/// <summary>
		/// Doors of this place won't be locked with a key.
		/// </summary>
		void ReserveDoors();

		void CloseAllDoors();
		void OpenAllDoors();
		uint GetLockColor();

		void SpawnSingleMob(string mobGroupName, string mobName = null);
	}

	public class PuzzlePlace : IPuzzlePlace
	{
		private DungeonFloorSection _section;
		private Puzzle _puzzle;
		private string _name;
		private LinkedListNode<RoomTrait> _placeNode = null;
		public bool IsLock { get; private set; }
		public uint LockColor { get; private set; }
		public bool IsUnlock { get; private set; }
		public bool IsBossLock { get; private set; }
		public Item Key { get; private set; }
		public int DoorDirection { get; private set; }
		private PuzzleDoor[] Doors;


		public PuzzlePlace(DungeonFloorSection section, Puzzle puzzle, string name)
		{
			this._section = section;
			this._puzzle = puzzle;
			this._name = name;
			this.IsLock = false;
			this.LockColor = 0;
			this.Key = null;
			this.IsBossLock = false;
			this.DoorDirection = 0;
			this.Doors = new PuzzleDoor[] { null, null, null, null };
		}

		public IPuzzle GetPuzzle()
		{
			return this._puzzle;
		}

		private void AddDoor(int direction, DungeonBlockType doorType)
		{
			var room = this._placeNode.Value;
			PuzzleDoor door = room.GetPuzzleDoor(direction);
			if (door != null)
				this.Doors[direction] = door;
			else
			{
				// create new door
				string doorName = this._name + "_door_" + direction;
				door = new PuzzleDoor(this, doorName, direction, doorType);
				this.Doors[direction] = door;
				this._puzzle.Props[doorName] = door;
				room.SetPuzzleDoor(door, direction);
				room.SetDoorType(direction, (int)doorType);
			}
		}

		public void DeclareLock()
		{
			var doorElement = this._section.GetLock();
			if (doorElement == null)
				return;
			this._placeNode = doorElement.placeNode;
			this.IsLock = true;
			this.LockColor = this._section.GetLockColor();
			this.DoorDirection = doorElement.direction;

			var room = this._placeNode.Value;
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
			else this.AddDoor(this.DoorDirection, DungeonBlockType.DoorWithLock);
		}

		public void DeclareLockSelf()
		{
			this.DeclareLock();
		}

		public void DeclareUnlock(IPuzzlePlace lockPlace)
		{
			var place = lockPlace as PuzzlePlace;
			if (place == null || place._placeNode == null)
			{
				throw new CPuzzleException("We can't declare unlock");
			}
			this._placeNode = this._section.GetUnlock(place._placeNode);
			if (this._placeNode != null)
				this.IsUnlock = true;
		}

		public void ReservePlace()
		{
			if (this.IsUnlock || this.IsLock || this.IsBossLock) this._section.ReservePlace(this._placeNode);
			else this._placeNode = this._section.ReservePlace();
		}

		public void ReserveDoors()
		{
			var room = this._placeNode.Value;
			room.ReserveDoors();
			this._section.CleanLockedDoorCandidates();
			for (var dir = 0; dir < 4; ++dir)
			{
				if (room.Links[dir] == LinkType.From || room.Links[dir] == LinkType.To)
					AddDoor(dir, DungeonBlockType.Door);
			}
			this.OpenAllDoors();
		}

		public void CloseAllDoors()
		{
			foreach (var door in Doors)
			{
				if (door == null) continue;
				door.Close();
			}
		}

		public void OpenAllDoors()
		{
			foreach (var door in Doors)
			{
				if (door == null) continue;
				door.Open();
			}
		}

		public void LockPlace(Item key)
		{
			if (!IsLock) return;
			this.Key = key;
			this.Doors[this.DoorDirection].SetKey(key);
		}

		public void OpenPlace()
		{
			if (!IsLock) return;
			if (this._placeNode.Value.DoorType[this.DoorDirection] == (int)DungeonBlockType.BossDoor)
				this._puzzle.GetDungeon().BossDoorBehavior(null, this.Doors[this.DoorDirection].GetDoorProp());
			else
				this.Doors[this.DoorDirection].Open();
		}

		public uint GetLockColor()
		{
			return this.LockColor;
		}

		public Position GetPosition()
		{
			return new Position(this._placeNode.Value.X, this._placeNode.Value.Y);
		}

		public Position GetWorldPosition()
		{
			var x = this._placeNode.Value.X * Dungeon.TileSize + Dungeon.TileSize / 2;
			var y = this._placeNode.Value.Y * Dungeon.TileSize + Dungeon.TileSize / 2;

			return new Position(x, y);
		}

		/// <summary>
		/// Creates mob in puzzle, in this place.
		/// </summary>
		/// <param name="mobGroupName">Name of the mob, for reference.</param>
		/// <param name="mobToSpawn">Mob to spawn (Mob1-3), leave as null for auto select.</param>
		public void SpawnSingleMob(string mobGroupName, string mobToSpawn = null)
		{
			DungeonMonsterGroupData data;
			if (mobToSpawn == null)
				data = _puzzle.GetMonsterData("Mob3") ?? _puzzle.GetMonsterData("Mob2") ?? _puzzle.GetMonsterData("Mob1");
			else
				data = _puzzle.GetMonsterData(mobToSpawn);

			if (data == null)
				throw new Exception("No monster data found.");

			_puzzle.AllocateAndSpawnMob(this, mobGroupName, data);
		}
	}
}
