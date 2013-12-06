// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.IO;

namespace Aura.Data.Database
{
	public class RegionInfo
	{
		public int Id { get; internal set; }
		public int X1 { get; internal set; }
		public int Y1 { get; internal set; }
		public int X2 { get; internal set; }
		public int Y2 { get; internal set; }
		public Dictionary<int, AreaInfo> Areas { get; internal set; }
	}

	public class AreaInfo
	{
		public int Id { get; internal set; }
		public int X1 { get; internal set; }
		public int Y1 { get; internal set; }
		public int X2 { get; internal set; }
		public int Y2 { get; internal set; }
		public Dictionary<long, PropInfo> Props { get; internal set; }
		public Dictionary<long, EventInfo> Events { get; internal set; }
	}

	public class PropInfo
	{
		public long Id { get; internal set; }
		public int Class { get; internal set; }
		public float X { get; internal set; }
		public float Y { get; internal set; }
		public float Direction { get; internal set; }
		public float Scale { get; internal set; }
		public List<PropShapeInfo> Shapes { get; internal set; }
	}

	public class PropShapeInfo
	{
		public int X1 { get; internal set; }
		public int Y1 { get; internal set; }
		public int X2 { get; internal set; }
		public int Y2 { get; internal set; }
		public int X3 { get; internal set; }
		public int Y3 { get; internal set; }
		public int X4 { get; internal set; }
		public int Y4 { get; internal set; }
	}

	public class EventInfo
	{
		public long Id { get; internal set; }
		public int Type { get; internal set; }
		public float X { get; internal set; }
		public float Y { get; internal set; }
		public bool IsAltar { get; internal set; }
		public List<EventElementInfo> Elements { get; internal set; }
	}

	public class EventElementInfo
	{
		public int Type { get; internal set; }
		public int Unk { get; internal set; }
	}

	public class RegionInfoDb : DatabaseDatIndexed<int, RegionInfo>
	{
		public Dictionary<long, PropInfo> PropEntries = new Dictionary<long, PropInfo>();
		public Dictionary<long, EventInfo> EventEntries = new Dictionary<long, EventInfo>();

		private Random _rnd = new Random(Environment.TickCount);

		public override void Clear()
		{
			base.Clear();
			this.PropEntries.Clear();
			this.EventEntries.Clear();
		}

		/// <summary>
		/// Returns area data if it exists, or null.
		/// </summary>
		/// <param name="region"></param>
		/// <param name="area"></param>
		/// <returns></returns>
		public AreaInfo Find(int region, int area)
		{
			if (!this.Entries.ContainsKey(region))
				return null;
			if (!this.Entries[region].Areas.ContainsKey(area))
				return null;

			return this.Entries[region].Areas[area];
		}

		/// <summary>
		/// Returns event data if it exists, or null.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public EventInfo FindEvent(long id)
		{
			EventInfo result;
			this.EventEntries.TryGetValue(id, out result);
			return result;
		}

		/// <summary>
		/// Returns prop data if it exists, or null.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public PropInfo FindProp(long id)
		{
			PropInfo result;
			this.PropEntries.TryGetValue(id, out result);
			return result;
		}

		/// <summary>
		/// Returns random coordinates inside the actual region.
		/// </summary>
		/// <param name="region"></param>
		/// <returns></returns>
		public Point RandomCoord(int region)
		{
			var result = new Point();

			var ri = this.Find(region);
			if (ri != null)
			{
				lock (_rnd)
				{
					result.X = _rnd.Next(ri.X1, ri.X2);
					result.Y = _rnd.Next(ri.Y1, ri.Y2);
				}
			}

			return result;
		}

		/// <summary>
		/// Returns area id for the given location, or 0 if no area exists.
		/// </summary>
		/// <param name="region"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public int GetAreaId(int region, uint x, uint y)
		{
			var ri = this.Find(region);
			if (ri == null)
				return int.MaxValue;

			foreach (var area in ri.Areas.Values)
			{
				if (x >= Math.Min(area.X1, area.X2) && x <= Math.Max(area.X1, area.X2) && y >= Math.Min(area.Y1, area.Y2) && y <= Math.Max(area.Y1, area.Y2))
					return area.Id;
			}

			return 0;
		}

		protected override void Read(BinaryReader br)
		{
			var cRegions = br.ReadInt32();
			for (int l = 0; l < cRegions; ++l)
			{
				var ri = new RegionInfo();

				ri.Id = br.ReadInt32();
				ri.X1 = br.ReadInt32();
				ri.Y1 = br.ReadInt32();
				ri.X2 = br.ReadInt32();
				ri.Y2 = br.ReadInt32();

				var cAreas = br.ReadInt32();
				ri.Areas = new Dictionary<int, AreaInfo>();
				for (int i = 0; i < cAreas; ++i)
				{
					var ai = new AreaInfo();

					ai.Id = br.ReadInt32();
					ai.X1 = br.ReadInt32();
					ai.Y1 = br.ReadInt32();
					ai.X2 = br.ReadInt32();
					ai.Y2 = br.ReadInt32();

					var cProps = br.ReadInt32();
					ai.Props = new Dictionary<long, PropInfo>();
					for (int j = 0; j < cProps; ++j)
					{
						var pi = new PropInfo();
						pi.Id = br.ReadInt64();
						pi.Class = br.ReadInt32();
						pi.X = br.ReadSingle();
						pi.Y = br.ReadSingle();
						pi.Direction = br.ReadSingle();
						pi.Scale = br.ReadSingle();

						var cShapes = br.ReadInt32();
						pi.Shapes = new List<PropShapeInfo>();
						for (int k = 0; k < cShapes; ++k)
						{
							var si = new PropShapeInfo();
							si.X1 = br.ReadInt32();
							si.Y1 = br.ReadInt32();
							si.X2 = br.ReadInt32();
							si.Y2 = br.ReadInt32();
							si.X3 = br.ReadInt32();
							si.Y3 = br.ReadInt32();
							si.X4 = br.ReadInt32();
							si.Y4 = br.ReadInt32();

							pi.Shapes.Add(si);
						}

						ai.Props.Add(pi.Id, pi);
						this.PropEntries.Add(pi.Id, pi);
					}

					var cEvents = br.ReadInt32();
					ai.Events = new Dictionary<long, EventInfo>();
					for (int j = 0; j < cEvents; ++j)
					{
						var ei = new EventInfo();
						ei.Id = br.ReadInt64();
						ei.X = br.ReadSingle();
						ei.Y = br.ReadSingle();
						ei.Type = br.ReadInt32();

						var cElements = br.ReadInt32();
						ei.Elements = new List<EventElementInfo>();
						for (int k = 0; k < cElements; ++k)
						{
							var eei = new EventElementInfo();
							eei.Type = br.ReadInt32();
							eei.Unk = br.ReadInt32();

							if (!ei.IsAltar && eei.Type == 2110 && eei.Unk == 103)
								ei.IsAltar = true;

							ei.Elements.Add(eei);
						}

						ai.Events.Add(ei.Id, ei);
						this.EventEntries.Add(ei.Id, ei);
					}

					ri.Areas.Add(ai.Id, ai);
				}

				this.Entries.Add(ri.Id, ri);
			}
		}
	}

	public struct Point
	{
		public int X;
		public int Y;

		public Point(int x, int y)
		{
			this.X = x;
			this.Y = y;
		}
	}
}
