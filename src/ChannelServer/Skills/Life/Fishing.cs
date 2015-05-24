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

			// Check for fishing rod and bait...

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

			creature.Temp.FishingProp = new Prop(274, creature.RegionId, pos.X, pos.Y, 1f);
			creature.Temp.FishingProp.State = "empty";

			creature.Region.AddProp(creature.Temp.FishingProp);

			Send.Effect(creature, 10, (byte)0, (byte)1);
			Send.SkillUse(creature, skill.Info.Id, targetPositionId, unkInt1, unkInt2);

			this.StartFishing(creature, 1000);
		}

		/// <summary>
		/// Called once ready to pull the fish out.
		/// </summary>
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
			creature.Temp.FishingProp.State = "empty";
			Send.PropUpdate(creature.Temp.FishingProp);

			// Get auto success
			if (method == FishingMethod.Auto)
				success = rnd.NextDouble() < skill.RankData.Var3 / 100f;

			// Check fishing ground
			if (creature.Temp.FishingGround == null)
			{
				Log.Error("Fishing.OnResponse: Failing, no matching fishing ground found.");
				success = false;
			}
			else if (creature.Temp.FishingGround.Items.Length == 0)
			{
				Log.Error("Fishing.OnResponse: Failing, no items defined for fishing ground.");
				success = false;
			}

			// Fail
			if (!success)
			{
				Send.Notice(creature, Localization.Get("I was hesistating for a bit, and it got away...")); // More responses?
				Send.Effect(creature, 10, (byte)4, (byte)1);
			}
			// Success
			else
			{
				// Get random item
				var drop = creature.Temp.FishingGround.Items[rnd.Next(creature.Temp.FishingGround.Items.Length)]; // TODO: Proper random

				// Create item
				var item = new Item(drop.ItemId);

				// Drop if inv add failed
				if (!creature.Inventory.Insert(item, false))
					item.Drop(creature.Region, creature.GetPosition().GetRandomInRange(100, rnd));
				// Show aquire (on inv add success?)
				else
					// TODO: Check packets for non fish?
					Send.AcquireInfo2(creature, "fishing", item.EntityId);

				var isFish = item.HasTag("/fish/");

				var unkInt = isFish ? 70 : 0;
				var propCaught = "prop_caught_fish_01"; // TODO: Get from fish db
				if (!isFish)
					propCaught = "prop_caught_objbox_01";

				// Holding up fish effect (item...?)
				Send.Effect(creature, 10, (byte)3, (byte)1, creature.Temp.FishingProp.EntityId, drop.ItemId, 0, propCaught, unkInt);
			}

			// Reduce durability
			// TODO: Create method
			if (!ChannelServer.Instance.Conf.World.NoDurabilityLoss && creature.RightHand != null)
			{
				var reduce = 15;

				// Half dura loss if blessed
				if (creature.RightHand.IsBlessed)
					reduce = Math.Max(1, reduce / 2);

				creature.RightHand.Durability -= reduce;
				Send.ItemDurabilityUpdate(creature, creature.RightHand);
			}

			// Remove bait
			// TODO: option
			if (creature.Magazine != null)
				creature.Inventory.Decrement(creature.Magazine);

			// Next round
			this.StartFishing(creature, 6000);
		}

		/// <summary>
		/// Starts fishing with given delay.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="delay"></param>
		public async void StartFishing(Creature creature, int delay)
		{
			var rnd = RandomProvider.Get();

			await Task.Delay(delay);
			if (creature.Temp.FishingProp == null)
				return;

			// Update prop state
			creature.Temp.FishingProp.State = "normal";
			Send.PropUpdate(creature.Temp.FishingProp);

			await Task.Delay(5000); // rnd
			if (creature.Temp.FishingProp == null)
				return;

			// Update prop state
			creature.Temp.FishingProp.State = "hooked";
			Send.PropUpdate(creature.Temp.FishingProp);

			// Get fishing ground and time
			creature.Temp.FishingGround = this.GetFishingGround(creature);

			// Random time
			var time = 10000;
			switch (rnd.Next(3))
			{
				case 0: time = 4000; break;
				case 1: time = 8000; break;
				case 2: time = 10000; break;
			}

			var catchSize = CatchSize.Something;
			var fishSpeed = 1f;

			// Request action
			Send.FishingActionRequired(creature, catchSize, time, fishSpeed);
		}

		/// <summary>
		/// Cancels skill, removing prop.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		public void Cancel(Creature creature, Skill skill)
		{
			if (creature.Temp.FishingProp == null)
				return;

			creature.Temp.FishingProp.Region.RemoveProp(creature.Temp.FishingProp);
			creature.Temp.FishingProp = null;
		}

		/// <summary>
		/// Finds best fishing ground match for creature's current fishing prop.
		/// </summary>
		/// <param name="creature"></param>
		/// <returns></returns>
		private FishingGroundData GetFishingGround(Creature creature)
		{
			var prop = creature.Temp.FishingProp;
			if (prop == null)
				return null;

			// More efficient way?

			// Check all grounds ordered by priority
			foreach (var entry in AuraData.FishingGroundsDb.Entries.Values.OrderByDescending(a => a.Priority))
			{
				// Check locations
				var locationCondition = (entry.Locations.Length == 0);
				foreach (var location in entry.Locations)
				{
					try
					{
						// Check events
						var evs = AuraData.RegionInfoDb.GetMatchingEvents(location);
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
						Log.Error("Fishing.GetFishingGround: {0}", ex.Message);
					}
				}

				// Event
				// Chance
				// Rod
				// Bait

				if (locationCondition/* && ...*/)
					return entry;
			}

			return null;
		}
	}
}
