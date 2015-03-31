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
		private Dictionary<int, DynamicRegion> _regions;

		/// <summary>
		/// Creates new dynamic region manager.
		/// </summary>
		public DynamicRegionManager()
		{
			_regions = new Dictionary<int, DynamicRegion>();
		}

		/// <summary>
		/// Adds dynamic region to internal list of dynamic regions, which is
		/// used to determine which ids are available. This is done automatically
		/// when you create a new instance of DynamicRegion.
		/// </summary>
		/// <param name="dynamicRegion"></param>
		public void Add(DynamicRegion dynamicRegion)
		{
			lock (_regions)
				_regions.Add(dynamicRegion.BaseId, dynamicRegion);
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
