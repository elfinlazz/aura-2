// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;

namespace Aura.Data.Database
{
	[Serializable]
	public class ExpData
	{
		//public uint Race;
		public int Level { get; set; }
		public int Exp { get; set; }
	}

	public class ExpDb : DatabaseCsv<ExpData>
	{
		public int MaxLevel { get; private set; }
		public long MaxExp { get; private set; }

		/// <summary>
		/// Returns total exp required to reach the level after the given one.
		/// </summary>
		/// <param name="currentLv"></param>
		/// <returns></returns>
		public int GetTotalForNextLevel(int currentLv)
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
		public int GetForLevel(int currentLv)
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

		protected override void ReadEntry(CsvEntry entry)
		{
			// Replace previous values if there is more than 1 line.
			this.Entries = new List<ExpData>(entry.Count);

			while (!entry.End)
			{
				var info = new ExpData();
				info.Level = (entry.Pointer + 1);
				info.Exp = entry.ReadInt();

				this.Entries.Add(info);
			}
		}

		protected override void AfterLoad()
		{
			this.MaxLevel = this.Entries.Count;
			this.MaxExp = this.GetTotalForNextLevel(this.MaxLevel);
		}
	}
}
