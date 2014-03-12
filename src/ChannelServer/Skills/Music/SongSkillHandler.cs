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
	/// Song skill handler
	/// </summary>
	/// <remarks>
	/// Prepare starts the singing, complete is sent once it's over.
	/// </remarks>
	[Skill(SkillId.Song)]
	public class SongSkillHandler : PlayingInstrumentHandler
	{
		private const int RandomSongScoreMin = 2001, RandomSongScoreMax = 2052;

		public override void Init()
		{
		}

		public override void Cancel(Creature creature, Skill skill)
		{
			base.Cancel(creature, skill);

			Send.Effect(creature, 356, (byte)0);
		}

		/// <summary>
		/// Returns instrument type to use.
		/// </summary>
		/// <param name="creature"></param>
		/// 
		/// <returns></returns>
		protected override InstrumentType GetInstrumentType(Creature creature)
		{
			if (creature.IsFemale)
				return InstrumentType.FemaleVoiceJp;
			else
				return InstrumentType.MaleVoiceJp;
		}

		/// <summary>
		/// Returns score field name in the item's meta data.
		/// </summary>
		protected override string MetaDataScoreField { get { return "SCSING"; } }

		/// <summary>
		/// Returns random score id.
		/// </summary>
		/// <param name="rnd"></param>
		/// <returns></returns>
		protected override int GetRandomScore(Random rnd)
		{
			return rnd.Next(RandomSongScoreMin, RandomSongScoreMax + 1);
		}

		/// <summary>
		/// Returns random success message.
		/// </summary>
		/// <param name="quality"></param>
		/// <returns></returns>
		protected override string GetRandomQualityMessage(PlayingQuality quality)
		{
			string[] msgs = null;
			switch (quality)
			{
				// Messages are stored in one line per quality, seperated by semicolons.
				case PlayingQuality.VeryGood: msgs = Localization.Get("skills.quality_song_verygood").Split(';'); break;
				case PlayingQuality.Good: msgs = Localization.Get("skills.quality_song_good").Split(';'); break;
				case PlayingQuality.Bad:
				case PlayingQuality.VeryBad: msgs = Localization.Get("skills.quality_song_bad").Split(';'); break;
			}

			if (msgs == null || msgs.Length < 1)
				return "...";

			return msgs[RandomProvider.Get().Next(0, msgs.Length)].Trim();
		}

		/// <summary>
		/// Called when starting playing (training).
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="quality"></param>
		protected override void OnPlay(Creature creature, Skill skill, PlayingQuality quality)
		{
			Send.Effect(creature, 356, (byte)1);

			if (skill.Info.Rank == SkillRank.Novice)
				skill.Train(1); // Use the skill.
		}

		/// <summary>
		/// Called when completing (training).
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="quality"></param>
		protected override void AfterPlay(Creature creature, Skill skill, PlayingQuality quality)
		{
			// All ranks above F have the same 3 first conditions.
			if (skill.Info.Rank >= SkillRank.RF && skill.Info.Rank <= SkillRank.R1)
			{
				if (quality >= PlayingQuality.Bad)
					skill.Train(1); // Use the skill successfully.

				if (quality == PlayingQuality.Good)
					skill.Train(2); // Give an excellent vocal performance.

				if (quality == PlayingQuality.VeryGood)
					skill.Train(3); // Give a heavenly performance.

				// Very bad training possible till E.
				if (skill.Info.Rank <= SkillRank.RE && quality == PlayingQuality.VeryBad)
					skill.Train(4); // Fail at using the skill.
			}

			// TODO: "Use the skill to grow crops faster."
			// TODO: "Grant a buff to a party member."
		}
	}
}
