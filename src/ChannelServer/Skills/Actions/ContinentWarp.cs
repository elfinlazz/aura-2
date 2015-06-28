// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.World.Entities;
using Aura.Mabi.Const;
using Aura.Shared.Network;
using Aura.Shared.Util;
using Aura.Mabi.Network;
using Aura.Data;

namespace Aura.Channel.Skills.Actions
{
	/// <summary>
	/// Continent Warp Action
	/// </summary>
	/// <remarks>
	/// The client disallows continent warps unless you have the
	/// keywords from the places. For Iria that's "portal_qilla_base_camp"
	/// (the portals for Fila and Vales probably work as well,
	/// and might overwrite each other), for Belvast it's
	/// "portal_belfast", and Uladh uses the respective portal keywords
	/// for Tir, Dun, etc.
	/// 
	/// Since you can always only select a whole continent, your destination
	/// seems to solely depend on the keywords. For example, if you
	/// select Uladh, and you have the "portal_dunbarton" keyword,
	/// you'll go to Dun, instead of Tir.
	/// 
	/// If a character doesn't have those keywords a message appears,
	/// saying something like "you can't warp with your first character".
	/// 
	/// TODO: Research destinations and how they overwrite each other.
	/// </remarks>
	[Skill(SkillId.ContinentWarp)]
	public class ContinentWarp : IPreparable, IUseable, ICompletable, ICancelable
	{
		/// <summary>
		/// Prepares the skill, called when opening map. Goes straight to Ready.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public bool Prepare(Creature creature, Skill skill, Packet packet)
		{
			if (!AuraData.FeaturesDb.IsEnabled("ContinentWarp"))
			{
				Send.ServerMessage(creature, Localization.Get("The Continent Warp feature hasn't been enabled yet."));
				return false;
			}

			Send.SkillReady(creature, skill.Info.Id);
			skill.State = SkillState.Ready;

			return true;
		}

		/// <summary>
		/// Handles skill usage, called when a destination was selected.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public void Use(Creature creature, Skill skill, Packet packet)
		{
			var destination = (Continent)packet.GetByte();

			// TODO: Check destination?

			Send.SkillUse(creature, skill.Info.Id, (byte)destination);
		}

		/// <summary>
		/// Completes skill, warping to destination.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public void Complete(Creature creature, Skill skill, Packet packet)
		{
			var destination = (Continent)packet.GetByte();

			int regionId, x, y;
			switch (destination)
			{
				case Continent.Uladh:
					if (creature.Keywords.Has("portal_dunbarton"))
					{
						regionId = 14; x = 41598; y = 36010; // Dun
					}
					else
					{
						regionId = 1; x = 12789; y = 38399; // Tir
					}
					break;
				case Continent.Iria:
					regionId = 3001; x = 164837; y = 170144; // Qilla
					break;
				case Continent.Belvast:
					regionId = 4005; x = 41760; y = 26924; // Belvast
					break;
				default:
					Send.Notice(creature, "Unknown destination.");
					Send.SkillCancel(creature);
					return;
			}

			creature.Warp(regionId, x, y);

			Send.SkillComplete(creature, skill.Info.Id, (byte)destination);
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
