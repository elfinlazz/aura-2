// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.World.Entities;
using Aura.Shared.Mabi;
using Aura.Shared.Mabi.Const;

namespace Aura.Channel.Skills.Magic
{
	/// <summary>
	/// Handles the Mana Shield skill.
	/// </summary>
	/// <remarks>
	/// Var1: Base Mana Efficiency
	/// Var2: Mana Use / s, ManaUse has the same value.
	///   Important if we want to automate that.
	/// 
	/// Skill is stopped by client once Mana reaches 0.
	/// </remarks>
	[Skill(SkillId.ManaShield)]
	public class ManaShieldSkillHandler : StartStopSkillHandler
	{
		public override StartStopResult Start(Creature creature, Skill skill, MabiDictionary dict)
		{
			creature.Conditions.Activate(ConditionsA.ManaShield);
			Send.Effect(creature, Effect.ManaShield);

			creature.Regens.Add("ManaShield", Stat.Mana, -skill.RankData.Var2, creature.ManaMax);

			return StartStopResult.Okay;
		}

		public override StartStopResult Stop(Creature creature, Skill skill, MabiDictionary dict)
		{
			creature.Conditions.Deactivate(ConditionsA.ManaShield);

			creature.Regens.Remove("ManaShield");

			return StartStopResult.Okay;
		}
	}
}
