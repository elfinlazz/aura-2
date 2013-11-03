// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Collections.Generic;

namespace Aura.Data.Database
{
	public class RaceSkillInfo
	{
		public int MonsterId { get; internal set; }
		public short SkillId { get; internal set; }
		public byte Rank { get; internal set; }
	}

	public class RaceSkillDb : DatabaseCSV<RaceSkillInfo>
	{
		public List<RaceSkillInfo> FindAll(int id)
		{
			return this.Entries.FindAll(a => a.MonsterId == id);
		}

		protected override void ReadEntry(CSVEntry entry)
		{
			if (entry.Count < 3)
				throw new FieldCountException(3);

			var info = new RaceSkillInfo();
			info.MonsterId = entry.ReadInt();
			info.SkillId = entry.ReadShort();
			info.Rank = (byte)(16 - entry.ReadByteHex());

			this.Entries.Add(info);
		}
	}
}
