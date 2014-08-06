// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Linq;
using System.Collections.Generic;
using Aura.Channel.Database;
using Aura.Channel.Scripting;
using Aura.Channel.Util;
using Aura.Channel.World.Entities;
using Aura.Shared.Network;

namespace Aura.Channel.Network
{
	public class ChannelClient : DefaultClient
	{
		public Account Account { get; set; }

		/// <summary>
		/// Main creature this client controls.
		/// </summary>
		public Creature Controlling { get; set; }

		/// <summary>
		/// List of creatures the client is controlling.
		/// </summary>
		public Dictionary<long, Creature> Creatures { get; protected set; }

		/// <summary>
		/// Information about a current NPC dialog.
		/// </summary>
		public NpcSession NpcSession { get; set; }

		public ChannelClient()
		{
			this.Creatures = new Dictionary<long, Creature>();
			this.NpcSession = new NpcSession();
		}

		/// <summary>
		/// Returns creature by entity id or null.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public Creature GetCreature(long id)
		{
			Creature creature;
			this.Creatures.TryGetValue(id, out creature);
			return creature;
		}

		/// <summary>
		/// Returns creature or throws security exception if creature
		/// couldn't be found in client.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public Creature GetCreatureSafe(long id)
		{
			var result = this.GetCreature(id);
			if (result == null)
				throw new SevereViolation("Client doesn't control creature 0x{0:X}", id);

			return result;
		}

		/// <summary>
		/// Calls <see cref="GetCreatureSafe(long)"/> and then checks the pet's master for null.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public Creature GetSummonedPetSafe(long id)
		{
			var pet = this.GetCreatureSafe(id);
			if (pet.Master == null)
				throw new ModerateViolation("Pet 0x{0:X} doesn't have a master.", id);

			return pet;
		}

		/// <summary>
		/// Saves characters, despawns and disposes them, etc.
		/// </summary>
		public override void CleanUp()
		{
			if (this.Account != null)
				ChannelDb.Instance.SaveAccount(this.Account);

			foreach (var creature in this.Creatures.Values.Where(a => a.Region != null))
			{
				if (creature.Client.NpcSession.Script != null)
					creature.Client.NpcSession.Clear();
				creature.Region.RemoveCreature(creature);
			}

			foreach (var creature in this.Creatures.Values)
				creature.Dispose();

			this.Creatures.Clear();
			this.Account = null;
		}
	}

	/// <summary>
	/// Dummy client for creatures, so we don't have to care about who is
	/// actually able to receive data.
	/// </summary>
	public class DummyClient : ChannelClient
	{
		public override void Send(byte[] buffer)
		{ }

		public override void Send(Packet packet)
		{ }

		public override void Kill()
		{ }

		public override void CleanUp()
		{ }
	}
}
