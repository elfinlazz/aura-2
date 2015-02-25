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
	/// Handles Production Mastery training.
	/// </summary>
	[Skill(SkillId.ProductionMastery)]
	public class ProductionMastery : IInitiableSkillHandler, ISkillHandler
	{
		/// <summary>
		/// Subscribes to events required for training.
		/// </summary>
		public void Init()
		{
			ChannelServer.Instance.Events.CreatureCollected += this.OnCreatureCollected;
		}

		/// <summary>
		/// Returns success chance, increased according to creature's
		/// Production Mastery rank.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="chance"></param>
		public static float IncreaseChance(Creature creature, float chance)
		{
			var skill = creature.Skills.Get(SkillId.ProductionMastery);
			if (skill != null)
				chance += skill.RankData.Var1;

			return chance;
		}

		/// <summary>
		/// Raised when creature collects something, handles gathering conditions.
		/// </summary>
		/// <param name="args"></param>
		private void OnCreatureCollected(CollectEventArgs args)
		{
			var skill = args.Creature.Skills.Get(SkillId.ProductionMastery);
			if (skill == null || !args.Success) return;

			skill.Train(1); // Collect any material without using a skill.

			if (skill.Info.Rank == SkillRank.R9 && args.ItemId == 51101)
				skill.Train(5); // Successfully pick a Bloody Herb.

			if (skill.Info.Rank == SkillRank.R8 && args.ItemId == 51103)
				skill.Train(5); // Successfully pick a Sunlight Herb

			if (skill.Info.Rank == SkillRank.R7 && args.ItemId == 51102)
				skill.Train(5); // Successfully pick a Mana Herb.

			if (skill.Info.Rank == SkillRank.R6 && args.ItemId == 51105)
				skill.Train(5); // Successfully pick a Golden Herb.
		}
	}
}
