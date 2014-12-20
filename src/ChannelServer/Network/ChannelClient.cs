// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Linq;
using System.Collections.Generic;
using Aura.Channel.Database;
using Aura.Channel.Scripting;
using Aura.Channel.Util;
using Aura.Channel.World.Entities;
using Aura.Shared.Network;
using Aura.Channel.Skills.Life;
using Aura.Shared.Mabi.Const;

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
		/// Returns controlled creature or throws security exception if
		/// it's null.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public Creature GetControlledCreatureSafe()
		{
			var result = this.Controlling;
			if (result == null)
				throw new SevereViolation("Client doesn't control any creature");

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
			// Dispose creatures, to remove subscriptions and stuff.
			// Do this before unspawning, the creature might need the region.
			foreach (var creature in this.Creatures.Values)
				creature.Dispose();

			foreach (var creature in this.Creatures.Values.Where(a => a.Region != null))
			{
				// Close NPC sessions
				if (creature.Client.NpcSession.Script != null)
					creature.Client.NpcSession.Clear();

				// Unspawn creature
				creature.Region.RemoveCreature(creature);
			}

			// Save everything after we're done cleaning up
			if (this.Account != null)
				ChannelServer.Instance.Database.SaveAccount(this.Account);

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
