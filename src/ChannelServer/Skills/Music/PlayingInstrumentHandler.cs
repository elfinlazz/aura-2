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

			// Get instrument type
			var instrumentType = this.GetInstrumentType(creature);

			// TODO: Make db for instruments with installable props.

			// Get mml from equipped score scroll if available.
			var mml = this.GetScore(creature);

			// Random score if no usable scroll was found.
			var rndScore = (mml == null ? this.GetRandomScore(rnd) : 0);

			// Quality seems to go from 0 (worst) to 3 (best).
			// TODO: Base quality on skills and score ranks.
			var quality = (PlayingQuality)rnd.Next((int)PlayingQuality.VeryBad, (int)PlayingQuality.VeryGood + 1);

			// Up quality by chance, based on Musical Knowledge
			var musicalKnowledgeSkill = creature.Skills.Get(SkillId.MusicalKnowledge);
			if (musicalKnowledgeSkill != null && rnd.Next(100) < musicalKnowledgeSkill.RankData.Var2)
				quality++;

			if (quality > PlayingQuality.VeryGood)
				quality = PlayingQuality.VeryGood;

			// Get quality for the effect, perfect play makes every sound perfect.
			var effectQuality = quality;
			if (ChannelServer.Instance.Conf.World.PerfectPlay)
			{
				effectQuality = PlayingQuality.VeryGood;
				Send.ServerMessage(creature, Localization.Get("skills.perfect_play"));
			}

			// Reduce scroll's durability.
			if (mml != null)
				creature.Inventory.ReduceDurability(creature.Magazine, DurabilityUse);

			// Music effect and Use
			Send.PlayEffect(creature, instrumentType, effectQuality, mml, rndScore);
			this.AdditionalPlayEffect(creature, skill, quality);
			Send.SkillUsePlayingInstrument(creature, skill.Info.Id, instrumentType, mml, rndScore);

			creature.Skills.ActiveSkill = skill;
			creature.Skills.Callback(skill.Info.Id, () =>
			{
				Send.Notice(creature, this.GetRandomQualityMessage(quality));
			});

			creature.Regens.Add("PlayingInstrument", Stat.Stamina, skill.RankData.StaminaActive, creature.StaminaMax);
		}

		public void Complete(Creature creature, Skill skill, Packet packet)
		{
			creature.Skills.ActiveSkill = null;

			this.Cancel(creature, skill);

			creature.Skills.Callback(skill.Info.Id);

			Send.SkillComplete(creature, skill.Info.Id);
		}

		public virtual void Cancel(Creature creature, Skill skill)
		{
			Send.Effect(creature, Effect.StopMusic);

			creature.Regens.Remove("PlayingInstrument");
		}

		/// <summary>
		/// Returns instrument type to use.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <returns></returns>
		protected virtual InstrumentType GetInstrumentType(Creature creature)
		{
			return creature.RightHand.Data.InstrumentType;
		}

		/// <summary>
		/// Plays additional effect, aside from playing.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		protected virtual void AdditionalPlayEffect(Creature creature, Skill skill, PlayingQuality quality)
		{
		}

		/// <summary>
		/// Returns score from magazine's item.
		/// </summary>
		/// <param name="creature"></param>
		/// <returns></returns>
		private string GetScore(Creature creature)
		{
			// Score scrolls go into the magazine pocket and need a specific tag.
			if (creature.Magazine == null || !creature.Magazine.MetaData1.Has(this.MetaDataScoreField) || creature.Magazine.OptionInfo.Durability < DurabilityUse)
				return null;

			return creature.Magazine.MetaData1.GetString(this.MetaDataScoreField);
		}

		/// <summary>
		/// Returns score field name in the item's meta data.
		/// </summary>
		protected virtual string MetaDataScoreField { get { return "SCORE"; } }

		/// <summary>
		/// Returns random score scroll id.
		/// </summary>
		/// <param name="rnd"></param>
		/// <returns></returns>
		protected virtual int GetRandomScore(Random rnd)
		{
			return rnd.Next(RandomScoreMin, RandomScoreMax + 1);
		}

		/// <summary>
		/// Returns a random result message for the given quality.
		/// </summary>
		/// <param name="quality"></param>
		/// <returns></returns>
		protected virtual string GetRandomQualityMessage(PlayingQuality quality)
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
