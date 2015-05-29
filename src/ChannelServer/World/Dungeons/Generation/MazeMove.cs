// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

namespace Aura.Channel.World.Dungeons.Generation
{
	public class MazeMove
	{
		public Position PosFrom { get; private set; }
		public Position PosTo { get; private set; }

		public int Direction { get; set; }

		public MazeMove(Position from, Position to, int direction)
		{
			this.PosFrom = new Position(from);
			this.PosTo = new Position(to);
			this.Direction = direction;
		}
	}
}
