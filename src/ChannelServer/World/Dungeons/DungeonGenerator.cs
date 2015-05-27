// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Data;
using System.Collections.Generic;

namespace Aura.Channel.World.Dungeons
{
	public class DungeonGenerator
	{
		public string Name { get; private set; }
		public int ItemId { get; private set; }
		public int Seed { get; private set; }
		public int FloorPlan { get; private set; }
		public string Option { get; private set; }
		public MTRandom RngMaze { get; private set; }
		public MTRandom RngPuzzles { get; private set; }
		public List<DungeonFloor> Floors { get; private set; }

		public DungeonGenerator(string dungeonName, int itemId, int seed, int floorPlan, string option)
		{
			this.Name = dungeonName.ToLower();
			var dungeonData = AuraData.DungeonDb.Find(dungeonName);

			this.Seed = seed;
			this.FloorPlan = floorPlan;
			this.Option = (option ?? "").ToLower();
			this.RngMaze = new MTRandom(dungeonData.BaseSeed + itemId + floorPlan);
			this.RngPuzzles = new MTRandom(seed);
			this.Floors = new List<DungeonFloor>();

			DungeonFloor prev = null;
			for (int i = 0; i < dungeonData.Floors.Count; i++)
			{
				var isLastFloor = (i == dungeonData.Floors.Count - 1);

				var floor = new DungeonFloor(this, dungeonData.Floors[i], isLastFloor, prev);
				this.Floors.Add(floor);

				prev = floor;
			}
		}
	}
}
