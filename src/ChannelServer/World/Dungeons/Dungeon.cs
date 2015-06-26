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
using System.Web.UI.WebControls;
using System.Xml.Linq;
using Aura.Channel.World.Dungeons.Props;
using Aura.Channel.World.Dungeons.Puzzles;

namespace Aura.Channel.World.Dungeons
{
	/// <summary>
	/// Represents a dungeon with its regions.
	/// </summary>
	public class Dungeon
	{
		private int _bossesRemaining;

		private List<TreasureChest> _treasureChests;
		private PlacementProvider _treasurePlacementProvider;

		private Door _bossDoor;
		private Prop _bossExitDoor;
		private bool _bossSpawned;

		/// <summary>
		/// The size (width and height) of a dungeon tile.
		/// </summary>
		public const int TileSize = 2400;

		/// <summary>
		/// The instance id of this dungeon.
		/// </summary>
		public long InstanceId { get; private set; }

		/// <summary>
		/// The name of the dungeon.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// The id of the item used to create this dungeon.
		/// </summary>
		public int ItemId { get; private set; }

		/// <summary>
		/// The seed used to create the puzzles in this dungeon.
		/// </summary>
		public int Seed { get; private set; }

		/// <summary>
		/// The floor plan used in the generation of this dungeon's floors.
		/// </summary>
		public int FloorPlan { get; private set; }

		/// <summary>
		/// Options for the dungeon (in XML).
		/// </summary>
		/// <remarks>
		/// Example: Option.SetAttributeValue("bossmusic", "Boss_Nuadha.mp3");
		/// 
		/// Known options:
		///   bool private
		///   int savestatueid
		///   int laststatueid
		///   bool no_minimap - Disables the minimap
		///   bool largebossroom - Enables larger boss room, crashes client if dungeon doesn't support it
		///   int lastfloorsight - 
		///   bytebool gloweffect - Enables a light glow effect
		///   int bossroom - ?
		///   string bossmusic - Name of the MP3 file to play in the boss room
		/// </remarks>
		public XElement Options { get; private set; }

		/// <summary>
		/// Generator used to generate this dungeon.
		/// </summary>
		public DungeonGenerator Generator { get; private set; }

		/// <summary>
		/// Data of this dungeno from the db.
		/// </summary>
		public DungeonData Data { get; private set; }

		/// <summary>
		/// Script that controls this dungeon.
		/// </summary>
		public DungeonScript Script { get; private set; }

		/// <summary>
		/// Regions of this dungeon, lobby + all floors.
		/// </summary>
		public List<DungeonRegion> Regions { get; private set; }

		/// <summary>
		/// The party that created this dungeon.
		/// </summary>
		public List<Creature> Party { get; private set; }

		/// <summary>
		/// Creates new dungeon.
		/// </summary>
		/// <param name="instanceId"></param>
		/// <param name="dungeonName"></param>
		/// <param name="itemId"></param>
		/// <param name="seed"></param>
		/// <param name="floorPlan"></param>
		/// <param name="creature"></param>
		public Dungeon(long instanceId, string dungeonName, int itemId, int seed, int floorPlan, Creature creature)
		{
			dungeonName = dungeonName.ToLower();

			// Get data
			this.Data = AuraData.DungeonDb.Find(dungeonName);
			if (this.Data == null)
				throw new ArgumentException("Dungeon '" + dungeonName + "' doesn't exist.");

			_treasureChests = new List<TreasureChest>();
			_treasurePlacementProvider = new PlacementProvider(Placement.Treasure8, 750);
			this.Regions = new List<DungeonRegion>();

			this.InstanceId = instanceId;
			this.Name = dungeonName;
			this.ItemId = itemId;
			this.Seed = seed;
			this.FloorPlan = floorPlan;
			this.Options = XElement.Parse("<option />");

			this.Party = new List<Creature>(); // = creature.Party; || = party;
			this.Party.Add(creature);

			// Get script
			this.Script = ChannelServer.Instance.ScriptManager.DungeonScripts.Get(this.Name);
			if (this.Script == null)
				Log.Warning("Dungeon: No script found for '{0}'.", this.Name);

			// Generate floors
			this.Generator = new DungeonGenerator(this.Name, this.ItemId, this.Seed, this.FloorPlan, this.Options.ToString());

			// Prepare puzzles
			for (int iFloor = 0; iFloor < this.Generator.Floors.Count; ++iFloor)
				this.GeneratePuzzles(iFloor);

			this.GenerateRegions();
		}

