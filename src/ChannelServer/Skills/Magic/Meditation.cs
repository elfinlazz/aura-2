// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Skills.Base;
using Aura.Channel.World.Entities;
using Aura.Mabi;
using Aura.Mabi.Const;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.Skills.Magic
{
	/// <summary>
	/// Meditation skill handler
	/// </summary>
	/// <remarks>
	/// Var1: MP regeneration rate
	/// </remarks>
	[Skill(SkillId.Meditation)]
	public class Meditation : StartStopSkillHandler
	{
		/// <summary>
		/// Starts Meditation.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="dict"></param>
		/// <returns></returns>
		public override StartStopResult Start(Creature creature, Skill skill, MabiDictionary dict)
		{
			var rate = (skill.RankData.Var1 / 100f) * 0.05f;

			// TODO: Night?

			// "Disable" stm regen, triple hunger
			creature.Regens.Add("Meditation", Stat.Stamina, -0.4f, creature.StaminaMax);
			if (ChannelServer.Instance.Conf.World.EnableHunger)
				creature.Regens.Add("Meditation", Stat.Hunger, 0.02f, creature.StaminaMax);

			// Add mana regen
			creature.Regens.Add("Meditation", Stat.Mana, rate, creature.ManaMax);
			creature.Conditions.Activate(ConditionsE.Meditation);

			return StartStopResult.Okay;
		}

		/// <summary>
		/// Stops meditation.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="dict"></param>
		/// <returns></returns>
		public override StartStopResult Stop(Creature creature, Skill skill, MabiDictionary dict)
		{
			creature.Regens.Remove("Meditation");
			creature.Conditions.Deactivate(ConditionsE.Meditation);

			return StartStopResult.Okay;
		}
	}
}
