// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Linq;

namespace Aura.Data.Database
{
	public class MapData
	{
		public int Id { get; internal set; }
		public string Name { get; internal set; }
	}

	/// <summary>
	/// Indexed by map name.
	/// </summary>
	public class RegionDb : DatabaseCSVIndexed<string, MapData>
	{
		public MapData Find(uint id)
		{
			return this.Entries.FirstOrDefault(a => a.Value.Id == id).Value;
		}

		public int TryGetRegionId(string region, int fallBack = 0)
		{
			int regionId = fallBack;
			if (!int.TryParse(region, out regionId))
			{
				var mapInfo = this.Find(region);
				if (mapInfo != null)
					regionId = mapInfo.Id;
			}

			return regionId;
		}

		[MinFieldCount(2)]
		protected override void ReadEntry(CSVEntry entry)
		{
			var info = new MapData();
			info.Id = entry.ReadInt();
			info.Name = entry.ReadString();

			var exists = this.Entries.ContainsKey(info.Name);
			this.Entries[info.Name] = info;
		}
	}
}
