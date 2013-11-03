// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Collections.Generic;

namespace Aura.Data.Database
{
	public class ExpInfo
	{
		//public uint Race;
		public short Level { get; internal set; }
		public int Exp { get; internal set; }
	}

	public class ExpDb : DatabaseCSV<ExpInfo>
	{
		public short MaxLevel { get; private set; }
		public long MaxExp { get; private set; }

		/// <summary>
		/// Returns total exp required to reach the level after the given one.
		/// </summary>
		/// <param name="currentLv"></param>
		/// <returns></returns>
		public int GetTotalForNextLevel(short currentLv)
		{
			var result = 0;

			if (currentLv >= this.Entries.Count)
				currentLv = (short)this.Entries.Count;

			for (ushort i = 0; i < currentLv; ++i)
			{
				result += this.Entries[i].Exp;
			}

			return result;
		}

		/// <summary>
		/// Returns the exp required for the next level.
		/// </summary>
		/// <param name="currentLv"></param>
		/// <returns></returns>
		public int GetForLevel(short currentLv)
		{
			if (currentLv < 1)
				return 0;
			if (currentLv > this.MaxLevel)
				currentLv = (short)this.Entries.Count;

			currentLv -= 1;
			return this.Entries[currentLv].Exp;
		}

		/// <summary>
		/// Calculates exp remaining till the next level. Required for stat update.
		/// </summary>
		/// <param name="currentLv"></param>
		/// <param name="totalExp"></param>
		/// <returns></returns>
		public long CalculateRemaining(short currentLv, long totalExp)
		{
			return this.GetForLevel(currentLv) - (this.GetTotalForNextLevel(currentLv) - totalExp) + this.GetForLevel((short)(currentLv - 1));
		}

		protected override void ReadEntry(CSVEntry entry)
		{
			// Replace previous values if there is more than 1 line.
			this.Entries = new List<ExpInfo>(entry.Count);

			while (!entry.End)
			{
				var info = new ExpInfo();
				info.Level = (short)(entry.Pointer + 1);
				info.Exp = entry.ReadInt();

				this.Entries.Add(info);
			}

			this.MaxLevel = (short)this.Entries.Count;
			this.MaxExp = this.GetTotalForNextLevel(this.MaxLevel);
		}
	}
}
