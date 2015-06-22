// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Text.RegularExpressions;
using Aura.Mabi.Const;
using System.Xml.Linq;

namespace Aura.Data.Database
{
	public class RegionInfoData
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public int GroupId { get; set; }
		public int X1 { get; set; }
		public int Y1 { get; set; }
		public int X2 { get; set; }
		public int Y2 { get; set; }
		public List<AreaData> Areas { get; set; }

		public RegionInfoData()
		{
			this.Areas = new List<AreaData>();
		}

		/// <summary>
		/// Returns area with given name or null if it doesn't exist.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public AreaData GetArea(string name)
		{
			return this.Areas.FirstOrDefault(a => a.Name == name);
		}

		/// <summary>
		/// Returns event by id or null if it doesn't exist.
		/// </summary>
		/// <returns></returns>
		public EventData GetEvent(long eventId)
		{
			foreach (var area in this.Areas)
			{
				if (area.Events.ContainsKey(eventId))
					return area.Events[eventId];
			}

			return null;
		}

		/// <summary>
		/// Returns index of the area in the list.
		/// </summary>
		/// <param name="areaId"></param>
		/// <returns></returns>
		public int GetAreaIndex(int areaId)
		{
			var id = 1;
			foreach (var area in this.Areas)
			{
				if (area.Id == areaId)
					return id;

				id++;
			}

			return -1;
		}

		/// <summary>
		/// Returns random coordinates inside the actual region.
		/// </summary>
		/// <param name="rnd"></param>
		/// <returns></returns>
		public Point RandomCoord(Random rnd)
		{
			var result = new Point();
			result.X = rnd.Next(this.X1, this.X2);
			result.Y = rnd.Next(this.Y1, this.Y2);

			return result;
		}

