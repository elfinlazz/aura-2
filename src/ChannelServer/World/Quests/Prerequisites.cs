// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.World.Entities;

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

			foreach (var p in this.Prerequisites)
			{
				if (!p.Met(character))
					return false;
			}

			return true;
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

			foreach (var p in this.Prerequisites)
			{
				if (p.Met(character))
					return true;
			}

			return false;
		}
	}
}
