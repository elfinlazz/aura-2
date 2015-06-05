// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Aura.Channel.World.Dungeons
{
	public class Dungeon
	{
		public const int TileSize = 2400;

		public long InstanceId { get; private set; }
		public string Name { get; private set; }
		public int ItemId { get; private set; }
		public int Seed { get; private set; }
		public int FloorPlan { get; private set; }
		public XElement Option { get; private set; }

		public DungeonGenerator Generator { get; private set; }
		public DungeonData Data { get; private set; }
		public DungeonScript Script { get; private set; }

		public List<DungeonRegion> Regions { get; private set; }

		public List<Creature> Party { get; private set; }

		private List<DungeonBoss> _bosses;
		private int _bossesCount, _bossesKilled;

		private Prop _bossDoor, _bossExitDoor;

		public Dungeon(long instanceId, string dungeonName, int itemId, Creature creature)
		{
			dungeonName = dungeonName.ToLower();

			this.Data = AuraData.DungeonDb.Find(dungeonName);
			if (this.Data == null)
				throw new ArgumentException("Dungeon '" + dungeonName + "' doesn't exist.");

			_bosses = new List<DungeonBoss>();

			this.InstanceId = instanceId;
			this.Name = dungeonName;
			this.ItemId = itemId;
			this.Seed = 0;
			this.FloorPlan = 0;

			this.Option = XElement.Parse("<option />");
			this.Option.SetAttributeValue("bossmusic", "Boss_Nuadha.mp3");

			// bool private
			// int savestatueid
			// int laststatueid
			// bool no_minimap - Disables the minimap
			// bool largebossroom - Enables larger boss room, crashes client if dungeon doesn't support it
			// int lastfloorsight - 
			// bytebool gloweffect - Enables a light glow effect
			// int bossroom - ?
			// string bossmusic

			this.Party = new List<Creature>();
			this.Party.Add(creature);

			this.Regions = new List<DungeonRegion>();

			this.Generator = new DungeonGenerator(this.Name, this.ItemId, 0, this.FloorPlan, this.Option.ToString());
			this.Script = ChannelServer.Instance.ScriptManager.DungeonScripts.Get(this.Name);
			if (this.Script == null)
				Log.Warning("Dungeon: No script found for '{0}'.", this.Name);

			// Create lobby
			var dungeonEntryRegionId = ChannelServer.Instance.World.DungeonManager.GetRegionId();
			var dungeonEntryRegion = new DungeonLobbyRegion(dungeonEntryRegionId, this.Data.LobbyRegionId, this);
			this.Regions.Add(dungeonEntryRegion);

			// Create floors
			for (int i = 0; i < this.Generator.Floors.Count; ++i)
			{
				var floor = this.Generator.Floors[i];

				var dungeonFloorRegionId = ChannelServer.Instance.World.DungeonManager.GetRegionId();
				var dungeonFloorRegion = new DungeonFloorRegion(dungeonFloorRegionId, this, i);
				this.Regions.Add(dungeonFloorRegion);
			}

			// Add everything to world
			foreach (var floor in this.Regions)
				ChannelServer.Instance.World.AddRegion(floor);

			// Fill
			this.InitRegions();

			if (this.Script != null)
				this.Script.OnCreation(this);
		}

		public void InitRegions()
		{
			for (int i = 0; i < this.Regions.Count; ++i)
			{
				var region = this.Regions[i];

				// Lobby
				if (i == 0)
					this.InitLobbyRegion(i);
				// Floors
				else
					this.InitFloorRegion(i);
			}
		}

		public void InitLobbyRegion(int i)
		{
			var region = this.Regions[i];

			region.GetPropById(this.Data.StairsPropId).Behavior = (cr, pr) =>
			{
				var regionId = this.Regions[1].Id;
				var x = this.Generator.Floors[0].MazeGenerator.StartPos.X * TileSize + TileSize / 2;
				var y = this.Generator.Floors[0].MazeGenerator.StartPos.Y * TileSize + TileSize / 2;

				cr.Warp(regionId, x, y);
			};

			region.GetPropById(this.Data.LastStatuePropId).Behavior = (cr, pr) =>
			{
				cr.Warp(this.Data.Exit);
			};
		}

		public void InitFloorRegion(int i)
		{
			this.SetUpHallwayProps(i);

			var region = this.Regions[i];
			var floor = this.Generator.Floors[i - 1];
			var gen = floor.MazeGenerator;

			var prevRegion = i - 1;
			var nextRegion = i + 1;

			var startTile = gen.StartPos;
			var startPos = new Dungeons.Generation.Position(startTile.X * Dungeon.TileSize + Dungeon.TileSize / 2, startTile.Y * Dungeon.TileSize + Dungeon.TileSize / 2);
			var startRoom = gen.GetRoom(startTile);
			var startRoomTrait = floor.GetRoom(startTile);

			var endTile = gen.EndPos;
			var endPos = new Dungeons.Generation.Position(endTile.X * Dungeon.TileSize + Dungeon.TileSize / 2, endTile.Y * Dungeon.TileSize + Dungeon.TileSize / 2);
			var endRoom = gen.GetRoom(endTile);
			var endRoomTrait = floor.GetRoom(endTile);

			var door = new Prop(this.Data.DoorId, region.Id, startPos.X, startPos.Y, Rotation(GetFirstDirection(startRoom.Directions)), 1, 0, "open");
			region.AddProp(door);

			var stairsBlock = this.Data.Style.Get(DungeonBlockType.StairsUp, startRoomTrait.DoorType);
			var stairs = new Prop(stairsBlock.PropId, region.Id, startPos.X, startPos.Y, MabiMath.DegreeToRadian(stairsBlock.Rotation), 1, 0, "single");
			region.AddProp(stairs);

			var portalBlock = this.Data.Style.Get(DungeonBlockType.PortalUp, startRoomTrait.DoorType);
			var portal = new Prop(portalBlock.PropId, region.Id, startPos.X, startPos.Y, MabiMath.DegreeToRadian(portalBlock.Rotation), 1, 0, "single", "_upstairs", Localization.Get("<mini>TO</mini> Upstairs"));
			portal.Behavior = (cr, pr) =>
			{
				var regionId = this.Regions[prevRegion].Id;
				int x, y;

				if (prevRegion == 0)
				{
					x = 3200;
					y = 4100;
				}
				else
				{
					x = this.Generator.Floors[prevRegion - 1].MazeGenerator.EndPos.X * TileSize + TileSize / 2;
					y = this.Generator.Floors[prevRegion - 1].MazeGenerator.EndPos.Y * TileSize + TileSize / 2;
				}

				cr.Warp(regionId, x, y);
			};
			region.AddProp(portal);

			if (floor.IsLastFloor)
			{
				_bossExitDoor = new Prop(this.Data.BossExitDoorId, region.Id, endPos.X, endPos.Y + Dungeon.TileSize / 2, Rotation(Direction.Up), 1, 0, "closed");
				region.AddProp(_bossExitDoor);

				_bossDoor = new Prop(this.Data.BossDoorId, region.Id, endPos.X, endPos.Y + Dungeon.TileSize / 2, Rotation(Direction.Down), 1, 0, "closed");
				_bossDoor.Behavior = this.BossDoorBehavior;
				region.AddProp(_bossDoor);

				var exitStatue = new Prop(this.Data.LastStatuePropId, region.Id, endPos.X, endPos.Y + Dungeon.TileSize * 2, Rotation(Direction.Up), 1, 0, "single");
				exitStatue.Extensions.Add(new ConfirmationPropExtension("GotoLobby", "_LT[code.standard.msg.dungeon_exit_notice_msg]", "_LT[code.standard.msg.dungeon_exit_notice_title]", "haskey(chest)"));
				exitStatue.Behavior = (cr, pr) => { cr.Warp(this.Data.Exit); };
				region.AddProp(exitStatue);
			}
			else
			{
				var endDoor = new Prop(this.Data.DoorId, region.Id, endPos.X, endPos.Y, Rotation(GetFirstDirection(endRoom.Directions)), 1, 0, "open");
				region.AddProp(endDoor);

				var stairsDownBlock = this.Data.Style.Get(DungeonBlockType.StairsDown, endRoomTrait.DoorType);
				var stairsDown = new Prop(stairsDownBlock.PropId, region.Id, endPos.X, endPos.Y, MabiMath.DegreeToRadian(stairsDownBlock.Rotation), 1, 0, "single");
				region.AddProp(stairsDown);

				var portalDownBlock = this.Data.Style.Get(DungeonBlockType.PortalDown, endRoomTrait.DoorType);
				var portalDown = new Prop(portalDownBlock.PropId, region.Id, endPos.X, endPos.Y, MabiMath.DegreeToRadian(portalDownBlock.Rotation), 1, 0, "single", "_downstairs", Localization.Get("<mini>TO</mini> Downstairs"));
				portalDown.Behavior = (cr, pr) =>
				{
					var regionId = this.Regions[nextRegion].Id;
					var x = this.Generator.Floors[nextRegion - 1].MazeGenerator.StartPos.X * TileSize + TileSize / 2;
					var y = this.Generator.Floors[nextRegion - 1].MazeGenerator.StartPos.Y * TileSize + TileSize / 2;

					cr.Warp(regionId, x, y);
				};
				region.AddProp(portalDown);
			}
		}

		public static int GetFirstDirection(int[] directions)
		{
			for (int dir = 0; dir < directions.Length; ++dir)
			{
				if (directions[dir] > 0)
				{
					return dir;
				}
			}

			return -1;
		}

		public void SetUpHallwayProps(int i)
		{
			// Doing this in the region makes more sense.
		}

		private static float Rotation(int direction)
		{
			switch (direction)
			{
				case Direction.Up: return MabiMath.DirectionToRadian(0, -1);
				case Direction.Down: return MabiMath.DirectionToRadian(0, 1);
				case Direction.Left: return MabiMath.DirectionToRadian(1, 0);
				case Direction.Right: return MabiMath.DirectionToRadian(-1, 0);
			}

			throw new ArgumentException("Invalid direction '" + direction + "'.");
		}

		public void AddBoss(int raceId, int amount = 1)
		{
			_bosses.Add(new DungeonBoss(raceId, amount));
			_bossesCount += amount;
		}

		public void BossDoorBehavior(Creature _, Prop prop)
		{
			if (prop.State == "open")
				return;

			var end = this.Generator.Floors[0].MazeGenerator.EndPos;
			var endX = end.X * TileSize + TileSize / 2;
			var endY = end.Y * TileSize + TileSize / 2;

			if (this.Script != null)
				this.Script.OnBoss(this);

			if (_bossesCount == 0)
			{
				_bossExitDoor.SetState("open");
				prop.SetState("open");
				return;
			}

			var rnd = RandomProvider.Get();

			foreach (var boss in _bosses)
			{
				for (int i = 0; i < boss.Amount; ++i)
				{
					var pos = new Position(endX, endY + TileSize / 2);
					pos = pos.GetRandomInRange(TileSize / 2, rnd);

					// TODO: NPC.Spawn method?
					var npc = ChannelServer.Instance.World.SpawnManager.Spawn(boss.RaceId, this.Regions[1].Id, pos.X, pos.Y, true, true);
					npc.Death += this.OnBossDeath;
				}
			}

			prop.SetState("open");
		}

		private void OnBossDeath(Creature creature, Creature killer)
		{
			Interlocked.Increment(ref _bossesKilled);

			if (_bossesKilled >= _bossesCount)
			{
				if (this.Script != null)
					this.Script.OnCleared(this);

				_bossExitDoor.SetState("open");
			}
		}
	}

	public class DungeonBoss
	{
		public int RaceId { get; set; }
		public int Amount { get; set; }

		public DungeonBoss(int raceId, int amount)
		{
			this.RaceId = raceId;
			this.Amount = Math.Max(1, amount);
		}
	}
}
