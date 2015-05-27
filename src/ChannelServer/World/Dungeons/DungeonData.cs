// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Aura.Channel.World.Dungeons
{
	public class DungeonData
	{
		public string Name { get; set; }
		public int BaseSeed { get; set; }
		public List<DungeonFloorData> Floors { get; set; }

		public DungeonData()
		{
			this.Floors = new List<DungeonFloorData>();
		}

		public static DungeonData LoadDungeonClass(string dungeonName)
		{
			foreach (var xmlName in new[] { "dungeondb2.xml", "dungeondb.xml", "dungeon_ruin.xml" })
			{
				var xml = XDocument.Load(xmlName);

				foreach (var dungeonXml in xml.Descendants("dungeon"))
				{
					var name = dungeonXml.Attribute("name").Value;
					if (name != null && name.ToLower() == dungeonName.ToLower())
					{
						var dungeon = new DungeonData();
						dungeon.Name = dungeonXml.Attribute("name").Value.ToLower();
						dungeon.BaseSeed = (int)dungeonXml.Attribute("baseseed");

						foreach (var floorXml in dungeonXml.Descendants("floordesc"))
						{
							var floor = new DungeonFloorData();
							floor.IsCustomFloor = floorXml.Attribute("custom") != null;
							floor.Width = (int)floorXml.Attribute("width");
							floor.Height = (int)floorXml.Attribute("height");
							floor.CritPathMin = (int)floorXml.Attribute("critpathmin");
							floor.CritPathMax = (int)floorXml.Attribute("critpathmax");
							floor.HasBossRoom = floorXml.Elements("boss").Count() > 0;
							floor.BranchProbability = (int)floorXml.Attribute("branch");
							floor.CoverageFactor = (int)floorXml.Attribute("coverage");

							dungeon.Floors.Add(floor);
						}

						return dungeon;
					}
				}
			}

			return null;
		}
	}

	public class DungeonFloorData
	{
		public int Width { get; set; }
		public int Height { get; set; }
		public int CritPathMin { get; set; }
		public int CritPathMax { get; set; }
		public bool IsCustomFloor { get; set; }
		public bool HasBossRoom { get; set; }
		public int BranchProbability { get; set; }
		public int CoverageFactor { get; set; }
	}
}
