// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.IO;

namespace Aura.Data.Database
{
	/// <summary>
	/// Holds information about the weather for the next years.
	/// </summary>
	public class WeatherDataDb : DatabaseDatIndexed<int, Dictionary<string, sbyte[]>>
	{
		public WeatherDetails GetWeather(int table, DateTime dt)
		{
			var sDate = dt.ToString("yyyy-MM-dd");

			if (!this.Entries.ContainsKey(table) || !this.Entries[table].ContainsKey(sDate))
				return null;

			var values = this.Entries[table][sDate];
			var value = values[dt.Hour * 3 + dt.Minute / 20];

			return new WeatherDetails() { Type = this.IntToWeatherType(value), RainStrength = Math.Max(0, (int)value) };
		}

		public WeatherType GetWeatherType(int table, DateTime dt)
		{
			var details = this.GetWeather(table, dt);
			if (details == null)
				details = null;

			return details.Type;
		}

		public int GetRainStrength(int table, DateTime dt)
		{
			var details = this.GetWeather(table, dt);
			if (details == null)
				details = null;

			return details.RainStrength;
		}

		private WeatherType IntToWeatherType(int value)
		{
			if (value == -2)
				return WeatherType.Clear;

			if (value == -1)
				return WeatherType.Clouds;

			return WeatherType.Rain;
		}

		protected override void Read(BinaryReader br)
		{
			var table = br.ReadSByte();

			var count = br.ReadInt32();
			for (int j = 0; j < count; ++j)
			{
				var date = br.ReadString();

				var values = new sbyte[72];
				for (int i = 0; i < 72; ++i)
					values[i] = br.ReadSByte();

				if (!this.Entries.ContainsKey(table))
					this.Entries[table] = new Dictionary<string, sbyte[]>();

				this.Entries[table][date] = values;
			}
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
