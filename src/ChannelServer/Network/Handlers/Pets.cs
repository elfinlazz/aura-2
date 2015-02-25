// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Util;
using Aura.Shared.Network;
using Aura.Shared.Util;
using Aura.Channel.World;
using Aura.Shared.Mabi.Const;
using Aura.Channel.World.Inventory;

namespace Aura.Channel.Network.Handlers
{
	public partial class ChannelServerHandlers : PacketHandlerManager<ChannelClient>
	{
		/// <summary>
		/// Sent to summon a pet or partner.
		/// </summary>
		/// <example>
		/// 001 [0010010000000002] Long   : 4504699138998274
		/// 002 [..............00] Byte   : 0
		/// </example>
		[PacketHandler(Op.SummonPet)]
		public void SummonPet(ChannelClient client, Packet packet)
		{
			var entityId = packet.GetLong();
			var unkByte = packet.GetByte();

			var creature = client.GetCreatureSafe(packet.Id);

			if (creature.Pet != null)
			{
				Log.Warning("SummonPet: Player '{0}' tried to spawn multiple pets.", client.Account.Id);
				Send.SummonPetR(creature, null);
				return;
			}

			var pet = client.Account.GetPetSafe(entityId);

			// Doesn't fix giant mount problems.
			if (creature.IsGiant)
				pet.StateEx |= CreatureStatesEx.SummonedByGiant;

			pet.Master = creature;
			creature.Pet = pet;

			pet.SetLocationNear(creature, 350);
			pet.Warping = true;

			pet.Save = true;
			pet.Client = client;
			client.Creatures.Add(pet.EntityId, pet);

			Send.PetRegister(creature, pet);
			Send.SummonPetR(creature, pet);
			Send.CharacterLock(pet, Locks.Default);
			Send.EnterRegion(pet);
		}

		/// <summary>
		/// Sent to unsummon the activate pet or partner.
		/// </summary>
		/// <example>
		/// 001 [0010010000000002] Long   : 4504699138998274
		/// </example>
		[PacketHandler(Op.UnsummonPet)]
		public void UnsummonPet(ChannelClient client, Packet packet)
		{
			var entityId = packet.GetLong();

			var creature = client.GetCreatureSafe(packet.Id);

			var pet = creature.Pet;
			if (pet == null || pet.EntityId != entityId)
			{
				Log.Warning("Player '{0}' tried to unsummon invalid pet.", client.Account.Id);
				Send.UnsummonPetR(creature, false, entityId);
				return;
			}

			client.Creatures.Remove(pet.EntityId);
			pet.Master = null;
			creature.Pet = null;

			var pos = pet.StopMove();

			Send.SpawnEffect(SpawnEffect.PetDespawn, creature.RegionId, pos.X, pos.Y, creature, pet);
			if (pet.Region != null)
				pet.Region.RemoveCreature(pet);
			Send.PetUnregister(creature, pet);
			Send.Disappear(pet);
			Send.UnsummonPetR(creature, true, entityId);
		}

		/// <summary>
		/// Sent by pet to perform some action, like admiring the master
		/// (sitting down with a heart, looking at him), bear dancing, etc.
		/// </summary>
		/// <remarks>
		/// Called "set_emotion" in client side AIs?
		/// Officials let the pets say "..." sometimes,
		/// maybe random on action 0?
		/// </remarks>
		/// <example>
		/// 001 [0010000000000002] Long   : 4503599627370498
		/// 002 [..............01] Byte   : 1
		/// </example>
		[PacketHandler(Op.PetAction)]
		public void PetAction(ChannelClient client, Packet packet)
		{
			var entityId = packet.GetLong();
			var action = (PetAction)packet.GetByte();

			var pet = client.GetCreatureSafe(packet.Id);

			//Send.Chat(pet, "...");

			Send.PetActionEffect(pet, action);
		}

		/// <summary>
		/// Sent by pet if it's far away from the master, to get near him.
		/// </summary>
		/// <remarks>
		/// TODO: Don't forget to make this safe when adding mounts.
		/// </remarks>
		/// <example>
		/// 001 [..............01] Byte   : 1
		/// 002 [........00000001] Int    : 1
		/// 003 [........00002CE6] Int    : 11494
		/// 004 [........0000922F] Int    : 37423
		/// </example>
		[PacketHandler(Op.TelePet)]
		public void TelePet(ChannelClient client, Packet packet)
		{
			var unkByte = packet.GetByte();
			var regionId = packet.GetInt();
			var x = packet.GetInt();
			var y = packet.GetInt();

			var pet = client.GetSummonedPetSafe(packet.Id);

			if (pet.Master.RegionId != pet.RegionId)
			{
				throw new ModerateViolation("Illegal pet teleport");
			}

			pet.Warp(pet.RegionId, x, y);

			Send.TelePetR(pet, true);
		}

