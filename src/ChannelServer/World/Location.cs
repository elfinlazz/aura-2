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
		/// New Location based on location id (i.e. 0x3000RRRRXXXXYYYY).
		/// </summary>
		/// <param name="locationId"></param>
		public Location(long locationId)
		{
			this.RegionId = (ushort)(locationId >> 32);
			this.X = (ushort)(locationId >> 16) * 20;
			this.Y = (ushort)(locationId >> 00) * 20;
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
			var region = ChannelServer.Instance.World.GetRegion(split[0]);
			if (region == null)
				throw new Exception("Region '" + split[0] + "' not found.");

			var regionData = region.RegionInfoData;
			if (region == null)
				throw new Exception("Region '" + region.Id + "' doesn't have data.");

			// Get area
			AreaData areaData = null;
			if (split.Length > 1)
			{
				areaData = regionData.GetArea(split[1]);
				if (areaData == null)
					throw new Exception("Area '" + split[1] + "' not found in region '" + regionData.Name + "'.");
			}

			// Get event
			EventData eventData = null;
			if (split.Length > 2)
			{
				eventData = areaData.GetEvent(split[2]);
				if (eventData == null)
					throw new Exception("Event '" + split[2] + "' not found in area '" + areaData.Name + "' of region '" + regionData.Name + "'.");
			}

			// Set region id
			this.RegionId = regionData.Id;

			// Set coordinates
			if (eventData != null)
			{
				// Based on event
				this.X = (int)eventData.X;
				this.Y = (int)eventData.Y;
			}
			else if (areaData != null)
			{
				// Based on area
				this.X = (areaData.X1 + areaData.X2) / 2;
				this.Y = (areaData.Y1 + areaData.Y2) / 2;
			}
			else
			{
				// Based on region
				this.X = (regionData.X1 + regionData.X2) / 2;
				this.Y = (regionData.Y1 + regionData.Y2) / 2;
			}
		}

		/// <summary>
		/// Returns location id (i.e. 0x3000RRRRXXXXYYYY).
		/// </summary>
		public long ToLocationId()
		{
			var result = 0x3000000000000000;

			result |= (long)(0xFFFF & this.RegionId) << 32;
			result |= (long)(0xFFFF & this.X / 20) << 16;
			result |= (long)(0xFFFF & this.Y / 20) << 0;

			return result;
		}

		/// <summary>
		/// Returns a string representing this Location.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return string.Format("(RegionId: {0}, X: {1}, Y: {2})", this.RegionId, this.X, this.Y);
		}
	}
}
