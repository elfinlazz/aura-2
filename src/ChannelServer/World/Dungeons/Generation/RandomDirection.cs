// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Data;
using Aura.Mabi.Util;

namespace Aura.Channel.World.Dungeons.Generation
{
	public class RandomDirection
	{
		public int[] Directions { get; private set; }

		public RandomDirection()
		{
			this.Directions = new int[] { 0, 0, 0, 0 };
		}

		public int GetDirection(MTRandom rnd)
		{
			var visited = true;
			var direction = 0;

			while (visited)
			{
				direction = (int)rnd.GetUInt32() & 3;
				visited = this.Directions[direction] != 0;
			}

			this.Directions[direction] = 1;

			return direction;
		}
	}
}
