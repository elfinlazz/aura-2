using System.Collections.Generic;
using System.Linq;
using Aura.Shared.Util;

namespace Aura.Channel.World.Dungeons.Puzzles
{
	/// <summary>
	/// Specifies how monsters and props are placed in dungeons.
	/// </summary>
	public enum Placement
	{
		/// <summary>
		/// Places one in each corner.
		/// </summary>
		Corner4,

		/// <summary>
		/// Places one in all 8 directions.
		/// </summary>
		Corner8,

		/// <summary>
		/// Places one in all 8 directions, with the bottom one slightly higher.
		/// </summary>
		Treasure8,

		/// <summary>
		/// Places one in all 8 directions and one in the center.
		/// </summary>
		Center9,

		/// <summary>
		/// Places one in the center.
		/// </summary>
		Center,

		/// <summary>
		/// Places randomly.
		/// </summary>
		Random,

		/// <summary>
		/// Places randomly, while maintaining a min distance to others.
		/// </summary>
		Herb,

		/// <summary>
		/// Places two in each corner.
		/// </summary>
		Ore,
	}

	/// <summary>
	/// Placement provider for dungeon places.
	/// </summary>
	public class PlacementProvider
	{
		private static readonly float[,] _corner4Offsets =
		{
			{ -1,1,315 }, { 1,1,225 },
			{ -1,-1,45 }, { 1,-1,135 },
		};

		private static readonly float[,] _corner8Offsets =
		{
			{ -1,1,315 }, { 0,1,270 }, { 1,1,225 },
			{ -1,0,0 },                { 1,0,180 },
			{ -1,-1,45 }, { 0,-1,90 }, { 1,-1,135 },
		};

		private static readonly float[,] _treasure8Offsets =
		{
			{ -1,1,315 },  { 0,1,270 },    { 1,1,225 },
			{ -1,0,0 },                    { 1,0,180 },
			{ -1,-1,45 }, { 0,-0.8f,90 }, { 1,-1,135 },
		};

		private static readonly float[,] _center9Offsets =
		{
			{ -0.8f,0.8f,315 },{ 0,0.8f,270 }, { 0.8f,0.8f,225 },
			{ -0.8f,0,0 },       { 0,0,270 },     { 0.8f,0,180 },
			{ -0.8f,-0.8f,45 },{ 0,-0.8f,90 },{ 0.8f,-0.8f,135 },
		};

		private static readonly float[,] _centerOffset =
		{
			{ 0,0,0 },
		};

		private static readonly float[,] _oreOffsets =
		{
			    { -0.9f,+1.3f,315 }, { 0.9f,+1.3f,225 },
			{ -1.3f,+0.9f,315 },          { 1.3f,+0.9f,225 },

			{ -1.3f,-0.9f,45 },           { 1.3f,-0.9f,135 },
			    { -0.9f,-1.3f,45 },  { 0.9f,-1.3f,135 },
		};

		private Queue<int[]> _positionQueue;
		private Placement _positionType;
		private int _radius;
		private List<Position> _reservedPositions;

		/// <summary>
		/// Creates new placement provider.
		/// </summary>
		/// <param name="positionType"></param>
		/// <param name="radius"></param>
		public PlacementProvider(Placement positionType, int radius = 600)
		{
			_positionQueue = new Queue<int[]>();
			_positionType = positionType;
			_radius = radius;
			_reservedPositions = new List<Position>();

			float[,] offsets = null;
			switch (positionType)
			{
				case Placement.Corner4:
					offsets = _corner4Offsets;
					break;
				case Placement.Corner8:
					offsets = _corner8Offsets;
					break;
				case Placement.Treasure8:
					offsets = _treasure8Offsets;
					break;
				case Placement.Center9:
					offsets = _center9Offsets;
					break;
				case Placement.Center:
					offsets = _centerOffset;
					break;
				case Placement.Ore:
					offsets = _oreOffsets;
					break;
			}

			if (offsets == null)
				return;

			var rnd = RandomProvider.Get();
			var shuffle = Enumerable.Range(0, offsets.GetLength(0));
			shuffle = shuffle.OrderBy(a => rnd.Next());
			foreach (int i in shuffle)
				_positionQueue.Enqueue(new int[] { (int)(offsets[i, 0] * radius), (int)(offsets[i, 1] * radius), (int)offsets[i, 2] });
		}

		/// <summary>
		/// Returns a new position.
		/// </summary>
		/// <remarks>
		/// TODO: Add a dedicated return type for this?
		/// </remarks>
		/// <returns></returns>
		public int[] GetPosition()
		{
			if (_positionType == Placement.Random)
			{
				var rnd = RandomProvider.Get();
				var pos = new int[] { rnd.Next(-_radius, _radius), rnd.Next(-_radius, _radius), rnd.Next(360) };
				pos[0] = (int)(pos[0] * 0.8f);
				pos[1] = (int)(pos[1] * 0.8f);
				return pos;
			}

			if (_positionType == Placement.Herb)
			{
				var rnd = RandomProvider.Get();
				var direction = rnd.Next(360);
				int x = 0, y = 0;
				var emptySpotFound = false;

				// Try 10 positions max
				for (int i = 0; i < 10 && !emptySpotFound; ++i)
				{
					x = rnd.Next(-_radius, _radius);
					y = rnd.Next(-_radius, _radius);
					emptySpotFound = !_reservedPositions.Any(a => a.InRange(new Position(x, y), 100));
				}

				_reservedPositions.Add(new Position(x, y));

				return new int[] { x, y, direction };
			}

			if (_positionQueue.Any())
				return _positionQueue.Dequeue();

			return null;
		}
	}
}
