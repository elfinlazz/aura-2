// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Linq;
using Aura.Channel.World.Entities;
using Aura.Shared.Mabi.Const;

namespace Aura.Channel.World.Quests
{
	public abstract class QuestPrerequisite
	{
		public abstract bool Met(Creature character);
	}

	/// <summary>
	/// Quest prerequisite, met if quest is complete.
	/// </summary>
	public class QuestPrerequisiteQuestCompleted : QuestPrerequisite
	{
		public int Id { get; protected set; }

		public QuestPrerequisiteQuestCompleted(int id)
		{
			this.Id = id;
		}

		public override bool Met(Creature character)
		{
			return character.Quests.IsComplete(this.Id);
		}
	}

	/// <summary>
	/// Level prerequisite, met if character's level is greater or equal.
	/// </summary>
	public class QuestPrerequisiteReachedLevel : QuestPrerequisite
	{
		public int Level { get; protected set; }

		public QuestPrerequisiteReachedLevel(int level)
		{
			this.Level = level;
		}

		public override bool Met(Creature character)
		{
			return (character.Level >= this.Level);
		}
	}

	/// <summary>
	/// Total Level prerequisite, met if character's total level is greater or equal.
	/// </summary>
	public class QuestPrerequisiteReachedTotalLevel : QuestPrerequisite
	{
		public int Level { get; protected set; }

		public QuestPrerequisiteReachedTotalLevel(int level)
		{
			this.Level = level;
		}

		public override bool Met(Creature character)
		{
			return (character.TotalLevel >= this.Level);
		}
	}

	/// <summary>
	/// Skill prerequisite, met if character doesn't have the skill or rank yet.
	/// </summary>
	public class QuestPrerequisiteNotSkill : QuestPrerequisite
	{
		public SkillId Id { get; protected set; }
		public SkillRank Rank { get; protected set; }

		public QuestPrerequisiteNotSkill(SkillId skillId, SkillRank rank = SkillRank.Novice)
		{
			this.Id = skillId;
			this.Rank = rank;
		}

		public override bool Met(Creature character)
		{
			return !character.Skills.Has(this.Id, this.Rank);
		}
	}

	/// <summary>
	/// Collection of prerequisites, met if all are met.
	/// </summary>
	public class QuestPrerequisiteAnd : QuestPrerequisite
	{
		public QuestPrerequisite[] Prerequisites { get; protected set; }

		public QuestPrerequisiteAnd(params QuestPrerequisite[] prerequisites)
		{
			this.Prerequisites = prerequisites;
		}

		public override bool Met(Creature character)
		{
			if (this.Prerequisites.Length == 0)
				return true;

			return this.Prerequisites.All(p => p.Met(character));
		}
	}

	/// <summary>
	/// Collection of prerequisites, met if at least one of them is met.
	/// </summary>
	public class QuestPrerequisiteOr : QuestPrerequisite
	{
		public QuestPrerequisite[] Prerequisites { get; protected set; }

		public QuestPrerequisiteOr(params QuestPrerequisite[] prerequisites)
		{
			this.Prerequisites = prerequisites;
		}

		public override bool Met(Creature character)
		{
			if (this.Prerequisites.Length == 0)
				return true;

			return this.Prerequisites.Any(p => p.Met(character));
		}
	}

	/// <summary>
	/// Inverts the return of a prerequisite's Met()
	/// </summary>
	public class QuestPrerequisiteNot : QuestPrerequisite
	{
		protected QuestPrerequisite _prereq;

		public QuestPrerequisiteNot(QuestPrerequisite prerequiste)
		{
			_prereq = prerequiste;
		}

		public override bool Met(Creature character)
		{
			return !_prereq.Met(character);
		}
	}
}
