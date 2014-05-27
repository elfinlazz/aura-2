// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Aura.Data.Database
{
	[Serializable]
	public class ShamalaData
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Category { get; set; }
		public byte Rank { get; set; }
		public float Rate { get; set; }
		public byte Required { get; set; }
		public float Size { get; set; }
		public uint Color1 { get; set; }
		public uint Color2 { get; set; }
		public uint Color3 { get; set; }
		public List<int> Races { get; set; }

		public ShamalaData()
		{
			this.Rank = 1;
			this.Rate = 100;
			this.Required = 1;
			this.Size = 1f;
			this.Color1 = this.Color2 = this.Color3 = 0x808080;
			this.Races = new List<int>();
		}

		/// <summary>
		/// Returns a random race id from this transformation's races list.
		/// </summary>
		public int GetRandomRace()
		{
			var rnd = new Random(Environment.TickCount);
			return this.Races[rnd.Next(Races.Count)];
		}
	}

	/// <summary>
	/// Indexed by transformation id.
	/// </summary>
	public class ShamalaDb : DatabaseCsvIndexed<int, ShamalaData>
	{
		[MinFieldCount(11)]
		protected override void ReadEntry(CsvEntry entry)
		{
			var info = new ShamalaData();
			info.Id = entry.ReadInt();
			info.Name = entry.ReadString();
			info.Category = entry.ReadString();
			info.Rank = entry.ReadByte();
			info.Rate = entry.ReadFloat();
			info.Required = entry.ReadByte();
			info.Size = entry.ReadFloat();
			info.Color1 = entry.ReadUIntHex();
			info.Color2 = entry.ReadUIntHex();
			info.Color3 = entry.ReadUIntHex();

			var races = entry.ReadStringList();
			foreach (var race in races)
				info.Races.Add(Convert.ToInt32(race));

			this.Entries[info.Id] = info;
		}
	}
}
