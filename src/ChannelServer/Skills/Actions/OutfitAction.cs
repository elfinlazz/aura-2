// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.World.Entities;
using Aura.Shared.Mabi;
using Aura.Shared.Mabi.Const;

namespace Aura.Channel.Skills.Actions
{
	/// <summary>
	/// Handle for the Outfit Action, available for items that have
	/// the "cloth_action" tag?
	/// </summary>
	[Skill(SkillId.OutfitAction)]
	public class OutfitAction : StartStopSkillHandler
	{
		/// <summary>
		/// Starts skill.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="dict"></param>
		/// <returns></returns>
		public override StartStopResult Start(Creature creature, Skill skill, MabiDictionary dict)
		{
			Send.Effect(creature, Effect.OutfitAction, true);

			return StartStopResult.Okay;
		}

		/// <summary>
		/// Stop skills.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="dict"></param>
		/// <returns></returns>
		public override StartStopResult Stop(Creature creature, Skill skill, MabiDictionary dict)
		{
			return StartStopResult.Okay;
		}
	}
}
