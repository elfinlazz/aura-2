// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

namespace Aura.Channel.World.Dungeons.Generation
{
	public class MazeRoomInternal
	{
		public int[] Directions { get; set; }
		public bool IsOnCriticalPath { get; set; }
		public int VisitedCount { get; set; }
		public bool IsReserved { get; set; }

		public bool Visited { get { return (this.VisitedCount != 0); } }
		public bool Occupied { get { return (this.Visited || this.IsReserved); } }

		public MazeRoomInternal()
		{
			this.Directions = new int[] { 0, 0, 0, 0 };
		}

		public int GetPassageType(int direction)
		{
			return this.Directions[direction];
		}
	}
}
