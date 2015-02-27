// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.World.Entities;
using Aura.Shared.Mabi;
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
	/// Called when using Massive Holy Water.
	/// </summary>
	[Skill(SkillId.BigBlessingWaterKit)]
	public class BigBlessingWaterKit : IPreparable, ICompletable, ICancelable
	{
		public bool Prepare(Creature creature, Skill skill, Packet packet)
		{
			var dict = packet.GetString();

			Send.SkillUse(creature, skill.Info.Id, dict);

			return true;
		}

		public void Complete(Creature creature, Skill skill, Packet packet)
		{
			var str = packet.GetString();
			var dict = new MabiDictionary(str);

			var hwEntityId = dict.GetLong("ITEMID");
			if (hwEntityId == 0)
				goto L_End;

			var hw = creature.Inventory.GetItemSafe(hwEntityId);
			if (!hw.HasTag("/large_blessing_potion/"))
				goto L_End;

			// TODO: Check loading time

			var items = creature.Inventory.ActualEquipment.ToArray();
			foreach (var item in items)
			{
				var blessable = (item.HasTag("/equip/") && !item.HasTag("/not_bless/"));

				if (blessable)
					item.OptionInfo.Flags |= ItemFlags.Blessed;
			}

			creature.Inventory.Decrement(hw, 1);
			Send.ItemBlessed(creature, items);

		L_End:
			Send.UseMotion(creature, 14, 0);
			Send.SkillComplete(creature, skill.Info.Id, str);
		}

		public void Cancel(Creature creature, Skill skill)
		{
		}
	}
}
