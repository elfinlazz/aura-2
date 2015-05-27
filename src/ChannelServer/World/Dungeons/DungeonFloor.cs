// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;

namespace Aura.Channel.World.Dungeons
{
	public class DungeonFloor
	{
		private DungeonGenerator _dungeonGenerator;
		private DungeonFloor _prevFloor;
		//private DungeonFloor next_floor_structure;

		private List<List<RoomTrait>> rooms;

		private Position _pos;
		private Position _startPos;
		private int _startDirection;

		private int _branchProbability;
		private int _coverageFactor;
		private int _width;
		private int _height;
		private bool _isLastFloor;

		public bool HasBossRoom { get; private set; }
		public MazeGenerator MazeGenerator { get; private set; }

		public DungeonFloor(DungeonGenerator dungeonGenerator, DungeonFloorData floorData, bool isLastFloor, DungeonFloor prevFloor)
		{
			_pos = new Position(0, 0);
			_startPos = new Position(0, 0);
			_startDirection = Direction.Down;
			_width = 1;
			_height = 1;
			this.MazeGenerator = new MazeGenerator();

			_dungeonGenerator = dungeonGenerator;
			_branchProbability = floorData.BranchProbability;
			_coverageFactor = floorData.CoverageFactor;
			_isLastFloor = isLastFloor;
			_prevFloor = prevFloor;

			this.HasBossRoom = floorData.HasBossRoom;

			this.CalculateSize(floorData);
			this.InitRoomtraits();
			this.GenerateMaze(floorData);
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
			_width = (int)(width - rndNum % (int)(width / 5.0));

			rndNum = _dungeonGenerator.RngMaze.GetUInt32();
			_height = (int)(height - rndNum % (int)(height / 5.0));
		}

		private void InitRoomtraits()
		{
			rooms = new List<List<RoomTrait>>();
			for (int h = 0; h < _width; h++)
			{
				var row = new List<RoomTrait>();
				for (int w = 0; w < _height; w++)
					row.Add(new RoomTrait());

				rooms.Add(row);
			}

			for (int y = 0; y < _height; y++)
			{
				for (int x = 0; x < _width; x++)
				{
					for (int direction = 0; direction < 4; direction++)
					{
						var biased_pos = new Position(x, y).GetBiasedPosition(direction);
						if ((biased_pos.X >= 0) && (biased_pos.Y >= 0))
							if ((biased_pos.X < _width) && (biased_pos.Y < _height))
								rooms[x][y].SetNeighbor(direction, rooms[biased_pos.X][biased_pos.Y]);
					}
				}
			}
		}



		private RoomTrait GetRoom(Position pos)
		{
			if ((pos.X < 0) || (pos.Y < 0) || (pos.X >= _width) || (pos.Y >= _height))
				throw new Exception();
			return rooms[pos.X][pos.Y];
		}

		private bool SetTraits(Position pos, int direction, int door_type)
		{
			Position biased_pos = pos.GetBiasedPosition(direction);
			if ((biased_pos.X >= 0) && (biased_pos.Y >= 0))
			{
				if ((biased_pos.X < _width) && (biased_pos.Y < _height))
				{
					if (!this.MazeGenerator.IsFree(biased_pos))
						return false;
					this.MazeGenerator.MarkReservedPosition(biased_pos);
				}
			}
			RoomTrait room = GetRoom(pos);
			if (room.IsLinked(direction))
				throw new Exception();

			if (room.GetDoorType(direction) != 0)
				throw new Exception();

			int link_type;
			if (door_type == 3100)
				link_type = 2;
			else if (door_type == 3000)
				link_type = 1;
			else
				throw new Exception();
			room.Link(direction, link_type);
			room.SetDoorType(direction, door_type);
			return true;
		}

		private void GenerateMaze(DungeonFloorData floor_desc)
		{
			int crit_path_min = floor_desc.CritPathMin;
			int crit_path_max = floor_desc.CritPathMax;
			if (crit_path_min < 1)
				crit_path_min = 1;
			if (crit_path_max < 1)
				crit_path_max = 1;
			if (crit_path_min > crit_path_max)
			{
				int temp = crit_path_max;
				crit_path_max = crit_path_min;
				crit_path_min = temp;
			}
			this.CreateCriticalPath(crit_path_min, crit_path_max);
			this.CreateSubPath(_coverageFactor, _branchProbability);
			this.UpdatePathPosition();
		}

		private List<MazeMove> CreateCriticalPath(int crit_path_min, int crit_path_max)
		{
			while (true)
			{
				this.MazeGenerator.SetSize(_width, _height);
				this.SetRandomPathPosition();

				if (this.MazeGenerator.GenerateCriticalPath(_dungeonGenerator.RngMaze, crit_path_min, crit_path_max))
				{
					_startPos = this.MazeGenerator.StartPos;
					if (this.SetTraits(_startPos, this.MazeGenerator.StartDirection, 3000))
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

			room.RoomType = 1;

			for (int direction = 0; direction < 4; direction++)
			{
				if (maze_room.GetPassageType(direction) == 2)
				{
					var biased_pos = pos.GetBiasedPosition(direction);
					if (room != null)
						room.Link(direction, 2);

					return this.CreateSubPathRecursive(biased_pos);
				}
			}
			return true;
		}

		private void UpdatePathPosition()
		{
			// TODO: _update_path_position
		}

		private void SetRandomPathPosition()
		{
			if (_prevFloor != null)
				_startDirection = Direction.GetOppositeDirection(_prevFloor._startDirection);
			else
				_startDirection = Direction.Down;

			this.MazeGenerator.StartDirection = _startDirection;

			var mt = _dungeonGenerator.RngMaze;
			if (this.HasBossRoom)
			{
				if (_dungeonGenerator.Option.Contains("largebossroom=" + '"' + "true"))  // <option largebossroom="true" />
				{
					while (true)
					{
						_pos.X = (int)(mt.GetUInt32() % (_width - 2) + 1);
						_pos.Y = (int)(mt.GetUInt32() % (_height - 3) + 1);
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
						_pos.X = (int)(mt.GetUInt32() % (_width - 2) + 1);
						_pos.Y = (int)(mt.GetUInt32() % (_height - 3) + 1);
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
					_pos.X = (int)(mt.GetUInt32() % _width);
					_pos.Y = (int)(mt.GetUInt32() % _height);

					free = this.MazeGenerator.IsFree(_pos);
				}
			}

			if (!_isLastFloor && !HasBossRoom)
			{
				var rnd_dir = new RandomDirection();
				while (true)
				{
					var direction = rnd_dir.GetDirection(mt);
					if (SetTraits(_pos, direction, 3100))
					{
						_startDirection = direction;
						break;
					}
					//			# core::ICommonAPI::stdapi_SetNPCDirection();  // Server stuff?
				}
			}

			this.MazeGenerator.SetPathPosition(_pos);
		}
	}
}
