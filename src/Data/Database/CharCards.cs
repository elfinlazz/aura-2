// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Collections.Generic;

namespace Aura.Data.Database
{
	public class CharCardInfo
	{
		public int Id { get; internal set; }
		public string Name { get; internal set; }
		public int SetId { get; internal set; }
		public List<int> Races { get; internal set; }

		public CharCardInfo()
		{
			this.Races = new List<int>();
		}

		public bool Enabled(int race)
		{
			return this.Races.Contains(race);
		}
	}

	/// <summary>
	/// Indexed by char card id.
	/// </summary>
	public class CharCardDb : DatabaseCSVIndexed<int, CharCardInfo>
	{
		protected override void ReadEntry(CSVEntry entry)
		{
			if (entry.Count < 3)
				throw new FieldCountException(3);

			var info = new CharCardInfo();
			info.Id = entry.ReadInt();
			info.SetId = entry.ReadInt();

			var races = entry.ReadUIntHex();
			if ((races & 0x01) != 0) info.Races.Add(10001);
			if ((races & 0x02) != 0) info.Races.Add(10002);
			if ((races & 0x04) != 0) info.Races.Add(9001);
			if ((races & 0x08) != 0) info.Races.Add(9002);
			if ((races & 0x10) != 0) info.Races.Add(8001);
			if ((races & 0x20) != 0) info.Races.Add(8002);

			this.Entries.Add(info.Id, info);
		}
	}
}
