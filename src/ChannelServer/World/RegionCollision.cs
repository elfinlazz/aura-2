// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Aura.Channel.Util;
using Aura.Data.Database;
using Aura.Shared.Util;

namespace Aura.Channel.World
{
	public class RegionCollision
	{
		private Quadtree<LinePath> _tree;

		public RegionCollision(int x, int y, int width, int height)
		{
			_tree = new Quadtree<LinePath>(new Size(1000, 1000), 2);
		}

		/// <summary>
		/// Loads collision objects from region data.
		/// </summary>
		/// <param name="data"></param>
		public void Init(RegionData data)
		{
			foreach (var area in data.Areas.Values)
			{
				foreach (var prop in area.Props.Values)
				{
					foreach (var shape in prop.Shapes)
					{
						var p1 = new Point(shape.X1, shape.Y1);
						var p2 = new Point(shape.X2, shape.Y2);
						var p3 = new Point(shape.X3, shape.Y3);
						var p4 = new Point(shape.X4, shape.Y4);

						_tree.Insert(new LinePath(p1, p2));
						_tree.Insert(new LinePath(p2, p3));
						_tree.Insert(new LinePath(p3, p4));
						_tree.Insert(new LinePath(p4, p1));
					}
				}
				foreach (var ev in area.Events.Values.Where(a => a.Type == EventType.Collision))
				{
					foreach (var shape in ev.Shapes)
					{
						var p1 = new Point(shape.X1, shape.Y1);
						var p2 = new Point(shape.X2, shape.Y2);
						var p3 = new Point(shape.X3, shape.Y3);
						var p4 = new Point(shape.X4, shape.Y4);

						_tree.Insert(new LinePath(p1, p2));
						_tree.Insert(new LinePath(p2, p3));
						_tree.Insert(new LinePath(p3, p4));
						_tree.Insert(new LinePath(p4, p1));
					}
				}
			}
		}

		/// <summary>
		/// Returns true if the path between from and to intersects with
		/// anything and returns the intersection position via out.
		/// </summary>
		/// <param name="region"></param>
		/// <param name="from"></param>
		/// <param name="to"></param>
		/// <param name="intersection"></param>
		/// <returns></returns>
		public bool Find(Position from, Position to, out Position intersection)
		{
			intersection = to;

			double x1 = from.X, y1 = from.Y;
			double x2 = to.X, y2 = to.Y;

			var intersections = new List<Position>();

			var lines = _tree.Query(new LinePath(from, to).Rect);

			foreach (var line in lines)
			{
				Position inter;
				if (this.FindIntersection(x1, y1, x2, y2, line.P1.X, line.P1.Y, line.P2.X, line.P2.Y, out inter))
					intersections.Add(inter);
			}

			if (intersections.Count < 1)
				return false;

			if (intersections.Count == 1)
			{
				intersection = intersections[0];
				return true;
			}

			// Select nearest intersection
			double distance = double.MaxValue;
			foreach (var inter in intersections)
			{
				//Aura.Shared.Util.Log.Debug("intersection: " + inter);
				//var region = ChannelServer.Instance.World.GetRegion(1);
				//region.AddProp(new Aura.Channel.World.Entities.Prop(229, 1, inter.X, inter.Y, 0));

				var interDist = Math.Pow(x1 - inter.X, 2) + Math.Pow(y1 - inter.Y, 2);
				if (interDist < distance)
				{
					intersection = inter;
					distance = interDist;
				}
			}

			return true;
		}

		/// <summary>
		/// Returns whether the lines x1/y1-x2/y2 and x3/y3-x4/y4 intersect.
		/// The intersection point is returned in the corresponding out-variable.
		/// </summary>
		private bool FindIntersection(double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4, out Position intersection)
		{
			intersection = Position.Zero;

			double denom = ((x2 - x1) * (y4 - y3)) - ((y2 - y1) * (x4 - x3));
			if (denom == 0) return false; // parallel 

			double numer = ((y1 - y3) * (x4 - x3)) - ((x1 - x3) * (y4 - y3));
			double r = numer / denom;
			double numer2 = ((y1 - y3) * (x2 - x1)) - ((x1 - x3) * (y2 - y1));
			double s = numer2 / denom;
			if ((r < 0 || r > 1) || (s < 0 || s > 1)) return false; // nointersect

			double interX = x1 + (r * (x2 - x1));
			double interY = y1 + (r * (y2 - y1));

			intersection = new Position((int)interX, (int)interY);

			return true;
		}
	}

	/// <summary>
	/// Holding two points, making up a path.
	/// </summary>
	public class LinePath : IQuadObject
	{
		public Point P1 { get; private set; }
		public Point P2 { get; private set; }
		public RectangleF Rect { get; private set; }

		public LinePath(Point p1, Point p2)
		{
			this.P1 = p1;
			this.P2 = p2;
			this.Rect = new Rectangle(
				Math.Min(P1.X, P2.X),
				Math.Min(P1.Y, P2.Y),
				Math.Abs(P1.X - P2.X),
				Math.Abs(P1.Y - P2.Y)
			);
		}

		public LinePath(Position p1, Position p2)
			: this(new Point(p1.X, p1.Y), new Point(p2.X, p2.Y))
		{
		}

		public override string ToString()
		{
			return ("(" + P1.X + "," + P1.Y + " - " + P2.X + "," + P2.Y + ")");
		}
	}
}
