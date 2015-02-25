// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Collections.Generic;
using System.Linq;
using Aura.Channel.World.Entities;
using Aura.Shared.Mabi.Const;
using Aura.Shared.Network;
using Aura.Shared.Util;
using Aura.Channel.Network.Sending.Helpers;
using System;
using Aura.Channel.World;
using Aura.Channel.World.Weather;

namespace Aura.Channel.Network.Sending
{
	public static partial class Send
	{
		/// <summary>
		/// Sends Weather to creature's client.
		/// </summary>
		public static void Weather(Creature creature, IWeatherProvider provider)
		{
			Send.Weather(creature.Client.Send, provider);
		}

		/// <summary>
		/// Sends empty Weather packet to creature's client.
		/// </summary>
		public static void Weather(Creature creature, int regionId, int groupId)
		{
			var packet = new Packet(Op.Weather, MabiId.Broadcast);
			packet.PutByte(0);
			packet.PutInt(regionId);
			packet.PutByte(0);
			packet.PutInt(groupId);
			packet.PutByte(0);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Broadcasts Weather in region.
		/// </summary>
		public static void Weather(Region region, IWeatherProvider provider)
		{
			Send.Weather(region.Broadcast, provider);
		}

		/// <summary>
		/// Sends Weather.
		/// </summary>
		private static void Weather(Action<Packet> sender, IWeatherProvider provider)
		{
			var packet = new Packet(Op.Weather, MabiId.Broadcast);
			packet.PutByte(0);
			packet.PutInt(provider.RegionId);

			if (provider is IWeatherProviderTable)
			{
				var table = (IWeatherProviderTable)provider;

				packet.PutByte(0);
				packet.PutInt(table.GroupId);
				packet.PutByte(2);
				packet.PutByte(1);
				packet.PutString("table");
				packet.PutString(table.Name);
				packet.PutLong(0);
				packet.PutByte(0);
			}
			else if (provider is IWeatherProviderConstant)
			{
				var constant = (IWeatherProviderConstant)provider;

				// Packet structure is guessed, even though it works,
				// based on constant_smooth.
				packet.PutByte(2);
				packet.PutByte(0);
				packet.PutByte(1);
				packet.PutString("constant");
				packet.PutFloat(constant.Weather);
				packet.PutLong(0);
				packet.PutByte(0);
			}
			else if (provider is IWeatherProviderConstantSmooth)
			{
				var constantSmooth = (IWeatherProviderConstantSmooth)provider;

				packet.PutByte(2);
				packet.PutByte(0);
				packet.PutByte(1);
				packet.PutString("constant_smooth");
				packet.PutFloat(constantSmooth.Weather);
				packet.PutLong(DateTime.Now); // Start
				packet.PutLong(DateTime.MinValue); // End
				packet.PutFloat(constantSmooth.WeatherBefore);
				packet.PutFloat(constantSmooth.WeatherBefore);
				packet.PutLong(constantSmooth.TransitionTime);
				packet.PutByte(false); // bool? Is table appended? byte? Appended type?
				packet.PutLong(DateTime.MinValue); // End
				packet.PutInt(2);

				// Append a table packet here to go back to that after end

				packet.PutByte(0);
			}

			sender(packet);
		}
	}
}
