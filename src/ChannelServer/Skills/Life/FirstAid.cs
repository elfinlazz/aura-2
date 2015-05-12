// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.World.Entities;
using Aura.Mabi;
using Aura.Mabi.Const;
using Aura.Mabi.Network;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.Skills.Life
{
	/// <summary>
	/// First Aid skill handler
	/// </summary>
	/// <remarks>
	/// Var1: Min Injury Heal
	/// Var2: Max Injury Heal
	/// 
	/// We need a new inventory method to get an item of a specific class,
	/// in a specific order to get the best bandage candidate.
	/// </remarks>
	[Skill(SkillId.FirstAid)]
	public class FirstAid : ISkillHandler, IPreparable, IReadyable, IUseable, ICompletable, ICancelable
	{
		private const int BandageItemId = 60005;
		private const int Range = 500;
		private const float FailChance = 0.5f;

		/// <summary>
		/// Prepares skill, fails if no Bandage is found.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		/// <returns></returns>
		public bool Prepare(Creature creature, Skill skill, Packet packet)
		{
			var itemEntityId = 0L;
			if (packet.Peek() == PacketElementType.String)
				itemEntityId = MabiDictionary.Fetch<long>("ITEMID", packet.GetString());

			// TODO: Get bandage if not item skill
			// Bandages are used starting from the bottom-right corner.
			// Higher quality bandages will take priority over lower quality bandages regardless of placement.
			// ~_______~
			// GetItem(ByTag) (starting in lower right?)

			// TODO: Check actual bandage
			if (creature.Inventory.Count(BandageItemId) == 0)
			{
				Send.Notice(creature, Localization.Get("You need more than one Bandage."));
				return false;
			}

			Send.SkillInitEffect(creature, null);
			Send.SkillPrepare(creature, skill.Info.Id, skill.GetCastTime());

			return true;
		}

		/// <summary>
		/// Readies the skill.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		/// <returns></returns>
		public bool Ready(Creature creature, Skill skill, Packet packet)
		{
			Send.SkillReady(creature, skill.Info.Id);

			return true;
		}

		/// <summary>
		/// Uses skill, the actual usage is in Complete.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public void Use(Creature creature, Skill skill, Packet packet)
		{
			var entityId = packet.GetLong();
			var unkInt1 = packet.GetInt();
			var unkInt2 = packet.GetInt();

			// Do checks in Complete.

			Send.SkillUse(creature, skill.Info.Id, entityId, unkInt1, unkInt2);
		}

		/// <summary>
		/// Completes skill, healing the target.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public void Complete(Creature creature, Skill skill, Packet packet)
		{
			var entityId = packet.GetLong();
			var unkInt1 = packet.GetInt();
			var unkInt2 = packet.GetInt();

			// Get target
			var target = ChannelServer.Instance.World.GetCreature(entityId);
			if (target == null)
			{
				Send.Notice(creature, Localization.Get("Invalid target."));
				goto L_End;
			}

			// Check range
			if (!creature.GetPosition().InRange(target.GetPosition(), Range))
			{
				Send.Notice(creature, Localization.Get("Out of range."));
				goto L_End;
			}

			// Remove bandage
			// TODO: Remove actual bandage
			if (!creature.Inventory.Remove(BandageItemId, 1))
			{
				Send.Notice(creature, Localization.Get("Invalid bandage."));
				goto L_End;
			}

			var rnd = RandomProvider.Get();

			// Can fail if target is moving.
			if (target.IsMoving && rnd.NextDouble() < FailChance)
			{
				// Unofficial
				Send.Notice(creature, Localization.Get("The target moved."));
				// Fail motion?
				goto L_End;
			}

			// Heal injuries
			var heal = rnd.Next((int)skill.RankData.Var1, (int)skill.RankData.Var2 + 1);

			target.Injuries -= heal;
			Send.StatUpdateDefault(creature);

			// Skill training
			if (skill.Info.Rank == SkillRank.Novice)
				skill.Train(1); // Use First Aid.

			// First Aid animation
			Send.Effect(creature, Effect.UseMagic, "healing_firstaid", entityId);

		L_End:
			Send.SkillComplete(creature, skill.Info.Id, entityId, unkInt1, unkInt2);
		}

		/// <summary>
		/// Cancels skill, by doing nothing special at all.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		public void Cancel(Creature creature, Skill skill)
		{
		}
	}
}
