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

			// TODO: Custom weather.
		}

		/// <summary>
		/// Sets up event subscriptions and loads weather from db.
		/// </summary>
		public void Initialize()
		{
			ChannelServer.Instance.Events.MinutesTimeTick += this.OnMinutesTimeTick;
			ChannelServer.Instance.Events.PlayerEntersRegion += this.OnPlayerEntersRegion;

			foreach (var data in AuraData.WeatherDb.Entries.Values)
				_providers[data.RegionId] = new WeatherProviderTable(data.Name);
		}

		/// <summary>
		/// Updates creature's client with current weather for its region.
		/// </summary>
		/// <param name="creature"></param>
		public void Update(Creature creature)
		{
			var provider = this.GetProvider(creature.RegionId);
			if (provider == null)
				return;

			var regionId = creature.RegionId;
			var groupId = AuraData.RegionInfoDb.GetGroupId(regionId);

			var official = provider as WeatherProviderTable;
			if (official != null)
				Send.Weather(creature, regionId, groupId, official.Name);
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
	}
}
