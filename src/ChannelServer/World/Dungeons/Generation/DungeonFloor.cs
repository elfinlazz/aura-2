// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Data.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using Aura.Mabi.Const;
using Aura.Shared.Util;

namespace Aura.Channel.World.Dungeons.Generation
{
	public class DungeonFloor
	{
		private DungeonGenerator _dungeonGenerator;
		private DungeonFloor _prevFloor;
		//private DungeonFloor next_floor_structure;

		private List<List<RoomTrait>> _rooms;

		public List<DungeonFloorSection> Sections { get; private set; }

		private Position _pos;
		private Position _startPos;
		private int _startDirection;

		private int _branchProbability;
		private int _coverageFactor;

		public bool HasBossRoom { get; private set; }
		public MazeGenerator MazeGenerator { get; private set; }
		public int Width { get; private set; }
		public int Height { get; private set; }
		public bool IsLastFloor { get; private set; }

		public DungeonFloor(DungeonGenerator dungeonGenerator, DungeonFloorData floorData, bool isLastFloor, DungeonFloor prevFloor)
		{
			_pos = new Position(0, 0);
			_startPos = new Position(0, 0);
			_startDirection = Direction.Down;
			this.Width = 1;
			this.Height = 1;
			this.MazeGenerator = new MazeGenerator();
			this.Sections = new List<DungeonFloorSection>();

			_dungeonGenerator = dungeonGenerator;
			_branchProbability = floorData.Branch;
			_coverageFactor = floorData.Coverage;
			this.IsLastFloor = isLastFloor;
			_prevFloor = prevFloor;

			this.HasBossRoom = floorData.HasBoss;

			this.CalculateSize(floorData);
			this.InitRoomtraits();
			this.GenerateMaze(floorData);
			this.GenerateRooms(floorData);
			this.InitSections(floorData);
		}

		/// <summary>
		/// Split this floor into sections.
		/// </summary>
		/// <param name="floorData"></param>
		private void InitSections(DungeonFloorData floorData)
		{
			var criticalPathLength = this.MazeGenerator.CriticalPath.Count - 1;
			var criticalPathLeft = criticalPathLength;
			var sectionCount = floorData.Sections.Count;
			if (sectionCount == 0) return;
			var sectionStart = 0;
			var puzzleCountSum = 0f;
			var weightsList = CalculateWeights();
			Log.Debug("Floor weightsList: " + string.Join(",", weightsList));
			var pathWeight = weightsList.Sum();
			floorData.Sections.ForEach(x => puzzleCountSum += x.Max);
			for (var i = 0; i < sectionCount; ++i)
			{
				List<MazeMove> sectionPath;
				var haveBossDoor = false;
				var sectionLength = (int)Math.Round(floorData.Sections[i].Max / puzzleCountSum * pathWeight);
				var sectionEnd = sectionStart;
				var currentWeight = 0;
				for (; sectionEnd < weightsList.Length; ++sectionEnd)
				{
					currentWeight += weightsList[sectionEnd];
					if (currentWeight >= sectionLength)
						break;
				}
				if (currentWeight > sectionLength)
				{
					if (currentWeight - weightsList[sectionEnd] >= (int) Math.Round(floorData.Sections[i].Max/puzzleCountSum*criticalPathLeft))
					{
						currentWeight -= weightsList[sectionEnd];
						--sectionEnd;
					}
				}

				pathWeight -= currentWeight;
				criticalPathLeft -= sectionEnd - sectionStart + 1;
				puzzleCountSum -= floorData.Sections[i].Max;

				// if last section
				if (i == sectionCount - 1)
				{
					sectionPath = this.MazeGenerator.CriticalPath.GetRange(sectionStart + 1, criticalPathLength - sectionStart);
					haveBossDoor = this.HasBossRoom;
				}
				else sectionPath = this.MazeGenerator.CriticalPath.GetRange(sectionStart + 1, sectionEnd - sectionStart + 1);
				this.Sections.Add(new DungeonFloorSection(this.GetRoom(sectionPath[0].PosFrom), sectionPath, haveBossDoor, this._dungeonGenerator.RngPuzzles));
				var weightsListSegment = (i == sectionCount - 1 ? 
					new ArraySegment<int>(weightsList, sectionStart, criticalPathLength - sectionStart) : 
					new ArraySegment<int>(weightsList, sectionStart, sectionEnd - sectionStart + 1));
				Log.Debug("section weightsList: " + string.Join(",", weightsListSegment));
				Log.Debug(string.Format("section {0}: max puzzles: {1}, wanted length: {2}, length: {3}", i, floorData.Sections[i].Max, sectionLength, weightsListSegment.Sum()));
				sectionStart = sectionEnd + 1;
			}
		}

