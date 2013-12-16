// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

namespace Aura.Data.Database
{
	public class AncientDropDb : DatabaseCSV<DropInfo>
	{
		[MinFieldCount(2)]
		protected override void ReadEntry(CSVEntry entry)
		{
			var info = new DropInfo();

			info.ItemId = entry.ReadInt(0);
			info.Chance = entry.ReadFloat(1);

			this.Entries.Add(info);
		}
	}
}