		/// <summary>
		/// Generates lobby and floor regions.
		/// </summary>
		private void GenerateRegions()
		{
			// Create lobby
			var lobbyRegionId = ChannelServer.Instance.World.DungeonManager.GetRegionId();
			var lobbyRegion = new DungeonLobbyRegion(lobbyRegionId, this.Data.LobbyRegionId, this);
			this.Regions.Add(lobbyRegion);

			// Create floors
			for (int iFloor = 0; iFloor < this.Generator.Floors.Count; ++iFloor)
			{
				var dungeonFloor = this.Generator.Floors[iFloor];

				var floorRegionId = ChannelServer.Instance.World.DungeonManager.GetRegionId();
				var floorRegion = new DungeonFloorRegion(floorRegionId, this, iFloor);
				this.Regions.Add(floorRegion);
			}

			// Add everything to world
			foreach (var floor in this.Regions)
				ChannelServer.Instance.World.AddRegion(floor);

			// Fill
			this.InitRegions();

			// Raise OnCreation
			if (this.Script != null)
				this.Script.OnCreation(this);
		}

		/// <summary>
		/// Generates random puzzles for the floor.
		/// </summary>
		/// <param name="iFloor">The index of the floor.</param>
		private void GeneratePuzzles(int iFloor)
		{
			var floorData = this.Generator.Data.Floors[iFloor];
			var rng = this.Generator.RngPuzzles;
			var sections = this.Generator.Floors[iFloor].Sections;

			var indexList = new List<int>();
			for (var section = 0; section < sections.Count; ++section)
			{
				var puzzleCount = floorData.Sections[section].Max;
				var allPuzzlesCount = floorData.Sections[section].Puzzles.Count;
				indexList.Clear();
				for (var s = 0; s < allPuzzlesCount; indexList.Add(s++)) ;
				for (var s = 0; s < allPuzzlesCount; ++s)
				{
					var pos = (int)rng.GetUInt32(0, (uint)allPuzzlesCount - 1);
					var tmp = indexList[pos];
					indexList[pos] = indexList[s];
					indexList[s] = tmp;
				}
				for (var p = 0; p < puzzleCount; ++p)
				{
					var randomPuzzle = indexList[p % puzzleCount];
					var scriptName = floorData.Sections[section].Puzzles[randomPuzzle].Script;
					var puzzleScript = ChannelServer.Instance.ScriptManager.PuzzleScripts.Get(scriptName);
					if (puzzleScript == null)
					{
						Log.Warning("DungeonFloor.GeneratePuzzles: '{0}' puzzle script not found.", scriptName);
						continue;
					}
					Puzzle puzzle = null;
					try
					{
						puzzle = sections[section].NewPuzzle(this, floorData, floorData.Sections[section].Puzzles[randomPuzzle], puzzleScript);
						puzzleScript.OnPrepare(puzzle);
					}
					catch (PuzzleException e)
					{
						sections[section].Puzzles.Remove(puzzle);
						Log.Debug("Section {0}, puzzle '{1}' : {2}", section, scriptName, e.Message);
					}
				}
			}
		}

		/// <summary>
		/// Initiates regions, spawning props
		/// </summary>
		public void InitRegions()
		{
			for (int iRegion = 0; iRegion < this.Regions.Count; ++iRegion)
			{
				// Lobby
				if (iRegion == 0)
					this.InitLobbyRegion(iRegion);
				// Floors
				else
					this.InitFloorRegion(iRegion);
			}
		}

