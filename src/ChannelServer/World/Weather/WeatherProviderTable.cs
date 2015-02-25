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
	public class WeatherProviderTable : IWeatherProviderTable
	{
		public string Name { get; private set; }
		public int RegionId { get; private set; }
		public int GroupId { get; private set; }

		public WeatherProviderTable(int regionId, string name)
		{
			this.Name = name;
			this.RegionId = regionId;
			this.GroupId = AuraData.RegionInfoDb.GetGroupId(regionId);
		}

		public WeatherDetails GetWeather(DateTime dt)
		{
			return AuraData.WeatherTableDb.GetWeather(this.Name, dt);
		}

		public float GetWeatherAsFloat(DateTime dt)
		{
			var details = this.GetWeather(dt);
			if (details == null)
				return 0.5f;

			if (details.Type == WeatherType.Clear)
				return 0.5f;

			if (details.Type == WeatherType.Clouds)
				return 1.0f;

			return 1.95f + (0.5f / 20 * details.RainStrength);
		}
	}
}
