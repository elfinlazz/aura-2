// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

namespace Aura.Channel.World.Dungeons.Generation
{
	public class Position
	{
		public int X { get; set; }
		public int Y { get; set; }

		public Position(Position pos)
			: this(pos.X, pos.Y)
		{
		}

		public Position(int x, int y)
		{
			this.X = x;
			this.Y = y;
		}

		public Position GetBias(int dir)
		{
			if (dir == Direction.Up)
				return new Position(0, 1);
			else if (dir == Direction.Right)
				return new Position(1, 0);
			else if (dir == Direction.Down)
				return new Position(0, -1);
			else if (dir == Direction.Left)
				return new Position(-1, 0);
			else
				return new Position(0, 0);
		}

		public Position GetBiasedPosition(int direction)
		{
			var bias = this.GetBias(direction);
			return new Position(this.X + bias.X, this.Y + bias.Y);
		}
	}
}
