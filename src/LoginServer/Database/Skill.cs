// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Shared.Mabi.Const;

namespace Aura.Login.Database
{
	public class Skill
	{
		public SkillId Id { get; set; }
		public SkillRank Rank { get; set; }

		public Skill(SkillId id, SkillRank rank = SkillRank.Novice)
		{
			this.Id = id;
			this.Rank = rank;
		}
	}
}
