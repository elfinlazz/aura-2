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
		public int BaseSeed { get; set; }
		public int LobbyRegionId { get; set; }
		public string Exit { get; set; }
		public int StairsPropId { get; set; }
		public int StatuePropId { get; set; }
		public int SaveStatuePropId { get; set; }
		public int DoorId { get; set; }
		public int BossDoorId { get; set; }
		public int BossExitDoorId { get; set; }
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
		public bool HasBoss { get; set; }
		public bool Custom { get; set; }
	}

	public class DungeonDb : DatabaseJsonIndexed<string, DungeonData>
	{
		protected override void ReadEntry(JObject entry)
		{
			entry.AssertNotMissing("name", "baseSeed", "lobby", "stairs", "statue", "saveStatue", "door", "bossDoor", "bossExitDoor", "floors");

			var dungeonData = new DungeonData();
			dungeonData.Name = entry.ReadString("name").ToLower();
			dungeonData.BaseSeed = entry.ReadInt("baseSeed");
			dungeonData.LobbyRegionId = entry.ReadInt("lobby");
			dungeonData.Exit = entry.ReadString("exit");
			dungeonData.StairsPropId = entry.ReadInt("stairs");
			dungeonData.StatuePropId = entry.ReadInt("statue");
			dungeonData.SaveStatuePropId = entry.ReadInt("saveStatue");
			dungeonData.DoorId = entry.ReadInt("door");
			dungeonData.BossDoorId = entry.ReadInt("bossDoor");
			dungeonData.BossExitDoorId = entry.ReadInt("bossExitDoor");

			foreach (JObject floorEntry in entry["floors"])
			{
				floorEntry.AssertNotMissing("width", "height", "critPathMin", "critPathMax", "branch", "coverage");

				var floorData = new DungeonFloorData();
				floorData.Width = floorEntry.ReadInt("width");
				floorData.Height = floorEntry.ReadInt("height");
				floorData.CritPathMin = floorEntry.ReadInt("critPathMin");
				floorData.CritPathMax = floorEntry.ReadInt("critPathMax");
				floorData.Branch = floorEntry.ReadInt("branch");
				floorData.Coverage = floorEntry.ReadInt("coverage");
				floorData.HasBoss = floorEntry.ReadBool("hasBoss");
				floorData.Custom = floorEntry.ReadBool("custom");

				dungeonData.Floors.Add(floorData);
			}

			this.Entries[dungeonData.Name] = dungeonData;
		}
	}
}
