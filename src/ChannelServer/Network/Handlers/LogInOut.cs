// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Linq;
using Aura.Channel.Database;
using Aura.Channel.Network.Sending;
using Aura.Shared.Network;
using Aura.Shared.Util;
using Aura.Channel.World;
using Aura.Shared.Mabi.Const;

namespace Aura.Channel.Network.Handlers
{
	public partial class ChannelServerHandlers : PacketHandlerManager<ChannelClient>
	{
		/// <summary>
		/// Login.
		/// </summary>
		/// <example>
		/// 001 [................] String : admin
		/// 002 [................] String : admin
		/// 003 [.79D55246A240C89] Long   : 548688344496999561
		/// 004 [..10000000000002] Long   : 4503599627370498
		/// </example>
		[PacketHandler(Op.ChannelLogin)]
		public void ChannelLogin(ChannelClient client, Packet packet)
		{
			var accountId = packet.GetString();
			// [160XXX] Double account name
			{
				packet.GetString();
			}
			var sessionKey = packet.GetLong();
			var characterId = packet.GetLong();

			// Check state
			if (client.State != ClientState.LoggingIn)
				return;

			// Check account
			var account = ChannelDb.Instance.GetAccount(accountId);
			if (account == null || account.SessionKey != sessionKey)
			{
				Log.Warning("ChannelLogin handler: Invalid account ({0}) or session ({1}).", accountId, sessionKey);
				client.Kill();
				return;
			}

			// Check character
			var character = account.GetCharacterOrPet(characterId);
			if (character == null)
			{
				Log.Warning("ChannelLogin handler: Account ({0}) doesn't contain character ({1}).", accountId, characterId);
				client.Kill();
				return;
			}

			client.Account = account;
			client.Controlling = character;
			client.Creatures.Add(character.EntityId, character);
			character.Client = client;

			client.State = ClientState.LoggedIn;

			Send.ChannelLoginR(client, character.EntityId);

			// Fallback for invalid region ids, like 0, dynamics, and dungeons.
			if (character.RegionId == 0 || Math2.Between(character.RegionId, 35000, 40000) || Math2.Between(character.RegionId, 10000, 11000))
				character.SetLocation(1, 12800, 38100);

			Send.CharacterLock(character, Locks.Default);
			Send.EnterRegion(character);

			character.Warping = true;
		}

		/// <summary>
		/// Sent after EnterRegion.
		/// </summary>
		/// <example>
		/// No parameters.
		/// </example>
		[PacketHandler(Op.EnterRegionRequest)]
		public void EnterRegionRequest(ChannelClient client, Packet packet)
		{
			var creature = client.GetPlayerCreature(packet.Id);
			if (creature == null)
				return;

			if (!creature.Warping)
			{
				Log.Warning("Unauthorized warp attemp from '{0}'.", creature.Name);
				return;
			}

			creature.Warping = false;

			var region = ChannelServer.Instance.World.GetRegion(creature.RegionId);
			if (region == null)
			{
				Log.Warning("Player '{0}' tried to enter unknown region '{1}'.", creature.Name, creature.RegionId);
				return;
			}

			creature.Save = true;

			var firstSpawn = (creature.Region == null);

			region.AddCreature(creature);

			Send.CharacterUnlock(creature, Locks.Default);

			if (firstSpawn)
				Send.EnterRegionRequestR(creature);
			else
				Send.WarpRegion(creature);

			var pos = creature.GetPosition();
			creature.Region.ActivateAis(creature, pos, pos);

			// Automatically done by the world update
			//Send.EntitiesAppear(client, region.GetEntitiesInRange(creature));
		}

		/// <summary>
		/// ?
		/// </summary>
		/// <remarks>
		/// Judging by the name I'd guess you normally get the entities here.
		/// Sent when logging in, spawning a pet, etc.
		/// </remarks>
		/// <example>
		/// Op: 000061A8, Id: 200000000000000F
		/// 0001 [0010010000000001] Long   : 4504699138998273
		/// </example>
		[PacketHandler(Op.AddObserverRequest)]
		public void AddObserverRequest(ChannelClient client, Packet packet)
		{
			var id = packet.GetLong();

			// ...
		}

		/// <summary>
		/// Request for character information.
		/// </summary>
		/// <example>
		/// No parameters.
		/// </example>
		[PacketHandler(Op.ChannelCharacterInfoRequest)]
		public void ChannelCharacterInfoRequest(ChannelClient client, Packet packet)
		{
			var creature = client.GetPlayerCreature(packet.Id);
			if (creature == null)
			{
				Send.ChannelCharacterInfoRequestR_Fail(client);
				return;
			}

			if (creature.Master != null)
			{
				var pos = creature.GetPosition();
				Send.Effect(creature.Master, Effect.Spawn, creature.RegionId, (float)pos.X, (float)pos.Y, SpawnEffect.Pet);
			}

			// Infamous 5209, aka char info
			Send.ChannelCharacterInfoRequestR(client, creature);
		}

		/// <summary>
		/// Disconnection request.
		/// </summary>
		/// <remarks>
		/// Client doesn't disconnect till we answer.
		/// </remarks>
		/// <example>
		/// ...
		/// </example>
		[PacketHandler(Op.DisconnectRequest)]
		public void HandleDisconnect(ChannelClient client, Packet packet)
		{
			var unk1 = packet.GetByte(); // 1 | 2 (maybe login vs exit?)

			Log.Info("'{0}' is closing the connection. Saving...", client.Account.Id);

			ChannelDb.Instance.SaveAccount(client.Account);

			foreach (var creature in client.Creatures.Values.Where(a => a.Region != null))
				creature.Region.RemoveCreature(creature);

			foreach (var creature in client.Creatures.Values)
				creature.Dispose();

			client.Creatures.Clear();
			//client.Character = null;
			client.Account = null;

			Send.ChannelDisconnectR(client);
		}
	}
}
