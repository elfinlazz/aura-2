// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Data;
using Aura.Data.Database;
using System;
using System.Text.RegularExpressions;

namespace Aura.Channel.World.Weather
{
	/// <summary>
	/// Official random weather pattern, based on data loaded from db.
	/// </summary>
	public class WeatherProviderConstant : IWeatherProviderConstant
	{
		public int RegionId { get; private set; }
		public float Weather { get; private set; }

		public WeatherProviderConstant(int regionId, float weather)
		{
			this.RegionId = regionId;
			this.Weather = weather;
		}

		public WeatherDetails GetWeather(DateTime dt)
		{
			var result = new WeatherDetails();
			var val = this.GetWeatherAsFloat(dt);

			if (val < 1.0f)
				result.Type = WeatherType.Clear;
			else if (val < 1.95f)
				result.Type = WeatherType.Clouds;
			else
			{
				result.Type = WeatherType.Rain;
				result.RainStrength = (int)((val - 1.95f) * 40);
			}

			return result;
		}

		public float GetWeatherAsFloat(DateTime dt)
		{
			return this.Weather;
		}
	}
}
