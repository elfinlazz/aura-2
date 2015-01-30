// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Linq;
using Aura.Data;
using Aura.Shared.Mabi.Const;
using Aura.Shared.Mabi;
using Aura.Channel.World.Entities;

namespace Aura.Channel.World.Quests
{
	public abstract class QuestObjective
	{
		public string Ident { get; set; }
		public string Description { get; set; }

		public int Amount { get; set; }

		public int RegionId { get; set; }
		public int X { get; set; }
		public int Y { get; set; }

		public MabiDictionary MetaData { get; protected set; }

		public abstract ObjectiveType Type { get; }

		protected QuestObjective(int amount)
		{
			this.MetaData = new MabiDictionary();
			this.Amount = amount;
		}
	}

	/// <summary>
	/// Objective to kill creatures of a race type.
	/// </summary>
	public class QuestObjectiveKill : QuestObjective
	{
		public override ObjectiveType Type { get { return ObjectiveType.Kill; } }

		public string[] RaceTypes { get; set; }

		public QuestObjectiveKill(int amount, params string[] raceTypes)
			: base(amount)
		{
			this.RaceTypes = raceTypes;

			this.MetaData.SetString("TGTSID", string.Join("|", raceTypes));
			this.MetaData.SetInt("TARGETCOUNT", amount);
			this.MetaData.SetShort("TGTCLS", 0);
		}

		/// <summary>
		/// Returns true if creature matches one of the race types.
		/// </summary>
		/// <param name="killedCreature"></param>
		/// <returns></returns>
		public bool Check(Creature killedCreature)
		{
			return this.RaceTypes.Any(type => killedCreature.RaceData.HasTag(type));
		}
	}

	/// <summary>
	/// Objective to collect a certain item.
	/// </summary>
	public class QuestObjectiveCollect : QuestObjective
	{
		public override ObjectiveType Type { get { return ObjectiveType.Collect; } }

		public int ItemId { get; set; }

		public QuestObjectiveCollect(int itemId, int amount)
			: base(amount)
		{
			this.ItemId = itemId;
			this.Amount = amount;

			this.MetaData.SetInt("TARGETITEM", itemId);
			this.MetaData.SetInt("TARGETCOUNT", amount);
			this.MetaData.SetInt("QO_FLAG", 1);
		}
	}

	/// <summary>
	/// Objective to talk to a specific NPC.
	/// </summary>
	public class QuestObjectiveTalk : QuestObjective
	{
		public override ObjectiveType Type { get { return ObjectiveType.Talk; } }

		public string Name { get; set; }

		public QuestObjectiveTalk(string npcName)
			: base(1)
		{
			this.Name = npcName;

			this.MetaData.SetString("TARGECHAR", npcName);
			this.MetaData.SetInt("TARGETCOUNT", 1);
		}
	}

	/// <summary>
	/// Objective to talk to a specific NPC.
	/// </summary>
	public class QuestObjectiveReachRank : QuestObjective
	{
		public override ObjectiveType Type { get { return ObjectiveType.ReachRank; } }

		public SkillId Id { get; set; }
		public SkillRank Rank { get; set; }

		public QuestObjectiveReachRank(SkillId skillId, SkillRank rank)
			: base(1)
		{
			this.Id = skillId;
			this.Rank = rank;

			this.MetaData.SetUShort("TGTSKL", (ushort)skillId);
			this.MetaData.SetShort("TGTLVL", (short)rank);
			this.MetaData.SetInt("TARGETCOUNT", 1);
		}
	}
}
