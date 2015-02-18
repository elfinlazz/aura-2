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
		/// Calculates bonus chance for Herbalism, based on rank.
		/// </summary>
		/// <remarks>
		/// Reference: http://wiki.mabinogiworld.com/view/Herbalism#Details
		/// 
		/// This actually seems to be incorrect and will require more research.
		/// Base patches on fields have a 15% chance with Herbalism on Novice
		/// and 50% with Herbalism on rF, according to the devCAT title debug
		/// output. With what we have here we'd get 0% on Novice and 75% on rF
		/// (plus the Production Mastery bonus.)
		/// </remarks>
		/// <param name="creature"></param>
		/// <param name="collectData"></param>
		/// <returns></returns>
		public static float GetChance(Creature creature, CollectingData collectData)
		{
			var successChance = 0f;

			var herbalism = creature.Skills.Get(SkillId.Herbalism);
			if (herbalism == null)
				return successChance;

			if (collectData.Target.Contains("/baseherb"))
			{
				if (herbalism.Info.Rank >= SkillRank.RF)
				{
					successChance += HerbalismPickBonus;
					successChance += HerbalismIdentifyBonus;
				}
			}
			else if (collectData.Target.Contains("/bloodyherb"))
			{
				if (herbalism.Info.Rank >= SkillRank.RF)
					successChance += HerbalismPickBonus;

				if (herbalism.Info.Rank >= SkillRank.RC)
					successChance += HerbalismIdentifyBonus;
			}
			else if (collectData.Target.Contains("/sunlightherb"))
			{
				if (herbalism.Info.Rank >= SkillRank.RF)
					successChance += HerbalismPickBonus;

				if (herbalism.Info.Rank >= SkillRank.RB)
					successChance += HerbalismIdentifyBonus;
			}
			else if (collectData.Target.Contains("/manaherb"))
			{
				if (herbalism.Info.Rank >= SkillRank.RF)
					successChance += HerbalismPickBonus;

				if (herbalism.Info.Rank >= SkillRank.R9)
					successChance += HerbalismIdentifyBonus;
			}
			else if (collectData.Target.Contains("/whiteherb"))
			{
				if (herbalism.Info.Rank >= SkillRank.RE)
					successChance += HerbalismPickBonus;

				if (herbalism.Info.Rank >= SkillRank.R6)
					successChance += HerbalismIdentifyBonus;
			}
			else if (collectData.Target.Contains("/goldherb"))
			{
				if (herbalism.Info.Rank >= SkillRank.RD)
					successChance += HerbalismPickBonus;

				if (herbalism.Info.Rank >= SkillRank.R3)
					successChance += HerbalismIdentifyBonus;
			}
			else if (collectData.Target.Contains("/ivoryherb"))
			{
				if (herbalism.Info.Rank >= SkillRank.RC)
					successChance += HerbalismPickBonus;

				if (herbalism.Info.Rank >= SkillRank.R5)
					successChance += HerbalismIdentifyBonus;
			}
			else if (collectData.Target.Contains("/purpleherb"))
			{
				if (herbalism.Info.Rank >= SkillRank.RB)
					successChance += HerbalismPickBonus;

				if (herbalism.Info.Rank >= SkillRank.R3)
					successChance += HerbalismIdentifyBonus;
			}
			else if (collectData.Target.Contains("/orangeherb/"))
			{
				if (herbalism.Info.Rank >= SkillRank.RA)
					successChance += HerbalismPickBonus;

				if (herbalism.Info.Rank >= SkillRank.R1)
					successChance += HerbalismIdentifyBonus;
			}

			return successChance;
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
				if (args.CollectData.Target.Contains("/baseherb"))
					skill.Train(1); // Try to pick a Base Herb.

				if (args.ItemId == 51104)
				{
					if (args.Success)
						skill.Train(2); // Succeed at picking a Base Herb.
				}
			}
			else if (skill.Info.Rank == SkillRank.RF)
			{
				if (args.CollectData.Target.Contains("/baseherb"))
					skill.Train(1); // Try to pick a Base Herb.
				else if (args.CollectData.Target.Contains("/bloodyherb"))
					skill.Train(3); // Try to pick a Bloody Herb.
				else if (args.CollectData.Target.Contains("/sunlightherb"))
					skill.Train(5); // Try to pick a Sunlight Herb.

				if (args.Success)
				{
					if (args.ItemId == 51104)
						skill.Train(2); // Succeed at picking a Base Herb.
					else if (args.ItemId == 51101)
						skill.Train(4); // Succeed at picking a Bloody Herb.
					else if (args.ItemId == 51103)
						skill.Train(6); // Succeed at picking a Sunlight Herb.
				}
			}
			else if (skill.Info.Rank == SkillRank.RE || skill.Info.Rank == SkillRank.RD)
			{
				if (args.CollectData.Target.Contains("/bloodyherb"))
					skill.Train(2); // Try to pick a Bloody Herb.
				else if (args.CollectData.Target.Contains("/sunlightherb"))
					skill.Train(4); // Try to pick a Sunlight Herb.
				else if (args.CollectData.Target.Contains("/manaherb"))
					skill.Train(6); // Try to pick a Mana Herb.
				else if (args.CollectData.Target.Contains("/goldherb") && skill.Info.Rank == SkillRank.RD)
					skill.Train(8); // Try to pick a Golden Herb.

				if (args.Success)
				{
					if (args.ItemId == 51104)
						skill.Train(1); // Succeed at picking a Base Herb.
					else if (args.ItemId == 51101)
						skill.Train(3); // Succeed at picking a Bloody Herb.
					else if (args.ItemId == 51103)
						skill.Train(5); // Succeed at picking a Sunlight Herb.
					else if (args.ItemId == 51102)
						skill.Train(7); // Succeed at picking a Mana Herb.
				}
			}
			else if (skill.Info.Rank == SkillRank.RC)
			{
				if (args.Success)
				{
					if (args.ItemId == 51104)
						skill.Train(1); // Succeed at picking a Base Herb.
					else if (args.ItemId == 51101)
						skill.Train(2); // Succeed at picking a Bloody Herb.
					else if (args.ItemId == 51103)
						skill.Train(3); // Succeed at picking a Sunlight Herb.
					else if (args.ItemId == 51102)
						skill.Train(4); // Succeed at picking a Mana Herb.
					else if (args.ItemId == 51105)
						skill.Train(5); // Succeed at picking a Golden Herb.
					else if (args.ItemId == 51107)
						skill.Train(6); // Succeed at picking a White Herb.
				}
			}
			else if (skill.Info.Rank == SkillRank.RB)
			{
				if (args.CollectData.Target.Contains("/sunlightherb"))
					skill.Train(1); // Try to pick a Sunlight Herb.
				else if (args.CollectData.Target.Contains("/manaherb"))
					skill.Train(3); // Try to pick a Mana Herb.
				else if (args.CollectData.Target.Contains("/goldherb"))
					skill.Train(5); // Try to pick a Golden Herb.

				if (args.Success)
				{
					if (args.ItemId == 51103)
						skill.Train(2); // Succeed at picking a Sunlight Herb.
					else if (args.ItemId == 51102)
						skill.Train(4); // Succeed at picking a Mana Herb.
					else if (args.ItemId == 51105)
						skill.Train(6); // Succeed at picking a Golden Herb.
					else if (args.ItemId == 51107)
						skill.Train(7); // Succeed at picking a White Herb.
				}
			}
			else if (skill.Info.Rank == SkillRank.RA)
			{
				if (args.CollectData.Target.Contains("/manaherb"))
					skill.Train(2); // Try to pick a Mana Herb.
				else if (args.CollectData.Target.Contains("/goldherb"))
					skill.Train(4); // Try to pick a Golden Herb.

				if (args.Success)
				{
					if (args.ItemId == 51103)
						skill.Train(1); // Succeed at picking a Sunlight Herb.
					else if (args.ItemId == 51102)
						skill.Train(3); // Succeed at picking a Mana Herb.
					else if (args.ItemId == 51105)
						skill.Train(5); // Succeed at picking a Golden Herb.
					else if (args.ItemId == 51107)
						skill.Train(6); // Succeed at picking a White Herb.
				}
			}
			else if (skill.Info.Rank >= SkillRank.R9 && skill.Info.Rank <= SkillRank.R6)
			{
				if (args.CollectData.Target.Contains("/orangeherb"))
					skill.Train(5); // Try to pick a Mandrake.

				if (args.Success)
				{
					if (args.ItemId == 51103)
						skill.Train(1); // Succeed at picking a Sunlight Herb.
					else if (args.ItemId == 51102)
						skill.Train(2); // Succeed at picking a Mana Herb.
					else if (args.ItemId == 51105)
						skill.Train(3); // Succeed at picking a Golden Herb.
					else if (args.ItemId == 51107)
						skill.Train(4); // Succeed at picking a White Herb.
					else if (args.ItemId == 51110)
						skill.Train(6); // Succeed at picking a Mandrake.
				}
			}
			else if (skill.Info.Rank == SkillRank.R5 || skill.Info.Rank == SkillRank.R4)
			{
				if (args.CollectData.Target.Contains("/ivoryherb"))
					skill.Train(4); // Try to pick an Antidote Herb.

				if (args.Success)
				{
					if (args.ItemId == 51102)
						skill.Train(1); // Succeed at picking a Mana Herb.
					else if (args.ItemId == 51105)
						skill.Train(2); // Succeed at picking a Golden Herb.
					else if (args.ItemId == 51110)
						skill.Train(3); // Succeed at picking a Mandrake.
					else if (args.ItemId == 51108)
						skill.Train(5); // Succeed at picking an Antidote Herb.
				}
			}
			else if (skill.Info.Rank >= SkillRank.R3 && skill.Info.Rank <= SkillRank.R1)
			{
				if (args.CollectData.Target.Contains("/purpleherb"))
					skill.Train(3); // Try to pick a Poison Herb.

				if (args.Success)
				{
					if (args.ItemId == 51105)
						skill.Train(1); // Succeed at picking a Golden Herb.
					else if (args.ItemId == 51110)
						skill.Train(2); // Succeed at picking a Mandrake.
					else if (args.ItemId == 51109)
						skill.Train(4); // Succeed at picking a Poison Herb.
				}
			}
		}
	}
}
