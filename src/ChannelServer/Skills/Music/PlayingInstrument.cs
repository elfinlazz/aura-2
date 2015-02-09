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
	public class PlayingInstrument : IPreparable, ICompletable, ICancelable, IInitiableSkillHandler
	{
		/// <summary>
		/// Minimum random score id.
		/// </summary>
		private const int RandomScoreMin = 1;

		/// <summary>
		/// Maximum random score id.
		/// </summary>
		private const int RandomScoreMax = 52;

		/// <summary>
		/// Amount of Durability used every time playing a scroll.
		/// </summary>
		private const int DurabilityUse = 1000;

		/// <summary>
		/// Subscribes handler to events required for training.
		/// </summary>
		public virtual void Init()
		{
			ChannelServer.Instance.Events.CreatureAttackedByPlayer += this.OnCreatureAttackedByPlayer;
		}

		/// <summary>
		/// Prepares skill, goes straight to Use and starts playing.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public bool Prepare(Creature creature, Skill skill, Packet packet)
		{
			var rnd = RandomProvider.Get();

			// Check for instrument
			if (creature.RightHand == null || creature.RightHand.Data.Type != ItemType.Instrument)
				return false;

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
				Send.ServerMessage(creature, Localization.Get("Perfect play is enabled, your performance will sound perfect."));
			}

			// Reduce scroll's durability.
			if (mml != null)
				creature.Inventory.ReduceDurability(creature.Magazine, DurabilityUse);

			// Music effect and Use
			Send.PlayEffect(creature, instrumentType, effectQuality, mml, rndScore);
			this.OnPlay(creature, skill, quality);
			Send.SkillUsePlayingInstrument(creature, skill.Info.Id, instrumentType, mml, rndScore);
			skill.State = SkillState.Used;

			creature.Skills.Callback(skill.Info.Id, () =>
			{
				Send.Notice(creature, this.GetRandomQualityMessage(quality));
				this.AfterPlay(creature, skill, quality);
			});

			creature.Regens.Add("PlayingInstrument", Stat.Stamina, skill.RankData.StaminaActive, creature.StaminaMax);

			return true;
		}

		/// <summary>
		/// Completes skill, called when done playing the current song.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public void Complete(Creature creature, Skill skill, Packet packet)
		{
			this.Cancel(creature, skill);

			creature.Skills.Callback(skill.Info.Id);

			Send.SkillComplete(creature, skill.Info.Id);
		}

		/// <summary>
		/// Cancales skill.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
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
				case PlayingQuality.VeryGood:
					msgs = new string[] {
						Localization.Get("It seemed as if the God of Music had descended"),
						Localization.Get("A perfect performance"),
					};
					break;
				case PlayingQuality.Good:
					msgs = new string[] {
						Localization.Get("The performance was quite alright"),
						Localization.Get("Not a bad performance"),
						Localization.Get("I'm slowly gaining confidence in playing instruments."),
					};
					break;
				case PlayingQuality.Bad:
					msgs = new string[] {
						Localization.Get("Lots of mistakes, although most didn't notice"),
						Localization.Get("An embarrassing performance"),
						Localization.Get("The song is still too difficult for me."),
						Localization.Get("I need more practice to play this song well."),
					};
					break;
				case PlayingQuality.VeryBad:
					msgs = new string[] {
						Localization.Get("A disastrous performance"),
						Localization.Get("That was a total mess..."),
						Localization.Get("That was a difficult song for me to play."),
					};
					break;
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
		protected virtual void OnPlay(Creature creature, Skill skill, PlayingQuality quality)
		{
			if (skill.Info.Rank == SkillRank.Novice)
				skill.Train(1); // Try the skill.
		}

		/// <summary>
		/// Called when completing (training).
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="quality"></param>
		protected virtual void AfterPlay(Creature creature, Skill skill, PlayingQuality quality)
		{
			// Success unless total failure, condition 2 for Novice.
			if (skill.Info.Rank == SkillRank.Novice && quality != PlayingQuality.VeryBad)
				skill.Train(2); // Use the skill successfully.

			// All ranks above F have the same 2 first conditions.
			if (skill.Info.Rank >= SkillRank.RF && skill.Info.Rank <= SkillRank.R1)
			{
				if (quality >= PlayingQuality.Bad)
					skill.Train(1); // Use the skill successfully.

				if (quality == PlayingQuality.VeryGood)
					skill.Train(2); // Get a very good result.
			}

			// Training by failing is possible between F and 6.
			if (skill.Info.Rank >= SkillRank.RF && skill.Info.Rank <= SkillRank.R6 && quality == PlayingQuality.Bad)
				skill.Train(3); // Fail at using the skill.

			// Training by failing badly is possible at F and E.
			if (skill.Info.Rank >= SkillRank.RF && skill.Info.Rank <= SkillRank.RE && quality == PlayingQuality.VeryBad)
				skill.Train(4); // Get a horrible result.

			// TODO: "Use the skill successfully to grow crops faster."
			// TODO: "Use a music buff skill."
		}

		/// <summary>
		/// Called when a player attacks someone (training).
		/// </summary>
		/// <param name="action"></param>
		protected virtual void OnCreatureAttackedByPlayer(TargetAction action)
		{
			// Check for instrument in attacker's right hand
			if (action.Attacker == null || action.Attacker.RightHand == null || action.Attacker.RightHand.Data.Type != ItemType.Instrument)
				return;

			// Get skill
			var skill = action.Attacker.Skills.Get(SkillId.PlayingInstrument);
			if (skill == null) return;

			// Equip an instrument and attack an enemy.
			if (skill.Info.Rank >= SkillRank.RF && skill.Info.Rank <= SkillRank.RE)
				skill.Train(6);
			else if (skill.Info.Rank >= SkillRank.RD && skill.Info.Rank <= SkillRank.R6)
				skill.Train(5);
			else if (skill.Info.Rank >= SkillRank.R7 && skill.Info.Rank <= SkillRank.R2)
				skill.Train(4);
			else if (skill.Info.Rank == SkillRank.R1)
				skill.Train(3);
		}
	}
}
