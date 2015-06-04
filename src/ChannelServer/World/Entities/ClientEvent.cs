// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Data.Database;
using Aura.Mabi.Const;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.World.Entities
{
	/// <summary>
	/// A client in an area of a region.
	/// </summary>
	/// <remarks>
	/// Is this considered to be an entity by the client?
	/// </remarks>
	public class ClientEvent : IShapedEntity
	{
		/// <summary>
		/// Handler that is called if this event is triggered
		/// </summary>
		public Collection<SignalType, Action<Creature, EventData>> Handlers { get; private set; }

		/// <summary>
		/// Event's id
		/// </summary>
		public long EntityId { get; private set; }

		/// <summary>
		/// Data for this event
		/// </summary>
		public EventData Data { get; private set; }

		/// <summary>
		/// Shapes of this event.
		/// </summary>
		public List<Point[]> Shapes { get; private set; }

		/// <summary>
		/// Specifies whether other entities collide with this one's shape.
		/// </summary>
		public bool IsCollision { get { return this.Data.Type == EventType.Collision; } }

		/// <summary>
		/// Creates new client event
		/// </summary>
		/// <param name="eventData"></param>
		public ClientEvent(long id, EventData eventData)
		{
			this.Shapes = new List<Point[]>();

			this.EntityId = id;
			this.Data = eventData;

			this.Handlers = new Collection<SignalType, Action<Creature, EventData>>();

			foreach (var shape in this.Data.Shapes)
				this.Shapes.Add(shape.GetPoints(0, 0, 0));
		}

		public bool IsInside(int x, int y)
		{
			if (this.Shapes.Count == 0)
				return false;

			var result = false;
			var point = new Point(x, y);

			foreach (var points in this.Shapes)
			{
				for (int i = 0, j = points.Length - 1; i < points.Length; j = i++)
				{
					if (((points[i].Y > point.Y) != (points[j].Y > point.Y)) && (point.X < (points[j].X - points[i].X) * (point.Y - points[i].Y) / (points[j].Y - points[i].Y) + points[i].X))
						result = !result;
				}

				if (result)
					return true;
			}

			return false;
		}
	}
}