		/// <summary>
		/// Initiates lobby, adding behavior to the stairs and statue.
		/// </summary>
		/// <param name="iRegion"></param>
		public void InitLobbyRegion(int iRegion)
		{
			var region = this.Regions[iRegion];

			var stairs = region.GetPropById(this.Data.StairsPropId);
			if (stairs == null)
				throw new Exception("Missing stairs prop '" + this.Data.StairsPropId + "'.");

			var statue = region.GetProp(a => a.Extensions.Any(x => x.SignalType == SignalType.Touch && x.EventType == EventType.Confirmation));
			if (statue == null)
				throw new Exception("Missing statue prop '" + this.Data.LastStatuePropId + "'.");

			stairs.Behavior = (cr, pr) =>
			{
				// Indoor_RDungeon_SB marks the start position for the next floor.
				var clientEvent = this.Regions[1].GetClientEvent("Indoor_RDungeon_SB");
				if (clientEvent == null)
				{
					Log.Error("Event 'Indoor_RDungeon_SB' not found while trying to warp to '{0}'.", this.Regions[1].Name);
					return;
				}

				// Warp to the second region, the 1st floor.
				var regionId = this.Regions[1].Id;
				var x = (int)clientEvent.Data.X;
				var y = (int)clientEvent.Data.Y;

				cr.Warp(regionId, x, y);
			};

			statue.Behavior = (cr, pr) =>
			{
				cr.Warp(this.Data.Exit);

				if (this.Script != null)
					this.Script.OnLeftEarly(this, cr);
			};
		}

