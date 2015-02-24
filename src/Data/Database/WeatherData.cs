// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Aura.Data.Database
{

	[Serializable]
	public class WeatherTableData
	{
		public long BaseTime { get; set; }
		public List<float> Values { get; set; }

		public WeatherTableData()
		{
			this.Values = new List<float>();
		}
	}

	/// <summary>
	/// Holds information about the weather.
	/// </summary>
	public class WeatherDataDb : DatabaseJsonIndexed<int, WeatherTableData>
	{
		protected override void ReadEntry(JObject entry)
		{
			var data = new WeatherTableData();
			
			DateTime dt;
			DateTime.TryParseExact(entry.ReadString("base_time"), "yyyyMMddhhmm", null, System.Globalization.DateTimeStyles.AssumeLocal, out dt);
			data.BaseTime = dt.Ticks;

			var unitTime = entry.ReadUInt("unit_time"); // number of hours
			var count = (int)(3600000 * unitTime / 1200000); // number of 20 min periods
			
			var seed = entry.ReadUInt("seed");
			var rnd = new CRandom(seed);

			foreach (var cols in entry["rows"].Select(row => row.ToObject<float[]>()))
				for (var i = 0; i < count; ++i)
					data.Values.Add(this.ComputeWeather(cols, rnd));

			var id = entry.ReadUShort("id");
			this.Entries[id-1] = data;
		}

		private float ComputeWeather(float[] cols, CRandom rnd)
		{
			var result = 2.0f;
			var addedCols = new float[4];

			addedCols[0] = cols[0];
			for (var i = 1; i < 4; ++i)
				addedCols[i] += addedCols[i - 1] + cols[i];

			var randFloat = rnd.RandomF32(0.0f, addedCols[3]);
			if (addedCols[1] >= randFloat)
			{
				if (addedCols[0] >= randFloat)
				{
					if (cols[0] <= 0.0)
						result = 0.5f;
					else
						result = randFloat / cols[0];
				}
				else
					result = (randFloat - addedCols[0]) * 0.95f / cols[1] + 1.0f;
			}
			else
				result = (randFloat - addedCols[1]) * 0.05f / cols[2] + 1.95f;

			return result;
		}

		public float GetWeatherAsFloat(int table, DateTime dt)
		{
			var entry = this.Entries[table];
			var index = (dt.Ticks - entry.BaseTime) / TimeSpan.TicksPerSecond;
			index = index / (60 * 20) + 1;
			index %= entry.Values.Count;
			return entry.Values[(int)index];
		}

		public WeatherDetails GetWeather(int table, DateTime dt)
		{
			var details = new WeatherDetails { Type = WeatherType.Clear, RainStrength = 0 };

			var weather = this.GetWeatherAsFloat(table, dt);
			details.Type = this.FloatToWeatherType(weather);
			if (details.Type == WeatherType.Rain)
			{
				var amount = (weather - 1.949900031089783d) * 20.0d;
				if (amount < 0.0d)
					amount = 0.0d;
				else if (amount > 1.0d)
					amount = 1.0d;
				details.RainStrength = (int)(amount / 0.05d);
			}
			return details;
		}

		public WeatherType GetWeatherType(int table, DateTime dt)
		{
			var details = this.GetWeather(table, dt);
			return details.Type;
		}

		public int GetRainStrength(int table, DateTime dt)
		{
			var details = this.GetWeather(table, dt);
			return details.RainStrength;
		}

		private WeatherType FloatToWeatherType(float value)
		{
			if (value < 1.0f)
				return WeatherType.Clear;
			if (value < 1.949900031089783d)
				return WeatherType.Clouds;
			return WeatherType.Rain;
		}
	}

	public class WeatherDetails
	{
		public WeatherType Type { get; set; }
		public int RainStrength { get; set; }

		public override string ToString()
		{
			var result = this.Type.ToString();
			if (this.Type == WeatherType.Rain)
				result += " " + this.RainStrength;

			return result;
		}
	}

	public enum WeatherType
	{
		Clear,
		Clouds,
		Rain,
	}
}
