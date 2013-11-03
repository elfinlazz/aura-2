// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Linq;

namespace Aura.Data.Database
{
	public class MapInfo
	{
		public int Id { get; internal set; }
		public string Name { get; internal set; }
	}

	/// <summary>
	/// Indexed by map name.
	/// </summary>
	public class RegionDb : DatabaseCSVIndexed<string, MapInfo>
	{
		public MapInfo Find(uint id)
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

		protected override void ReadEntry(CSVEntry entry)
		{
			if (entry.Count < 2)
				throw new FieldCountException(2);

			var info = new MapInfo();
			info.Id = entry.ReadInt();
			info.Name = entry.ReadString();

			var exists = this.Entries.ContainsKey(info.Name);
			this.Entries[info.Name] = info;

			if (exists)
				throw new DatabaseWarningException("Duplicate '{0}', replacing.", info.Name);
		}
	}
}
