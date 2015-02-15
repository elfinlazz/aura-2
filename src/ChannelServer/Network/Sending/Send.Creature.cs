// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using Aura.Channel.World.Entities;
using Aura.Shared.Mabi.Const;
using Aura.Shared.Network;
using Aura.Shared.Util;
using Aura.Channel.World;
using Aura.Channel.World.Entities.Creatures;
using System.Collections;
using System.Collections.Generic;
using Aura.Data;
using Aura.Shared.Mabi.Structs;
using Aura.Channel.Network.Sending.Helpers;

namespace Aura.Channel.Network.Sending
{
	public static partial class Send
	{
		/// <summary>
		/// Broadcasts Running|Walking in range of creature.
		/// </summary>
		public static void Move(Creature creature, Position from, Position to, bool walking)
		{
			var packet = new Packet(!walking ? Op.Running : Op.Walking, creature.EntityId);
			packet.PutInt(from.X);
			packet.PutInt(from.Y);
			packet.PutInt(to.X);
			packet.PutInt(to.Y);

			creature.Region.Broadcast(packet, creature);
		}

		/// <summary>
		/// Broadcasts ForceRunTo in creature's range.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="to"></param>
		public static void ForceRunTo(Creature creature)
		{
			ForceRunTo(creature, creature.GetPosition());
		}

		/// <summary>
		/// Broadcasts ForceRunTo in creature's range.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="to"></param>
		public static void ForceRunTo(Creature creature, Position to)
		{
			var pos = creature.GetPosition();

			var packet = new Packet(Op.ForceRunTo, creature.EntityId);

			// From
			packet.PutInt(pos.X);
			packet.PutInt(pos.Y);

			// To
			packet.PutInt(to.X);
			packet.PutInt(to.Y);

			packet.PutByte(1);
			packet.PutByte(0);

			creature.Region.Broadcast(packet, creature);
		}

		/// <summary>
		/// Broadcasts ForceWalkTo in creature's range.
		/// </summary>
		/// <param name="creature"></param>
		public static void ForceWalkTo(Creature creature)
		{
			ForceWalkTo(creature, creature.GetPosition());
		}

		/// <summary>
		/// Broadcasts ForceWalkTo in creature's range.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="to"></param>
		public static void ForceWalkTo(Creature creature, Position to)
		{
			var pos = creature.GetPosition();

			var packet = new Packet(Op.ForceWalkTo, creature.EntityId);

			// From
			packet.PutInt(pos.X);
			packet.PutInt(pos.Y);

			// To
			packet.PutInt(to.X);
			packet.PutInt(to.Y);

			packet.PutByte(1);
			packet.PutByte(0);

			creature.Region.Broadcast(packet, creature);
		}

		/// <summary>
		/// Sends StatUpdatePublic and StatUpdatePrivate to relevant clients,
		/// with a list of new stat values.
		/// </summary>
		public static void StatUpdate(Creature creature, StatUpdateType type, params Stat[] stats)
		{
			StatUpdate(creature, type, stats, null, null, null);
		}

		/// <summary>
		/// Sends a public and private stat update for various default values,
		/// like Life, Mana, Str, Dex, etc.
		/// </summary>
		public static void StatUpdateDefault(Creature creature)
		{
			// Private
			StatUpdate(creature, StatUpdateType.Private,
				Stat.Height, Stat.Upper, Stat.Lower,
				Stat.Life, Stat.LifeInjured, Stat.LifeMax, Stat.LifeMaxMod,
				Stat.Stamina, Stat.Hunger, Stat.StaminaMax, Stat.StaminaMaxMod,
				Stat.Mana, Stat.ManaMax, Stat.ManaMaxMod,
				Stat.Str, Stat.Dex, Stat.Int, Stat.Luck, Stat.Will,
				Stat.StrMod, Stat.DexMod, Stat.IntMod, Stat.LuckMod, Stat.WillMod,
				Stat.DefenseBaseMod, Stat.DefenseMod, Stat.ProtectionBaseMod, Stat.ProtectionMod,
				Stat.Level, Stat.Experience, Stat.AbilityPoints, Stat.CombatPower, Stat.Age
			);

			// Public
			StatUpdate(creature, StatUpdateType.Public,
				Stat.Life, Stat.LifeInjured, Stat.LifeMax, Stat.LifeMaxMod
			);
		}