		/// <summary>
		/// Creates copy of this region data.
		/// </summary>
		/// <returns></returns>
		public RegionInfoData Copy()
		{
			var result = new RegionInfoData();
			result.Id = this.Id;
			result.Name = this.Name;
			result.GroupId = this.GroupId;
			result.X1 = this.X1;
			result.Y1 = this.Y1;
			result.X2 = this.X2;
			result.Y2 = this.Y2;

			foreach (var area in this.Areas)
				result.Areas.Add(area.Copy(true, true));

			return result;
		}
	}

	public class AreaData
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public int X1 { get; set; }
		public int Y1 { get; set; }
		public int X2 { get; set; }
		public int Y2 { get; set; }
		public Dictionary<long, PropData> Props { get; set; }
		public Dictionary<long, EventData> Events { get; set; }

		public AreaData()
		{
			this.Props = new Dictionary<long, PropData>();
			this.Events = new Dictionary<long, EventData>();
		}

		/// <summary>
		/// Creates a copy of the area data.
		/// </summary>
		/// <param name="copyProps"></param>
		/// <param name="copyEvents"></param>
		/// <returns></returns>
		public AreaData Copy(bool copyProps, bool copyEvents)
		{
			var result = new AreaData();
			result.Id = this.Id;
			result.Name = this.Name;
			result.X1 = this.X1;
			result.Y1 = this.Y1;
			result.X2 = this.X2;
			result.Y2 = this.Y2;
			result.Props = new Dictionary<long, PropData>();
			result.Events = new Dictionary<long, EventData>();

			if (copyProps)
			{
				foreach (var original in this.Props.Values)
				{
					var item = original.Copy();
					result.Props.Add(item.EntityId, item);
				}
			}

			if (copyEvents)
			{
				foreach (var original in this.Events.Values)
				{
					var item = original.Copy();
					result.Events.Add(item.Id, item);
				}
			}

			return result;
		}

		/// <summary>
		/// Returns prop with given name or null if it doesn't exist.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public PropData GetProp(string name)
		{
			return this.Props.Values.FirstOrDefault(a => a.Name.ToLower() == name.ToLower());
		}

		/// <summary>
		/// Returns event with given name or null if it doesn't exist.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public EventData GetEvent(string name)
		{
			return this.Events.Values.FirstOrDefault(a => a.Name.ToLower() == name.ToLower());
		}
	}

	public class PropData
	{
		public long EntityId { get; set; }
		public int Id { get; set; }
		public string Name { get; set; }
		public float X { get; set; }
		public float Y { get; set; }
		public float Direction { get; set; }
		public float Scale { get; set; }
		public List<ShapeData> Shapes { get; set; }
		public List<RegionElementData> Parameters { get; set; }

		public PropData()
		{
			this.Shapes = new List<ShapeData>();
			this.Parameters = new List<RegionElementData>();
		}

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
				if (param.XML == null || param.XML.Attribute("droptype") == null)
					continue;

				return int.Parse(param.XML.Attribute("droptype").Value);
			}

			return -1;
		}

		public PropData Copy()
		{
			var result = new PropData();
			result.EntityId = this.EntityId;
			result.Id = this.Id;
			result.Name = this.Name;
			result.X = this.X;
			result.Y = this.Y;
			result.Direction = this.Direction;
			result.Scale = this.Scale;

			result.Shapes = new List<ShapeData>(this.Shapes.Count);
			foreach (var item in this.Shapes)
				result.Shapes.Add(item.Copy());

			result.Parameters = new List<RegionElementData>(this.Parameters.Count);
			foreach (var item in this.Parameters)
				result.Parameters.Add(item.Copy());

			return result;
		}
	}

	public class ShapeData
	{
		public float DirX1 { get; set; }
		public float DirX2 { get; set; }
		public float DirY1 { get; set; }
		public float DirY2 { get; set; }
		public float LenX { get; set; }
		public float LenY { get; set; }
		public float PosX { get; set; }
		public float PosY { get; set; }

		public Point[] GetPoints(float radianAngle, int pivotX, int pivotY)
		{
			var points = new Point[4];

			double a00 = this.DirX1 * this.LenX;
			double a01 = this.DirX2 * this.LenX;
			double a02 = this.DirY1 * this.LenY;
			double a03 = this.DirY2 * this.LenY;

			double sx1 = this.PosX - a00 - a02; if (sx1 < this.PosX) sx1 = Math.Ceiling(sx1);
			double sy1 = this.PosY - a01 - a03; if (sy1 < this.PosY) sy1 = Math.Ceiling(sy1);
			double sx2 = this.PosX + a00 - a02; if (sx2 < this.PosX) sx2 = Math.Ceiling(sx2);
			double sy2 = this.PosY + a01 - a03; if (sy2 < this.PosY) sy2 = Math.Ceiling(sy2);
			double sx3 = this.PosX + a00 + a02; if (sx3 < this.PosX) sx3 = Math.Ceiling(sx3);
			double sy3 = this.PosY + a01 + a03; if (sy3 < this.PosY) sy3 = Math.Ceiling(sy3);
			double sx4 = this.PosX - a00 + a02; if (sx4 < this.PosX) sx4 = Math.Ceiling(sx4);
			double sy4 = this.PosY - a01 + a03; if (sy4 < this.PosY) sy4 = Math.Ceiling(sy4);

			if (a02 * a01 > a03 * a00)
			{
				points[0] = new Point((int)sx1, (int)sy1);
				points[1] = new Point((int)sx2, (int)sy2);
				points[2] = new Point((int)sx3, (int)sy3);
				points[3] = new Point((int)sx4, (int)sy4);
			}
			else
			{
				points[0] = new Point((int)sx1, (int)sy1);
				points[3] = new Point((int)sx2, (int)sy2);
				points[2] = new Point((int)sx3, (int)sy3);
				points[1] = new Point((int)sx4, (int)sy4);
			}

			var cosTheta = Math.Cos(radianAngle);
			var sinTheta = Math.Sin(radianAngle);

			for (int i = 0; i < points.Length; ++i)
			{
				var x = (int)(cosTheta * (points[i].X /*- pivotX*/) - sinTheta * (points[i].Y /*- pivotY*/) + pivotX);
				var y = (int)(sinTheta * (points[i].X /*- pivotX*/) + cosTheta * (points[i].Y /*- pivotY*/) + pivotY);
				points[i].X = x;
				points[i].Y = y;
			}

			return points;
		}

		public ShapeData Copy()
		{
			var result = new ShapeData();
			result.DirX1 = this.DirX1;
			result.DirX2 = this.DirX2;
			result.DirY1 = this.DirY1;
			result.DirY2 = this.DirY2;
			result.LenX = this.LenX;
			result.LenY = this.LenY;
			result.PosX = this.PosX;
			result.PosY = this.PosY;

			return result;
		}
	}

	public class EventData
	{
		public long Id { get; set; }
		public string Name { get; set; }
		public string Path { get; set; }
		public EventType Type { get; set; }
		public int RegionId { get; set; }
		public float X { get; set; }
		public float Y { get; set; }
		public bool IsAltar { get; set; }
		public List<ShapeData> Shapes { get; set; }
		public List<RegionElementData> Parameters { get; set; }

		public EventData()
		{
			this.Shapes = new List<ShapeData>();
			this.Parameters = new List<RegionElementData>();
		}

		public EventData Copy()
		{
			var result = new EventData();
			result.Id = this.Id;
			result.Name = this.Name;
			result.Type = this.Type;
			result.RegionId = this.RegionId;
			result.X = this.X;
			result.Y = this.Y;
			result.IsAltar = this.IsAltar;

			result.Shapes = new List<ShapeData>(this.Shapes.Count);
			foreach (var item in this.Shapes)
				result.Shapes.Add(item.Copy());

			result.Parameters = new List<RegionElementData>(this.Parameters.Count);
			foreach (var item in this.Parameters)
				result.Parameters.Add(item.Copy());

			return result;
		}
	}

	public class RegionElementData
	{
		public EventType EventType { get; set; }
		public SignalType SignalType { get; set; }
		public string Name { get; set; }
		public XElement XML { get; set; }

		public RegionElementData Copy()
		{
			var result = new RegionElementData();
			result.EventType = this.EventType;
			result.SignalType = this.SignalType;
			result.Name = this.Name;
			result.XML = this.XML != null ? new XElement(this.XML) : null;

			return result;
		}
	}

	public class RegionInfoDb : DatabaseDatIndexed<int, RegionInfoData>
	{
		private Random _rnd = new Random(Environment.TickCount);

		/// <summary>
		/// Returns region with given name or null if it doesn't exist.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public RegionInfoData GetRegion(string name)
		{
			return this.Entries.Values.FirstOrDefault(a => a.Name.ToLower() == name.ToLower());
		}

		/// <summary>
		/// Returns random coordinates inside the actual region.
		/// </summary>
		/// <param name="region"></param>
		/// <returns></returns>
		public Point RandomCoord(int region)
		{
			var ri = this.Find(region);
			if (ri == null)
				return new Point();

			lock (_rnd)
				return ri.RandomCoord(_rnd);
		}

		/// <summary>
		/// Returns group id for the given region.
		/// </summary>
		/// <param name="regionId"></param>
		/// <returns></returns>
		public int GetGroupId(int regionId)
		{
			var data = this.Find(regionId);
			if (data == null)
				return -1;

			return data.GroupId;
		}

		/// <summary>
		/// Loads data.
		/// </summary>
		/// <param name="br"></param>
		protected override void Read(BinaryReader br)
		{
			var cRegions = br.ReadInt32();
			for (int l = 0; l < cRegions; ++l)
			{
				var ri = new RegionInfoData();

				ri.Id = br.ReadInt32();
				ri.Name = br.ReadString();
				ri.GroupId = br.ReadInt32();
				ri.X1 = br.ReadInt32();
				ri.Y1 = br.ReadInt32();
				ri.X2 = br.ReadInt32();
				ri.Y2 = br.ReadInt32();

				var cAreas = br.ReadInt32();
				for (int i = 0; i < cAreas; ++i)
				{
					var ai = new AreaData();

					ai.Id = br.ReadInt32();
					ai.Name = br.ReadString();
					ai.X1 = br.ReadInt32();
					ai.Y1 = br.ReadInt32();
					ai.X2 = br.ReadInt32();
					ai.Y2 = br.ReadInt32();

					var cProps = br.ReadInt32();
					for (int j = 0; j < cProps; ++j)
					{
						var pi = new PropData();
						pi.EntityId = br.ReadInt64();
						pi.Id = br.ReadInt32();
						pi.Name = br.ReadString();
						pi.X = br.ReadSingle();
						pi.Y = br.ReadSingle();
						pi.Direction = br.ReadSingle();
						pi.Scale = br.ReadSingle();

						var cShapes = br.ReadInt32();
						for (int k = 0; k < cShapes; ++k)
						{
							var si = new ShapeData();
							si.DirX1 = br.ReadSingle();
							si.DirX2 = br.ReadSingle();
							si.DirY1 = br.ReadSingle();
							si.DirY2 = br.ReadSingle();
							si.LenX = br.ReadSingle();
							si.LenY = br.ReadSingle();
							si.PosX = br.ReadSingle();
							si.PosY = br.ReadSingle();

							pi.Shapes.Add(si);
						}

						var cElements = br.ReadInt32();
						for (int k = 0; k < cElements; ++k)
						{
							var red = new RegionElementData();
							red.EventType = (EventType)br.ReadInt32();
							red.SignalType = (SignalType)br.ReadInt32();
							red.Name = br.ReadString();

							var xml = br.ReadString();
							red.XML = !string.IsNullOrWhiteSpace(xml) ? XElement.Parse(xml) : null;

							pi.Parameters.Add(red);
						}

						ai.Props.Add(pi.EntityId, pi);
					}

					var cEvents = br.ReadInt32();
					for (int j = 0; j < cEvents; ++j)
					{
						var ei = new EventData();
						ei.Id = br.ReadInt64();
						ei.Name = br.ReadString();
						ei.Path = string.Format("{0}/{1}/{2}", ri.Name, ai.Name, ei.Name);
						ei.RegionId = ri.Id;
						ei.X = br.ReadSingle();
						ei.Y = br.ReadSingle();
						ei.Type = (EventType)br.ReadInt32();

						var cShapes = br.ReadInt32();
						for (int k = 0; k < cShapes; ++k)
						{
							var si = new ShapeData();
							si.DirX1 = br.ReadSingle();
							si.DirX2 = br.ReadSingle();
							si.DirY1 = br.ReadSingle();
							si.DirY2 = br.ReadSingle();
							si.LenX = br.ReadSingle();
							si.LenY = br.ReadSingle();
							si.PosX = br.ReadSingle();
							si.PosY = br.ReadSingle();

							ei.Shapes.Add(si);
						}

						var cElements = br.ReadInt32();
						for (int k = 0; k < cElements; ++k)
						{
							var red = new RegionElementData();
							red.EventType = (EventType)br.ReadInt32();
							red.SignalType = (SignalType)br.ReadInt32();
							red.Name = br.ReadString();

							var xml = br.ReadString();
							red.XML = !string.IsNullOrWhiteSpace(xml) ? XElement.Parse(xml) : null;

							if (!ei.IsAltar && red.EventType == EventType.Altar && red.SignalType == SignalType.StepOn)
								ei.IsAltar = true;

							ei.Parameters.Add(red);
						}

						ai.Events.Add(ei.Id, ei);
					}

					ri.Areas.Add(ai);
				}

				this.Entries.Add(ri.Id, ri);
			}
		}
	}
}