		/// <summary>
		/// Initiates floor, creating puzzles and props.
		/// </summary>
		/// <param name="iRegion"></param>
		public void InitFloorRegion(int iRegion)
		{
			this.CreatePuzzles(iRegion);

			var region = this.Regions[iRegion];
			var floor = this.Generator.Floors[iRegion - 1];
			var gen = floor.MazeGenerator;
			var floorData = this.Data.Floors[iRegion - 1];

			var iPrevRegion = iRegion - 1;
			var iNextRegion = iRegion + 1;

			var startTile = gen.StartPos;
			var startPos = new Generation.Position(startTile.X * Dungeon.TileSize + Dungeon.TileSize / 2, startTile.Y * Dungeon.TileSize + Dungeon.TileSize / 2);
			var startRoomTrait = floor.GetRoom(startTile);
			var startRoomIncomingDirection = startRoomTrait.GetIncomingDirection();

			var endTile = gen.EndPos;
			var endPos = new Generation.Position(endTile.X * Dungeon.TileSize + Dungeon.TileSize / 2, endTile.Y * Dungeon.TileSize + Dungeon.TileSize / 2);
			var endRoomTrait = floor.GetRoom(endTile);
			var endRoomDirection = 0;
			for (int dir = 0; dir < 4; ++dir)
			{
				if (endRoomTrait.Links[dir] == LinkType.To)
				{
					endRoomDirection = dir;
					break;
				}
			}

			// Create upstairs prop
			var stairsBlock = this.Data.Style.Get(DungeonBlockType.StairsUp, startRoomIncomingDirection);
			var stairs = new Prop(stairsBlock.PropId, region.Id, startPos.X, startPos.Y, MabiMath.DegreeToRadian(stairsBlock.Rotation), 1, 0, "single");
			stairs.Info.Color1 = floorData.Color1;
			stairs.Info.Color2 = floorData.Color1;
			stairs.Info.Color3 = floorData.Color3;
			region.AddProp(stairs);

			// Create portal prop leading to prev floor
			var portalBlock = this.Data.Style.Get(DungeonBlockType.PortalUp, startRoomIncomingDirection);
			var portal = new Prop(portalBlock.PropId, region.Id, startPos.X, startPos.Y, MabiMath.DegreeToRadian(portalBlock.Rotation), 1, 0, "single", "_upstairs", Localization.Get("<mini>TO</mini> Upstairs"));
			portal.Info.Color1 = floorData.Color1;
			portal.Info.Color2 = floorData.Color1;
			portal.Info.Color3 = floorData.Color3;
			portal.Behavior = (cr, pr) =>
			{
				// Indoor_RDungeon_EB marks the end position on the prev floor.
				var clientEvent = this.Regions[iPrevRegion].GetClientEvent("Indoor_RDungeon_EB");
				if (clientEvent == null)
				{
					Log.Error("Event 'Indoor_RDungeon_EB' not found while trying to warp to '{0}'.", this.Regions[iPrevRegion].Name);
					return;
				}

				// Warp to prev region
				var regionId = this.Regions[iPrevRegion].Id;
				var x = (int)clientEvent.Data.X;
				var y = (int)clientEvent.Data.Y;

				cr.Warp(regionId, x, y);
			};
			region.AddProp(portal);

			// Create save statue
			var saveStatue = new Prop(this.Data.SaveStatuePropId, region.Id, startPos.X, startPos.Y, MabiMath.DegreeToRadian(stairsBlock.Rotation + 180), 1, 0, "single");
			saveStatue.Info.Color1 = floorData.Color1;
			saveStatue.Info.Color2 = floorData.Color1;
			saveStatue.Info.Color3 = floorData.Color3;
			saveStatue.Behavior = (cr, pr) =>
			{
				cr.DungeonSaveLocation = new Location(cr.RegionId, cr.GetPosition());
				Send.Notice(cr, Localization.Get("You have memorized this location."));
			};
			region.AddProp(saveStatue);

			// Spawn boss or downstair props
			// TODO: There is one dungeon that has two boss rooms.
			if (floor.IsLastFloor)
			{
				// Create door to treasure room
				_bossExitDoor = new Prop(this.Data.BossExitDoorId, region.Id, endPos.X, endPos.Y + Dungeon.TileSize / 2, Rotation(Direction.Up), 1, 0, "closed");
				_bossExitDoor.Info.Color1 = floorData.Color1;
				_bossExitDoor.Info.Color2 = floorData.Color1;
				_bossExitDoor.Info.Color3 = floorData.Color3;
				region.AddProp(_bossExitDoor);

				// Get or create boss door
				if (endRoomTrait.PuzzleDoors[Direction.Down] == null)
				{
					Log.Warning("Dungeon.InitFloorRegion: No locked place in last section of '{0}'.", this.Name);

					_bossDoor = new Door(this.Data.BossDoorId, region.Id, endPos.X, endPos.Y - Dungeon.TileSize, Direction.Up, DungeonBlockType.BossDoor, "", "closed");
					_bossDoor.Info.Color1 = floorData.Color1;
					_bossDoor.Info.Color2 = floorData.Color1;
					_bossDoor.Info.Color3 = floorData.Color3;
					_bossDoor.Behavior = (cr, pr) => { _bossDoor.Open(); };
					_bossDoor.Behavior += this.BossDoorBehavior;
					_bossDoor.UpdateShapes();
					endRoomTrait.PuzzleDoors[Direction.Down] = _bossDoor; // making sure another open dummy door won't be added here
					region.AddProp(_bossDoor);
				}
				else
				{
					_bossDoor = endRoomTrait.PuzzleDoors[Direction.Down];
				}

				// Create exit statue
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
				// Create downstairs prop
				var stairsDownBlock = this.Data.Style.Get(DungeonBlockType.StairsDown, endRoomDirection);
				var stairsDown = new Prop(stairsDownBlock.PropId, region.Id, endPos.X, endPos.Y, MabiMath.DegreeToRadian(stairsDownBlock.Rotation), 1, 0, "single");
				stairsDown.Info.Color1 = floorData.Color1;
				stairsDown.Info.Color2 = floorData.Color1;
				stairsDown.Info.Color3 = floorData.Color3;
				region.AddProp(stairsDown);

				// Create portal leading to the next floor
				var portalDownBlock = this.Data.Style.Get(DungeonBlockType.PortalDown, endRoomDirection);
				var portalDown = new Prop(portalDownBlock.PropId, region.Id, endPos.X, endPos.Y, MabiMath.DegreeToRadian(portalDownBlock.Rotation), 1, 0, "single", "_downstairs", Localization.Get("<mini>TO</mini> Downstairs"));
				portalDown.Info.Color1 = floorData.Color1;
				portalDown.Info.Color2 = floorData.Color1;
				portalDown.Info.Color3 = floorData.Color3;
				portalDown.Behavior = (cr, pr) =>
				{
					// Indoor_RDungeon_SB marks the start position on the next floor.
					var clientEvent = this.Regions[iNextRegion].GetClientEvent("Indoor_RDungeon_SB");
					if (clientEvent == null)
					{
						Log.Error("Event 'Indoor_RDungeon_SB' not found while trying to warp to '{0}'.", this.Regions[iNextRegion].Name);
						return;
					}

					// Warp to next floor
					var regionId = this.Regions[iNextRegion].Id;
					var x = (int)clientEvent.Data.X;
					var y = (int)clientEvent.Data.Y;

					cr.Warp(regionId, x, y);
				};
				region.AddProp(portalDown);
			}

			// Place dummy doors
			for (int x = 0; x < floor.MazeGenerator.Width; ++x)
			{
				for (int y = 0; y < floor.MazeGenerator.Height; ++y)
				{
					var room = floor.MazeGenerator.GetRoom(x, y);
					var roomTrait = floor.GetRoom(x, y);
					var isRoom = (roomTrait.RoomType >= RoomType.Start);

					if (!room.Visited || !isRoom)
						continue;

					for (var dir = 0; dir < 4; ++dir)
					{
						// Skip stairs
						if (roomTrait.RoomType == RoomType.Start && dir == startRoomIncomingDirection)
							continue;
						if (roomTrait.RoomType == RoomType.End && dir == endRoomDirection)
							continue;

						if (roomTrait.Links[dir] == LinkType.None)
							continue;

						if (roomTrait.PuzzleDoors[dir] == null)
						{
							var doorX = x * Dungeon.TileSize + Dungeon.TileSize / 2;
							var doorY = y * Dungeon.TileSize + Dungeon.TileSize / 2;

							var doorBlock = this.Data.Style.Get(DungeonBlockType.Door, dir);
							var doorProp = new Prop(doorBlock.PropId, region.Id, doorX, doorY, MabiMath.DegreeToRadian(doorBlock.Rotation), state: "open");
							doorProp.Info.Color1 = floorData.Color1;
							doorProp.Info.Color2 = floorData.Color2;
							doorProp.Info.Color3 = floorData.Color3;
							region.AddProp(doorProp);
						}
						else if (roomTrait.PuzzleDoors[dir].EntityId == 0)
						{
							// Add doors from failed puzzles
							roomTrait.PuzzleDoors[dir].Info.Region = region.Id;
							region.AddProp(roomTrait.PuzzleDoors[dir]);
						}
					}
				}
			}
		}

