// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Aura.Data.Database
{
	[Serializable]
	public class WeatherData
	{
		public int RegionId { get; set; }
		public WeatherDataType Type { get; set; }
		public string Name { get; set; }
		public float Weather { get; set; }
	}

	public enum WeatherDataType
	{
		/// <summary>
		/// Official random weather based on XML data
		/// </summary>
		Table,

		/// <summary>
		/// Official random weather based on seed
		/// </summary>
		//Random,

		/// <summary>
		/// Official constant weather
		/// </summary>
		Constant,

		/// <summary>
		/// Official constant weather with transition?
		/// </summary>
		//ConstantSmooth,
	}

	/// <summary>
	/// Weather database
	/// </summary>
	/// <remarks>
	/// Indexed by region id.
	/// </remarks>
	public class WeatherDb : DatabaseJsonIndexed<int, WeatherData>
	{
		protected override void ReadEntry(JObject entry)
		{
			entry.AssertNotMissing("regions", "type");

			var regions = new List<int>();
			foreach (var region in entry["regions"])
				regions.Add((int)region);

			var stype = entry.ReadString("type");
			var name = entry.ReadString("name");
			var weather = entry.ReadFloat("weather");
			WeatherDataType type;

			switch (stype)
			{
				case "table":
					entry.AssertNotMissing("name");
					type = WeatherDataType.Table;
					break;

				case "constant":
					entry.AssertNotMissing("weather");
					type = WeatherDataType.Constant;
					break;

				default:
					throw new ArgumentException("Unknown weather type '" + stype + "'");
			}

			foreach (var region in regions)
			{
				var data = new WeatherData();
				data.RegionId = region;
				data.Type = type;
				data.Name = name;
				data.Weather = weather;

				this.Entries[data.RegionId] = data;
			}
		}
	}
}
