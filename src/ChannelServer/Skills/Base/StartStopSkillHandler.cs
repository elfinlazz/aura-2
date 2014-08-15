// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Channel.World.Entities;
using Aura.Shared.Network;
using Aura.Shared.Mabi;
using Aura.Channel.Network.Sending;
using Aura.Shared.Util;
using Aura.Shared.Mabi.Const;
using Aura.Data.Database;

namespace Aura.Channel.Skills.Base
{
	/// <summary>
	/// Base class for skills that use only Start and Stop.
	/// </summary>
	/// <remarks>
	/// Sends back Skill(Start|Stop) with string or byte parameter,
	/// depending on incoming packet. Always passes a dictionary to the
	/// next methods, since the byte seems useless =|
	/// The parameter can also be missing in Stop, example:
	/// Auto stop of ManaShield on 0 mana.
	/// 
	/// If Start|Stop returns fail a silent cancel will be sent.
	/// </remarks>
	public abstract class StartStopSkillHandler : IStartStoppable
	{
		public void Start(Creature creature, Skill skill, Packet packet)
		{
			// Check mana and stamina
			if (!this.CheckMana(creature, skill))
			{
				Send.SystemMessage(creature, Localization.Get("Insufficient Mana"));
				Send.SkillStartSilentCancel(creature, skill.Info.Id);
				return;
			}
			if (!this.CheckStamina(creature, skill))
			{
				Send.SystemMessage(creature, Localization.Get("Insufficient Stamina"));
				Send.SkillStartSilentCancel(creature, skill.Info.Id);
				return;
			}

			// Get parameters
			var stringParam = packet.NextIs(PacketElementType.String);
			var dict = new MabiDictionary();
			byte unkByte = 0;

			if (stringParam)
				dict.Parse(packet.GetString());
			else
				unkByte = packet.GetByte();

			// Run skill
			var result = this.Start(creature, skill, dict);

			if (result == StartStopResult.Fail)
			{
				Send.SkillStartSilentCancel(creature, skill.Info.Id);
				return;
			}

			skill.Activate(SkillFlags.InUse);

			// Use mana/stamina
			this.UseMana(creature, skill);
			this.UseStamina(creature, skill);

			Send.StatUpdate(creature, StatUpdateType.Private, Stat.Mana, Stat.Stamina);

			if (stringParam)
				Send.SkillStart(creature, skill, dict.ToString());
			else
				Send.SkillStart(creature, skill, unkByte);
		}

		public void Stop(Creature creature, Skill skill, Packet packet)
		{
			var stringParam = packet.NextIs(PacketElementType.String);
			var dict = new MabiDictionary();
			byte unkByte = 0;

			if (stringParam)
				dict.Parse(packet.GetString());
			else if (packet.NextIs(PacketElementType.Byte))
				unkByte = packet.GetByte();

			var result = this.Stop(creature, skill, dict);

			skill.Deactivate(SkillFlags.InUse);

			if (result == StartStopResult.Fail)
				Send.SkillStopSilentCancel(creature, skill.Info.Id);
			else if (stringParam)
				Send.SkillStop(creature, skill, dict.ToString());
			else
				Send.SkillStop(creature, skill, unkByte);
		}

		public void Stop(Creature creature, Skill skill)
		{
			var result = this.Stop(creature, skill, new MabiDictionary());

			if (result == StartStopResult.Fail)
				Send.SkillStopSilentCancel(creature, skill.Info.Id);
			else
				Send.SkillStop(creature, skill, "");
		}

		public virtual StartStopResult Start(Creature creature, Skill skill, MabiDictionary dict)
		{
			throw new NotImplementedException();
		}

		public virtual StartStopResult Stop(Creature creature, Skill skill, MabiDictionary dict)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Returns true if creature has enough mana to use use skill.
		/// </summary>
		/// <returns></returns>
		public virtual bool CheckMana(Creature creature, Skill skill)
		{
			return (creature.Mana >= skill.RankData.ManaCost);
		}

		/// <summary>
		/// Reduces mana for one usage of the skill.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		public virtual void UseMana(Creature creature, Skill skill)
		{
			creature.Mana -= skill.RankData.ManaCost;
		}

		/// <summary>
		/// Returns true if creature has enough stamina to use use skill.
		/// </summary>
		/// <returns></returns>
		public virtual bool CheckStamina(Creature creature, Skill skill)
		{
			return (creature.Stamina >= skill.RankData.StaminaCost);
		}

		/// <summary>
		/// Reduces stamina for one usage of the skill.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		public virtual void UseStamina(Creature creature, Skill skill)
		{
			creature.Stamina -= skill.RankData.StaminaCost;
		}
	}

	public enum StartStopResult { Okay, Fail }
}
