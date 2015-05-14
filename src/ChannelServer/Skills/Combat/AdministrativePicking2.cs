// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.World.Entities;
using Aura.Mabi.Const;
using Aura.Mabi.Network;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.Skills.Combat
{
	/// <summary>
	/// Administrative Picking2 skill
	/// </summary>
	/// <remarks>
	/// Var1: Range
	/// 
	/// Locks character in this skill with Prepare, Ready, and Use response.
	/// Locks character on Complete response.
	/// Handle and SilentPrepareCancel? Or special response?
	/// 
	/// Actual use unknown, presumably used to either pick up all items
	/// in range, or make them disappear. Since picking them up seems weird
	/// (inv flooding) we'll clear instead, which could be fun to use.
	/// </remarks>
	[Skill(SkillId.AdministrativePicking2)]
	public class AdministrativePicking2 : ISkillHandler, IPreparable
	{
		public bool Prepare(Creature creature, Skill skill, Packet packet)
		{
			var unkStr = packet.GetString();

			// Get all items on floor and remove those that are within skill range
			var items = creature.Region.GetAllItems();
			foreach (var item in items.Where(a => a.GetPosition().InRange(creature.GetPosition(), (int)skill.RankData.Var1)))
				creature.Region.RemoveItem(item);

			return false; // Silent cancel
		}
	}
}
