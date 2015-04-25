// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Mabi.Const;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aura.Channel.World
{
	/// <summary>
	/// Manages dynamic regions.
	/// </summary>
	public class DynamicRegionManager
	{
		private Dictionary<int, Region> _regions;

		/// <summary>
		/// Creates new dynamic region manager.
		/// </summary>
		public DynamicRegionManager()
		{
			_regions = new Dictionary<int, Region>();
		}

		/// <summary>
		/// Adds dynamic region to internal list of dynamic regions, which is
		/// used to determine which ids are available. This is done automatically
		/// when you create a new instance of DynamicRegion.
		/// </summary>
		/// <param name="dynamicRegion"></param>
		public void Add(Region dynamicRegion)
		{
			lock (_regions)
				_regions.Add(dynamicRegion.Id, dynamicRegion);
		}

		/// <summary>
		/// Removes dynamic region from internal list, which is used to find
		/// available ids, and from the world.
		/// </summary>
		/// <remarks>
		/// TODO: We should probbaly add a fail-safe for when players are still
		///   in there, here or in World.
		/// </remarks>
		/// <param name="dynamicRegionId"></param>
		public void Remove(int dynamicRegionId)
		{
			lock (_regions)
				_regions.Remove(dynamicRegionId);
		}

		/// <summary>
		/// Returns a free dynamic region id.
		/// </summary>
		/// <returns></returns>
		public int GetFreeDynamicRegionId()
		{
			lock (_regions)
			{
				for (int i = MabiId.DynamicRegions; i < ushort.MaxValue; ++i)
				{
					if (!_regions.ContainsKey(i))
						return i;
				}
			}

			throw new Exception("No dynamic region ids available.");
		}
	}
}
