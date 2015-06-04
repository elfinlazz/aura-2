// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Scripting.Scripts;
using Aura.Channel.World.Dungeons.Generation;
using Aura.Channel.World.Entities;
using Aura.Data;
using Aura.Data.Database;
using Aura.Mabi;
using Aura.Mabi.Const;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.World.Dungeons
{
	public abstract class DungeonRegion : Region
	{
		public Dungeon Dungeon { get; private set; }

		public DungeonRegion(int regionId, Dungeon dungeon)
			: base(regionId)
		{
			this.Dungeon = dungeon;
		}

		/// <summary>
		/// Returns the first prop with the given id or null if none were found.
		/// </summary>
		/// <param name="propId"></param>
		/// <returns></returns>
		public Prop GetPropById(int propId)
		{
			_propsRWLS.EnterReadLock();
			try
			{
				return _props.Values.FirstOrDefault(a => a.Info.Id == propId);
			}
			finally
			{
				_propsRWLS.ExitReadLock();
			}
		}
	}

	public class DungeonFloorRegion : DungeonRegion
	{
		public int FloorId { get; private set; }
		public DungeonFloor Floor { get; private set; }

		public DungeonFloorRegion(int regionId, Dungeon dungeon, int floorId)
			: base(regionId, dungeon)
		{
			this.FloorId = floorId;
			this.Floor = dungeon.Generator.Floors[floorId];

			this.GenerateAreas();
		}

		private void GenerateAreas()
		{
			this.RegionInfoData = new RegionInfoData();

			var areaId = 2;
			var floor = this.Floor;
			var blocks = AuraData.DungeonBlocksDb.Find(this.Dungeon.Data.Style);

			for (int x = 0; x < floor.MazeGenerator.Width; ++x)
			{
				for (int y = 0; y < floor.MazeGenerator.Height; ++y)
				{
					var room = floor.MazeGenerator.GetRoom(new Generation.Position(x, y));

					if (!floor.MazeGenerator.Rooms[x][y].Visited)
						continue;

					var isStart = (x == floor.MazeGenerator.StartPos.X && y == floor.MazeGenerator.StartPos.Y);
					var isEnd = (x == floor.MazeGenerator.EndPos.X && y == floor.MazeGenerator.EndPos.Y);
					var isRoom = (isStart || isEnd);
					var isBossRoom = (floor.HasBossRoom && isEnd);

					if (!isBossRoom)
					{
						var areaData = new AreaData();
						areaData.Id = areaId++;

						areaData.X1 = x * Dungeon.TileSize;
						areaData.Y1 = y * Dungeon.TileSize;
						areaData.X2 = x * Dungeon.TileSize + Dungeon.TileSize;
						areaData.Y2 = y * Dungeon.TileSize + Dungeon.TileSize;

						this.RegionInfoData.Areas.Add(areaData);

						var type = (isBossRoom ? DungeonBlockType.BossRoom : isRoom ? DungeonBlockType.Room : DungeonBlockType.Alley);

						var propEntityId = MabiId.ClientProps | ((long)this.Id << 32) | ((long)areaData.Id << 16) | 1;
						var top = room.Directions[Direction.Up] != 0 ? 1 : 0;
						var right = room.Directions[Direction.Right] != 0 ? 1 : 0;
						var bottom = room.Directions[Direction.Down] != 0 ? 1 : 0;
						var left = room.Directions[Direction.Left] != 0 ? 1 : 0;
						var block = blocks.FirstOrDefault(a => a.Type == type && a.Top == top && a.Right == right && a.Bottom == bottom && a.Left == left);
						var tileCenter = new Point(x * Dungeon.TileSize + Dungeon.TileSize / 2, y * Dungeon.TileSize + Dungeon.TileSize / 2);

						if (block == null)
							Log.Debug("no block found:  {0}, {1}, {2}, {3}", top, right, bottom, left);
						else
						{
							var prop = new Prop(propEntityId, block.PropId, this.Id, tileCenter.X, tileCenter.Y, MabiMath.DegreeToRadian(block.Rotation), 1, 0, "", "", "");
							this.AddProp(prop);
							//if (isRoom)
							{
								foreach (var points in prop.Shapes)
								{
									foreach (var point in points)
									{
										Log.Debug("{2}: {0},{1}", point.X, point.Y, block.PropId);
										var pole = new Prop(Prop.GetNewEntityId(this.Id, 0), 30, this.Id, point.X, point.Y, 0, 1, 0, "", "", "");
										pole.Shapes.Clear();
										this.AddProp(pole);
									}
									Log.Debug("");
								}
							}
						}
					}
					else
					{
						// Big main room
						var areaData = new AreaData();
						areaData.Id = areaId++;

						areaData.X1 = x * Dungeon.TileSize - Dungeon.TileSize;
						areaData.Y1 = y * Dungeon.TileSize;
						areaData.X2 = x * Dungeon.TileSize + Dungeon.TileSize * 2;
						areaData.Y2 = y * Dungeon.TileSize + Dungeon.TileSize * 2;

						this.RegionInfoData.Areas.Add(areaData);

						var block = blocks.FirstOrDefault(a => a.Type == DungeonBlockType.BossRoom);
						var propEntityId = MabiId.ClientProps | ((long)this.Id << 32) | ((long)areaData.Id << 16) | 1;
						var tileCenter = new Point(x * Dungeon.TileSize + Dungeon.TileSize / 2, y * Dungeon.TileSize + Dungeon.TileSize);
						var prop = new Prop(propEntityId, block.PropId, this.Id, tileCenter.X, tileCenter.Y, MabiMath.DegreeToRadian(block.Rotation), 1, 0, "", "", "");
						this.AddProp(prop);
						foreach (var points in prop.Shapes)
						{
							foreach (var point in points)
							{
								//Log.Debug("{2}: {0},{1}", point.X, point.Y, block.PropId);
								var pole = new Prop(Prop.GetNewEntityId(this.Id, 0), 30, this.Id, point.X, point.Y, 0, 1, 0, "", "", "");
								pole.Shapes.Clear();
								this.AddProp(pole);
							}
						}

						// Treasure room
						areaData = new AreaData();
						areaData.Id = areaId++;

						areaData.X1 = x * Dungeon.TileSize;
						areaData.Y1 = y * Dungeon.TileSize + Dungeon.TileSize * 2;
						areaData.X2 = x * Dungeon.TileSize + Dungeon.TileSize;
						areaData.Y2 = y * Dungeon.TileSize + Dungeon.TileSize * 2 + Dungeon.TileSize;

						this.RegionInfoData.Areas.Add(areaData);
					}
				}
			}
		}
	}

	public class DungeonLobbyRegion : DungeonRegion
	{
		public DungeonLobbyRegion(int regionId, int baseRegionId, Dungeon dungeon)
			: base(regionId, dungeon)
		{
			var baseRegionInfoData = AuraData.RegionInfoDb.Find(baseRegionId);
			if (baseRegionInfoData == null)
				throw new Exception("DungeonLobbyRegion: No region info data found for '" + baseRegionId + "'.");

			this.RegionInfoData = baseRegionInfoData.Copy();
			FixIds(this.RegionInfoData, this.Id);

			this.InitializeFromData();
		}

		private static void FixIds(RegionInfoData data, int regionId)
		{
			var areaId = 1;

			foreach (var areaData in data.Areas)
			{
				areaData.Id = areaId++;

				foreach (var propData in areaData.Props.Values)
				{
					var entityId = (propData.EntityId & ~0x0000FFFF00000000) | ((long)regionId << 32) | ((long)areaData.Id << 16);
					propData.EntityId = entityId;
				}

				foreach (var eventData in areaData.Events.Values)
				{
					var entityId = (eventData.Id & ~0x0000FFFF00000000) | ((long)regionId << 32) | ((long)areaData.Id << 16);
					eventData.Id = entityId;
					eventData.RegionId = regionId;
				}
			}
		}
	}
}