		/// <summary>
		/// Sent when moving an item into the pet inventory.
		/// </summary>
		/// <remarks>
		/// We're ignoring the colliding item id the client gives us.
		/// </remarks>
		/// <example>
		/// 0001 [0010010000064D26] Long   : 4504699139411238
		/// 0002 [005000CC171C5B9A] Long   : 22518874697915290
		/// 0003 [0000000000000000] Long   : 0
		/// 0004 [........00000002] Int    : 2
		/// 0005 [........00000003] Int    : 3
		/// 0006 [........00000003] Int    : 3
		/// </example>
		[PacketHandler(Op.PutItemIntoPetInv)]
		public void PutItemIntoPetInv(ChannelClient client, Packet packet)
		{
			var petEntityId = packet.GetLong();
			var itemEntityId = packet.GetLong();
			var collidingItemId = packet.GetLong();
			var pocket = (Pocket)packet.GetInt();
			var x = packet.GetInt();
			var y = packet.GetInt();

			// Get creature
			var creature = client.GetCreatureSafe(packet.Id);

			// Get pet
			var pet = client.GetSummonedPetSafe(petEntityId);

			// Get item
			var item = creature.Inventory.GetItemSafe(itemEntityId);

			// Check pocket
			// If the pet is a partner, limit pockets to normally accessible pockets
			// TODO: Should really limit to equip + Inventory
			// If the pet is not a partner, limit to inventory
			if ((pet.IsPartner && !CreatureInventory.AccessiblePockets.Contains(pocket)) || (!pet.IsPartner && pocket != Pocket.Inventory))
			{
				throw new ModerateViolation("Attempted to put an item into an invalid pet pocket ({0})", pocket);
			}

			// Try move
			if (!creature.Inventory.MovePet(pet, item, pet, pocket, x, y))
			{
				Log.Warning("PutItemIntoPetInv: Moving item failed.");
				goto L_Fail;
			}

			ChannelServer.Instance.Events.OnPlayerRemovesItem(creature, item.Info.Id, item.Info.Amount);
			ChannelServer.Instance.Events.OnPlayerReceivesItem(pet, item.Info.Id, item.Info.Amount);

			Send.PutItemIntoPetInvR(creature, true);
			return;

		L_Fail:
			Send.PutItemIntoPetInvR(creature, false);
		}

		/// <summary>
		/// Sent when moving an item into the pet inventory.
		/// </summary>
		/// <example>
		/// 0001 [0010010000064D26] Long   : 4504699139411238
		/// 0002 [005000CC171C5B9A] Long   : 22518874697915290
		/// </example>
		[PacketHandler(Op.TakeItemFromPetInv)]
		public void TakeItemFromPetInv(ChannelClient client, Packet packet)
		{
			var petEntityId = packet.GetLong();
			var itemEntityId = packet.GetLong();

			// Get creature
			var creature = client.GetCreatureSafe(packet.Id);

			// Get pet
			var pet = client.GetSummonedPetSafe(petEntityId);

			// Get item
			var item = pet.Inventory.GetItemSafe(itemEntityId);

			// Try move
			if (!pet.Inventory.MovePet(pet, item, creature, Pocket.Cursor, 0, 0))
			{
				Log.Warning("TakeItemFromPetInv: Moving item failed.");
				Send.TakeItemFromPetInvR(creature, false);
				return;
			}

			ChannelServer.Instance.Events.OnPlayerRemovesItem(pet, item.Info.Id, item.Info.Amount);
			ChannelServer.Instance.Events.OnPlayerReceivesItem(creature, item.Info.Id, item.Info.Amount);

			Send.TakeItemFromPetInvR(creature, true);
		}

		/// <summary>
		/// Sent to hop on a vehicle.
		/// </summary>
		/// <example>
		/// 0001 [0010010000066A4A] Long   : 4504699139418698
		/// </example>
		[PacketHandler(Op.PetMount)]
		public void PetMount(ChannelClient client, Packet packet)
		{
			var mountEntityId = packet.GetLong();

			var creature = client.GetCreatureSafe(packet.Id);

			var mount = creature.Region.GetCreature(mountEntityId);
			if (mount == null || mount == creature)
				return;

			// ...

			Send.ServerMessage(creature, "Mounts aren't implemented yet.");
			Send.PetMountR(creature, false);
		}

		/// <summary>
		/// Sent to get off a vehicle.
		/// </summary>
		/// <example>
		/// No parameters.
		/// </example>
		[PacketHandler(Op.PetUnmount)]
		public void PetUnmount(ChannelClient client, Packet packet)
		{
			var creature = client.GetCreatureSafe(packet.Id);

			// ...

			Send.ServerMessage(creature, "Mounts aren't implemented yet.");
			Send.PetUnmountR(creature, false);
		}

		/// <summary>
		/// Sent when changing the AI.
		/// </summary>
		/// <remarks>
		/// There's no response to this packet.
		/// 
		/// The default AI files can be found in data/db/ in the client.
		/// </remarks>
		/// <example>
		/// 0001 [................] String : OasisRuleSupport.xml
		/// </example>
		[PacketHandler(Op.SetPetAi)]
		public void SetPetAi(ChannelClient client, Packet packet)
		{
			var ai = packet.GetString();

			var pet = client.GetCreatureSafe(packet.Id);

			pet.Vars.Perm.PetAI = ai;
		}

		/// <summary>
		/// Sent on summon to get the set AI.
		/// </summary>
		/// <example>
		/// No parameters.
		/// </example>
		[PacketHandler(Op.GetPetAi)]
		public void GetPetAi(ChannelClient client, Packet packet)
		{
			var pet = client.GetCreatureSafe(packet.Id);

			// Send back AI, default to OasisRulePassive on null.
			Send.GetPetAiR(pet, pet.Vars.Perm.PetAI ?? "OasisRulePassive.xml");
		}
	}
}
