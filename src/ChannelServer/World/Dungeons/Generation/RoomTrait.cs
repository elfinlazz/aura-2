// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using Aura.Channel.World.Dungeons.Props;
using Aura.Channel.World.Dungeons.Puzzles;
using Aura.Mabi.Const;

namespace Aura.Channel.World.Dungeons.Generation
{
	public class RoomTrait
	{
		public RoomTrait[] Neighbor { get; private set; }

		/// <summary>
		///	Is this room should be walked though to get puzzle done
		/// </summary>
		public bool isOnPath;

		/// <summary>
		/// Doors for puzzles
		/// </summary>
		public Door[] PuzzleDoors { get; private set; }

		/// <summary>
		/// Is door in direction reserved for a puzzle
		/// </summary>
		public bool[] ReservedDoor { get; private set; }

		/// <summary>
		/// Is this room reserved for a puzzle
		/// </summary>
		public bool isReserved;

		/// <summary>
		/// This room is locked, don't try to put UnlockPlace in here
		/// </summary>
		public bool isLocked;

		/// <summary>
		/// Index of this room in DungeonFloorSection.Places list
		/// </summary>
		public int RoomIndex;

		/// <summary>
		/// Paths
		/// </summary>
		public LinkType[] Links { get; private set; }

		/// <summary>
		/// Types of the room's doors (up/down).
		/// </summary>
		public int[] DoorType { get; private set; }

		public RoomType RoomType { get; set; }

		public int X { get; private set; }
		public int Y { get; private set; }

		//public int ShapeType { get; private set; }
		//public int ShapeRotationCount { get; private set; }

		public RoomTrait(int x, int y)
		{
			this.Neighbor = new RoomTrait[4];
			this.Links = new LinkType[] { LinkType.None, LinkType.None, LinkType.None, LinkType.None };
			this.DoorType = new int[] { 0, 0, 0, 0 };

			this.X = x;
			this.Y = y;

			this.isOnPath = false;
			this.isReserved = false;
			this.PuzzleDoors = new Door[] {null, null, null, null };
			this.ReservedDoor = new bool[] { false, false, false, false };
			this.RoomType = RoomType.None;
			this.RoomIndex = -1;
		}

		public void SetNeighbor(int direction, RoomTrait room)
		{
			this.Neighbor[direction] = room;
		}

		public bool IsLinked(int direction)
		{
			if (direction > 3)
				throw new ArgumentException("Direction out of bounds.");

			return this.Links[direction] != LinkType.None;
		}

		public int GetDoorType(int direction)
		{
			if (direction > 3)
				throw new ArgumentException("Direction out of bounds.");

			return this.DoorType[direction];
		}

		public void SetPuzzleDoor(Door door, int direction)
		{
			this.PuzzleDoors[direction] = door;

			var opposite_direction = Direction.GetOppositeDirection(direction);

			var room = this.Neighbor[direction];
			if (room != null)
				room.PuzzleDoors[opposite_direction] = door;
		}

		public Door GetPuzzleDoor(int direction)
		{
			return this.PuzzleDoors[direction];
		}

		public void ReserveDoors()
		{
			if (this.RoomType != RoomType.End || this.RoomType != RoomType.Start)
				this.RoomType = RoomType.Room;
			for (var dir = 0; dir < 4; ++dir)
			{
				this.ReserveDoor(dir);
				this.SetDoorType(dir, (int) DungeonBlockType.Door);
			}
		}

		public void ReserveDoor(int direction)
		{
			if (direction > 3)
				throw new ArgumentException("Direction out of bounds.");

			this.ReservedDoor[direction] = true;

			var opposite_direction = Direction.GetOppositeDirection(direction);

			var room = this.Neighbor[direction];
			if (room != null)
				room.ReservedDoor[opposite_direction] = true;
		}

		public void Link(int direction, LinkType linkType)
		{
			if (direction > 3)
				throw new ArgumentException("Direction out of bounds.");

			this.Links[direction] = linkType;

			if (this.Neighbor[direction] != null)
			{
				int opposite_direction = Direction.GetOppositeDirection(direction);
				if (linkType == LinkType.From)
					this.Neighbor[direction].Links[opposite_direction] = LinkType.To;
				else if (linkType == LinkType.To)
					this.Neighbor[direction].Links[opposite_direction] = LinkType.From;
				else
					this.Neighbor[direction].Links[opposite_direction] = LinkType.None;
			}
		}

		public void SetDoorType(int direction, int doorType)
		{
			if (direction > 3)
				throw new ArgumentException("Direction out of bounds.");

			this.DoorType[direction] = doorType;

			var opposite_direction = Direction.GetOppositeDirection(direction);

			var room = this.Neighbor[direction];
			if (room != null)
				room.DoorType[opposite_direction] = doorType;
		}

		public int GetIncomingDirection()
		{
			for (var dir = 0; dir < 4; ++dir)
			{
				if (this.Links[dir] == LinkType.From) return dir;
			}
			return 0;
		}
	}

	public enum RoomType
	{
		None,
		Alley,
		Start,
		End,
		Room,
	}

	public enum LinkType
	{
		None,
		From,
		To,
	}
}
