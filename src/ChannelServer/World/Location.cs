// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aura.Channel.World
{
	public struct Location
	{
		public int RegionId;
		public int X, Y;

		public Location(int regionId, int x, int y)
		{
			this.RegionId = regionId;
			this.X = x;
			this.Y = y;
		}

		public Location(int regionId, Position pos)
			: this(regionId, pos.X, pos.Y)
		{
		}
	}
}
