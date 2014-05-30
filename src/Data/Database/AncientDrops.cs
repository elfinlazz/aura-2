// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

namespace Aura.Data.Database
{
	public class AncientDropDb : DatabaseCsv<DropData>
	{
		[MinFieldCount(2)]
		protected override void ReadEntry(CsvEntry entry)
		{
			var info = new DropData();

			info.ItemId = entry.ReadInt(0);
			info.Chance = entry.ReadFloat(1);

			info.Chance /= 100;
			if (info.Chance > 1)
				info.Chance = 1;
			else if (info.Chance < 0)
				info.Chance = 0;

			this.Entries.Add(info);
		}
	}
}
