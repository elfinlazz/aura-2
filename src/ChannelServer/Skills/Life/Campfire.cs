// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.Util;
using Aura.Channel.World;
using Aura.Channel.World.Entities;
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
	[Skill(SkillId.Campfire)]
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

		public bool Prepare(Creature creature, Skill skill, Packet packet)
		{
			var itemId = packet.GetInt();

			Send.SkillPrepare(creature, skill.Info.Id, itemId);

			return true;
		}

		public bool Ready(Creature creature, Skill skill, Packet packet)
		{
			creature.Temp.FirewoodItemId = packet.GetInt();

			Send.SkillReady(creature, skill.Info.Id, creature.Temp.FirewoodItemId);

			return true;
		}

		public void Use(Creature creature, Skill skill, Packet packet)
		{
			var positionId = packet.GetLong();
			var unkInt1 = packet.GetInt();
			var unkInt2 = packet.GetInt();

			Send.SkillUse(creature, skill.Info.Id, positionId, unkInt1, unkInt2);
		}

		public void Complete(Creature creature, Skill skill, Packet packet)
		{
			var positionId = packet.GetLong();
			var unkInt1 = packet.GetInt();
			var unkInt2 = packet.GetInt();

			// Check Firewood, the client should stop the player long before Complete.
			if (creature.Inventory.Count(creature.Temp.FirewoodItemId) < FirewoodCost)
				throw new ModerateViolation("Used Campfire without Firewood.");

			// Remove Firewood
			// TODO: Use the item id from Ready.
			creature.Inventory.Remove(creature.Temp.FirewoodItemId, FirewoodCost);

			// Set up Campfire
			var pos = new Position(positionId);
			var prop = new Prop(PropId, creature.RegionId, pos.X, pos.Y, MabiMath.ByteToRadian(creature.Direction), 1); // Logs
			prop.State = "single";
			prop.Xml.SetAttributeValue("EFFECT", "campfire_01"); // Fire effect
			prop.DisappearTime = DateTime.Now.AddMinutes(this.GetDuration(skill.Info.Rank, creature.RegionId)); // Disappear after x minutes

			creature.Region.AddProp(prop);

			// Complete
			Send.SkillComplete(creature, skill.Info.Id, positionId, unkInt1, unkInt2);
		}

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
				duration /= 2; // ?

			return duration;
		}
	}
}