		/// <summary>
		/// Calls OnCreate on all of region's puzzles.
		/// </summary>
		/// <param name="iRegion"></param>
		private void CreatePuzzles(int iRegion)
		{
			var region = this.Regions[iRegion];

			var sections = this.Generator.Floors[iRegion - 1].Sections;
			foreach (var section in sections)
			{
				foreach (var puzzle in section.Puzzles)
				{
					try
					{
						puzzle.OnCreate(region);
					}
					catch (PuzzleException e)
					{
						Log.Warning("Section {0}, puzzle '{1}' : {2}", section, puzzle.Script.Name, e.Message);
					}
				}
			}
		}

		/// <summary>
		/// Returns the radian rotation for the given dungeon direction.
		/// </summary>
		/// <remarks>
		/// TODO: Move somewhere? Direction maybe? Or an extension?
		/// </remarks>
		/// <param name="direction"></param>
		/// <returns></returns>
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

		/// <summary>
		/// Adds boss to list of bosses to spawn.
		/// </summary>
		/// <param name="raceId"></param>
		/// <param name="amount"></param>
		public void AddBoss(int raceId, int amount = 1)
		{
			var rnd = RandomProvider.Get();
			var end = this.Generator.Floors.Last().MazeGenerator.EndPos;
			var endX = end.X * TileSize + TileSize / 2;
			var endY = end.Y * TileSize + TileSize / 2;
			var regionId = this.Regions.Last().Id;

			for (int i = 0; i < amount; ++i)
			{
				var pos = new Position(endX, endY + TileSize / 2);
				pos = pos.GetRandomInRange(TileSize / 2, rnd);

				var npc = new NPC(raceId);
				npc.Death += this.OnBossDeath;
				npc.Spawn(regionId, pos.X, pos.Y);
				Send.SpawnEffect(SpawnEffect.Monster, regionId, pos.X, pos.Y, npc, npc);
				if (npc.AI != null)
					npc.AI.Activate(0);
			}

			Interlocked.Add(ref _bossesRemaining, amount);
		}

