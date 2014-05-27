// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;

namespace Aura.Data.Database
{
	[Serializable]
	public class SpeedData
	{
		public string Ident { get; set; }
		public float Speed { get; set; }
	}

	/// <summary>
	/// Contains Information about walking speed of several races.
	/// This is for information purposes only, actually changing
	/// the speed would require client modifications.
	/// Indexed by group identification.
	/// </summary>
	public class SpeedDb : DatabaseCsvIndexed<string, SpeedData>
	{
		[MinFieldCount(2)]
		protected override void ReadEntry(CsvEntry entry)
		{
			var info = new SpeedData();
			info.Ident = entry.ReadString();
			info.Speed = entry.ReadFloat();

			this.Entries[info.Ident] = info;
		}
	}
}
