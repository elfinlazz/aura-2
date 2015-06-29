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
			/// <summary>
			/// Dungeon room reference.
			/// </summary>
			public RoomTrait Room = null;

			/// <summary>
			/// Direction of the door.
			/// </summary>
			public int Direction = -1;

			/// <summary>
			/// Index of this place in DungeonFloorSection.Places list.
			/// </summary>
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
			/// <summary>
			/// Dungeon room reference.
			/// </summary>
			public RoomTrait Room;

			/// <summary>
			/// Moves needed to get to this place from the start.
			/// </summary>
			public int Depth;

			/// <summary>
			/// Is this place used by puzzle.
			/// </summary>
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
		///	Random generator used by this section.
		/// </summary>
		private MTRandom _rng;

		/// <summary>
		/// Was boss door placed in last floor last section or not.
		/// </summary>
		private bool _placeBossDoor;

		/// <summary>
		/// Provides unique colors for this section locked doors.
		/// </summary>
		private LockColorProvider _lockColorProvider;

		/// <summary>
		/// List of nodes that can serve as locked place.
		/// </summary>
		private LinkedList<LockedDoorCandidateNode> _lockedDoorCandidates = null;

		/// <summary>
		/// Linear list of all places for this section, so N-th place always behind N+1 place.
		/// </summary>
		public List<PlaceNode> Places { get; private set; }

		/// <summary>
		/// List of puzzles this section contains.
		/// </summary>
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

			this.InitLists(startRoom, path);
		}

		/// <summary>
		/// Creates Places list, walking critical path and adding subpaths recursively.
		/// Creates _lockedDoorCandidates list, places on the way to end room and to chest places.
		/// </summary>
		/// <param name="startRoom"></param>
		/// <param name="path"></param>
		private void InitLists(RoomTrait startRoom, List<MazeMove> path)
		{
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
		}

		/// <summary>
		///	Recursively add subpath rooms to Places list.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="depth"></param>
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

		/// <summary>
		/// Gets a node from _lockedDoorCandidates, that will be used to make locked place, and removes it from the list.
		/// </summary>
		/// <param name="lockSelf"></param>
		/// <returns></returns>
		public LockedDoorCandidateNode GetLock(bool lockSelf = false)
		{
			LockedDoorCandidateNode result = null;

			var count = _lockedDoorCandidates.Count;
			if (count == 0)
				throw new PuzzleException("Out of locked door candidates.");

			// Always place locked place before the boss door.
			// todo: always place a locked place at the end of section, add script handler
			if (_placeBossDoor)
			{
				result = _lockedDoorCandidates.FirstOrDefault(x =>
					x.Room.Neighbor[Direction.Up] != null && x.Room.Neighbor[Direction.Up].RoomType == RoomType.End);
			}

			if (result == null)
			{
				var lockedDoor = _lockedDoorCandidates.Last;
				if (lockSelf)
					while (lockedDoor != null && lockedDoor.Value.Room.isReserved)
						lockedDoor = lockedDoor.Previous;
				else
					while (lockedDoor != null)
					{
						// Test if there is a free room for unlock place before this door
						var placeIndex = lockedDoor.Value.PlaceIndex;
						// Get index of room behind lockedPlace door.
						placeIndex = this.Places[placeIndex].Room.Neighbor[lockedDoor.Value.Direction].RoomIndex;
						if (placeIndex == 0 || placeIndex == -1) // should be last room in section.
							placeIndex = this.Places.Count;
						--placeIndex;
						while (placeIndex >= 0 && (placeIndex == lockedDoor.Value.PlaceIndex || this.Places[placeIndex].IsUsed || this.Places[placeIndex].Room.isLocked))
						{
							--placeIndex;
						}
						if (placeIndex >= 0)
							break;
						lockedDoor = lockedDoor.Previous;
					}
				if (lockedDoor == null)
				{
					if (lockSelf)
						throw new PuzzleException("Out of candidates for self lock.");
					throw new PuzzleException("None of lock candidates can serve as lock with unlock place.");
				}
				result = lockedDoor.Value;
			}

			if (_placeBossDoor)
				_placeBossDoor = false;

			_lockedDoorCandidates.Remove(result);

			return result;
		}

		/// <summary>
		/// Finds appropriate place to use as unlock place for given locked place.
		/// </summary>
		/// <param name="lockedPlace"></param>
		/// <returns>Index of unlock place in Places list</returns>
		public int GetUnlock(PuzzlePlace lockedPlace)
		{
			var lockedPlaceIndex = lockedPlace.PlaceIndex;
			// Get index of room behind lockedPlace door.
			lockedPlaceIndex = this.Places[lockedPlaceIndex].Room.Neighbor[lockedPlace.DoorDirection].RoomIndex;
			if (lockedPlaceIndex == 0 || lockedPlaceIndex == -1) // should be last room in section.
				lockedPlaceIndex = this.Places.Count;

			List<int> possiblePlacesOnPath = new List<int>();
			List<int> possiblePlacesNotOnPath = new List<int>();
			var deadEnd = -1;
			var deadEndDepth = 0;
			RoomTrait room;
			// places before lockedPlaceIndex are always behind it.
			for (var i = 0; i < lockedPlaceIndex; ++i)
			{
				room = this.Places[i].Room;
				if (!this.Places[i].IsUsed && !room.isLocked && room.RoomType != RoomType.Start)
				{
					// Check is this place have locked doors.
					// Locked places tend to not reserve themselves, so they could share place with some puzzle, like another locked place.
					// But they couldn't be shared with unlock places, because they have to control all doors.
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
						if (room.isOnPath)
							possiblePlacesOnPath.Add(i);
						else
							possiblePlacesNotOnPath.Add(i);
					}
				}
			}

			// Chance to not use deepest corner for unlock place.
			if (_rng.GetUInt32(100) < 40)
				deadEnd = -1;

			if (possiblePlacesOnPath.Count == 0 && possiblePlacesNotOnPath.Count == 0)
			{
				// Convert locked place room back to alley if there are no more locked doors.
				room = this.Places[lockedPlace.PlaceIndex].Room;
				room.SetDoorType(lockedPlace.DoorDirection, (int)DungeonBlockType.Alley);
				room.SetPuzzleDoor(null, lockedPlace.DoorDirection);
				var isLockedRoom = room.DoorType.Any(x => (x == (int)DungeonBlockType.DoorWithLock || x == (int)DungeonBlockType.BossDoor));
				if (!isLockedRoom)
				{
					room.isLocked = false;
					room.RoomType = RoomType.Alley;
				}

				// Return locked place door to list on available doors.
				var lockedDoorCandidate = new LockedDoorCandidateNode(room, lockedPlace.DoorDirection, room.RoomIndex);
				_lockedDoorCandidates.AddFirst(lockedDoorCandidate);
				throw new PuzzleException("Out of unlock places.");
			}

			var possiblePlaces = (possiblePlacesNotOnPath.Count > 0 ? possiblePlacesNotOnPath : possiblePlacesOnPath);

			var placeIndex = deadEnd != -1 ? deadEnd : possiblePlaces[(int)_rng.GetUInt32((uint)possiblePlaces.Count)];

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
				//if (room.ReservedDoor[Direction.GetOppositeDirection(dir)]) continue;
				// skip reserved places
				if (room.isReserved) continue;

				var lockedDoorCandidate = new LockedDoorCandidateNode(room, Direction.GetOppositeDirection(dir), room.RoomIndex);
				_lockedDoorCandidates.AddFirst(lockedDoorCandidate);
			}
			return placeIndex;
		}

		public void ReservePlace(int placeIndex)
		{

			this.Places[placeIndex].IsUsed = true;
			this.Places[placeIndex].Room.isReserved = true;
		}

		/// <summary>
		/// Get random unused place from Places list.
		/// </summary>
		/// <returns>Index of place in Places list.</returns>
		public int ReservePlace()
		{
			var unusedCount = this.Places.Count(x => !x.IsUsed);
			if (unusedCount == 0)
				throw new PuzzleException("Out of empty places.");
			var i = (int)_rng.GetUInt32(0, (uint)unusedCount - 1);
			var placeIndex = 0;
			for (; placeIndex < this.Places.Count; ++placeIndex)
			{
				if (this.Places[placeIndex].IsUsed) continue;
				if (i == 0) break;
				--i;
			}
			this.Places[placeIndex].IsUsed = true;
			this.Places[placeIndex].Room.isReserved = true;
			return placeIndex;
		}

		/// <summary>
		/// Remove reserved rooms from this._lockedDoorCandidates
		/// </summary>
		public void CleanLockedDoorCandidates()
		{
			var node = _lockedDoorCandidates.First;
			while (node != null)
			{
				var next = node.Next;
				if (node.Value.Room.isReserved) _lockedDoorCandidates.Remove(node);
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
