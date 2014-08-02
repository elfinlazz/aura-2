// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Drawing;
using System.Threading;
using Aura.Shared.Util;

namespace Aura.Channel.Scripting
{
	public class CreatureSpawn
	{
		private static int _id;

		public int Id { get; private set; }
		public int RaceId { get; private set; }
		public int Amount { get; private set; }
		public int RegionId { get; private set; }

		private Point[] _points;
		private int _minX = int.MaxValue, _minY = int.MaxValue, _maxX = 0, _maxY = 0;

		public CreatureSpawn(int raceId, int amount, int regionId, params int[] coordinates)
		{
			this.Id = Interlocked.Increment(ref _id);
			this.RaceId = raceId;
			this.Amount = amount;
			this.RegionId = regionId;

			if (coordinates.Length < 2 || coordinates.Length % 2 != 0)
				throw new Exception("CreatureSpawn: Invalid amount of coordinates.");

			_points = new Point[coordinates.Length / 2];
			for (int i = 0, j = 0; i < coordinates.Length; ++j, i += 2)
			{
				_points[j] = new Point(coordinates[i], coordinates[i + 1]);
				if (coordinates[i] < _minX) _minX = coordinates[i];
				if (coordinates[i] > _maxX) _maxX = coordinates[i];
				if (coordinates[i + 1] < _minY) _minY = coordinates[i + 1];
				if (coordinates[i + 1] > _maxY) _maxY = coordinates[i + 1];
			}

			if (_minX > _maxX)
				Log.Debug(_minY + " - " + _maxY);
		}

		/// <summary>
		/// Returns random spawn position.
		/// </summary>
		/// <returns></returns>
		public Point GetRandomPosition()
		{
			// Single position
			if (_points.Length == 1)
				return _points[0];

			var rnd = RandomProvider.Get();

			// Line
			if (_points.Length == 2)
			{
				var d = rnd.NextDouble();
				var x = _points[0].X + (_points[1].X - _points[0].X) * d;
				var y = _points[0].Y + (_points[1].Y - _points[0].Y) * d;
				return new Point((int)x, (int)y);
			}

			// Polygon
			var result = new Point();
			while (!this.IsPointInside(result = new Point(rnd.Next(_minX, _maxX), rnd.Next(_minY, _maxY))))
			{ }

			return result;
		}

		/// <summary>
		/// Returns true if point is within the spawn points.
		/// </summary>
		/// <param name="point"></param>
		/// <returns></returns>
		private bool IsPointInside(Point point)
		{
			var result = false;

			for (int i = 0, j = _points.Length - 1; i < _points.Length; j = i++)
			{
				if (((_points[i].Y > point.Y) != (_points[j].Y > point.Y)) && (point.X < (_points[j].X - _points[i].X) * (point.Y - _points[i].Y) / (_points[j].Y - _points[i].Y) + _points[i].X))
					result = !result;
			}

			return result;
		}
	}
}
