// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.World;
using Aura.Channel.World.Entities;
using Aura.Data;
using Aura.Data.Database;
using Aura.Mabi.Const;
using Aura.Mabi.Network;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aura.Channel.Skills.Life
{
	/// <summary>
	/// Fishing skill
	/// </summary>
	/// <remarks>
	/// Var1: Fish Size
	/// Var2: Chance to Lure
	/// Var3: Automatic Fishing Success Rate
	/// Var4: Automatic Fishing Catch Size
	/// </remarks>
	[Skill(SkillId.Fishing)]
	public class Fishing : ISkillHandler, IPreparable, IReadyable, IUseable, ICancelable
	{
		/// <summary>
		/// Loads skill.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		/// <returns></returns>
		public bool Prepare(Creature creature, Skill skill, Packet packet)
		{
			var unkStr = packet.GetString();

			// Check rod and bait
			if (!this.CheckEquipment(creature))
			{
				Send.MsgBox(creature, Localization.Get("You need a Fishing Rod in your right hand\nand a Bait Tin in your left."));
				return false;
			}

			Send.SkillPrepare(creature, skill.Info.Id, unkStr);

			return true;
		}

		/// <summary>
		/// Readies skill.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		/// <returns></returns>
		public bool Ready(Creature creature, Skill skill, Packet packet)
		{
			var unkStr = packet.GetString();

			Send.SkillReady(creature, skill.Info.Id, unkStr);

			return true;
		}

		/// <summary>
		/// Starts fishing at target location.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public void Use(Creature creature, Skill skill, Packet packet)
		{
			var targetPositionId = packet.GetLong();
			var unkInt1 = packet.GetInt();
			var unkInt2 = packet.GetInt();

			var pos = new Position(targetPositionId);

			creature.Temp.FishingProp = new Prop(274, creature.RegionId, pos.X, pos.Y, 1, 1, 0, "empty");
			creature.Region.AddProp(creature.Temp.FishingProp);

			creature.TurnTo(pos);

			Send.Effect(creature, Effect.Fishing, (byte)FishingEffectType.Cast, true);
			Send.SkillUse(creature, skill.Info.Id, targetPositionId, unkInt1, unkInt2);

			this.StartFishing(creature, 1000);
		}

		/// <summary>
		/// Called once ready to pull the fish out.
		/// </summary>
		/// <remarks>
		/// When you catch something just before running out of bait,
		/// and you send MotionCancel2 from Cancel, there's a
		/// visual bug on Aura, where the item keeps flying to you until
		/// you move. This does not happen on NA for unknown reason.
		/// The workaround: Check for cancellation in advance and only
		/// send the real in effect if the skill wasn't canceled.
		/// </remarks>
		/// <param name="creature"></param>
		/// <param name="method">Method used on this try</param>
		/// <param name="success">Success of manual try</param>
		public void OnResponse(Creature creature, FishingMethod method, bool success)
		{
			// Get skill
			var skill = creature.Skills.Get(SkillId.Fishing);
			if (skill == null)
			{
				Log.Error("Fishing.OnResponse: Missing skill.");
				return;
			}

			var rnd = RandomProvider.Get();

			// Update prop state
			// TODO: update prop state method
			creature.Temp.FishingProp.SetState("empty");

			// Get auto success
			if (method == FishingMethod.Auto)
				success = rnd.NextDouble() < skill.RankData.Var3 / 100f;

			// Perfect fishing
			if (ChannelServer.Instance.Conf.World.PerfectFishing)
				success = true;

			// Check fishing ground
			if (creature.Temp.FishingDrop == null)
			{
				Send.ServerMessage(creature, "Error: No items found.");
				Log.Error("Fishing.OnResponse: Failing, no drop found.");
				success = false;
			}

			// Check equipment
			if (!this.CheckEquipment(creature))
			{
				Send.ServerMessage(creature, "Error: Missing equipment.");
				Log.Error("Fishing.OnResponse: Failing, Missing equipment.");
				// TODO: Security violation once we're sure this can't happen
				//   without modding.
				success = false;
			}

			var cancel = false;

			// Reduce durability
			if (creature.RightHand != null && !ChannelServer.Instance.Conf.World.NoDurabilityLoss)
			{
				var reduce = 15;

				// Half dura loss if blessed
				if (creature.RightHand.IsBlessed)
					reduce = Math.Max(1, reduce / 2);

				creature.RightHand.Durability -= reduce;
				Send.ItemDurabilityUpdate(creature, creature.RightHand);

				// Check rod durability
				if (creature.RightHand.Durability == 0)
					cancel = true;
			}

			// Remove bait
			if (creature.Magazine != null && !ChannelServer.Instance.Conf.World.InfiniteBait)
			{
				creature.Inventory.Decrement(creature.Magazine);

				// Check if bait was removed because it was empty
				if (creature.Magazine == null)
					cancel = true;
			}

			// Fail
			Item item = null;
			if (!success)
			{
				Send.Notice(creature, Localization.Get("I was hesistating for a bit, and it got away...")); // More responses?
				Send.Effect(creature, Effect.Fishing, (byte)FishingEffectType.Fall, true);
			}
			// Success
			else
			{
				var propName = "prop_caught_objbox_01";
				var propSize = 0;
				var size = 0;

				// Create item
				item = new Item(creature.Temp.FishingDrop);

				// Check fish
				var fish = AuraData.FishDb.Find(creature.Temp.FishingDrop.ItemId);
				if (fish != null)
				{
					propName = fish.PropName;
					propSize = fish.PropSize;

					// Random fish size, unofficial
					if (fish.SizeMin + fish.SizeMax != 0)
					{
						var min = fish.SizeMin + (int)Math.Max(0, (item.Data.BaseSize - fish.SizeMin) / 100f * skill.RankData.Var4);
						var max = fish.SizeMax;

						size = Math2.Clamp(fish.SizeMin, fish.SizeMax, rnd.Next(min, max + 1) + (int)skill.RankData.Var1);
						var scale = (1f / item.Data.BaseSize * size);

						item.MetaData1.SetFloat("SCALE", scale);
					}
				}

				// Set equipment durability
				if (item.HasTag("/equip/") && item.OptionInfo.DurabilityMax >= 1)
					item.Durability = 0;

				// Drop if inv add failed
				List<Item> changed;
				if (!creature.Inventory.Insert(item, false, out changed))
					item.Drop(creature.Region, creature.GetPosition().GetRandomInRange(100, rnd));

				var itemEntityId = (changed == null || changed.Count == 0 ? item.EntityId : changed.First().EntityId);

				// Show acquire using the item's entity id if it wasn't added
				// to a stack, or using the stack's id if it was.
				Send.AcquireInfo2(creature, "fishing", itemEntityId);

				// Holding up fish effect
				if (!cancel)
					Send.Effect(creature, Effect.Fishing, (byte)FishingEffectType.ReelIn, true, creature.Temp.FishingProp.EntityId, item.Info.Id, size, propName, propSize);
			}

			creature.Temp.FishingDrop = null;

			// Handle training
			this.Training(creature, skill, success, item);

			// Cancel
			if (cancel)
			{
				creature.Skills.CancelActiveSkill();
				return;
			}

			// Next round
			this.StartFishing(creature, 6000);
		}

		/// <summary>
		/// Starts fishing with given delay.
		/// </summary>
		/// <remarks>
		/// This method uses async Tasks to control when the skill continues
		/// (basically timers). Since the player could cancel the skill before
		/// the method continues, or props could be removed because of a reload,
		/// we need to make sure not to continue and not to crash because of
		/// a change in the prop situation.
		/// 
		/// TODO: Use cancellation tokens?
		/// TODO: Don't reload spawned props, but only scripted ones?
		/// </remarks>
		/// <param name="creature"></param>
		/// <param name="delay"></param>
		public async void StartFishing(Creature creature, int delay)
		{
			var rnd = RandomProvider.Get();
			var prop = creature.Temp.FishingProp;

			await Task.Delay(delay);

			// Check that the prop is still the same (player could have canceled
			// and restarted, spawning a new one) and the prop wasn't removed
			// from region (e.g. >reloadscripts).
			if (creature.Temp.FishingProp != prop || creature.Temp.FishingProp.Region == Region.Limbo)
				return;

			// Update prop state
			creature.Temp.FishingProp.SetState("normal");

			await Task.Delay(rnd.Next(5000, 120000));
			if (creature.Temp.FishingProp != prop || creature.Temp.FishingProp.Region == Region.Limbo)
				return;

			// Update prop state
			creature.Temp.FishingProp.SetState("hooked");

			// Get fishing drop 
			creature.Temp.FishingDrop = this.GetFishingDrop(creature, rnd);

			// Random time
			var time = 10000;
			switch (rnd.Next(4))
			{
				case 0: time = 4000; break;
				case 1: time = 8000; break;
				case 2: time = 6000; break;
				case 3: time = 10000; break;
			}

			var catchSize = CatchSize.Something;
			var fishSpeed = 1f;

			// Request action
			creature.Temp.FishingActionRequested = true;
			Send.FishingActionRequired(creature, catchSize, time, fishSpeed);
		}

		/// <summary>
		/// Cancels skill, removing prop.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		public void Cancel(Creature creature, Skill skill)
		{
			if (creature.Temp.FishingProp != null && creature.Temp.FishingProp.Region != Region.Limbo)
				creature.Temp.FishingProp.Region.RemoveProp(creature.Temp.FishingProp);

			Send.MotionCancel2(creature, 0);

			creature.Temp.FishingProp = null;
		}

		/// <summary>
		/// Finds best fishing ground match for creature's current fishing prop
		/// and gets a random item from it.
		/// </summary>
		/// <param name="creature"></param>
		/// <returns></returns>
		private DropData GetFishingDrop(Creature creature, Random rnd)
		{
			var prop = creature.Temp.FishingProp;
			if (prop == null)
				return null;

			// More efficient way?

			// Check all grounds ordered by priority
			foreach (var fishingGround in AuraData.FishingGroundsDb.Entries.Values.OrderByDescending(a => a.Priority))
			{
				// Check locations
				var locationCondition = (fishingGround.Locations.Length == 0);
				foreach (var location in fishingGround.Locations)
				{
					try
					{
						// Check events
						var evs = creature.Region.GetMatchingEvents(location);
						foreach (var ev in evs)
						{
							// Check if prop is inside event shape, break at first success
							if (ev.IsInside((int)prop.Info.X, (int)prop.Info.Y))
							{
								locationCondition = true;
								break;
							}
						}

						if (locationCondition)
							break;
					}
					catch (ArgumentException ex)
					{
						Log.Error("Fishing.GetFishingDrop: {0}", ex.Message);
					}
				}

				// Event
				// Chance
				// Rod
				// Bait

				// Conditions not met
				if (!locationCondition)
					continue;

				// Conditions met

				// Get random item
				var n = 0.0;
				var chance = rnd.NextDouble() * fishingGround.TotalItemChance;
				foreach (var item in fishingGround.Items)
				{
					n += item.Chance;
					if (chance <= n)
						return item;
				}

				throw new Exception("Fishing.GetFishingDrop: Failed to get item.");
			}

			return null;
		}

		/// <summary>
		/// Returns true if creature has valid fishing equipment equipped.
		/// </summary>
		/// <param name="creature"></param>
		/// <returns></returns>
		public bool CheckEquipment(Creature creature)
		{
			return (creature.RightHand != null && creature.RightHand.HasTag("/fishingrod/") && creature.Magazine != null && creature.Magazine.HasTag("/fishing/bait/"));
		}

		/// <summary>
		/// Handles skill training.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="success"></param>
		/// <param name="item"></param>
		/// <exception cref="ArgumentException"></exception>
		public void Training(Creature creature, Skill skill, bool success, Item item)
		{
			if (success && item == null)
				throw new ArgumentException("Item shouldn't be null if fishing was successful.");

			if (skill.Info.Rank == SkillRank.Novice)
			{
				skill.Train(2); // Attempt to fish.

				if (success && item.HasTag("/fish/"))
					skill.Train(1); // Catch a fish.

				if (!success)
					skill.Train(3); // Fail at fishing.

				return;
			}

			if (skill.Info.Rank >= SkillRank.RF && skill.Info.Rank <= SkillRank.R1)
			{
				if (success)
				{
					if (item.HasTag("/fish/"))
						skill.Train(1); // Catch a fish.
					else if (item.Info.Id == 70031)
						skill.Train(2); // Catch a quest scroll.
					else
						skill.Train(3); // Catch an item.
				}

				if (skill.Info.Rank <= SkillRank.RA)
					skill.Train(4); // Attempt to fish.

				return;
			}
		}
	}
}