		/// <summary>
		/// Sends StatUpdatePublic and StatUpdatePrivate to relevant clients,
		/// with a list of regens to remove.
		/// </summary>
		public static void NewRegens(Creature creature, StatUpdateType type, params StatRegen[] regens)
		{
			StatUpdate(creature, type, null, regens, null, null);
		}

		/// <summary>
		/// Sends StatUpdatePublic and StatUpdatePrivate to relevant clients,
		/// with a list of regens to remove.
		/// </summary>
		public static void RemoveRegens(Creature creature, StatUpdateType type, params StatRegen[] regens)
		{
			StatUpdate(creature, type, null, null, regens, null);
		}

		/// <summary>
		/// Sends StatUpdatePublic and StatUpdatePrivate to relevant clients,
		/// with a list of new change and max values for the regens.
		/// </summary>
		public static void UpdateRegens(Creature creature, StatUpdateType type, params StatRegen[] regens)
		{
			StatUpdate(creature, type, null, null, null, regens);
		}

		/// <summary>
		/// Sends StatUpdatePublic to creature's in range,
		/// or StatUpdatePrivate to creature's client.
		/// </summary>
		/// <remarks>
		/// In private mode this packet has simply 4 lists.
		/// - A list of stats and their (new) values.
		/// - A list of (new) regens.
		/// - A list of regens to remove (by id).
		/// - A list of regens to update, with new change and max values.
		/// (The last one is speculation.)
		/// Since it's private, it's only sent to the creature's client,
		/// and they get every stat and regen.
		/// 
		/// In public mode the same information is sent, but limited to stats
		/// like life, that's required for displaying life bars for others.
		/// It also has 3 more lists, that seem to do almost the same as the
		/// last 3 of private, regens, removing, and updating.
		/// - Some regens are sent in the first list, some in the second.
		///   (Like life vs injuries when using Rest.)
		/// - Regens that are to be removed are sent in both lists.
		/// - Updates are only sent in the first list.
		/// More research is required, to find out what the second lists
		/// actually do.
		/// </remarks>
		public static void StatUpdate(Creature creature, StatUpdateType type, ICollection<Stat> stats, ICollection<StatRegen> regens, ICollection<StatRegen> regensRemove, ICollection<StatRegen> regensUpdate)
		{
			var packet = new Packet(type == StatUpdateType.Public ? Op.StatUpdatePublic : Op.StatUpdatePrivate, creature.EntityId);
			packet.PutByte((byte)type);

			// Stats
			if (stats == null)
				packet.PutInt(0);
			else
			{
				packet.PutInt(stats.Count);
				foreach (var stat in stats)
				{
					packet.PutInt((int)stat);
					switch (stat)
					{
						case Stat.Height: packet.PutFloat(creature.Height); break;
						case Stat.Weight: packet.PutFloat(creature.Weight); break;
						case Stat.Upper: packet.PutFloat(creature.Upper); break;
						case Stat.Lower: packet.PutFloat(creature.Lower); break;

						case Stat.CombatPower: packet.PutFloat(creature.CombatPower); break;
						case Stat.Level: packet.PutShort(creature.Level); break;
						case Stat.AbilityPoints: packet.PutShort(creature.AbilityPoints); break;
						case Stat.Experience: packet.PutLong(AuraData.ExpDb.CalculateRemaining(creature.Level, creature.Exp) * 1000); break;

						case Stat.Life: packet.PutFloat(creature.Life); break;
						case Stat.LifeMax: packet.PutFloat(creature.LifeMaxBaseTotal); break;
						case Stat.LifeMaxMod: packet.PutFloat(creature.StatMods.Get(Stat.LifeMaxMod)); break;
						case Stat.LifeInjured: packet.PutFloat(creature.LifeInjured); break;
						case Stat.LifeMaxFoodMod: packet.PutFloat(creature.LifeFoodMod); break;
						case Stat.Mana: packet.PutFloat(creature.Mana); break;
						case Stat.ManaMax: packet.PutFloat(creature.ManaMaxBaseTotal); break;
						case Stat.ManaMaxMod: packet.PutFloat(creature.StatMods.Get(Stat.ManaMaxMod)); break;
						case Stat.ManaMaxFoodMod: packet.PutFloat(creature.ManaFoodMod); break;
						case Stat.Stamina: packet.PutFloat(creature.Stamina); break;
						case Stat.Hunger: packet.PutFloat(creature.StaminaHunger); break;
						case Stat.StaminaMax: packet.PutFloat(creature.StaminaMaxBaseTotal); break;
						case Stat.StaminaMaxMod: packet.PutFloat(creature.StatMods.Get(Stat.StaminaMaxMod)); break;
						case Stat.StaminaMaxFoodMod: packet.PutFloat(creature.StaminaFoodMod); break;

						case Stat.StrMod: packet.PutFloat(creature.StatMods.Get(Stat.StrMod)); break;
						case Stat.DexMod: packet.PutFloat(creature.StatMods.Get(Stat.DexMod)); break;
						case Stat.IntMod: packet.PutFloat(creature.StatMods.Get(Stat.IntMod)); break;
						case Stat.LuckMod: packet.PutFloat(creature.StatMods.Get(Stat.LuckMod)); break;
						case Stat.WillMod: packet.PutFloat(creature.StatMods.Get(Stat.WillMod)); break;
						case Stat.Str: packet.PutFloat(creature.StrBaseTotal); break;
						case Stat.Int: packet.PutFloat(creature.IntBaseTotal); break;
						case Stat.Dex: packet.PutFloat(creature.DexBaseTotal); break;
						case Stat.Will: packet.PutFloat(creature.WillBaseTotal); break;
						case Stat.Luck: packet.PutFloat(creature.LuckBaseTotal); break;
						case Stat.StrFoodMod: packet.PutFloat(creature.StrFoodMod); break;
						case Stat.DexFoodMod: packet.PutFloat(creature.DexFoodMod); break;
						case Stat.IntFoodMod: packet.PutFloat(creature.IntFoodMod); break;
						case Stat.LuckFoodMod: packet.PutFloat(creature.LuckFoodMod); break;
						case Stat.WillFoodMod: packet.PutFloat(creature.WillFoodMod); break;

						case Stat.DefenseBase: packet.PutShort((short)creature.DefenseBase); break;
						case Stat.ProtectionBase: packet.PutFloat(creature.ProtectionBase); break;
						case Stat.DefenseBaseMod: packet.PutShort((short)creature.DefenseBaseMod); break;
						case Stat.ProtectionBaseMod: packet.PutFloat(creature.ProtectionBaseMod); break;
						case Stat.DefenseMod: packet.PutShort((short)creature.DefenseMod); break;
						case Stat.ProtectionMod: packet.PutFloat(creature.ProtectionMod); break;

						case Stat.BalanceBaseMod: packet.PutShort((short)(creature.BalanceBase * 100)); break;
						case Stat.CriticalBaseMod: packet.PutFloat(creature.CriticalBase * 100); break;

						case Stat.AttackMinBaseMod: packet.PutShort((short)creature.AttackMinBaseMod); break;
						case Stat.AttackMaxBaseMod: packet.PutShort((short)creature.AttackMaxBaseMod); break;
						case Stat.InjuryMinBaseMod: packet.PutShort((short)creature.InjuryMinBaseMod); break;
						case Stat.InjuryMaxBaseMod: packet.PutShort((short)creature.InjuryMaxBaseMod); break;
						case Stat.AttackMinMod: packet.PutShort((short)creature.AttackMinMod); break;
						case Stat.AttackMaxMod: packet.PutShort((short)creature.AttackMaxMod); break;

						case Stat.Age: packet.PutShort((short)creature.Age); break;

						// Client might crash with a mismatching value, 
						// take a chance and put an int by default.
						default:
							Log.Warning("StatUpdate: Unknown stat '{0}'.", stat);
							packet.PutInt(0);
							break;
					}
				}
			}

			// Regens
			if (regens == null)
				packet.PutInt(0);
			else
			{
				packet.PutInt(regens.Count);
				foreach (var regen in regens)
					packet.AddRegen(regen);
			}

			// Regens to Remove
			if (regensRemove == null)
				packet.PutInt(0);
			else
			{
				packet.PutInt(regensRemove.Count);
				foreach (var regen in regensRemove)
					packet.PutInt(regen.Id);
			}

			// ?
			// Maybe update of change and max?
			if (regensUpdate == null)
				packet.PutInt(0);
			else
			{
				packet.PutInt(regensUpdate.Count);
				foreach (var regen in regensUpdate)
				{
					packet.PutInt(regen.Id);
					packet.PutFloat(regen.Change);
					packet.PutFloat(regen.Max);
				}
			}

			if (type == StatUpdateType.Public)
			{
				// Another list of regens...?
				packet.PutInt(0);

				if (regensRemove == null)
					packet.PutInt(0);
				else
				{
					// Regens to Remove (again...?)
					packet.PutInt(regensRemove.Count);
					foreach (var regen in regensRemove)
						packet.PutInt(regen.Id);
				}

				// Update?
				packet.PutInt(0);
			}

			if (type == StatUpdateType.Private)
				creature.Client.Send(packet);
			else if (creature.Region != null)
				creature.Region.Broadcast(packet, creature);
		}

