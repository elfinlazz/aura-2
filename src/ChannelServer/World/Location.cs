// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Data;
using Aura.Data.Database;
using System;
namespace Aura.Channel.World
{
	/// <summary>
	/// Representation of a location in the world.
	/// </summary>
	/// <remarks>
	/// Used to save locations, ie a location to warp back to.
	/// </remarks>
	public struct Location
	{
		public int RegionId;
		public int X, Y;

		/// <summary>
		/// Location's position
		/// </summary>
		public Position Position { get { return new Position(X, Y); } }

		/// <summary>
		/// Creates new Location based on region id and coordinates.
		/// </summary>
		/// <param name="regionId"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public Location(int regionId, int x, int y)
		{
			this.RegionId = regionId;
			this.X = x;
			this.Y = y;
		}

		/// <summary>
		/// Creates new Location based on region id and position.
		/// </summary>
		/// <param name="regionId"></param>
		/// <param name="pos"></param>
		public Location(int regionId, Position pos)
			: this(regionId, pos.X, pos.Y)
		{
		}

		/// <summary>
		/// Creates new location based on "client path".
		/// E.g. "Ula_DgnHall_Runda_after/_Ula_DgnHall_Runda_after"
		/// </summary>
		/// <remarks>
		/// Officials use "paths" to describe locations in some cases,
		/// based on the world files. "MapName/Area/Event", etc.
		/// 
		/// The coordinates are the center of the element for regions and areas
		/// and the actual coordinates of the event for events.
		/// </remarks>
		/// <param name="location"></param>
		/// <exception cref="ArgumentException">Invalid location.</exception>
		/// <exception cref="Exception">Region, area, or event not found.</exception>
		public Location(string location)
		{
			// Check location
			if (string.IsNullOrWhiteSpace(location))
				throw new ArgumentException("Location may not be empty.");

			// Split path
			var split = location.Split('/');

			// Get region
			var region = AuraData.RegionInfoDb.GetRegion(split[0]);
			if (region == null)
				throw new Exception("Region '" + split[0] + "' not found.");

			// Get area
			AreaData area = null;
			if (split.Length > 1)
			{
				area = region.GetArea(split[1]);
				if (area == null)
					throw new Exception("Area '" + split[1] + "' not found in region '" + region.Name + "'.");
			}

			// Get event
			EventData ev = null;
			if (split.Length > 2)
			{
				ev = area.GetEvent(split[2]);
				if (ev == null)
					throw new Exception("Event '" + split[2] + "' not found in area '" + area.Name + "' of region '" + region.Name + "'.");
			}

			// Set region id
			this.RegionId = region.Id;

			// Set coordinates
			if (ev != null)
			{
				// Based on event
				this.X = (int)ev.X;
				this.Y = (int)ev.Y;
			}
			else if (area != null)
			{
				// Based on area
				this.X = (area.X1 + area.X2) / 2;
				this.Y = (area.Y1 + area.Y2) / 2;
			}
			else
			{
				// Based on region
				this.X = (region.X1 + region.X2) / 2;
				this.Y = (region.Y1 + region.Y2) / 2;
			}
		}
	}
}
