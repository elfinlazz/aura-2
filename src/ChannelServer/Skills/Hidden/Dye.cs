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
	public class Dye : IPreparable, IUseable, ICompletable, ICancelable
	{
		/// <summary>
		/// Prepares skill, goes straight to being ready.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public bool Prepare(Creature creature, Skill skill, Packet packet)
		{
			var itemEntityId = packet.GetLong();
			var dyeEntityId = packet.GetLong();

			var item = creature.Inventory.GetItem(itemEntityId);
			var dye = creature.Inventory.GetItem(dyeEntityId);
			if (item == null || dye == null) return false;

			if (!dye.Data.HasTag("/*dye_ampul/")) return false;

			creature.Temp.SkillItem1 = item;
			creature.Temp.SkillItem2 = dye;

			Send.SkillReadyDye(creature, skill.Info.Id, itemEntityId, dyeEntityId);
			skill.State = SkillState.Ready;

			return true;
		}

		/// <summary>
		/// Handles usage of the skill, called once a part was selected.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public void Use(Creature creature, Skill skill, Packet packet)
		{
			var part = packet.GetInt();

			if (packet.Peek() == PacketElementType.Short)
				this.UseRegular(creature, skill, packet, part);
			else if (packet.Peek() == PacketElementType.Byte)
				this.UseFixed(creature, skill, packet, part);
		}

		/// <summary>
		/// Handles usage of the skill if it's a regular dye.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		/// <param name="part"></param>
		private void UseRegular(Creature creature, Skill skill, Packet packet, int part)
		{
			var x = packet.GetShort();
			var y = packet.GetShort();

			Send.SkillUseDye(creature, skill.Info.Id, part, x, y);
		}

		/// <summary>
		/// Handles usage of the skill if it's a fixed dye.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		/// <param name="part"></param>
		private void UseFixed(Creature creature, Skill skill, Packet packet, int part)
		{
			var unk = packet.GetByte();

			Send.SkillUseDye(creature, skill.Info.Id, part, unk);
		}

		/// <summary>
		/// Completes skill usage, called once the dyeing is completed.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public void Complete(Creature creature, Skill skill, Packet packet)
		{
			var part = packet.GetInt();

			if (creature.Skills.ActiveSkill != skill) return;
			if (creature.Temp.SkillItem1 == null || creature.Temp.SkillItem2 == null) return;

			if (packet.Peek() == PacketElementType.Short)
				this.CompleteRegular(creature, packet, skill, part);
			else if (packet.Peek() == PacketElementType.Byte)
				this.CompleteFixed(creature, packet, skill, part);
		}

		/// <summary>
		/// Completes skill usage if it was a regular dye.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="packet"></param>
		/// <param name="skill"></param>
		/// <param name="part"></param>
		private void CompleteRegular(Creature creature, Packet packet, Skill skill, int part)
		{
			var x = packet.GetShort();
			var y = packet.GetShort();

			// TODO: Get the distort formula for regulars

			Send.ServerMessage(creature, Localization.Get("This skill isn't implemented completely yet."));
			Send.SkillCompleteDye(creature, skill.Info.Id, part);
		}

		/// <summary>
		/// Completes skill usage if it was a fixed dye.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="packet"></param>
		/// <param name="skill"></param>
		/// <param name="part"></param>
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

		/// <summary>
		/// Called when canceling the skill (do nothing).
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		public void Cancel(Creature creature, Skill skill)
		{
		}
	}
}