		/// <summary>
		/// Broadcasts CreatureBodyUpdate in range of creature.
		/// </summary>
		/// <param name="creature"></param>
		public static void CreatureBodyUpdate(Creature creature)
		{
			var packet = new Packet(Op.CreatureBodyUpdate, creature.EntityId);
			packet.PutBin(creature.Body);

			creature.Region.Broadcast(packet, creature);
		}

		/// <summary>
		/// Broadcasts ConditionUpdate in range of creature.
		/// </summary>
		/// <param name="creature"></param>
		public static void ConditionUpdate(Creature creature)
		{
			var packet = new Packet(Op.ConditionUpdate, creature.EntityId);
			packet.AddConditions(creature.Conditions);

			creature.Region.Broadcast(packet, creature);
		}

		/// <summary>
		/// Broadcasts SitDown in range of creature.
		/// </summary>
		/// <remarks>
		/// The byte parameter is the rest post to use, 0 being the default.
		/// To keep sitting in that position for others, even if they run
		/// out of range, CreatureStateEx is required to be set (see Rest).
		/// It's unknown which state is the one for Rest R1 though,
		/// it might not be implemented at all yet.
		/// </remarks>
		/// <param name="creature"></param>
		public static void SitDown(Creature creature)
		{
			var packet = new Packet(Op.SitDown, creature.EntityId);
			packet.PutByte(creature.GetRestPose());

			creature.Region.Broadcast(packet, creature);
		}

