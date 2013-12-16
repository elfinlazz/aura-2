// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;

namespace Aura.Data.Database
{
	public class WeatherData
	{
		public int Region { get; internal set; }
		public WeatherInfoType Type { get; internal set; }
		public List<float> Values { get; internal set; }
	}

	/// <summary>
	/// Indexed by region.
	/// </summary>
	public class WeatherDb : DatabaseCSVIndexed<int, WeatherData>
	{
		protected override void ReadEntry(CSVEntry entry)
		{
			if (entry.Count < 3)
				throw new FieldCountException(3);

			// Read everything first, we might need it for multiple regions.
			var regions = entry.ReadStringList();
			var type = (WeatherInfoType)entry.ReadByte();
			var values = new List<float>();
			while (!entry.End)
				values.Add(entry.ReadFloat());

			// Every type has at least 1 value.
			if (values.Count < 1)
				throw new DatabaseWarningException("Too few values.");

			foreach (var region in regions)
			{
				var info = new WeatherData();
				info.Region = Convert.ToInt32(region);
				info.Type = type;
				info.Values = values;

				this.Entries[info.Region] = info;
			}
		}
	}

	public enum WeatherInfoType { Official, Custom, Pattern, OWM }
}
