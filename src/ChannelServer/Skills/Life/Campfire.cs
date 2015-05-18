// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.Util;
using Aura.Channel.World;
using Aura.Channel.World.Entities;
using Aura.Data;
using Aura.Data.Database;
using Aura.Mabi;
using Aura.Mabi.Const;
using Aura.Mabi.Network;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.Skills.Life
{
	/// <summary>
	/// Campfire skill handler
	/// </summary>
	/// <remarks>
	/// Var1: Regeneration Bonus
	/// Var2: ?
	/// Var3: Players Accommodated
	/// Var4: ?
	/// 
	/// Duration is apparently not in the db.
	/// 
	/// Without Firewood you get the msg "not an appropriate place".
	/// </remarks>
	[Skill(SkillId.Campfire, SkillId.CampfireKit)]
	public class Campfire : ISkillHandler, IPreparable, IReadyable, IUseable, ICompletable, ICancelable
	{
		/// <summary>
		/// Prop to spawn
		/// </summary>
		private const int PropId = 203;

		/// <summary>
		/// How much Firewood is required/being removed.
		/// </summary>
		/// <remarks>
		/// TODO: Move to conf?
		/// </remarks>
		private const int FirewoodCost = 5;

		/// <summary>
		/// Prepares skill (effectively does nothing)
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		/// <returns></returns>
		public bool Prepare(Creature creature, Skill skill, Packet packet)
		{
			if (skill.Info.Id == SkillId.Campfire)
			{
				var itemId = packet.GetInt();

				Send.SkillPrepare(creature, skill.Info.Id, itemId);
			}
			else
			{
				var dict = packet.GetString();

				Send.SkillPrepare(creature, skill.Info.Id, dict);
			}

			return true;
		}

		/// <summary>
		/// Readies skill, saving the item id to use for later.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		/// <returns></returns>
		public bool Ready(Creature creature, Skill skill, Packet packet)
		{
			if (skill.Info.Id == SkillId.Campfire)
			{
				creature.Temp.FirewoodItemId = packet.GetInt();

				Send.SkillReady(creature, skill.Info.Id, creature.Temp.FirewoodItemId);
			}
			else
			{
				var dict = packet.GetString();

				creature.Temp.CampfireKitItemEntityId = MabiDictionary.Fetch<long>("ITEMID", dict);

				Send.SkillReady(creature, skill.Info.Id, dict);
			}

			return true;
		}

		/// <summary>
		/// Uses skill (effectively does nothing)
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public void Use(Creature creature, Skill skill, Packet packet)
		{
			var positionId = packet.GetLong();
			var unkInt1 = packet.GetInt();
			var unkInt2 = packet.GetInt();

			Send.SkillUse(creature, skill.Info.Id, positionId, unkInt1, unkInt2);
		}

		/// <summary>
		/// Completes skill, placing the campfire.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public void Complete(Creature creature, Skill skill, Packet packet)
		{
			var positionId = packet.GetLong();
			var unkInt1 = packet.GetInt();
			var unkInt2 = packet.GetInt();

			// Handle items
			if (skill.Info.Id == SkillId.Campfire)
			{
				// Check Firewood, the client should stop the player long before Complete.
				if (creature.Inventory.Count(creature.Temp.FirewoodItemId) < FirewoodCost)
					throw new ModerateViolation("Used Campfire without Firewood.");

				// Remove Firewood
				creature.Inventory.Remove(creature.Temp.FirewoodItemId, FirewoodCost);
			}
			else
			{
				// Check kit
				var item = creature.Inventory.GetItem(creature.Temp.CampfireKitItemEntityId);
				if (item == null)
					throw new ModerateViolation("Used CampfireKit with invalid kit.");

				// Reduce kit
				creature.Inventory.Decrement(item);
			}

			// Set up Campfire
			var pos = new Position(positionId);
			var effect = (skill.Info.Rank < SkillRank.RB ? "campfire_01" : "campfire_02");
			var prop = new Prop(PropId, creature.RegionId, pos.X, pos.Y, MabiMath.ByteToRadian(creature.Direction), 1); // Logs
			prop.State = "single";
			prop.Xml.SetAttributeValue("EFFECT", effect); // Fire effect
			prop.DisappearTime = DateTime.Now.AddMinutes(this.GetDuration(skill.Info.Rank, creature.RegionId)); // Disappear after x minutes

			// Temp data for Rest
			prop.Temp.CampfireSkillRank = skill.RankData;
			if (skill.Info.Id == SkillId.Campfire)
				prop.Temp.CampfireFirewood = AuraData.ItemDb.Find(creature.Temp.FirewoodItemId);

			creature.Region.AddProp(prop);

			// Complete
			Send.SkillComplete(creature, skill.Info.Id, positionId, unkInt1, unkInt2);
		}

		/// <summary>
		/// Canceles skill (no special actions required)
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		public void Cancel(Creature creature, Skill skill)
		{
		}

		/// <summary>
		/// Returns duration for rank in minutes.
		/// </summary>
		/// <param name="rank"></param>
		/// <returns></returns>
		private int GetDuration(SkillRank rank, int regionId)
		{
			var duration = 4;
			if (rank >= SkillRank.RC && rank <= SkillRank.R6)
				duration = 5;
			else if (rank >= SkillRank.R5)
				duration = 6;

			// Lower duration during rain
			var weatherType = ChannelServer.Instance.Weather.GetWeatherType(regionId);
			if (weatherType == WeatherType.Rain)
				duration /= 2; // Unofficial

			return duration;
		}
	}
}
