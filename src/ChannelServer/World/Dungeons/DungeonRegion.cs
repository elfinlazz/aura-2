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
			this.Name = dungeon.Name + "_" + regionId;
		}

		/// <summary>
		/// Returns the first prop with the given id or null if none were found.
		/// </summary>
		/// <param name="propId"></param>
		/// <returns></returns>
		public Prop GetPropById(int propId)
		{
			return this.GetProp(a => a.Info.Id == propId);
		}

		/// <summary>
		/// Returns the first prop that matches the given predicate.
		/// </summary>
		/// <param name="propId"></param>
		/// <returns></returns>
		public Prop GetProp(Func<Prop, bool> predicate)
		{
			_propsRWLS.EnterReadLock();
			try
			{
				return _props.Values.FirstOrDefault(predicate);
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

			for (int x = 0; x < floor.MazeGenerator.Width; ++x)
			{
				for (int y = 0; y < floor.MazeGenerator.Height; ++y)
				{
					var room = floor.MazeGenerator.GetRoom(x, y);
					var roomTrait = floor.GetRoom(x, y);

					if (!room.Visited)
						continue;

					var isStart = (roomTrait.RoomType == RoomType.Start);
					var isEnd = (roomTrait.RoomType == RoomType.End);
					var isRoom = (roomTrait.RoomType >= RoomType.Start);
					var isBossRoom = (floor.HasBossRoom && isEnd);
					var eventId = 0L;

					if (!isBossRoom)
					{
						var areaData = new AreaData();
						areaData.Id = areaId++;
						areaData.Name = "Tile" + areaData.Id;

						areaData.X1 = x * Dungeon.TileSize;
						areaData.Y1 = y * Dungeon.TileSize;
						areaData.X2 = x * Dungeon.TileSize + Dungeon.TileSize;
						areaData.Y2 = y * Dungeon.TileSize + Dungeon.TileSize;

						this.RegionInfoData.Areas.Add(areaData);

						var type = (isRoom ? DungeonBlockType.Room : DungeonBlockType.Alley);

						var propEntityId = MabiId.ClientProps | ((long)this.Id << 32) | ((long)areaData.Id << 16) | 1;
						var block = this.Dungeon.Data.Style.Get(type, room.Directions);
						var tileCenter = new Point(x * Dungeon.TileSize + Dungeon.TileSize / 2, y * Dungeon.TileSize + Dungeon.TileSize / 2);

						var prop = new Prop(propEntityId, block.PropId, this.Id, tileCenter.X, tileCenter.Y, MabiMath.DegreeToRadian(block.Rotation), 1, 0, "", "", "");
						this.AddProp(prop);

						// Debug
						foreach (var points in prop.Shapes)
						{
							foreach (var point in points)
							{
								var pole = new Prop(30, this.Id, point.X, point.Y, 0, 1, 0, "", "", "");
								pole.Shapes.Clear();
								this.AddProp(pole);
							}
						}

						// TODO: This region/data stuff is a mess... create
						//   proper classes, put them in the regions and be
						//   done with it.

						if (isStart || isEnd)
						{
							var xp = tileCenter.X;
							var yp = tileCenter.Y;

							if (roomTrait.DoorType[Direction.Up] >= 3000)
								yp += 400;
							else if (roomTrait.DoorType[Direction.Right] >= 3000)
								xp += 400;
							else if (roomTrait.DoorType[Direction.Down] >= 3000)
								yp -= 400;
							else if (roomTrait.DoorType[Direction.Left] >= 3000)
								xp -= 400;

							var eventData = new EventData();
							eventData.Id = MabiId.AreaEvents | ((long)this.Id << 32) | ((long)areaData.Id << 16) | eventId++;
							eventData.Name = (isStart ? "Indoor_RDungeon_SB" : "Indoor_RDungeon_EB");
							eventData.X = xp;
							eventData.Y = yp;

							var shape = new ShapeData();
							shape.DirX1 = 1;
							shape.DirY2 = 1;
							shape.LenX = 100;
							shape.LenY = 100;
							shape.PosX = xp;
							shape.PosY = yp;
							eventData.Shapes.Add(shape);

							areaData.Events.Add(eventData.Id, eventData);
							_clientEvents.Add(eventData.Id, new ClientEvent(eventData.Id, eventData));
						}
					}
					else
					{
						// Big main room
						var areaData = new AreaData();
						areaData.Id = areaId++;
						areaData.Name = "Tile" + areaData.Id;

						areaData.X1 = x * Dungeon.TileSize - Dungeon.TileSize;
						areaData.Y1 = y * Dungeon.TileSize;
						areaData.X2 = x * Dungeon.TileSize + Dungeon.TileSize * 2;
						areaData.Y2 = y * Dungeon.TileSize + Dungeon.TileSize * 2;

						this.RegionInfoData.Areas.Add(areaData);

						var block = this.Dungeon.Data.Style.Get(DungeonBlockType.BossRoom);
						var propEntityId = MabiId.ClientProps | ((long)this.Id << 32) | ((long)areaData.Id << 16) | 1;
						var tileCenter = new Point(x * Dungeon.TileSize + Dungeon.TileSize / 2, y * Dungeon.TileSize + Dungeon.TileSize);

						var prop = new Prop(propEntityId, block.PropId, this.Id, tileCenter.X, tileCenter.Y, MabiMath.DegreeToRadian(block.Rotation), 1, 0, "", "", "");
						this.AddProp(prop);

						// Debug
						foreach (var points in prop.Shapes)
						{
							foreach (var point in points)
							{
								var pole = new Prop(30, this.Id, point.X, point.Y, 0, 1, 0, "", "", "");
								pole.Shapes.Clear();
								this.AddProp(pole);
							}
						}

						// Treasure room
						areaData = new AreaData();
						areaData.Id = areaId++;
						areaData.Name = "Tile" + areaData.Id;

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
