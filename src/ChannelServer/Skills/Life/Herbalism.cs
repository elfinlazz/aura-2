// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Skills.Base;
using Aura.Channel.World;
using Aura.Channel.World.Entities;
using Aura.Data.Database;
using Aura.Shared.Mabi.Const;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.Skills.Life
{
	/// <summary>
	/// Handles Herbalism training.
	/// </summary>
	[Skill(SkillId.Herbalism)]
	public class Herbalism : IInitiableSkillHandler, ISkillHandler
	{
		/// <summary>
		/// Bonus for success chance at a certain Herbalism rank.
		/// </summary>
		private const float HerbalismPickBonus = 25;

		/// <summary>
		/// Bonus for success chance at a certain Herbalism rank.
		/// </summary>
		private const float HerbalismIdentifyBonus = 50;

		/// <summary>
		/// Subscribes to events required for training.
		/// </summary>
		public void Init()
		{
			ChannelServer.Instance.Events.CreatureCollected += this.OnCreatureCollected;
		}

		/// <summary>
		/// Raised when creature collects something, handles gathering conditions.
		/// </summary>
		/// <param name="args"></param>
		private void OnCreatureCollected(CollectEventArgs args)
		{
			var skill = args.Creature.Skills.Get(SkillId.Herbalism);
			if (skill == null) return;

			if (skill.Info.Rank == SkillRank.Novice)
			{
				if (args.ItemId == 51104)
				{
					skill.Train(1); // Try to pick a Base Herb.
					if (args.Success)
						skill.Train(2); // Succeed at picking a Base Herb.
				}
			}
			else if (skill.Info.Rank == SkillRank.RF)
			{
				if (args.ItemId == 51104)
				{
					skill.Train(1); // Try to pick a Base Herb.
					if (args.Success)
						skill.Train(2); // Succeed at picking a Base Herb.
				}
				else if (args.ItemId == 51101)
				{
					skill.Train(3); // Try to pick a Bloody Herb.
					if (args.Success)
						skill.Train(4); // Succeed at picking a Bloody Herb.
				}
				else if (args.ItemId == 51103)
				{
					skill.Train(5); // Try to pick a Sunlight Herb.
					if (args.Success)
						skill.Train(6); // Succeed at picking a Sunlight Herb.
				}
			}
			else if (skill.Info.Rank == SkillRank.RE || skill.Info.Rank == SkillRank.RD)
			{
				if (args.ItemId == 51104)
				{
					if (args.Success)
						skill.Train(1); // Succeed at picking a Base Herb.
				}
				else if (args.ItemId == 51101)
				{
					skill.Train(2); // Try to pick a Bloody Herb.
					if (args.Success)
						skill.Train(3); // Succeed at picking a Bloody Herb.
				}
				else if (args.ItemId == 51103)
				{
					skill.Train(4); // Try to pick a Sunlight Herb.
					if (args.Success)
						skill.Train(5); // Succeed at picking a Sunlight Herb.
				}
				else if (args.ItemId == 51102)
				{
					skill.Train(6); // Try to pick a Mana Herb.
					if (args.Success)
						skill.Train(7); // Succeed at picking a Mana Herb.
				}
				else if (args.ItemId == 51105 && skill.Info.Rank == SkillRank.RD)
				{
					skill.Train(8); // Try to pick a Golden Herb.
				}
			}
			else if (skill.Info.Rank == SkillRank.RC)
			{
				if (args.ItemId == 51104)
				{
					if (args.Success)
						skill.Train(1); // Succeed at picking a Base Herb.
				}
				else if (args.ItemId == 51101)
				{
					if (args.Success)
						skill.Train(2); // Succeed at picking a Bloody Herb.
				}
				else if (args.ItemId == 51103)
				{
					if (args.Success)
						skill.Train(3); // Succeed at picking a Sunlight Herb.
				}
				else if (args.ItemId == 51102)
				{
					if (args.Success)
						skill.Train(4); // Succeed at picking a Mana Herb.
				}
				else if (args.ItemId == 51105)
				{
					if (args.Success)
						skill.Train(5); // Succeed at picking a Golden Herb.
				}
				else if (args.ItemId == 51107)
				{
					if (args.Success)
						skill.Train(6); // Succeed at picking a White Herb.
				}
			}
			else if (skill.Info.Rank == SkillRank.RB)
			{
				if (args.ItemId == 51103)
				{
					skill.Train(1); // Try to pick a Sunlight Herb.
					if (args.Success)
						skill.Train(2); // Succeed at picking a Sunlight Herb.
				}
				else if (args.ItemId == 51102)
				{
					skill.Train(3); // Try to pick a Mana Herb.
					if (args.Success)
						skill.Train(4); // Succeed at picking a Mana Herb.
				}
				else if (args.ItemId == 51105)
				{
					skill.Train(5); // Try to pick a Golden Herb.
					if (args.Success)
						skill.Train(6); // Succeed at picking a Golden Herb.
				}
				else if (args.ItemId == 51107)
				{
					if (args.Success)
						skill.Train(7); // Succeed at picking a White Herb.
				}
			}
			else if (skill.Info.Rank == SkillRank.RA)
			{
				if (args.ItemId == 51103)
				{
					if (args.Success)
						skill.Train(1); // Succeed at picking a Sunlight Herb.
				}
				else if (args.ItemId == 51102)
				{
					skill.Train(2); // Try to pick a Mana Herb.
					if (args.Success)
						skill.Train(3); // Succeed at picking a Mana Herb.
				}
				else if (args.ItemId == 51105)
				{
					skill.Train(4); // Try to pick a Golden Herb.
					if (args.Success)
						skill.Train(5); // Succeed at picking a Golden Herb.
				}
				else if (args.ItemId == 51107)
				{
					if (args.Success)
						skill.Train(6); // Succeed at picking a White Herb.
				}
			}
			else if (skill.Info.Rank >= SkillRank.R9 && skill.Info.Rank <= SkillRank.R6)
			{
				if (args.ItemId == 51103)
				{
					if (args.Success)
						skill.Train(1); // Succeed at picking a Sunlight Herb.
				}
				else if (args.ItemId == 51102)
				{
					if (args.Success)
						skill.Train(2); // Succeed at picking a Mana Herb.
				}
				else if (args.ItemId == 51105)
				{
					if (args.Success)
						skill.Train(3); // Succeed at picking a Golden Herb.
				}
				else if (args.ItemId == 51107)
				{
					if (args.Success)
						skill.Train(4); // Succeed at picking a White Herb.
				}
				else if (args.ItemId == 51110)
				{
					skill.Train(5); // Try to pick a Mandrake.
					if (args.Success)
						skill.Train(6); // Succeed at picking a Mandrake.
				}
			}
			else if (skill.Info.Rank == SkillRank.R5 || skill.Info.Rank == SkillRank.R4)
			{
				if (args.ItemId == 51102)
				{
					if (args.Success)
						skill.Train(1); // Succeed at picking a Mana Herb.
				}
				else if (args.ItemId == 51105)
				{
					if (args.Success)
						skill.Train(2); // Succeed at picking a Golden Herb.
				}
				else if (args.ItemId == 51110)
				{
					if (args.Success)
						skill.Train(3); // Succeed at picking a Mandrake.
				}
				else if (args.ItemId == 51108)
				{
					skill.Train(4); // Try to pick an Antidote Herb.
					if (args.Success)
						skill.Train(5); // Succeed at picking an Antidote Herb.
				}
			}
			else if (skill.Info.Rank >= SkillRank.R3 && skill.Info.Rank <= SkillRank.R1)
			{
				if (args.ItemId == 51105)
				{
					if (args.Success)
						skill.Train(1); // Succeed at picking a Golden Herb.
				}
				else if (args.ItemId == 51110)
				{
					if (args.Success)
						skill.Train(2); // Succeed at picking a Mandrake.
				}
				else if (args.ItemId == 51109)
				{
					skill.Train(3); // Try to pick a Poison Herb.
					if (args.Success)
						skill.Train(4); // Succeed at picking a Poison Herb.
				}
			}
		}
	}
}
