// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.World
{
	public class DynamicRegion : Region
	{
		public int BaseId { get; protected set; }
		public string Variant { get; protected set; }

		/// <summary>
		/// Creates new dynamic region, based on regionId and variant.
		/// Region is automatically added to the dynamic region manager.
		/// </summary>
		/// <param name="regionId"></param>
		/// <param name="variant"></param>
		public DynamicRegion(int regionId, string variant = null)
			: base(regionId)
		{
			// Replace id with dynamic id, the original id is only needed for
			// initialization.
			this.Id = ChannelServer.Instance.World.DynamicRegions.GetFreeDynamicRegionId();

			this.BaseId = regionId;
			this.Variant = variant;

			ChannelServer.Instance.World.DynamicRegions.Add(this);
		}

		/// <summary>
		/// Creates list of areas.
		/// </summary>
		/// <remarks>
		/// Areas in the client files aren't in order, it can be id 1, 2, 3,
		/// but it can just as well be 2, 1, 3. When the client creates a dynamic
		/// region it changes the ids to be order, so 2, 1, 3 would become
		/// 1, 2, 3 in the dynamic version. We have to mimic this to get the
		/// correct ids for the props. Genius system, thanks, devCAT.
		/// </remarks>
		protected override void LoadAreas()
		{
			base.LoadAreas();

			var id = 1;

			lock (_areas)
				foreach (var area in _areas)
					area.Id = id++;
		}
	}
}
