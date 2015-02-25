// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.World.Entities;
using Aura.Shared.Mabi.Const;
using Aura.Shared.Network;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.Skills.Hidden
{
	/// <summary>
	/// Called when using Holy Water.
	/// </summary>
	[Skill(SkillId.HiddenBlessing)]
	public class Blessing : IPreparable, ICompletable, ICancelable
	{
		public bool Prepare(Creature creature, Skill skill, Packet packet)
		{
			var itemEntityId = packet.GetLong();
			var hwEntityId = packet.GetLong();

			// Beware, the client uses the entity ids you send back for the
			// Complete packet. If you switch them around the handler would
			// bless the HW and delete the item without security checks.
			Send.SkillUse(creature, skill.Info.Id, itemEntityId, hwEntityId);

			return true;
		}

		public void Complete(Creature creature, Skill skill, Packet packet)
		{
			var itemEntityId = packet.GetLong();
			var hwEntityId = packet.GetLong();

			var item = creature.Inventory.GetItemSafe(itemEntityId);
			var hw = creature.Inventory.GetItemSafe(hwEntityId);

			var blessable = (item.HasTag("/equip/") && !item.HasTag("/not_bless/"));
			var isHolyWater = (hw.Info.Id == 63016); // There's only one item using this skill.

			// TODO: Check loading time

			if (blessable && isHolyWater)
			{
				creature.Inventory.Decrement(hw, 1);
				item.OptionInfo.Flags |= ItemFlags.Blessed;
			}
			else
				Log.Warning("Blessing.Complete: Invalid item or Holy Water.");

			Send.ItemBlessed(creature, item);
			Send.UseMotion(creature, 14, 0);
			Send.SkillComplete(creature, skill.Info.Id, itemEntityId, hwEntityId);
		}

		public void Cancel(Creature creature, Skill skill)
		{
		}
	}
}
