// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Data.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.World.Weather
{
	public interface IWeatherProvider
	{
		float GetWeatherAsFloat(DateTime dt);
		WeatherDetails GetWeather(DateTime dt);
	}
}
