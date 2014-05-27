// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;

namespace Aura.Data.Database
{
	[Serializable]
	public class MotionData
	{
		public string Name { get; set; }
		public short Category { get; set; }
		public short Type { get; set; }
		public bool Loop { get; set; }
	}

	/// <summary>
	/// Indexed by motion name.
	/// </summary>
	public class MotionDb : DatabaseCsvIndexed<string, MotionData>
	{
		[MinFieldCount(4)]
		protected override void ReadEntry(CsvEntry entry)
		{
			var info = new MotionData();
			info.Name = entry.ReadString();
			info.Category = entry.ReadShort();
			info.Type = entry.ReadShort();
			info.Loop = entry.ReadBool();

			this.Entries[info.Name] = info;
		}
	}
}
