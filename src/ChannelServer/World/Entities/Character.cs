// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

namespace Aura.Channel.World.Entities
{
	/// <summary>
	/// Normal character
	/// </summary>
	public class Character : PlayerCreature
	{
		/// <summary>
		/// Returns whether creature is able to learn skills automatically
		/// (e.g. Counterattack).
		/// </summary>
		public override bool LearningSkillsEnabled { get { return true; } }
	}
}
