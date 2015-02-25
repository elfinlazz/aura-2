// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

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
		Unknown,
	}

	/// <summary>
	/// Holds information about the weather.
	/// </summary>
	public class WeatherTableDb : DatabaseJsonIndexed<string, WeatherTableData>
	{
		protected override void ReadEntry(JObject entry)
		{
			var name = entry.ReadString("name");
			var unitTime = entry.ReadUInt("unitTime"); // number of hours
			var seed = entry.ReadUInt("seed");
			var baseTime = entry.ReadString("baseTime");

			var data = new WeatherTableData();

			DateTime dt;
			DateTime.TryParseExact(baseTime, "yyyyMMddhhmm", null, DateTimeStyles.AssumeLocal, out dt);
			data.BaseTime = dt.Ticks;

			var rnd = new CRandom(seed);
			var count = (int)(3600000 * unitTime / 1200000); // number of 20 min periods

			foreach (var cols in entry["rows"].Select(row => row.ToObject<float[]>()))
				for (var i = 0; i < count; ++i)
					data.Values.Add(this.ComputeWeather(cols, rnd));

			this.Entries[name] = data;
		}

		private float ComputeWeather(float[] cols, CRandom rnd)
		{
			var result = 2.0f;
			var addedCols = new float[4];

			addedCols[0] = cols[0];
			for (var i = 1; i < 4; ++i)
				addedCols[i] += addedCols[i - 1] + cols[i];

			float randFloat;
			try
			{
				randFloat = rnd.RandomF32(0.0f, addedCols[3]);
			}
			catch (CRandomException)
			{
				return -1.0f;
			}

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

		public float GetWeatherAsFloat(string tableName, DateTime dt)
		{
			var entry = this.Entries[tableName];
			var index = (dt.Ticks - entry.BaseTime) / TimeSpan.TicksPerSecond;
			index = index / (60 * 20) + 1;
			index %= entry.Values.Count;

			return entry.Values[(int)index];
		}

		public WeatherDetails GetWeather(string tableName, DateTime dt)
		{
			var details = new WeatherDetails { Type = WeatherType.Clear, RainStrength = 0 };

			var weather = this.GetWeatherAsFloat(tableName, dt);
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

		public WeatherType GetWeatherType(string tableName, DateTime dt)
		{
			var details = this.GetWeather(tableName, dt);
			return details.Type;
		}

		public int GetRainStrength(string tableName, DateTime dt)
		{
			var details = this.GetWeather(tableName, dt);
			return details.RainStrength;
		}

		private WeatherType FloatToWeatherType(float value)
		{
			if (value < 0.0f)
				return WeatherType.Unknown;

			if (value < 1.0f)
				return WeatherType.Clear;

			if (value < 1.949900031089783d)
				return WeatherType.Clouds;

			return WeatherType.Rain;
		}
	}
}