		/// <summary>
		/// InitSections helper method.
		/// Walks critical path and for each room calculates number of subpath rooms.
		/// </summary>
		/// <returns></returns>
		private int[] CalculateWeights()
		{
			var criticalPathLength = this.MazeGenerator.CriticalPath.Count - 1;
			var weightsList = new int[criticalPathLength];
			
			for (var i = 0; i < criticalPathLength; ++i)
			{
				var move = this.MazeGenerator.CriticalPath[i + 1];
				var room = this.GetRoom(move.PosFrom);
				weightsList[i] = 1;
				for (var direction = 0; direction < 4; direction++)
				{
					if (move.Direction != direction && room.Links[direction] == LinkType.To)
					{
						var nextRoom = room.Neighbor[direction];
						if (nextRoom != null)
							weightsList[i] += CalculateSubPathWeightRecursive(nextRoom, 1);
					}
				}
			}
			return weightsList;
		}

		/// <summary>
		/// CalculateWeights helper method.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		private int CalculateSubPathWeightRecursive(RoomTrait room, int count)
		{
			for (var direction = 0; direction < 4; direction++)
			{
				if (room.Links[direction] != LinkType.To) continue;
				var nextRoom = room.Neighbor[direction];
				if (nextRoom != null)
					count = CalculateSubPathWeightRecursive(nextRoom, count + 1);
			}
			return count;
		}

		private void GenerateRooms(DungeonFloorData floorData)
		{
			if (this.HasBossRoom)
			{
				var endPos = this.MazeGenerator.EndPos;

				var preEndRoom = this.GetRoom(endPos.GetBiasedPosition(Direction.Down));
				preEndRoom.RoomType = RoomType.Room;
				preEndRoom.SetDoorType(Direction.Up, (int)DungeonBlockType.BossDoor);
			}
		}

		private void CalculateSize(DungeonFloorData floorData)
		{
			var width = floorData.Width;
			var height = floorData.Height;

			if (floorData.Width < 6)
				width = 6;
			else if (floorData.Width > 18)
				width = 18;

			if (floorData.Height < 6)
				height = 6;
			else if (floorData.Height > 18)
				height = 18;

			var rndNum = _dungeonGenerator.RngMaze.GetUInt32();
			this.Width = (int)(width - rndNum % (int)(width / 5.0));

			rndNum = _dungeonGenerator.RngMaze.GetUInt32();
			this.Height = (int)(height - rndNum % (int)(height / 5.0));
		}

		private void InitRoomtraits()
		{
			_rooms = new List<List<RoomTrait>>();
			for (int x = 0; x < this.Width; x++)
			{
				var row = new List<RoomTrait>();
				for (int y = 0; y < this.Height; y++)
					row.Add(new RoomTrait(x, y));

				_rooms.Add(row);
			}

			for (int y = 0; y < this.Height; y++)
			{
				for (int x = 0; x < this.Width; x++)
				{
					for (int direction = 0; direction < 4; direction++)
					{
						var biased_pos = new Position(x, y).GetBiasedPosition(direction);
						if ((biased_pos.X >= 0) && (biased_pos.Y >= 0))
							if ((biased_pos.X < this.Width) && (biased_pos.Y < this.Height))
								_rooms[x][y].SetNeighbor(direction, _rooms[biased_pos.X][biased_pos.Y]);
					}
				}
			}
		}

		public List<RoomTrait> GetRooms()
		{
			var result = new List<RoomTrait>();

			for (int x = 0; x < this.Width; x++)
			{
				for (int y = 0; y < this.Height; y++)
				{
					var room = this.GetRoom(x, y);
					if (room.RoomType == RoomType.Room)
						result.Add(room);
				}
			}

			return result;
		}

		public RoomTrait GetRoom(Position pos)
		{
			return this.GetRoom(pos.X, pos.Y);
		}

		public RoomTrait GetRoom(int x, int y)
		{
			if ((x < 0) || (y < 0) || (x >= this.Width) || (y >= this.Height))
				throw new ArgumentException("Position out of bounds.");

			return _rooms[x][y];
		}

