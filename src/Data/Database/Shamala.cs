// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence file in the main folder

using System;
using System.Collections.Generic;

namespace Aura.Data.Database
{
	public class ShamalaInfo
	{
		public int Id { get; internal set; }
		public string Name { get; internal set; }
		public string Category { get; internal set; }
		public byte Rank { get; internal set; }
		public float Rate { get; internal set; }
		public byte Required { get; internal set; }
		public float Size { get; internal set; }
		public uint Color1 { get; internal set; }
		public uint Color2 { get; internal set; }
		public uint Color3 { get; internal set; }
		public List<int> Races { get; internal set; }

		public ShamalaInfo()
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
		public int Race
		{
			get
			{
				var rnd = new Random(Environment.TickCount);
				return this.Races[rnd.Next(Races.Count)];
			}
		}
	}

	/// <summary>
	/// Indexed by transformation id.
	/// </summary>
	public class ShamalaDb : DatabaseCSVIndexed<int, ShamalaInfo>
	{
		protected override void ReadEntry(CSVEntry entry)
		{
			if (entry.Count < 11)
				throw new FieldCountException(11);

			var info = new ShamalaInfo();
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

			if (this.Entries.ContainsKey(info.Id))
				throw new DatabaseWarningException("Duplicate: " + info.Id);
			this.Entries.Add(info.Id, info);
		}
	}
}
