// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Data.Database
{
	public class DungeonData
	{
		public string Name { get; set; }
		public DungeonStyleData Style { get; set; }
		public int BaseSeed { get; set; }
		public int LobbyRegionId { get; set; }
		public string Exit { get; set; }
		public int StairsPropId { get; set; }
		public int SaveStatuePropId { get; set; }
		public int LastStatuePropId { get; set; }
		public int DoorId { get; set; }
		public int BossDoorId { get; set; }
		public int BossExitDoorId { get; set; }
		public int ChestId { get; set; }
		public int LockedChestId { get; set; }
		public bool BlockBoss { get; set; }
		public List<DungeonFloorData> Floors { get; set; }

		public DungeonData()
		{
			this.Floors = new List<DungeonFloorData>();
		}
	}

	public class DungeonFloorData
	{
		public int Width { get; set; }
		public int Height { get; set; }
		public int CritPathMin { get; set; }
		public int CritPathMax { get; set; }
		public int Branch { get; set; }
		public int Coverage { get; set; }
		public uint Color1 { get; set; }
		public uint Color2 { get; set; }
		public uint Color3 { get; set; }
		public uint LightColor1 { get; set; }
		public uint LightColor2 { get; set; }
		public uint LightColor3 { get; set; }
		public bool HasBoss { get; set; }
		public bool Custom { get; set; }
		public string Extra { get; set; }
		public List<DungeonSectionData> Sections { get; set; }

		public DungeonFloorData()
		{
			this.Sections = new List<DungeonSectionData>();
		}
	}

	public class DungeonSectionData
	{
		public int Min { get; set; }
		public int Max { get; set; }
		public List<DungeonPuzzleData> Puzzles { get; set; }

		public DungeonSectionData()
		{
			this.Puzzles = new List<DungeonPuzzleData>();
		}
	}

	public class DungeonPuzzleData
	{
		public string Script { get; set; }
		public string Arg { get; set; }
		public List<DungeonMonsterGroupData> Groups { get; set; }

		public DungeonPuzzleData()
		{
			this.Groups = new List<DungeonMonsterGroupData>();
		}
	}

	public class DungeonMonsterGroupData : List<DungeonMonsterData>
	{
		public DungeonMonsterGroupData Copy()
		{
			var result = new DungeonMonsterGroupData();
			foreach (var monsterData in this)
				result.Add(monsterData.Copy());

			return result;
		}
	}

	public class DungeonMonsterData
	{
		public int RaceId { get; set; }
		public int Amount { get; set; }

		public DungeonMonsterData Copy()
		{
			var result = new DungeonMonsterData();
			result.RaceId = this.RaceId;
			result.Amount = this.Amount;

			return result;
		}
	}

	public class DungeonDb : DatabaseJsonIndexed<string, DungeonData>
	{
		protected override void ReadEntry(JObject entry)
		{
			entry.AssertNotMissing("name", "style", "baseSeed", "lobby", "stairs", "saveStatue", "lastStatue", "door", "bossDoor", "bossExitDoor", "floors");

			var dungeonData = new DungeonData();
			dungeonData.Name = entry.ReadString("name").ToLower();
			dungeonData.BaseSeed = entry.ReadInt("baseSeed");
			dungeonData.LobbyRegionId = entry.ReadInt("lobby");
			dungeonData.Exit = entry.ReadString("exit");
			dungeonData.StairsPropId = entry.ReadInt("stairs");
			dungeonData.SaveStatuePropId = entry.ReadInt("saveStatue");
			dungeonData.LastStatuePropId = entry.ReadInt("lastStatue");
			dungeonData.DoorId = entry.ReadInt("door");
			dungeonData.BossDoorId = entry.ReadInt("bossDoor");
			dungeonData.BossExitDoorId = entry.ReadInt("bossExitDoor");
			dungeonData.ChestId = entry.ReadInt("chest", 10200);
			dungeonData.LockedChestId = entry.ReadInt("lockedChest", 10201);
			dungeonData.BlockBoss = entry.ReadBool("blockBoss");

			var style = entry.ReadInt("style");
			dungeonData.Style = AuraData.DungeonBlocksDb.Find(style);
			if (dungeonData.Style == null)
				throw new DatabaseErrorException("Dungeon style '" + style + "' not found in dungeon_blocks.");

			foreach (var floorObj in entry["floors"])
			{
				// Comments
				var floorEntry = floorObj as JObject;
				if (floorEntry == null)
					continue;

				floorEntry.AssertNotMissing("width", "height", "critPathMin", "critPathMax", "branch", "coverage", "color", "lightColor");

				var floorData = new DungeonFloorData();
				floorData.Width = floorEntry.ReadInt("width");
				floorData.Height = floorEntry.ReadInt("height");
				floorData.CritPathMin = floorEntry.ReadInt("critPathMin");
				floorData.CritPathMax = floorEntry.ReadInt("critPathMax");
				floorData.Branch = floorEntry.ReadInt("branch");
				floorData.Coverage = floorEntry.ReadInt("coverage");
				floorData.HasBoss = floorEntry.ReadBool("hasBoss");
				floorData.Custom = floorEntry.ReadBool("custom");
				floorData.Extra = floorEntry.ReadString("extra", null);

				floorData.Color1 = (uint)floorEntry["color"][0];
				floorData.Color2 = (uint)floorEntry["color"][1];
				floorData.Color3 = (uint)floorEntry["color"][2];
				floorData.LightColor1 = (uint)floorEntry["lightColor"][0];
				floorData.LightColor2 = (uint)floorEntry["lightColor"][1];
				floorData.LightColor3 = (uint)floorEntry["lightColor"][2];

				if (floorEntry.ContainsKey("sections"))
				{
					foreach (JObject sectionEntry in floorEntry["sections"])
					{
						sectionEntry.AssertNotMissing("min", "max", "puzzles");

						var sectionData = new DungeonSectionData();
						sectionData.Min = sectionEntry.ReadInt("min");
						sectionData.Max = sectionEntry.ReadInt("max");

						foreach (JObject puzzleEntry in sectionEntry["puzzles"])
						{
							puzzleEntry.AssertNotMissing("script");

							var puzzleData = new DungeonPuzzleData();
							puzzleData.Script = puzzleEntry.ReadString("script");
							puzzleData.Arg = puzzleEntry.ReadString("arg", null);

							if (puzzleEntry.ContainsKey("groups"))
							{
								foreach (var groupsEntry in puzzleEntry["groups"])
								{
									var list = new DungeonMonsterGroupData();

									foreach (JObject groupEntry in groupsEntry)
									{
										groupEntry.AssertNotMissing("raceId", "amount");

										var monsterData = new DungeonMonsterData();
										monsterData.RaceId = groupEntry.ReadInt("raceId");
										monsterData.Amount = groupEntry.ReadInt("amount");

										if (!AuraData.RaceDb.Entries.ContainsKey(monsterData.RaceId))
											_warnings.Add(new DatabaseWarningException("Race '" + monsterData.RaceId + "' not found."));

										list.Add(monsterData);
									}

									puzzleData.Groups.Add(list);
								}
							}

							sectionData.Puzzles.Add(puzzleData);
						}

						floorData.Sections.Add(sectionData);
					}
				}

				dungeonData.Floors.Add(floorData);
			}

			this.Entries[dungeonData.Name] = dungeonData;
		}
	}
}
