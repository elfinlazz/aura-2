// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

namespace Aura.Channel.World
{
	/// <summary>
	/// Representation of a location in the world.
	/// </summary>
	/// <remarks>
	/// Used to save locations, ie a location to warp back to.
	/// </remarks>
	public struct Location
	{
		public int RegionId;
		public int X, Y;

		public Position Position { get { return new Position(X, Y); } }

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
