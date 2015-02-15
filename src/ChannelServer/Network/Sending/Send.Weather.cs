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
		public static void Weather(Creature creature, IWeatherProviderTable provider)
		{
			var packet = new Packet(Op.Weather, MabiId.Broadcast);
			packet.PutByte(0);
			packet.PutInt(provider.RegionId);
			packet.PutByte(0);
			packet.PutInt(provider.GroupId);
			packet.PutByte(2);
			packet.PutByte(1);
			packet.PutString("table");
			packet.PutString(provider.Name);
			packet.PutLong(0);
			packet.PutByte(0);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends Weather to creature's client.
		/// </summary>
		/// <remarks>
		/// Packet structure is guessed, even though it works,
		/// based on constant_smooth.
		/// </remarks>
		public static void Weather(Creature creature, IWeatherProviderConstant provider)
		{
			var packet = new Packet(Op.Weather, MabiId.Broadcast);
			packet.PutByte(0);
			packet.PutInt(provider.RegionId);
			packet.PutByte(2);
			packet.PutByte(0);
			packet.PutByte(1);
			packet.PutString("constant");
			packet.PutFloat(provider.Weather);
			packet.PutLong(0);
			packet.PutByte(0);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends Weather to creature's client.
		/// </summary>
		public static void Weather(Creature creature, IWeatherProviderConstantSmooth provider)
		{
			var packet = new Packet(Op.Weather, MabiId.Broadcast);
			packet.PutByte(0);
			packet.PutInt(creature.Region.Id);
			packet.PutByte(2);
			packet.PutByte(0);
			packet.PutByte(1);
			packet.PutString("constant_smooth");
			packet.PutFloat(provider.Weather);
			packet.PutLong(DateTime.Now);
			packet.PutLong(DateTime.MinValue);
			packet.PutFloat(provider.WeatherBefore);
			packet.PutFloat(provider.WeatherBefore);
			packet.PutLong(provider.TransitionTime);
			packet.PutByte(false);
			packet.PutLong(DateTime.MinValue);
			packet.PutInt(2);
			packet.PutByte(0);

			creature.Client.Send(packet);
		}
	}
}