		private bool SetTraits(Position pos, int direction, int doorType)
		{
			var biased_pos = pos.GetBiasedPosition(direction);
			if ((biased_pos.X >= 0) && (biased_pos.Y >= 0))
			{
				if ((biased_pos.X < this.Width) && (biased_pos.Y < this.Height))
				{
					if (!this.MazeGenerator.IsFree(biased_pos))
						return false;

					this.MazeGenerator.MarkReservedPosition(biased_pos);
				}
			}

			var room = this.GetRoom(pos);
			if (room.IsLinked(direction))
				throw new Exception("Room in direction isn't linked");

			if (room.GetDoorType(direction) != 0)
				throw new Exception();

			LinkType linkType;
			if (doorType == (int)DungeonBlockType.StairsDown)
				linkType = LinkType.To;
			else if (doorType == (int)DungeonBlockType.StairsUp)
				linkType = LinkType.From;
			else
				throw new Exception("Invalid door_type");

			room.Link(direction, linkType);
			room.SetDoorType(direction, doorType);

			return true;
		}

		private void GenerateMaze(DungeonFloorData floorDesc)
		{
			var critPathMin = Math.Max(1, floorDesc.CritPathMin);
			var critPathMax = Math.Max(1, floorDesc.CritPathMax);

			if (critPathMin > critPathMax)
			{
				var temp = critPathMax;
				critPathMax = critPathMin;
				critPathMin = temp;
			}

			this.CreateCriticalPath(critPathMin, critPathMax);
			this.CreateSubPath(_coverageFactor, _branchProbability);

			this.SetRoomTypes();
		}

		private void SetRoomTypes()
		{
			for (int y = 0; y < this.MazeGenerator.Height; ++y)
			{
				for (int x = 0; x < this.MazeGenerator.Width; ++x)
				{
					var pos = new Position(x, y);
					var room = this.MazeGenerator.GetRoom(pos);
					var roomTrait = this.GetRoom(pos);

					if (this.MazeGenerator.StartPos.X == x && this.MazeGenerator.StartPos.Y == y)
						roomTrait.RoomType = RoomType.Start;
					else if (this.MazeGenerator.EndPos.X == x && this.MazeGenerator.EndPos.Y == y)
						roomTrait.RoomType = RoomType.End;
					else if (room.Visited)
						roomTrait.RoomType = RoomType.Alley;
				}
			}
		}

		private List<MazeMove> CreateCriticalPath(int crit_path_min, int crit_path_max)
		{
			while (true)
			{
				this.MazeGenerator.SetSize(this.Width, this.Height);
				this.SetRandomPathPosition();

				if (this.MazeGenerator.GenerateCriticalPath(_dungeonGenerator.RngMaze, crit_path_min, crit_path_max))
				{
					_startPos = this.MazeGenerator.StartPos;
					if (this.SetTraits(_startPos, this.MazeGenerator.StartDirection, (int)DungeonBlockType.StairsUp))
						break;
				}

				this.MazeGenerator = new MazeGenerator();
				this.InitRoomtraits();
			}

			return this.MazeGenerator.CriticalPath;
		}

		private bool CreateSubPath(int coverageFactor, int branchProbability)
		{
			this.MazeGenerator.GenerateSubPath(_dungeonGenerator.RngMaze, coverageFactor, branchProbability);
			return this.CreateSubPathRecursive(_startPos);
		}

		private bool CreateSubPathRecursive(Position pos)
		{
			var room = GetRoom(pos);
			var maze_room = this.MazeGenerator.GetRoom(pos);

			for (int direction = 0; direction < 4; direction++)
			{
				if (maze_room.GetPassageType(direction) == 2)
				{
					var biased_pos = pos.GetBiasedPosition(direction);
					if (room != null)
						room.Link(direction, LinkType.To);

					this.CreateSubPathRecursive(biased_pos);
				}
			}
			return true;
		}

