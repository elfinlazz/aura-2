// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

namespace Aura.Data.Database
{
	public class MotionData
	{
		public string Name { get; internal set; }
		public short Category { get; internal set; }
		public short Type { get; internal set; }
		public bool Loop { get; internal set; }
	}

	/// <summary>
	/// Indexed by motion name.
	/// </summary>
	public class MotionDb : DatabaseCSVIndexed<string, MotionData>
	{
		[MinFieldCount(4)]
		protected override void ReadEntry(CSVEntry entry)
		{
			var info = new MotionData();
			info.Name = entry.ReadString();
			info.Category = entry.ReadShort();
			info.Type = entry.ReadShort();
			info.Loop = entry.ReadBool();

			this.Entries.Add(info.Name, info);
		}
	}
}
