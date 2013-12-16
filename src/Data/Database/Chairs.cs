// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

namespace Aura.Data.Database
{
	public class ChairData
	{
		public int ItemId { get; internal set; }
		public int PropId { get; internal set; }
		public int GiantPropId { get; internal set; }
		public int Effect { get; internal set; }
	}

	/// <summary>
	/// Indexed by item id.
	/// </summary>
	public class ChairDb : DatabaseCSVIndexed<int, ChairData>
	{
		[MinFieldCount(5)]
		protected override void ReadEntry(CSVEntry entry)
		{
			var info = new ChairData();
			info.ItemId = entry.ReadInt();
			entry.ReadString();
			info.PropId = entry.ReadInt();
			info.GiantPropId = entry.ReadInt();
			info.Effect = entry.ReadInt();

			this.Entries.Add(info.ItemId, info);
		}
	}
}