		private void SetRandomPathPosition()
		{
			this.MazeGenerator.StartDirection = _startDirection = (_prevFloor == null ? Direction.Down : Direction.GetOppositeDirection(_prevFloor._startDirection));

			var mt = _dungeonGenerator.RngMaze;
			if (this.HasBossRoom)
			{
				if (_dungeonGenerator.Option.Contains("largebossroom=" + '"' + "true"))  // <option largebossroom="true" />
				{
					while (true)
					{
						_pos.X = (int)(mt.GetUInt32() % (this.Width - 2) + 1);
						_pos.Y = (int)(mt.GetUInt32() % (this.Height - 3) + 1);
						if (this.MazeGenerator.IsFree(_pos))
							if (this.MazeGenerator.IsFree(new Position(_pos.X - 1, _pos.Y)))
								if (this.MazeGenerator.IsFree(new Position(_pos.X + 1, _pos.Y)))
									if (this.MazeGenerator.IsFree(new Position(_pos.X, _pos.Y + 1)))
										if (this.MazeGenerator.IsFree(new Position(_pos.X - 1, _pos.Y + 1)))
											if (this.MazeGenerator.IsFree(new Position(_pos.X + 1, _pos.Y + 1)))
												if (this.MazeGenerator.IsFree(new Position(_pos.X, _pos.Y + 2)))
													if (this.MazeGenerator.IsFree(new Position(_pos.X - 1, _pos.Y + 2)))
														if (this.MazeGenerator.IsFree(new Position(_pos.X + 1, _pos.Y + 2)))
															break;
					}

					this.MazeGenerator.MarkReservedPosition(new Position(_pos.X - 1, _pos.Y));
					this.MazeGenerator.MarkReservedPosition(new Position(_pos.X + 1, _pos.Y));
					this.MazeGenerator.MarkReservedPosition(new Position(_pos.X, _pos.Y + 1));
					this.MazeGenerator.MarkReservedPosition(new Position(_pos.X - 1, _pos.Y + 1));
					this.MazeGenerator.MarkReservedPosition(new Position(_pos.X + 1, _pos.Y + 1));
					this.MazeGenerator.MarkReservedPosition(new Position(_pos.X, _pos.Y + 2));
					this.MazeGenerator.MarkReservedPosition(new Position(_pos.X - 1, _pos.Y + 2));
					this.MazeGenerator.MarkReservedPosition(new Position(_pos.X + 1, _pos.Y + 2));
				}
				else
				{
					while (true)
					{
						_pos.X = (int)(mt.GetUInt32() % (this.Width - 2) + 1);
						_pos.Y = (int)(mt.GetUInt32() % (this.Height - 3) + 1);
						if (this.MazeGenerator.IsFree(_pos))
							if (this.MazeGenerator.IsFree(new Position(_pos.X - 1, _pos.Y)))
								if (this.MazeGenerator.IsFree(new Position(_pos.X + 1, _pos.Y)))
									if (this.MazeGenerator.IsFree(new Position(_pos.X, _pos.Y + 1)))
										if (this.MazeGenerator.IsFree(new Position(_pos.X - 1, _pos.Y + 1)))
											if (this.MazeGenerator.IsFree(new Position(_pos.X + 1, _pos.Y + 1)))
												if (this.MazeGenerator.IsFree(new Position(_pos.X, _pos.Y + 2)))
													break;
					}

					this.MazeGenerator.MarkReservedPosition(new Position(_pos.X - 1, _pos.Y));
					this.MazeGenerator.MarkReservedPosition(new Position(_pos.X + 1, _pos.Y));
					this.MazeGenerator.MarkReservedPosition(new Position(_pos.X, _pos.Y + 1));
					this.MazeGenerator.MarkReservedPosition(new Position(_pos.X - 1, _pos.Y + 1));
					this.MazeGenerator.MarkReservedPosition(new Position(_pos.X + 1, _pos.Y + 1));
					this.MazeGenerator.MarkReservedPosition(new Position(_pos.X, _pos.Y + 2));
				}
			}
			else
			{
				var free = false;
				while (!free)
				{
					_pos.X = (int)(mt.GetUInt32() % this.Width);
					_pos.Y = (int)(mt.GetUInt32() % this.Height);

					free = this.MazeGenerator.IsFree(_pos);
				}
			}

			if (!this.IsLastFloor && !HasBossRoom)
			{
				var rndDir = new RandomDirection();
				while (true)
				{
					var direction = rndDir.GetDirection(mt);
					if (this.SetTraits(_pos, direction, (int)DungeonBlockType.StairsDown))
					{
						_startDirection = direction;
						break;
					}
				}
			}

			this.MazeGenerator.SetPathPosition(_pos);
		}
	}
}
