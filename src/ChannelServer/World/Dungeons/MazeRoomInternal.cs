// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

namespace Aura.Channel.World.Dungeons
{
	public class MazeRoomInternal
	{
		public int[] Directions { get; set; }
		public bool IsOnCriticalPath { get; set; }
		public int IsVisited { get; set; }
		public bool IsReserved { get; set; }

		public MazeRoomInternal()
		{
			this.Directions = new int[] { 0, 0, 0, 0 };
		}

		public bool IsOccupied()
		{
			return (this.IsVisited != 0 || this.IsReserved);
		}

		public void Visited(int cnt)
		{
			this.IsVisited = cnt;
		}

		public int GetPassageType(int direction)
		{
			return this.Directions[direction];
		}
	}
}