		/// <summary>
		/// Broadcasts StandUp in range of creature.
		/// </summary>
		/// <param name="creature"></param>
		public static void StandUp(Creature creature)
		{
			var packet = new Packet(Op.StandUp, creature.EntityId);
			packet.PutByte(1);

			creature.Region.Broadcast(packet, creature);
		}

		/// <summary>
		/// Broadcasts AssignSittingProp in range of creature.
		/// </summary>
		/// <remarks>
		/// Moves creature into position, to sit down on the prop.
		/// Pass 0 for the parameters to undo.
		/// </remarks>
		/// <param name="creature"></param>
		/// <param name="propEntityId"></param>
		/// <param name="unk"></param>
		public static void AssignSittingProp(Creature creature, long propEntityId, int unk)
		{
			var packet = new Packet(Op.AssignSittingProp, creature.EntityId);
			packet.PutLong(propEntityId);
			packet.PutInt(unk);

			creature.Region.Broadcast(packet);
		}

		/// <summary>
		/// Broadcasts LevelUp in range of creature.
		/// </summary>
		/// <param name="creature"></param>
		public static void LevelUp(Creature creature)
		{
			var packet = new Packet(Op.LevelUp, creature.EntityId);
			packet.PutShort(creature.Level);

			creature.Region.Broadcast(packet, creature);
		}
	}

	public enum StatUpdateType : byte { Private = 3, Public = 4 }
}
