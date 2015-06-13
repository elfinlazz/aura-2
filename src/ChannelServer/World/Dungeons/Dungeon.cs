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
using Aura.Channel.World.Dungeons.Puzzles;

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

		private List<TreasureChest> _treasureChests;

		private Prop _bossDoor, _bossExitDoor;

		public Dungeon(long instanceId, string dungeonName, int itemId, Creature creature)
		{
			dungeonName = dungeonName.ToLower();

			this.Data = AuraData.DungeonDb.Find(dungeonName);
			if (this.Data == null)
				throw new ArgumentException("Dungeon '" + dungeonName + "' doesn't exist.");

			_bosses = new List<DungeonBoss>();
			_treasureChests = new List<TreasureChest>();

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

			var stairs = region.GetPropById(this.Data.StairsPropId);
			if (stairs == null)
				throw new Exception("Missing stairs prop '" + this.Data.StairsPropId + "'.");

			var statue = region.GetPropById(this.Data.LastStatuePropId);
			if (statue == null)
				throw new Exception("Missing statue prop '" + this.Data.LastStatuePropId + "'.");

			stairs.Behavior = (cr, pr) =>
			{
				var regionId = this.Regions[1].Id;
				var x = this.Generator.Floors[0].MazeGenerator.StartPos.X * TileSize + TileSize / 2;
				var y = this.Generator.Floors[0].MazeGenerator.StartPos.Y * TileSize + TileSize / 2;

				cr.Warp(regionId, x, y);
			};

			statue.Behavior = (cr, pr) =>
			{
				cr.Warp(this.Data.Exit);

				if (this.Script != null)
					this.Script.OnLeftEarly(this, cr);
			};
		}

		private void InitPuzzles(int i)
		{
			var region = this.Regions[i];
			var floorData = this.Generator.Data.Floors[i - 1];
			var rng = this.Generator.RngPuzzles;
			var sections = this.Generator.Floors[i - 1].Sections;

			for (var section = 0; section < sections.Count; ++section)
			{
				var puzzleCount = (int)rng.GetUInt32((uint)floorData.Sets[section].Min, (uint)floorData.Sets[section].Max);
				for (var p = 0; p < puzzleCount; ++p)
				{
					var randomPuzzle = (int)rng.GetUInt32((uint)floorData.Sets[section].Puzzles.Count);
					var scriptName = floorData.Sets[section].Puzzles[randomPuzzle].Script;
					var puzzleScript = ChannelServer.Instance.ScriptManager.PuzzleScripts.Get(scriptName);
					var monsterGroups = floorData.Sets[section].Puzzles[randomPuzzle].Groups;
					if (puzzleScript == null)
					{
						Log.Warning("DungeonFloor.GeneratePuzzles: '{0}' puzzle script not found.", scriptName);
						continue;
					}
					try
					{
						puzzleScript.OnPrepare(sections[section].NewPuzzle(this, region, puzzleScript, monsterGroups));
					}
					catch (CPuzzleException e)
					{
						Log.Warning("Section {0}, puzzle '{1}' : {2}", section, scriptName, e.Message);
					}
				}
			}
		}

		public void InitFloorRegion(int i)
		{
			this.SetUpHallwayProps(i);
			this.InitPuzzles(i);

			var region = this.Regions[i];
			var floor = this.Generator.Floors[i - 1];
			var gen = floor.MazeGenerator;
			var floorData = this.Data.Floors[i - 1];

			var prevRegion = i - 1;
			var nextRegion = i + 1;

			var startTile = gen.StartPos;
			var startPos = new Dungeons.Generation.Position(startTile.X * Dungeon.TileSize + Dungeon.TileSize / 2, startTile.Y * Dungeon.TileSize + Dungeon.TileSize / 2);
			var startRoom = gen.GetRoom(startTile);
			var startRoomTrait = floor.GetRoom(startTile);
			var startRoomIncomingDirection = new int[] { 0, 0, 0, 0 };
			startRoomIncomingDirection[startRoomTrait.GetIncomingDirection()] = 1;

			var endTile = gen.EndPos;
			var endPos = new Dungeons.Generation.Position(endTile.X * Dungeon.TileSize + Dungeon.TileSize / 2, endTile.Y * Dungeon.TileSize + Dungeon.TileSize / 2);
			var endRoom = gen.GetRoom(endTile);
			var endRoomTrait = floor.GetRoom(endTile);
			var endRoomDirection = 0;
			for (int dir = 0; dir < 4; ++dir)
				if (endRoomTrait.Links[dir] == LinkType.To)
				{
					endRoomDirection = dir;
					break;
				}

			//var door = new Prop(this.Data.DoorId, region.Id, startPos.X, startPos.Y, Rotation(GetFirstDirection(startRoom.Directions)), 1, 0, "open");
			//door.Info.Color1 = floorData.Color1;
			//door.Info.Color2 = floorData.Color1;
			//door.Info.Color3 = floorData.Color3;
			//region.AddProp(door);

			var stairsBlock = this.Data.Style.Get(DungeonBlockType.StairsUp, startRoomIncomingDirection);
			var stairs = new Prop(stairsBlock.PropId, region.Id, startPos.X, startPos.Y, MabiMath.DegreeToRadian(stairsBlock.Rotation), 1, 0, "single");
			stairs.Info.Color1 = floorData.Color1;
			stairs.Info.Color2 = floorData.Color1;
			stairs.Info.Color3 = floorData.Color3;
			region.AddProp(stairs);

			var portalBlock = this.Data.Style.Get(DungeonBlockType.PortalUp, startRoomIncomingDirection);
			var portal = new Prop(portalBlock.PropId, region.Id, startPos.X, startPos.Y, MabiMath.DegreeToRadian(portalBlock.Rotation), 1, 0, "single", "_upstairs", Localization.Get("<mini>TO</mini> Upstairs"));
			portal.Info.Color1 = floorData.Color1;
			portal.Info.Color2 = floorData.Color1;
			portal.Info.Color3 = floorData.Color3;
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
				_bossExitDoor.Info.Color1 = floorData.Color1;
				_bossExitDoor.Info.Color2 = floorData.Color1;
				_bossExitDoor.Info.Color3 = floorData.Color3;
				region.AddProp(_bossExitDoor);

				if (endRoomTrait.PuzzleDoors[Direction.Down] == null)
				{
					_bossDoor = new Prop(this.Data.BossDoorId, region.Id, endPos.X, endPos.Y + Dungeon.TileSize / 2, Rotation(Direction.Down), 1, 0, "closed");
					_bossDoor.Info.Color1 = floorData.Color1;
					_bossDoor.Info.Color2 = floorData.Color1;
					_bossDoor.Info.Color3 = floorData.Color3;
					_bossDoor.Behavior = this.BossDoorBehavior;
					_bossDoor.Behavior = this.BossDoorBehavior;
					region.AddProp(_bossDoor);
				}
				else
				{
					_bossDoor = endRoomTrait.PuzzleDoors[Direction.Down].GetDoorProp();
				}

				//var dummyDoor = new Prop(this.Data.DoorId, region.Id, endPos.X, endPos.Y - Dungeon.TileSize, Rotation(GetFirstDirection(gen.GetRoom(endTile.GetBiasedPosition(Direction.Down)).Directions, Direction.Right)), 1, 0, "open");
				//dummyDoor.Info.Color1 = floorData.Color1;
				//dummyDoor.Info.Color2 = floorData.Color1;
				//dummyDoor.Info.Color3 = floorData.Color3;
				//region.AddProp(dummyDoor);

				var exitStatue = new Prop(this.Data.LastStatuePropId, region.Id, endPos.X, endPos.Y + Dungeon.TileSize * 2, Rotation(Direction.Up), 1, 0, "single");
				exitStatue.Info.Color1 = floorData.Color1;
				exitStatue.Info.Color2 = floorData.Color1;
				exitStatue.Info.Color3 = floorData.Color3;
				exitStatue.Extensions.Add(new ConfirmationPropExtension("GotoLobby", "_LT[code.standard.msg.dungeon_exit_notice_msg]", "_LT[code.standard.msg.dungeon_exit_notice_title]", "haskey(chest)"));
				exitStatue.Behavior = (cr, pr) => { cr.Warp(this.Data.Exit); };
				region.AddProp(exitStatue);
			}
			else
			{
				//var endDoor = new Prop(this.Data.DoorId, region.Id, endPos.X, endPos.Y, Rotation(GetFirstDirection(endRoom.Directions)), 1, 0, "open");
				//endDoor.Info.Color1 = floorData.Color1;
				//endDoor.Info.Color2 = floorData.Color1;
				//endDoor.Info.Color3 = floorData.Color3;
				//region.AddProp(endDoor);

				var stairsDownBlock = this.Data.Style.Get(DungeonBlockType.StairsDown, endRoomDirection);
				var stairsDown = new Prop(stairsDownBlock.PropId, region.Id, endPos.X, endPos.Y, MabiMath.DegreeToRadian(stairsDownBlock.Rotation), 1, 0, "single");
				stairsDown.Info.Color1 = floorData.Color1;
				stairsDown.Info.Color2 = floorData.Color1;
				stairsDown.Info.Color3 = floorData.Color3;
				region.AddProp(stairsDown);

				var portalDownBlock = this.Data.Style.Get(DungeonBlockType.PortalDown, endRoomDirection);
				var portalDown = new Prop(portalDownBlock.PropId, region.Id, endPos.X, endPos.Y, MabiMath.DegreeToRadian(portalDownBlock.Rotation), 1, 0, "single", "_downstairs", Localization.Get("<mini>TO</mini> Downstairs"));
				portalDown.Info.Color1 = floorData.Color1;
				portalDown.Info.Color2 = floorData.Color1;
				portalDown.Info.Color3 = floorData.Color3;
				portalDown.Behavior = (cr, pr) =>
				{
					var regionId = this.Regions[nextRegion].Id;
					var x = this.Generator.Floors[nextRegion - 1].MazeGenerator.StartPos.X * TileSize + TileSize / 2;
					var y = this.Generator.Floors[nextRegion - 1].MazeGenerator.StartPos.Y * TileSize + TileSize / 2;

					cr.Warp(regionId, x, y);
				};
				region.AddProp(portalDown);
			}

			// Place lacking doors.
			for (int x = 0; x < floor.MazeGenerator.Width; ++x)
				for (int y = 0; y < floor.MazeGenerator.Height; ++y)
				{
					var room = floor.MazeGenerator.GetRoom(x, y);
					var roomTrait = floor.GetRoom(x, y);

					if (!room.Visited)
						continue;

					var isRoom = (roomTrait.RoomType >= RoomType.Start);

					if (isRoom)
						for (var dir = 0; dir < 4; ++dir)
							if ((roomTrait.Links[dir] != LinkType.None) && roomTrait.PuzzleDoors[dir] == null)
								if (roomTrait.DoorType[dir] == (int)DungeonBlockType.Door || roomTrait.DoorType[dir] == (int)DungeonBlockType.Alley)
								{
									var doorX = x * Dungeon.TileSize + Dungeon.TileSize / 2;
									var doorY = y * Dungeon.TileSize + Dungeon.TileSize / 2;

									var doorBlock = this.Data.Style.Get(DungeonBlockType.Door, dir);

									var doorProp = new Prop(doorBlock.PropId, region.Id, doorX, doorY, MabiMath.DegreeToRadian(doorBlock.Rotation), state: "open");
									doorProp.Info.Color1 = 0xFFFFFF;
									doorProp.Info.Color2 = 0xFFFFFF;
									region.AddProp(doorProp);
								}
				}

		}

		public static int GetFirstDirection(int[] directions, int start = 0)
		{
			for (int dir = start; dir < directions.Length; ++dir)
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

		public void AddChest(TreasureChest chest)
		{
			_treasureChests.Add(chest);
		}

		public void BossDoorBehavior(Creature _, Prop prop)
		{
			if (prop.State == "open")
				return;

			var end = this.Generator.Floors.Last().MazeGenerator.EndPos;
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
					var npc = ChannelServer.Instance.World.SpawnManager.Spawn(boss.RaceId, this.Regions.Last().Id, pos.X, pos.Y, true, true);
					npc.Death += this.OnBossDeath;
				}
			}

			prop.SetState("open");
		}

		private void OnBossDeath(Creature creature, Creature killer)
		{
			Interlocked.Increment(ref _bossesKilled);

			if (this.Script != null)
				this.Script.OnBossDeath(this, creature);

			if (_bossesKilled >= _bossesCount)
				this.Complete();
		}

		public void Complete()
		{
			if (this.Script != null)
				this.Script.OnCleared(this);

			this.SpawnTreasureChests();

			_bossExitDoor.SetState("open");
		}

		private void SpawnTreasureChests()
		{
			var region = this.Regions.Last();
			var rnd = RandomProvider.Get();
			var offsets = new int[,]
			{
				{ -600,+800,315 }, { +0,+600,270 }, { +600,+800,225 },
				{ -600,+0,0 },                        { +600,+0,180 },
				{ -600,-800,45 },  { +0,-600,90 },  { +600,-800,135 },
			};

			for (int i = 0, j = rnd.Next(8); i < _treasureChests.Count; ++i, ++j)
			{
				var pos = new Generation.Position(this.Generator.Floors.Last().MazeGenerator.EndPos);

				pos.X *= Dungeon.TileSize;
				pos.Y *= Dungeon.TileSize;

				pos.X += Dungeon.TileSize / 2;
				pos.Y += (int)(Dungeon.TileSize * 2.5f);

				pos.X += offsets[j % 8, 0];
				pos.Y += offsets[j % 8, 1];
				var rotation = MabiMath.DegreeToRadian(offsets[j % 8, 2]);

				var prop = _treasureChests[i].CreateProp(region, pos.X, pos.Y, rotation);
				region.AddProp(prop);
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
