// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Text.RegularExpressions;

namespace Aura.Data.Database
{
	public class RegionData
	{
		public int Id { get; set; }
		public int X1 { get; set; }
		public int Y1 { get; set; }
		public int X2 { get; set; }
		public int Y2 { get; set; }
		public Dictionary<int, AreaData> Areas { get; set; }
	}

	public class AreaData
	{
		public int Id { get; set; }
		public int X1 { get; set; }
		public int Y1 { get; set; }
		public int X2 { get; set; }
		public int Y2 { get; set; }
		public Dictionary<long, PropData> Props { get; set; }
		public Dictionary<long, EventData> Events { get; set; }
	}

	public class PropData
	{
		public long EntityId { get; set; }
		public int Id { get; set; }
		public float X { get; set; }
		public float Y { get; set; }
		public float Direction { get; set; }
		public float Scale { get; set; }
		public List<ShapeData> Shapes { get; set; }
		public List<RegionElementData> Parameters { get; set; }

		/// <summary>
		/// Returns drop type, if one exists, or -1.
		/// </summary>
		/// <returns></returns>
		public int GetDropType()
		{
			foreach (var param in this.Parameters)
			{
				// TODO: Event or SignalType can probably be checked as
				//   well for finding drop props.
				var match = Regex.Match(param.XML, @"<xml droptype=""(?<type>[0-9]+)""/>", RegexOptions.Compiled);
				if (!match.Success)
					continue;

				return int.Parse(match.Groups["type"].Value);
			}

			return -1;
		}
	}

	public class ShapeData
	{
		public int X1 { get; set; }
		public int Y1 { get; set; }
		public int X2 { get; set; }
		public int Y2 { get; set; }
		public int X3 { get; set; }
		public int Y3 { get; set; }
		public int X4 { get; set; }
		public int Y4 { get; set; }
	}

	public class EventData
	{
		public long Id { get; set; }
		public EventType Type { get; set; }
		public int RegionId { get; set; }
		public float X { get; set; }
		public float Y { get; set; }
		public bool IsAltar { get; set; }
		public List<ShapeData> Shapes { get; set; }
		public List<RegionElementData> Parameters { get; set; }
	}

	public enum EventType : int
	{
		Unk1 = 1,
		AreaChange = 10, // ? (texts, bgm change)
		Collision = 14,
		CreatureSpawn = 2000,
	}

	public enum SignalType : int
	{
		/// <summary>
		/// Triggered by entering event area.
		/// </summary>
		Enter = 101,

		/// <summary>
		/// Triggered by leaving event area.
		/// </summary>
		Leave = 102,
	}

	public class RegionElementData
	{
		public int EventType { get; set; }
		public int SignalType { get; set; }
		public string Name { get; set; }
		public string XML { get; set; }
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
						pi.Shapes = new List<ShapeData>();
						for (int k = 0; k < cShapes; ++k)
						{
							var si = new ShapeData();
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

						var cElements = br.ReadInt32();
						pi.Parameters = new List<RegionElementData>();
						for (int k = 0; k < cElements; ++k)
						{
							var red = new RegionElementData();
							red.EventType = br.ReadInt32();
							red.SignalType = br.ReadInt32();
							red.Name = br.ReadString();
							red.XML = br.ReadString();

							pi.Parameters.Add(red);
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
						ei.RegionId = ri.Id;
						ei.X = br.ReadSingle();
						ei.Y = br.ReadSingle();
						ei.Type = (EventType)br.ReadInt32();

						var cShapes = br.ReadInt32();
						ei.Shapes = new List<ShapeData>();
						for (int k = 0; k < cShapes; ++k)
						{
							var si = new ShapeData();
							si.X1 = br.ReadInt32();
							si.Y1 = br.ReadInt32();
							si.X2 = br.ReadInt32();
							si.Y2 = br.ReadInt32();
							si.X3 = br.ReadInt32();
							si.Y3 = br.ReadInt32();
							si.X4 = br.ReadInt32();
							si.Y4 = br.ReadInt32();

							ei.Shapes.Add(si);
						}

						var cElements = br.ReadInt32();
						ei.Parameters = new List<RegionElementData>();
						for (int k = 0; k < cElements; ++k)
						{
							var red = new RegionElementData();
							red.EventType = br.ReadInt32();
							red.SignalType = br.ReadInt32();
							red.Name = br.ReadString();
							red.XML = br.ReadString();

							if (!ei.IsAltar && red.EventType == 2110 && red.SignalType == 103)
								ei.IsAltar = true;

							ei.Parameters.Add(red);
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
