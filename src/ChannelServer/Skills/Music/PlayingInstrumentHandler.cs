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
using Aura.Data.Database;
using Aura.Shared.Mabi.Const;
using Aura.Shared.Network;
using Aura.Shared.Util;

namespace Aura.Channel.Skills.Music
{
	/// <summary>
	/// Playing Instrument skill handler
	/// </summary>
	/// <remarks>
	/// Prepare starts the playing, complete is sent once it's over.
	/// </remarks>
	[Skill(SkillId.PlayingInstrument)]
	public class PlayingInstrumentHandler : IPreparable, ICompletable, ICancelable
	{
		private const int RandomScoreMin = 1, RandomScoreMax = 52;
		private const int DurabilityUse = 1000;

		public void Prepare(Creature creature, Skill skill, Packet packet)
		{
			var rnd = RandomProvider.Get();

			// Check for instrument
			if (creature.RightHand == null || creature.RightHand.Data.Type != ItemType.Instrument)
			{
				Send.SkillPrepareSilentCancel(creature, skill.Info.Id);
				return;
			}

			creature.StopMove();

			// Score scrolls go into the magazine pocket and need a SCORE tag.
			var hasScroll = (creature.Magazine != null && creature.Magazine.MetaData1.Has("SCORE") && creature.Magazine.OptionInfo.Durability >= DurabilityUse);
			string mml = (hasScroll ? creature.Magazine.MetaData1.GetString("SCORE") : null);

			// Random score if no usable scroll was found.
			var rndScore = (!hasScroll ? rnd.Next(RandomScoreMin, RandomScoreMax + 1) : 0);

			// Quality seems to go from 0 (worst) to 3 (best).
			var quality = (PlayingQuality)rnd.Next((int)PlayingQuality.VeryBad, (int)PlayingQuality.VeryGood + 1);

			// Up quality by chance, based on Musical Knowledge
			var musicalKnowledgeSkill = creature.Skills.Get(SkillId.MusicalKnowledge);
			if (musicalKnowledgeSkill != null && rnd.Next(100) < musicalKnowledgeSkill.RankData.Var2)
				quality++;

			if (quality > PlayingQuality.VeryGood)
				quality = PlayingQuality.VeryGood;

			// Reduce scroll's durability.
			if (hasScroll)
				creature.Inventory.ReduceDurability(creature.Magazine, DurabilityUse);

			// Music effect and Use
			Send.PlayEffect(creature, creature.RightHand.Data.InstrumentType, quality, mml, rndScore);
			Send.SkillUsePlayingInstrument(creature, skill.Info.Id, creature.RightHand.Data.InstrumentType, mml, rndScore);

			creature.Skills.ActiveSkill = skill;
			creature.Skills.Callback(SkillId.PlayingInstrument, () => Send.Notice(creature, this.GetRandomQualityMessage(quality)));

			creature.Regens.Add("PlayingInstrument", Stat.Stamina, skill.RankData.StaminaActive, creature.StaminaMax);
		}

		public void Complete(Creature creature, Skill skill, Packet packet)
		{
			creature.Skills.ActiveSkill = null;

			this.Cancel(creature, skill);

			creature.Skills.Callback(SkillId.PlayingInstrument);

			Send.SkillComplete(creature, skill.Info.Id);
		}

		public void Cancel(Creature creature, Skill skill)
		{
			Send.Effect(creature, Effect.StopMusic);
			creature.Regens.Remove("PlayingInstrument");
		}

		/// <summary>
		/// Returns a random result message for the given quality.
		/// </summary>
		/// <param name="quality"></param>
		/// <returns></returns>
		private string GetRandomQualityMessage(PlayingQuality quality)
		{
			string[] msgs = null;
			switch (quality)
			{
				// Messages are stored in one line per quality, seperated by semicolons.
				case PlayingQuality.VeryGood: msgs = Localization.Get("skills.quality_verygood").Split(';'); break;
				case PlayingQuality.Good: msgs = Localization.Get("skills.quality_good").Split(';'); break;
				case PlayingQuality.Bad: msgs = Localization.Get("skills.quality_bad").Split(';'); break;
				case PlayingQuality.VeryBad: msgs = Localization.Get("skills.quality_verybad").Split(';'); break;
			}

			if (msgs == null || msgs.Length < 1)
				return "...";

			return msgs[RandomProvider.Get().Next(0, msgs.Length)].Trim();
		}
	}
}
