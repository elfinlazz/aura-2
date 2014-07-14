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
using Aura.Shared.Mabi.Const;
using Aura.Shared.Network;
using Aura.Shared.Util;

namespace Aura.Channel.Skills.Hidden
{
	/// <summary>
	/// Dyeing skill handler
	/// </summary>
	/// <remarks>
	/// Hidden skill for using any type of dye ampoule. Prepare is called with
	/// the item entity id of the dye and the item to be dyed and goes straight
	/// to Use from there. Before sending Use, the color is determined for
	/// regulars.
	/// </remarks>
	[Skill(SkillId.Dye)]
	public class DyeSkillHandler : IPreparable, IUseable, ICompletable, ICancelable
	{
		public void Prepare(Creature creature, Skill skill, int castTime, Packet packet)
		{
			var itemEntityId = packet.GetLong();
			var dyeEntityId = packet.GetLong();

			var item = creature.Inventory.GetItem(itemEntityId);
			var dye = creature.Inventory.GetItem(dyeEntityId);
			if (item == null || dye == null) return;

			if (!dye.Data.HasTag("/*dye_ampul/")) return;

			creature.Temp.SkillItem1 = item;
			creature.Temp.SkillItem2 = dye;

			creature.Skills.ActiveSkill = skill;

			Send.SkillReadyDye(creature, skill.Info.Id, itemEntityId, dyeEntityId);
		}

		public void Use(Creature creature, Skill skill, Packet packet)
		{
			var part = packet.GetInt();

			if (packet.Peek() == PacketElementType.Short)
				this.UseRegular(creature, skill, packet, part);
			else if (packet.Peek() == PacketElementType.Byte)
				this.UseFixed(creature, skill, packet, part);
		}

		private void UseRegular(Creature creature, Skill skill, Packet packet, int part)
		{
			var x = packet.GetShort();
			var y = packet.GetShort();

			Send.SkillUseDye(creature, skill.Info.Id, part, x, y);
		}

		private void UseFixed(Creature creature, Skill skill, Packet packet, int part)
		{
			var unk = packet.GetByte();

			Send.SkillUseDye(creature, skill.Info.Id, part, unk);
		}

		public void Complete(Creature creature, Skill skill, Packet packet)
		{
			var part = packet.GetInt();

			if (creature.Skills.ActiveSkill != skill) return;
			if (creature.Temp.SkillItem1 == null || creature.Temp.SkillItem2 == null) return;

			creature.Skills.ActiveSkill = null;

			if (packet.Peek() == PacketElementType.Short)
				this.CompleteRegular(creature, packet, skill, part);
			else if (packet.Peek() == PacketElementType.Byte)
				this.CompleteFixed(creature, packet, skill, part);
		}

		private void CompleteRegular(Creature creature, Packet packet, Skill skill, int part)
		{
			var x = packet.GetShort();
			var y = packet.GetShort();

			// TODO: Get the distort formula for regulars

			Send.ServerMessage(creature, Localization.Get("This skill isn't implemented completely yet."));
			Send.SkillCompleteDye(creature, skill.Info.Id, part);
		}

		private void CompleteFixed(Creature creature, Packet packet, Skill skill, int part)
		{
			// Older logs seem to have an additional byte (like Use?)
			//var unk = packet.GetByte();

			switch (part)
			{
				default:
				case 0: creature.Temp.SkillItem1.Info.Color1 = creature.Temp.SkillItem2.Info.Color1; break;
				case 1: creature.Temp.SkillItem1.Info.Color2 = creature.Temp.SkillItem2.Info.Color1; break;
				case 2: creature.Temp.SkillItem1.Info.Color3 = creature.Temp.SkillItem2.Info.Color1; break;
			}

			this.DyeSuccess(creature);

			Send.AcquireFixedDyedItemInfo(creature, creature.Temp.SkillItem1.EntityId);
			Send.SkillCompleteDye(creature, skill.Info.Id, part);
		}

		/// <summary>
		/// Sends success effect, deletes dye, and updates item color.
		/// </summary>
		/// <param name="creature"></param>
		private void DyeSuccess(Creature creature)
		{
			// Remove dye
			creature.Inventory.Decrement(creature.Temp.SkillItem2);

			// Update item color
			Send.ItemUpdate(creature, creature.Temp.SkillItem1);
			if (creature.Temp.SkillItem1.Info.Pocket.IsEquip())
				Send.EquipmentChanged(creature, creature.Temp.SkillItem1);

			// Success effect
			Send.Effect(creature, 2, (byte)4);
		}

		public void Cancel(Creature creature, Skill skill)
		{
		}
	}
}
