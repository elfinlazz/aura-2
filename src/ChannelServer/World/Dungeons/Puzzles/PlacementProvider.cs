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
		Corner4,
		Corner8,
		Center9,
		Center,
		Random,
	}

	/// <summary>
	/// Placement provider for dungeon places.
	/// </summary>
	public class PlacementProvider
	{
		private static readonly int[,] _corner4Offsets =
		{
			{ -1,1,315 }, { 1,1,225 },
			{ -1,-1,45 }, { 1,-1,135 },
		};

		private static readonly int[,] _corner8Offsets =
		{
			{ -1,1,315 }, { 0,1,270 }, { 1,1,225 },
			{ -1,0,0 },                { 1,0,180 },
			{ -1,-1,45 }, { 0,-1,90 }, { 1,-1,135 },
		};

		private static readonly int[,] _center9Offsets =
		{
			{ -1,1,0 }, { 0,1,0 }, { 1,1,0 },
			{ -1,0,0 }, { 0,0,0 }, { 1,0,0 },
			{ -1,-1,0 },{ 0,-1,0 },{ 1,-1,0 },
		};

		private static readonly int[,] _centerOffset =
		{
			{ 0,0,0 },
		};

		private Queue<int[]> _positionQueue;
		private Placement _positionType;
		private int _radius;

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

			int[,] offsets = null;
			switch (positionType)
			{
				case Placement.Corner4:
					offsets = _corner4Offsets;
					break;
				case Placement.Corner8:
					offsets = _corner8Offsets;
					break;
				case Placement.Center9:
					offsets = _center9Offsets;
					break;
				case Placement.Center:
					offsets = _centerOffset;
					break;
				case Placement.Random:
					return;
			}

			if (offsets == null)
				return;

			var rnd = RandomProvider.Get();
			var shuffle = Enumerable.Range(0, offsets.GetLength(0));
			shuffle = shuffle.OrderBy(a => rnd.Next());
			foreach (int i in shuffle)
				_positionQueue.Enqueue(new int[] { offsets[i, 0] * radius, offsets[i, 1] * radius, offsets[i, 2] });
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
			return _positionQueue.Dequeue();
		}
	}
}
