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

namespace Aura.Channel.Skills.Base
{
	/// <summary>
	/// Base class for skills that use only Start and Stop.
	/// </summary>
	/// <remarks>
	/// Sends back Skill(Start|Stop) with string or byte parameter,
	/// depending on incoming packet. Always passes a dictionary to the
	/// next methods, since the byte seems useless =|
	/// If Start|Stop returns fail a silent cancel will be sent.
	/// </remarks>
	public abstract class StartStopSkillHandler : IStartStoppable
	{
		public void Start(Creature creature, Skill skill, Packet packet)
		{
			var stringParam = packet.NextIs(PacketElementType.String);
			var dict = new MabiDictionary();
			byte unkByte = 0;

			if (stringParam)
				dict.Parse(packet.GetString());
			else
				unkByte = packet.GetByte();

			var result = this.Start(creature, skill, dict);

			if (result == StartStopResult.Fail)
				Send.SkillStartSilentCancel(creature, skill.Info.Id);
			else if (stringParam)
				Send.SkillStart(creature, skill.Info.Id, dict.ToString());
			else
				Send.SkillStart(creature, skill.Info.Id, unkByte);
		}

		public void Stop(Creature creature, Skill skill, Packet packet)
		{
			var stringParam = packet.NextIs(PacketElementType.String);
			var dict = new MabiDictionary();
			byte unkByte = 0;

			if (stringParam)
				dict.Parse(packet.GetString());
			else
				unkByte = packet.GetByte();

			var result = this.Stop(creature, skill, dict);

			if (result == StartStopResult.Fail)
				Send.SkillStopSilentCancel(creature, skill.Info.Id);
			else if (stringParam)
				Send.SkillStop(creature, skill.Info.Id, dict.ToString());
			else
				Send.SkillStop(creature, skill.Info.Id, unkByte);
		}

		public virtual StartStopResult Start(Creature creature, Skill skill, MabiDictionary dict)
		{
			throw new NotImplementedException();
		}

		public virtual StartStopResult Stop(Creature creature, Skill skill, MabiDictionary dict)
		{
			throw new NotImplementedException();
		}
	}

	public enum StartStopResult { Okay, Fail }
}
