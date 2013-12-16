// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

namespace Aura.Data.Database
{
	public class SpeedData
	{
		public string Ident { get; internal set; }
		public float Speed { get; internal set; }
	}

	/// <summary>
	/// Contains Information about walking speed of several races.
	/// This is for information purposes only, actually changing
	/// the speed would require client modifications.
	/// Indexed by group identification.
	/// </summary>
	public class SpeedDb : DatabaseCSVIndexed<string, SpeedData>
	{
		[MinFieldCount(2)]
		protected override void ReadEntry(CSVEntry entry)
		{
			var info = new SpeedData();
			info.Ident = entry.ReadString();
			info.Speed = entry.ReadFloat();

			this.Entries[info.Ident] = info;
		}
	}
}
