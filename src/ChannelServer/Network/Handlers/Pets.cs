// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

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

			var creature = client.GetCreature(packet.Id);
			if (creature == null)
				return;

			if (creature.Pet != null)
			{
				Log.Warning("SummonPet: Player '{0}' tried to spawn multiple pets.", client.Account.Id);
				Send.SummonPetR(creature, null);
				return;
			}

			var pet = client.Account.GetPet(entityId);
			if (pet == null)
			{
				Log.Warning("SummonPet: Failed to get pet '{0}', for '{1}'.", entityId.ToString("X16"), client.Account.Id);
				Send.SummonPetR(creature, null);
				return;
			}

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

			var creature = client.GetCreature(packet.Id);
			if (creature == null)
				return;

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

			var pet = client.GetCreature(packet.Id);
			if (pet == null)
				return;

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

			var pet = client.GetPlayerCreature(packet.Id);
			if (pet == null || pet.Master == null)
				return;

			if (pet.Master.RegionId != pet.RegionId)
			{
				Log.Warning("Illegal pet teleport by '{0}'.", packet.Id.ToString("X16"));
				Send.TelePetR(pet, false);
				return;
			}

			pet.Warp(pet.RegionId, x, y);

			Send.TelePetR(pet, true);
		}
	}
}
