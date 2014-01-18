// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;

namespace Aura.Data.Database
{
	public class RegionData
	{
		public int Id { get; internal set; }
		public int X1 { get; internal set; }
		public int Y1 { get; internal set; }
		public int X2 { get; internal set; }
		public int Y2 { get; internal set; }
		public Dictionary<int, AreaData> Areas { get; internal set; }
	}

	public class AreaData
	{
		public int Id { get; internal set; }
		public int X1 { get; internal set; }
		public int Y1 { get; internal set; }
		public int X2 { get; internal set; }
		public int Y2 { get; internal set; }
		public Dictionary<long, PropData> Props { get; internal set; }
		public Dictionary<long, EventData> Events { get; internal set; }
	}

	public class PropData
	{
		public long EntityId { get; internal set; }
		public int Id { get; internal set; }
		public float X { get; internal set; }
		public float Y { get; internal set; }
		public float Direction { get; internal set; }
		public float Scale { get; internal set; }
		public List<PropShapeData> Shapes { get; internal set; }
	}

	public class PropShapeData
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

	public class EventData
	{
		public long Id { get; internal set; }
		public int Type { get; internal set; }
		public float X { get; internal set; }
		public float Y { get; internal set; }
		public bool IsAltar { get; internal set; }
		public List<EventElementData> Elements { get; internal set; }
	}

	public class EventElementData
	{
		public int Type { get; internal set; }
		public int Unk { get; internal set; }
	}

	public class RegionInfoDb : DatabaseDatIndexed<int, RegionData>
	{
		public Dictionary<long, PropData> PropEntries = new Dictionary<long, PropData>();
		public Dictionary<long, EventData> EventEntries = new Dictionary<long, EventData>();

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
		public AreaData Find(int region, int area)
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
		public EventData FindEvent(long id)
		{
			EventData result;
			this.EventEntries.TryGetValue(id, out result);
			return result;
		}

		/// <summary>
		/// Returns prop data if it exists, or null.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public PropData FindProp(long id)
		{
			PropData result;
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
		public int GetAreaId(int region, int x, int y)
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
				var ri = new RegionData();

				ri.Id = br.ReadInt32();
				ri.X1 = br.ReadInt32();
				ri.Y1 = br.ReadInt32();
				ri.X2 = br.ReadInt32();
				ri.Y2 = br.ReadInt32();

				var cAreas = br.ReadInt32();
				ri.Areas = new Dictionary<int, AreaData>();
				for (int i = 0; i < cAreas; ++i)
				{
					var ai = new AreaData();

					ai.Id = br.ReadInt32();
					ai.X1 = br.ReadInt32();
					ai.Y1 = br.ReadInt32();
					ai.X2 = br.ReadInt32();
					ai.Y2 = br.ReadInt32();

					var cProps = br.ReadInt32();
					ai.Props = new Dictionary<long, PropData>();
					for (int j = 0; j < cProps; ++j)
					{
						var pi = new PropData();
						pi.EntityId = br.ReadInt64();
						pi.Id = br.ReadInt32();
						pi.X = br.ReadSingle();
						pi.Y = br.ReadSingle();
						pi.Direction = br.ReadSingle();
						pi.Scale = br.ReadSingle();

						var cShapes = br.ReadInt32();
						pi.Shapes = new List<PropShapeData>();
						for (int k = 0; k < cShapes; ++k)
						{
							var si = new PropShapeData();
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

						ai.Props.Add(pi.EntityId, pi);
						this.PropEntries.Add(pi.EntityId, pi);
					}

					var cEvents = br.ReadInt32();
					ai.Events = new Dictionary<long, EventData>();
					for (int j = 0; j < cEvents; ++j)
					{
						var ei = new EventData();
						ei.Id = br.ReadInt64();
						ei.X = br.ReadSingle();
						ei.Y = br.ReadSingle();
						ei.Type = br.ReadInt32();

						var cElements = br.ReadInt32();
						ei.Elements = new List<EventElementData>();
						for (int k = 0; k < cElements; ++k)
						{
							var eei = new EventElementData();
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
}
