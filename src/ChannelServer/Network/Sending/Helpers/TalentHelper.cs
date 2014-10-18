// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.World.Entities;
using Aura.Shared.Network;

namespace Aura.Channel.Network.Sending.Helpers
{
	public static class TalentHelper
	{
		public static Packet AddPrivateTalentInfo(this Packet packet, Creature creature)
		{
			packet.PutShort(0);
			packet.PutByte(0);
			packet.PutInt(0);
			packet.PutInt(0);
			packet.PutInt(0);
			packet.PutInt(0);
			packet.PutInt(0);
			packet.PutInt(0);
			packet.PutInt(0);
			packet.PutInt(0);
			packet.PutInt(0);
			packet.PutInt(0);
			packet.PutInt(0);
			packet.PutInt(0);
			packet.PutInt(0);
			packet.PutInt(0);
			packet.PutInt(0);
			packet.PutInt(0);
			packet.PutInt(0);
			// --v
			//packet.PutShort((short)creature.Talents.SelectedTitle);
			//packet.PutByte((byte)creature.Talents.Grandmaster);
			//packet.PutInt(creature.Talents.GetExp(TalentId.Adventure));
			//packet.PutInt(creature.Talents.GetExp(TalentId.Warrior));
			//packet.PutInt(creature.Talents.GetExp(TalentId.Mage));
			//packet.PutInt(creature.Talents.GetExp(TalentId.Archer));
			//packet.PutInt(creature.Talents.GetExp(TalentId.Merchant));
			//packet.PutInt(creature.Talents.GetExp(TalentId.BattleAlchemy));
			//packet.PutInt(creature.Talents.GetExp(TalentId.Fighter));
			//packet.PutInt(creature.Talents.GetExp(TalentId.Bard));
			//packet.PutInt(creature.Talents.GetExp(TalentId.Puppeteer));
			//packet.PutInt(creature.Talents.GetExp(TalentId.Knight));
			//packet.PutInt(creature.Talents.GetExp(TalentId.HolyArts));
			//packet.PutInt(creature.Talents.GetExp(TalentId.Transmutaion));
			//packet.PutInt(creature.Talents.GetExp(TalentId.Cooking));
			//packet.PutInt(creature.Talents.GetExp(TalentId.Blacksmith));
			//packet.PutInt(creature.Talents.GetExp(TalentId.Tailoring));
			//packet.PutInt(creature.Talents.GetExp(TalentId.Medicine));
			//packet.PutInt(creature.Talents.GetExp(TalentId.Carpentry));
			// [180100] Zero Talent
			{
				packet.PutInt(0);
			}
			// [180800, NA189 (23.07.2014)] Ninja?
			{
				packet.PutInt(0);
			}

			// Talent titles
			// ----------------------------------------------------------
			//var titles = creature.Talents.GetTitles();

			packet.PutByte(0);// --v
			//packet.PutByte((byte)titles.Count);
			//foreach (var title in titles)
			//    packet.PutShort(title);

			return packet;
		}
	}
}
