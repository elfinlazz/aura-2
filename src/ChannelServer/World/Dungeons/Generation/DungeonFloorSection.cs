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
			public RoomTrait room = null;
			public int direction = -1;
			public LinkedListNode<RoomTrait> placeNode = null;

			public LockedDoorCandidateNode(RoomTrait room, int direction, LinkedListNode<RoomTrait> emptyPlaceNode)
			{
				this.room = room;
				this.direction = direction;
				this.placeNode = emptyPlaceNode;
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
		/// List of all available rooms for this section.
		/// </summary>
		private LinkedList<RoomTrait> _emptyPlaces = null;

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
			this._lockColorProvider = new LockColorProvider();
			this._lockedDoorCandidates = new LinkedList<LockedDoorCandidateNode>();
			this._emptyPlaces = new LinkedList<RoomTrait>();
			this.Puzzles = new List<Puzzle>();

			this._placeBossDoor = haveBossRoom;
			this._rng = rng;

			var room = startRoom;
			var endMove = path.Last();
			LinkedListNode<RoomTrait> currentPlace;

			CreateEmptyPlacesRecursive(startRoom, endMove);
			currentPlace = _emptyPlaces.First;

			// Generate _lockedDoorCandidates
			foreach (var move in path)
			{
				room.isOnPath = true;
				while (currentPlace != null)
				{
					if (currentPlace.Value == room) break;
					currentPlace = currentPlace.Next;
				}
				if (currentPlace == null)
					throw new PuzzleException("Can't find a path room in generated _emptyPlaces");
				var lockedDoorCandidate = new LockedDoorCandidateNode(room, move.Direction, currentPlace);
				_lockedDoorCandidates.AddLast(lockedDoorCandidate);
				room = room.Neighbor[move.Direction];
			}
			_lockedDoorCandidates.RemoveFirst();
			_lockedDoorCandidates.RemoveFirst();
		}

		private bool CreateEmptyPlacesRecursive(RoomTrait room, MazeMove lastMove)
		{
			this._emptyPlaces.AddLast(room);
			for (int direction = 0; direction < 4; direction++)
			{
				if (room.Links[direction] == LinkType.To)
				{
					if (room.Neighbor[direction].X == lastMove.PosTo.X &&
						room.Neighbor[direction].Y == lastMove.PosTo.Y) continue;
					var nextRoom = room.Neighbor[direction];
					if (nextRoom != null)
						CreateEmptyPlacesRecursive(nextRoom, lastMove);
				}
			}
			return true;
		}

		private LinkedListNode<RoomTrait> FindPlaceNode(RoomTrait room)
		{
			LinkedListNode<RoomTrait> result = _emptyPlaces.First;
			while (result.Value != room)
			{
				result = result.Next;
				if (result == null)
					throw new PuzzleException("Can't find specified room in _emptyPlaces");
			}
			return result;
		}

		public uint GetLockColor()
		{
			return this._lockColorProvider.GetLockColor();
		}

		public LockedDoorCandidateNode GetLock()
		{
			LockedDoorCandidateNode result = null;

			var count = this._lockedDoorCandidates.Count;
			if (count == 0)
			{
				throw new PuzzleException("We out of locked door candidates");
			}

			// Always place locked place before the boss door.
			// todo: always place a locked place at the end of section, add script handler
			if (this._placeBossDoor)
			{
				result = this._lockedDoorCandidates.FirstOrDefault(x =>
					x.room.Neighbor[Direction.Up] != null && x.room.Neighbor[Direction.Up].RoomType == RoomType.End);
			}

			if (result == null)
			{
				var random_index = (int)this._rng.GetUInt32(0, (uint)count - 1);
				result = this._lockedDoorCandidates.ElementAt(random_index);
			}

			if (this._placeBossDoor)
				this._placeBossDoor = false;

			this._lockedDoorCandidates.Remove(result);

			return result;
		}

		public LinkedListNode<RoomTrait> GetUnlock(LinkedListNode<RoomTrait> lockedPlaceNode)
		{
			// Make list of candidates for unlock place
			var place = lockedPlaceNode.Previous;
			List<LinkedListNode<RoomTrait>> possibleUnlockRooms = new List<LinkedListNode<RoomTrait>>();
			while (place != null)
			{
				if (!place.Value.isLocked && place.Value.RoomType != RoomType.Start)
				{
					var fail = false;
					for (var dir = 0; dir < 4; ++dir)
						if (place.Value.DoorType[dir] == (int)DungeonBlockType.DoorWithLock)
						{
							fail = true;
							break;
						}
					if (!fail) possibleUnlockRooms.Add(place);
				}
				place = place.Previous;
			}

			if (possibleUnlockRooms.Count == 0)
			{
				// TODO: Return locked place to list on available doors.
				throw new PuzzleException("We out of unlock places");
			}

			var random_index = (int)this._rng.GetUInt32(0, (uint)possibleUnlockRooms.Count - 1);
			place = possibleUnlockRooms[random_index];

			// Walk down from current place to our path and add new possible doors to this._lockedDoorCandidates
			var room = place.Value;
			while (room != lockedPlaceNode.Value)
			{
				if (room.isOnPath)
					break;
				var dir = 0;
				for (; dir < 4; ++dir)
					if (room.Links[dir] == LinkType.From)
						break;
				room.isOnPath = true;
				room = room.Neighbor[dir];

				// skip reserved doors
				if (room.ReservedDoor[Direction.GetOppositeDirection(dir)]) continue;
				// skip reserved places
				if (room.isReserved) continue;

				var lockedDoorCandidate = new LockedDoorCandidateNode(room, Direction.GetOppositeDirection(dir), this.FindPlaceNode(room));
				_lockedDoorCandidates.AddLast(lockedDoorCandidate);
			}
			return place;
		}

		public void ReservePlace(LinkedListNode<RoomTrait> place)
		{
			place.Value.isReserved = true;
			this._emptyPlaces.Remove(place);
		}

		public LinkedListNode<RoomTrait> ReservePlace()
		{
			var place = this._emptyPlaces.Last;
			if (this._emptyPlaces.Count < 3)
				throw new PuzzleException("We out of empty places");
			var count = (int)this._rng.GetUInt32(1, (uint)this._emptyPlaces.Count - 2);
			while (count-- > 0)
			{
				place = place.Previous;
			}
			this._emptyPlaces.Remove(place);
			return place;
		}

		/// <summary>
		///  Remove reserved doors from this._lockedDoorCandidates
		/// </summary>
		public void CleanLockedDoorCandidates()
		{
			var node = this._lockedDoorCandidates.First;
			while (node != null)
			{
				var next = node.Next;
				if (node.Value.room.ReservedDoor[node.Value.direction]) this._lockedDoorCandidates.Remove(node);
				node = next;
			}
		}

		public Puzzle NewPuzzle(Dungeon dungeon, DungeonFloorData floorData,
			PuzzleScript puzzleScript, List<DungeonMonsterGroupData> monsterGroups)
		{
			var puzzle = new Puzzle(dungeon, this, floorData, puzzleScript, monsterGroups);
			this.Puzzles.Add(puzzle);
			return puzzle;
		}

	}


}
