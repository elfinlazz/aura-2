// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.World.Entities;
using Aura.Data;
using Aura.Data.Database;
using Aura.Shared.Mabi;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.World.Weather
{
	/// <summary>
	/// Manages weather, updating clients and supplying weather information.
	/// </summary>
	public class WeatherManager
	{
		private Dictionary<int, IWeatherProvider> _providers;

		/// <summary>
		/// Raised every 20 minutes for every region,
		/// when the weather (might) change.
		/// </summary>
		public event Action<int, WeatherDetails> WeatherChange;

		/// <summary>
		/// Creates new weather manager.
		/// </summary>
		public WeatherManager()
		{
			_providers = new Dictionary<int, IWeatherProvider>();
		}

		/// <summary>
		/// Called when creature enters region, sends weather update.
		/// </summary>
		/// <param name="creature"></param>
		private void OnPlayerEntersRegion(Creature creature)
		{
			this.Update(creature);
		}

		/// <summary>
		/// Called once a minute, used for custom weather and the change weather event.
		/// </summary>
		/// <param name="time"></param>
		private void OnMinutesTimeTick(ErinnTime time)
		{
			if (time.DateTime.Minute % 20 == 0)
			{
				var ev = this.WeatherChange;
				if (ev != null)
				{
					foreach (var provider in _providers)
						ev(provider.Key, provider.Value.GetWeather(time.DateTime));
				}
			}

			// TODO: Custom weather?
		}

		/// <summary>
		/// Sets up event subscriptions and loads weather from db.
		/// </summary>
		public void Initialize()
		{
			ChannelServer.Instance.Events.MinutesTimeTick += this.OnMinutesTimeTick;
			ChannelServer.Instance.Events.PlayerEntersRegion += this.OnPlayerEntersRegion;

			foreach (var data in AuraData.WeatherDb.Entries.Values)
			{
				switch (data.Type)
				{
					case WeatherDataType.Table:
						_providers[data.RegionId] = new WeatherProviderTable(data.RegionId, data.Name);
						break;

					case WeatherDataType.Constant:
						_providers[data.RegionId] = new WeatherProviderConstant(data.RegionId, data.Weather);
						break;

					default:
						Log.Unimplemented("Weather type '{0}'.", data.Type);
						break;
				}
			}
		}

		/// <summary>
		/// Updates creature's client with current weather for its region.
		/// </summary>
		/// <param name="creature"></param>
		public void Update(Creature creature)
		{
			var provider = this.GetProvider(creature.RegionId);
			if (provider == null)
			{
				var groupId = AuraData.RegionInfoDb.GetGroupId(creature.RegionId);
				Send.Weather(creature, creature.RegionId, groupId);
				return;
			}

			Send.Weather(creature, provider);
		}

		/// <summary>
		/// Updates clients in region with current weather.
		/// </summary>
		/// <param name="regionId"></param>
		public void Update(int regionId)
		{
			var provider = this.GetProvider(regionId);
			if (provider == null)
				return;

			var region = ChannelServer.Instance.World.GetRegion(regionId);
			if (region == null)
				return;

			Send.Weather(region, provider);
		}

		/// <summary>
		/// Returns the provider for the region.
		/// </summary>
		/// <param name="regionId"></param>
		/// <returns></returns>
		private IWeatherProvider GetProvider(int regionId)
		{
			if (!_providers.ContainsKey(regionId))
				return null;

			return _providers[regionId];
		}

		/// <summary>
		/// Sets provider for region and updates clients in it.
		/// </summary>
		/// <param name="regionId"></param>
		/// <param name="provider"></param>
		public void SetProviderAndUpdate(int regionId, IWeatherProvider provider)
		{
			_providers[regionId] = provider;
			this.Update(provider.RegionId);
		}

		/// <summary>
		/// Returns the weather information for the region.
		/// </summary>
		/// <param name="regionId"></param>
		/// <returns></returns>
		public WeatherDetails GetWeather(int regionId)
		{
			var provider = this.GetProvider(regionId);
			if (provider == null)
				return null;

			return provider.GetWeather(DateTime.Now);
		}

		/// <summary>
		/// Returns the current weather type for the region.
		/// </summary>
		/// <param name="regionId"></param>
		/// <returns></returns>
		public WeatherType GetWeatherType(int regionId)
		{
			var provider = this.GetProvider(regionId);
			if (provider == null)
				return WeatherType.Clear;

			var weather = provider.GetWeather(DateTime.Now);
			return weather.Type;
		}

		/// <summary>
		/// Returns the current rain strength for the region.
		/// </summary>
		/// <remarks>
		/// Return value ranges from 0 (light) to 20 (thunder),
		/// returns 0 even if it's not raining or there's no weather defined.
		/// </remarks>
		/// <param name="regionId"></param>
		/// <returns></returns>
		public int GetRainStrength(int regionId)
		{
			var provider = this.GetProvider(regionId);
			if (provider == null)
				return 0;

			var weather = provider.GetWeather(DateTime.Now);
			return weather.RainStrength;
		}

		/// <summary>
		/// Returns the current weather as float, as used in some weather packets.
		/// </summary>
		/// <remarks>
		/// Value ranges from 0.0 to 2.0, 0.5 is the default, clouds start
		/// at 1.5, rain starts at 1.95, and 2.0 is thunder. Values above that
		/// don't do much.
		/// </remarks>
		/// <param name="regionId"></param>
		/// <returns></returns>
		public float GetWeatherAsFloat(int regionId)
		{
			var provider = this.GetProvider(regionId);
			if (provider == null)
				return 0.5f;

			return provider.GetWeatherAsFloat(DateTime.Now);
		}
	}
}
