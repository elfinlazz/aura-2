// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Linq;

namespace Aura.Data.Database
{
	[Serializable]
	public class RegionData
	{
		public int Id { get; set; }
		public string Name { get; set; }
	}

	/// <summary>
	/// Indexed by region id.
	/// </summary>
	public class RegionDb : DatabaseCsvIndexed<int, RegionData>
	{
		public RegionData Find(uint id)
		{
			return this.Entries.FirstOrDefault(a => a.Value.Id == id).Value;
		}

		public bool TryGetRegionName(int regionId, out string name)
		{
			name = null;

			if (!this.Entries.ContainsKey(regionId))
				return false;

			name = this.Entries[regionId].Name;

			return true;
		}

		[MinFieldCount(2)]
		protected override void ReadEntry(CsvEntry entry)
		{
			var info = new RegionData();
			info.Id = entry.ReadInt();
			info.Name = entry.ReadString();

			this.Entries[info.Id] = info;
		}
	}
}
