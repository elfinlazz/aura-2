// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.World
{
	public class NormalRegion : Region
	{
		public NormalRegion(int regionId)
			: base(regionId)
		{
			this.RegionInfoData = AuraData.RegionInfoDb.Find(this.Id);
			if (this.RegionInfoData == null)
				throw new Exception("Region.CreateNormal: No region info data found for '" + this.Id + "'.");

			this.Name = this.RegionInfoData.Name;

			this.InitializeFromData();
		}
	}
}
