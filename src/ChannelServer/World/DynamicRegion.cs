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
	}
}
