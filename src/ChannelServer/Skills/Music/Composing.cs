// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.World.Entities;
using Aura.Shared.Mabi;
using Aura.Shared.Mabi.Const;
using Aura.Shared.Network;

namespace Aura.Channel.Skills.Music
{
	/// <summary>
	/// Composing skill handler
	/// </summary>
	/// <remarks>
	/// Goes straight to Complete from Prepare, by sending the response of Use.
	/// 
	/// Var 1: Melody max length
	/// Var 2: Harmony 1 max length
	/// Var 3: Harmony 2 max length
	/// Var 4: Magical Effect chance
	/// </remarks>
	[Skill(SkillId.Composing)]
	public class Composing : IPreparable, ICompletable, ICancelable
	{
		/// <summary>
		/// Maximum length of the MML.
		/// </summary>
		private const int MmlMaxLength = 10000;

		/// <summary>
		/// Prepares the skill, called after entering the MML,
		/// goes straight to Use.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public bool Prepare(Creature creature, Skill skill, Packet packet)
		{
			var scrollId = packet.GetLong();
			var title = packet.GetString();
			var author = packet.GetString();
			var mml = packet.GetString();
			var song = packet.GetString(); // [180300, NA166 (18.09.2013)] Singing
			var hidden = packet.GetByte(); // bool, but the meta data is a byte

			// Get item
			var item = creature.Inventory.GetItem(scrollId);
			if (item == null) goto L_Fail;

			// Get all parts of the MML (Melody, Haromony 1+2)
			var mmlParts = mml.Split(',');

			// Check lengths
			if (mml.Length > MmlMaxLength || song.Length > MmlMaxLength) goto L_Fail; // Total
			if (mmlParts.Length > 0 && mmlParts[0].Length > skill.RankData.Var1) goto L_Fail; // Melody
			if (mmlParts.Length > 1 && mmlParts[1].Length > skill.RankData.Var2) goto L_Fail; // Harmony 1
			if (mmlParts.Length > 2 && mmlParts[2].Length > skill.RankData.Var3) goto L_Fail; // Harmony 2

			// Score level = Musical Knowledge rank
			var level = SkillRank.Novice;
			var knowledge = creature.Skills.Get(SkillId.MusicalKnowledge);
			if (knowledge != null) level = knowledge.Info.Rank;

			// Update item and send skill complete from Complete
			creature.Skills.Callback(SkillId.Composing, () =>
			{
				item.MetaData1.SetString("TITLE", title);
				item.MetaData1.SetString("AUTHOR", author);
				item.MetaData1.SetString("SCORE", MabiZip.Compress(mml));
				item.MetaData1.SetString("SCSING", MabiZip.Compress(song)); // [180300, NA166 (18.09.2013)] Singing
				item.MetaData1.SetByte("HIDDEN", hidden);
				item.MetaData1.SetShort("LEVEL", (short)level);

				Send.ItemUpdate(creature, item);
				Send.SkillCompleteEntity(creature, skill.Info.Id, item.EntityId);
			});

			// Finish
			Send.SkillUseEntity(creature, skill.Info.Id, scrollId);
			skill.State = SkillState.Used;
			return true;

		L_Fail:
			return false;
		}

		/// <summary>
		/// Completes skill, writing the MML to the item.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public void Complete(Creature creature, Skill skill, Packet packet)
		{
			creature.Skills.Callback(SkillId.Composing);
		}

		/// <summary>
		/// Cancels skill (do nothing).
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		public void Cancel(Creature creature, Skill skill)
		{
		}
	}
}