		/// <summary>
		/// Adds chest to list of chests to spawn.
		/// </summary>
		/// <param name="chest"></param>
		public void AddChest(TreasureChest chest)
		{
			_treasureChests.Add(chest);
		}

		/// <summary>
		/// Behavior for this dungeon's boss door.
		/// </summary>
		/// <param name="_"></param>
		/// <param name="prop"></param>
		public void BossDoorBehavior(Creature _, Prop prop)
		{
			// Get door
			var door = prop as Door;
			if (door == null)
			{
				Log.Error("Dungeon.BossDoorBehavior: Boss door... is not a door!?");
				return;
			}

			// Make sure it got unlocked
			if (door.IsLocked)
				return;

			// Check if bosses were already spawned
			if (_bossSpawned)
				return;
			_bossSpawned = true;

			// Remove all monsters
			this.Regions.ForEach(a => a.RemoveAllMonsters());

			// Call OnBoss
			if (this.Script != null)
				this.Script.OnBoss(this);

			// Open boss and exit door if no bosses were spawned
			if (_bossesRemaining == 0)
			{
				_bossDoor.SetState("open");
				_bossExitDoor.SetState("open");
			}
		}

		/// <summary>
		/// Raised when one of the bosses dies.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="killer"></param>
		private void OnBossDeath(Creature creature, Creature killer)
		{
			Interlocked.Add(ref _bossesRemaining, -1);

			// Call OnBossDeath
			if (this.Script != null)
				this.Script.OnBossDeath(this, creature);

			// Complete dungeon when all bosses were killed
			if (_bossesRemaining == 0)
				this.Complete();
		}

		// Completes dungeon, opening doors and spawning chests.
		public void Complete()
		{
			// Call OnCleared
			if (this.Script != null)
				this.Script.OnCleared(this);

			// Spawn chests
			this.SpawnTreasureChests();

			// Open doors
			_bossDoor.SetState("open");
			_bossExitDoor.SetState("open");
		}

		/// <summary>
		/// Spawns treasure chests in treasure room.
		/// </summary>
		private void SpawnTreasureChests()
		{
			var region = this.Regions.Last();
			var rnd = RandomProvider.Get();

			for (int i = 0; i < _treasureChests.Count; ++i)
			{
				var pos = new Generation.Position(this.Generator.Floors.Last().MazeGenerator.EndPos);

				pos.X *= Dungeon.TileSize;
				pos.Y *= Dungeon.TileSize;

				pos.X += Dungeon.TileSize / 2;
				pos.Y += (int)(Dungeon.TileSize * 2.5f);

				var placement = _treasurePlacementProvider.GetPosition();

				pos.X += placement[0];
				pos.Y += placement[1];
				var rotation = MabiMath.DegreeToRadian(placement[2]);

				var prop = _treasureChests[i];
				prop.RegionId = region.Id;
				prop.Info.X = pos.X;
				prop.Info.Y = pos.Y;
				prop.Info.Direction = rotation;
				region.AddProp(prop);
			}
		}

		/// <summary>
		/// Returns amount of players in all regions that are part of the dungeon.
		/// </summary>
		/// <returns></returns>
		public int CountPlayers()
		{
			return this.Regions.Sum(a => a.CountPlayers());
		}
	}
}
