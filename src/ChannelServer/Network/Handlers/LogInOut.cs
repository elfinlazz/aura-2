// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Linq;
using Aura.Channel.Database;
using Aura.Channel.Network.Sending;
using Aura.Shared.Network;
using Aura.Shared.Util;
using Aura.Channel.World;
using Aura.Shared.Mabi.Const;
using Aura.Channel.World.Entities;
using System;
using Aura.Shared.Mabi;

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
				// This doesn't autoban because the client is not yet "authenticated",
				// so an evil person might be able to use it to inflate someone's
				// autoban score without knowing their password
				Log.Warning("ChannelLogin handler: Invalid account ({0}) or session ({1}).", accountId, sessionKey);
				client.Kill();
				return;
			}

			// Check character
			var character = account.GetCharacterOrPetSafe(characterId);

			client.Account = account;
			client.Controlling = character;
			client.Creatures.Add(character.EntityId, character);
			character.Client = client;

			client.State = ClientState.LoggedIn;

			Send.ChannelLoginR(client, character.EntityId);

			// Log into world
			if (character.Has(CreatureStates.EverEnteredWorld) || character.IsPet)
			{
				// Fallback for invalid region ids, like 0, dynamics, and dungeons.
				if (character.RegionId == 0 || Math2.Between(character.RegionId, 35000, 40000) || Math2.Between(character.RegionId, 10000, 11000))
					character.SetLocation(1, 12800, 38100);

				Send.CharacterLock(character, Locks.Default);
				Send.EnterRegion(character);
				character.Warping = true;
			}
			// Special login to Soul Stream for new chars
			else
			{
				var npcEntityId = (character.IsCharacter ? MabiId.Nao : MabiId.Tin);
				var npc = ChannelServer.Instance.World.GetCreature(npcEntityId);
				if (npc == null)
					Log.Warning("ChannelLogin: Intro NPC not found ({0}).", npcEntityId.ToString("X16"));

				character.Temp.InSoulStream = true;
				character.Activate(CreatureStates.EverEnteredWorld);

				Send.SpecialLogin(character, 1000, 3200, 3200, npcEntityId);
			}
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
			var creature = client.GetCreatureSafe(packet.Id);

			// Check permission
			// This can happen from time to time, client lag?
			if (!creature.Warping)
			{
				Log.Warning("Unauthorized warp attemp from '{0}'.", creature.Name);
				return;
			}

			creature.Warping = false;

			// Get region
			var region = ChannelServer.Instance.World.GetRegion(creature.RegionId);
			if (region == null)
			{
				Log.Warning("Player '{0}' tried to enter unknown region '{1}'.", creature.Name, creature.RegionId);
				return;
			}

			// Characters that spawned at least once need to be saved.
			if (creature is PlayerCreature)
				(creature as PlayerCreature).Save = true;

			// Add to region
			var firstSpawn = (creature.Region == null);
			if (!firstSpawn)
				ChannelServer.Instance.Events.OnPlayerLeavesRegion(creature);
			region.AddCreature(creature);
			ChannelServer.Instance.Events.OnPlayerEntersRegion(creature);

			// Unlock and warp
			Send.CharacterUnlock(creature, Locks.Default);
			if (firstSpawn)
				Send.EnterRegionRequestR(creature);
			else
				Send.WarpRegion(creature);

			// Activate AIs around spawn
			var pos = creature.GetPosition();
			creature.Region.ActivateAis(creature, pos, pos);

			// Warp pets and other creatures as well
			foreach (var c in client.Creatures.Values.Where(a => a.RegionId != creature.RegionId))
				c.Warp(creature.RegionId, pos.X, pos.Y);

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
			var creature = client.GetCreatureSafe(packet.Id);

			if (creature.Master != null)
			{
				var pos = creature.GetPosition();
				Send.SpawnEffect(SpawnEffect.Pet, creature.RegionId, pos.X, pos.Y, creature.Master, creature);
			}

			// Infamous 5209, aka char info
			Send.ChannelCharacterInfoRequestR(client, creature);

			// Special treatment for pets
			if (creature.Master != null)
			{
				// Send vehicle info to make mounts mountable
				if (creature.RaceData.VehicleType > 0)
					Send.VehicleInfo(creature);
			}

			var playerCreature = creature as PlayerCreature;
			if (playerCreature != null)
			{
				// Update last login
				playerCreature.LastLogin = DateTime.Now;

				// Age check
				var lastSaturday = ErinnTime.Now.GetLastSaturday();
				var lastAging = playerCreature.LastAging;
				var diff = (lastSaturday - lastAging).TotalDays;

				if (lastAging < lastSaturday)
					playerCreature.AgeUp((short)(1 + diff / 7));
			}
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
		public void DisconnectRequest(ChannelClient client, Packet packet)
		{
			var unk1 = packet.GetByte(); // 1 | 2 (maybe login vs exit?)

			ChannelServer.Instance.Events.OnPlayerDisconnect(client.Controlling);

			Log.Info("'{0}' is closing the connection. Saving...", client.Account.Id);

			client.CleanUp();

			Send.ChannelDisconnectR(client);
		}

		/// <summary>
		/// Sent when entering the Soul Stream.
		/// </summary>
		/// <remarks>
		/// Purpose unknown, no answer sent in logs.
		/// </remarks>
		/// <example>
		/// No parameters.
		/// </example>
		[PacketHandler(Op.EnterSoulStream)]
		public void EnterSoulStream(ChannelClient client, Packet packet)
		{
		}

		/// <summary>
		/// Sent after ending the conversation with Nao.
		/// </summary>
		/// <example>
		/// No parameters.
		/// </example>
		[PacketHandler(Op.LeaveSoulStream)]
		public void LeaveSoulStream(ChannelClient client, Packet packet)
		{
			var creature = client.GetCreatureSafe(packet.Id);
			if (!creature.Temp.InSoulStream)
				return;

			creature.Temp.InSoulStream = false;

			Send.LeaveSoulStreamR(creature);

			Send.CharacterLock(creature, Locks.Default);
			Send.EnterRegion(creature);
			creature.Warping = true;
		}
	}
}
