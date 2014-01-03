// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;

namespace Aura.Channel.World
{
	/// <summary>
	/// Describes the current position of an entity.
	/// </summary>
	public struct Position
	{
		public readonly int X;
		public readonly int Y;

		public Position(int x, int y)
		{
			this.X = x;
			this.Y = y;
		}

		public Position(Position pos)
		{
			this.X = pos.X;
			this.Y = pos.Y;
		}

		/// <summary>
		/// Returns distance between this and another position.
		/// </summary>
		/// <param name="otherPos"></param>
		/// <returns></returns>
		public int GetDistance(Position otherPos)
		{
			return (int)Math.Sqrt(Math.Pow(X - otherPos.X, 2) + Math.Pow(Y - otherPos.Y, 2));
		}

		/// <summary>
		/// Returns true if the other position is within range.
		/// </summary>
		/// <param name="otherPos"></param>
		/// <param name="range"></param>
		/// <returns></returns>
		public bool InRange(Position otherPos, int range)
		{
			return (Math.Pow(X - otherPos.X, 2) + Math.Pow(Y - otherPos.Y, 2) <= Math.Pow(range, 2));
		}

		public static bool operator ==(Position pos1, Position pos2)
		{
			return (pos1.X == pos2.X && pos1.Y == pos2.Y);
		}

		public static bool operator !=(Position pos1, Position pos2)
		{
			return !(pos1 == pos2);
		}

		public override int GetHashCode()
		{
			return this.X.GetHashCode() ^ this.Y.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return obj is Position && this == (Position)obj;
		}

		public override string ToString()
		{
			return "(Position: " + this.X + ", " + this.Y + ")";
		}
	}
}
