// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Collections.Generic;
using System.Linq;
using Aura.Channel.Scripting.Scripts;
using Aura.Channel.World.Dungeons.Puzzles;
using Aura.Data;
using Aura.Data.Database;
using Aura.Mabi.Const;
using Aura.Shared.Util;

namespace Aura.Channel.World.Dungeons.Generation
{
	public class DungeonFloorSection
	{
		public class LockedDoorCandidateNode
		{
			public RoomTrait Room = null;
			public int Direction = -1;
			public int PlaceIndex = -1;

			public LockedDoorCandidateNode(RoomTrait room, int direction, int placeIndex)
			{
				this.Room = room;
				this.Direction = direction;
				this.PlaceIndex = placeIndex;
			}
		}

		public class PlaceNode
		{
			public RoomTrait Room;
			public int Depth;
			public bool IsUsed;

			public PlaceNode(RoomTrait room, int depth, bool isUsed)
			{
				this.Room = room;
				this.Depth = depth;
				this.IsUsed = isUsed;
			}
		}

		private class LockColorProvider
		{
			public static readonly uint[] Colors = new uint[]
			{ 
				0xe76c74, // Red
				0x654e9f, // Purple
				0x4fbcee, // Blue
				0xbfd46c, // Green
				0xf7b356, // Orange
				0xfdf06c, // Yellow
				0xff76bd, // Pink
				0x6df8f3, // Turquoise
			};

			private Queue<uint> _availableColors;

			public LockColorProvider()
			{
				var rnd = RandomProvider.Get();

				_availableColors = new Queue<uint>(Colors.OrderBy(a => rnd.Next()));
			}

			/// <summary>
			/// Get unique color for a locked place.
			/// </summary>
			public uint GetLockColor()
			{
				if (_availableColors.Count == 0)
					// We out of awailable colours, lets return random one
					return (uint)RandomProvider.Get().Next(0xFFFFFF);

				return _availableColors.Dequeue();
			}
		}

		/// <summary>
		/// List of nodes that can serve as locked place.
		/// </summary>
		private LinkedList<LockedDoorCandidateNode> _lockedDoorCandidates = null;

		/// <summary>
		/// List of all places for this section.
		/// </summary>
		public List<PlaceNode> Places { get; private set; }

		/// <summary>
		/// Was boss door placed in last floor last section or not.
		/// </summary>
		private bool _placeBossDoor;

		private LockColorProvider _lockColorProvider;
		private MTRandom _rng;

		public List<Puzzle> Puzzles { get; private set; }

		/// <summary>
		/// Section of teh floor, contain Puzzles.
		/// </summary>
		/// <param name="startRoom">Start room of this section.</param>
		/// <param name="path">Critical path for this section.</param>
		/// <param name="haveBossRoom">Should we place a boss door in this section.</param>
		/// <param name="rng">Randrom generator for this dungeon.</param>
		public DungeonFloorSection(RoomTrait startRoom, List<MazeMove> path, bool haveBossRoom, MTRandom rng)
		{
			_lockColorProvider = new LockColorProvider();
			_lockedDoorCandidates = new LinkedList<LockedDoorCandidateNode>();
			this.Places = new List<PlaceNode>();
			this.Puzzles = new List<Puzzle>();

			_placeBossDoor = haveBossRoom;
			_rng = rng;

			var room = startRoom;
			var depth = 0;
			foreach (var move in path)
			{
				this.Places.Add(new PlaceNode(room, depth, false));
				room.RoomIndex = this.Places.Count - 1;
				room.isOnPath = true;
				var lockedDoorCandidate = new LockedDoorCandidateNode(room, move.Direction, this.Places.Count - 1);
				_lockedDoorCandidates.AddLast(lockedDoorCandidate);
				for (var direction = 0; direction < 4; direction++)
				{
					if (move.Direction != direction && room.Links[direction] == LinkType.To)
					{
						var nextRoom = room.Neighbor[direction];
						if (nextRoom != null)
							CreateEmptyPlacesRecursive(nextRoom, depth + 1);
					}
				}
				room = room.Neighbor[move.Direction];
				depth++;
			}
			_lockedDoorCandidates.RemoveFirst();
		}

		private void CreateEmptyPlacesRecursive(RoomTrait room, int depth)
		{
			this.Places.Add(new PlaceNode(room, depth, false));
			room.RoomIndex = this.Places.Count - 1;
			for (var direction = 0; direction < 4; direction++)
			{
				if (room.Links[direction] != LinkType.To) continue;
				var nextRoom = room.Neighbor[direction];
				if (nextRoom != null)
					CreateEmptyPlacesRecursive(nextRoom, depth + 1);
			}
		}

		public uint GetLockColor()
		{
			return _lockColorProvider.GetLockColor();
		}

