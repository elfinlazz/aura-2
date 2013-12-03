// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Database;
using Aura.Shared.Network;
using Aura.Channel.World.Entities;
using System.Collections.Generic;

namespace Aura.Channel.Network
{
	public class ChannelClient : Client
	{
		public Account Account { get; set; }

		//public PlayerCreature Character { get; set; }
		public Dictionary<long, Creature> Creatures { get; protected set; }

		public ChannelClient()
		{
			this.Creatures = new Dictionary<long, Creature>();
		}

		public Creature GetCreature(long id)
		{
			Creature creature;
			this.Creatures.TryGetValue(id, out creature);
			return creature;
		}

		public PlayerCreature GetPlayerCreature(long id)
		{
			return this.GetCreature(id) as PlayerCreature;
		}
	}

	/// <summary>
	/// Dummy client for creatures, so we don't have to care about who is
	/// actually able to receive data.
	/// </summary>
	public class DummyClient : Client
	{
		public override void Send(byte[] buffer)
		{ }

		public override void Send(Packet packet)
		{ }

		public override void Kill()
		{ }
	}
}