		public LockedDoorCandidateNode GetLock(bool lockSelf=false)
		{
			LockedDoorCandidateNode result = null;

			var count = _lockedDoorCandidates.Count;
			if (count == 0)
			{
				throw new PuzzleException("We out of locked door candidates");
			}

			// Always place locked place before the boss door.
			// todo: always place a locked place at the end of section, add script handler
			if (_placeBossDoor)
			{
				result = _lockedDoorCandidates.FirstOrDefault(x =>
					x.Room.Neighbor[Direction.Up] != null && x.Room.Neighbor[Direction.Up].RoomType == RoomType.End);
			}

			if (result == null)
			{
				//var random_index = (int)this._rng.GetUInt32(0, (uint)count - 1);
				//result = this._lockedDoorCandidates.ElementAt(random_index);
				var lockedDoor = _lockedDoorCandidates.Last;
				if(lockSelf)
					while (lockedDoor != null && lockedDoor.Value.Room.isReserved)
						lockedDoor = lockedDoor.Previous;
				if (lockedDoor == null)
					throw new PuzzleException("We out of candidates for self lock.");
				result = lockedDoor.Value;
			}

			if (_placeBossDoor)
				_placeBossDoor = false;

			_lockedDoorCandidates.Remove(result);

			return result;
		}

		public int GetUnlock(int lockedPlaceIndex)
		{
			List<int> possibleUnlockRooms = new List<int>();
			var deadEnd = -1;
			var deadEndDepth = 0;
			RoomTrait room;
			for (var i = 0; i < lockedPlaceIndex; ++i)
			{
				room = this.Places[i].Room;
				if (!this.Places[i].IsUsed && !room.isLocked && room.RoomType != RoomType.Start)
				{
					var haveLockedDoors = false;
					for (var j = 0; j < 4; ++j)
						if (room.DoorType[j] == (int)DungeonBlockType.DoorWithLock && room.Links[j] == LinkType.To)
						{
							haveLockedDoors = true;
							break;
						}
					if (!haveLockedDoors)
					{
						var emptyNeighborCount = 0;
						for (var dir = 0; dir < 4; ++dir)
							if (room.IsLinked(dir) && !room.Neighbor[dir].isReserved)
								emptyNeighborCount++;
						if (emptyNeighborCount < 2)
						{
							if (deadEndDepth < this.Places[i].Depth)
								deadEnd = i;
						}
						possibleUnlockRooms.Add(i);
					}
				}
			}

			// Chance to not use deepest corner for unlock place.
			if (_rng.GetUInt32(100) < 40)
				deadEnd = -1;

			if (possibleUnlockRooms.Count == 0)
			{
				// TODO: Return locked place to list on available doors.
				throw new PuzzleException("We out of unlock places");
			}

			var placeIndex = deadEnd != -1 ? deadEnd : possibleUnlockRooms[(int)_rng.GetUInt32((uint)possibleUnlockRooms.Count)];

			// Walk down from current place to our path and add new possible doors to this._lockedDoorCandidates
			room = this.Places[placeIndex].Room;
			while (room.RoomType != RoomType.Start)
			{
				if (room.isOnPath)
					break;
				var dir = room.GetIncomingDirection();
				room.isOnPath = true;
				room = room.Neighbor[dir];
				
				// skip reserved doors
				if (room.ReservedDoor[Direction.GetOppositeDirection(dir)]) continue;
				// skip reserved places
				if (room.isReserved) continue;

				var lockedDoorCandidate = new LockedDoorCandidateNode(room, Direction.GetOppositeDirection(dir), room.RoomIndex);
				_lockedDoorCandidates.AddLast(lockedDoorCandidate);
			}
			return placeIndex;
		}

		public void ReservePlace(int placeIndex)
		{

			this.Places[placeIndex].IsUsed = true;
			this.Places[placeIndex].Room.isReserved = true;
		}

		public int ReservePlace()
		{
			var unused = this.Places.FindAll(x => !x.IsUsed);
			if (unused.Count == 0)
				throw new PuzzleException("We out of empty places");
			var i = (int)this._rng.GetUInt32(0, (uint)unused.Count - 1);
			unused[i].IsUsed = true;
			unused[i].Room.isReserved = true;
			return i;
		}

		/// <summary>
		///  Remove reserved doors from this._lockedDoorCandidates
		/// </summary>
		public void CleanLockedDoorCandidates()
		{
			var node = _lockedDoorCandidates.First;
			while (node != null)
			{
				var next = node.Next;
				if (node.Value.Room.ReservedDoor[node.Value.Direction]) _lockedDoorCandidates.Remove(node);
				node = next;
			}
		}

		public Puzzle NewPuzzle(Dungeon dungeon, DungeonFloorData floorData, DungeonPuzzleData puzzleData, PuzzleScript puzzleScript)
		{
			var puzzle = new Puzzle(dungeon, this, floorData, puzzleData, puzzleScript);
			this.Puzzles.Add(puzzle);
			return puzzle;
		}
	}
}
