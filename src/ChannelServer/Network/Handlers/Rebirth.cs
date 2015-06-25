// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Util;
using Aura.Channel.World.Entities;
using Aura.Data;
using Aura.Data.Database;
using Aura.Mabi.Const;
using Aura.Mabi.Network;
using Aura.Shared.Network;
using System;

namespace Aura.Channel.Network.Handlers
{
	public partial class ChannelServerHandlers : PacketHandlerManager<ChannelClient>
	{
		/// <summary>
		/// Sent after selecting all rebirth options.
		/// </summary>
		/// <remarks>
		/// To get into the rebirth screen send "<rebirth style='-1'/>"
		/// as a NPC message. The dialog result will either be "@rebirth"
		/// or "@cancel".
		/// 
		/// You can always go to Nao to rebirth from the login screen,
		/// because the last rebirth time is only sent in the full
		/// creature info in 5209. In-game the client checks the date,
		/// preventing one from going there before it should be possible
		/// under NA rules.
		/// </remarks>
		/// <example>
		/// 001 [0000000000000000] Long   : 0
		/// 002 [..............01] Byte   : 1 - Reset age toggle
		/// 003 [............0011]	Short  : 17 - New age if ^ true
		/// 004 [..............01] Byte   : 1 - Reset level toggle
		/// 005 [..............01] Byte   : 1 - Change gender toggle
		/// 006 [........00002712]	Int    : 10002 - New race if ^ true
		/// 007 [..............10] Byte   : 16
		/// 008 [........00000FF9] Int    : 4089
		/// 009 [..............1C] Byte   : 28
		/// 010 [............0020] Short  : 32
		/// 011 [..............1D] Byte   : 29
		/// 012 [..............02] Byte   : 2
		/// 013 [........00001324] Int    : 4900
		/// 014 [..............00] Byte   : 0
		/// 015 [..............12] Byte   : 18
		/// 
		/// Old rebirth packet
		/// 0001 [0000000000000000] Long   : 0
		/// 0002 [..............01] Byte   : 1
		/// 0003 [............0011] Short  : 17
		/// 0004 [..............01] Byte   : 1
		/// 0005 [..............00] Byte   : 0
		/// 0006 [..............00] Byte   : 0
		/// </example>
		[PacketHandler(Op.RequestRebirth)]
		public void RequestRebirth(ChannelClient client, Packet packet)
		{
			var unkLong = packet.GetLong();
			var resetAge = packet.GetBool();
			short age = 0;
			if (resetAge)
				age = packet.GetShort();
			var resetLevel = packet.GetBool();
			var resetGender = packet.GetBool();
			var race = 0;
			if (resetGender)
				race = packet.GetInt();
			var skinColor = packet.GetByte();
			var hairItemId = packet.GetInt();
			var hairColor = packet.GetByte();
			var eyeType = packet.GetShort();
			var eyeColor = packet.GetByte();
			var mouthType = packet.GetByte();
			var faceItemId = packet.GetInt();
			var location = (RebirthLocation)packet.GetByte();
			var talent = packet.GetByte();

			// Get required data
			var creature = client.GetCreatureSafe(packet.Id);
			var hair = creature.Inventory.GetItemAt(Pocket.Hair, 0, 0);
			var face = creature.Inventory.GetItemAt(Pocket.Face, 0, 0);
			var hairItemData = AuraData.ItemDb.Find(hairItemId);
			var faceItemData = AuraData.ItemDb.Find(faceItemId);

			// Check age
			if (resetAge && (age < 10 || age > 17 || age > creature.Age))
				throw new SevereViolation("Player tried to rebirth with invalid age ({0}).", age);

			// Check race, the new one would be the same as the old one,
			// if you remove the last number (1 for female, 2 for male).
			if (resetGender && (creature.RaceId & ~3) != (race & ~3))
				throw new SevereViolation("Player tried to rebirth with invalid race ({0}).", race);

			// Check hair
			if (hairItemData == null || (hairItemData.Type != ItemType.Hair && hairItemData.Type != ItemType.Face))
				throw new SevereViolation("Player tried to rebirth with invalid hair ({0}).", hairItemId);

			// Check face
			if (faceItemData == null || (faceItemData.Type != ItemType.Face && faceItemData.Type != ItemType.Hair))
				throw new SevereViolation("Player tried to rebirth with invalid face ({0}).", faceItemId);

			// Check location
			if (location < RebirthLocation.Last || location > RebirthLocation.Iria)
				throw new SevereViolation("Player tried to rebirth with invalid location ({0}).", location);

			// Reset age
			if (resetAge)
			{
				creature.Age = age;
				creature.Height = Math.Min(1.0f, 1.0f / 7.0f * (age - 10.0f)); // 0 ~ 1.0
			}

			// Reset level and stats
			if (resetLevel)
			{
				creature.Level = 1;
				creature.Exp = 0;

				var ageStats = AuraData.StatsBaseDb.Find(creature.RaceId, creature.Age);
				creature.LifeMaxBase = ageStats.Life;
				creature.ManaMaxBase = ageStats.Mana;
				creature.StaminaMaxBase = ageStats.Stamina;
				creature.StrBase = ageStats.Str;
				creature.IntBase = ageStats.Int;
				creature.DexBase = ageStats.Dex;
				creature.WillBase = ageStats.Will;
				creature.LuckBase = ageStats.Luck;
				creature.FullHeal();
			}

			// Reset body
			creature.EyeType = eyeType;
			creature.EyeColor = eyeColor;
			creature.MouthType = mouthType;
			creature.SkinColor = skinColor;
			creature.Weight = 1;
			creature.Upper = 1;
			creature.Lower = 1;

			// Update face
			face.Info.Id = faceItemId;
			face.Info.Color1 = skinColor;

			// Update hair
			hair.Info.Id = hairItemId;
			hair.Info.Color1 = hairColor + 0x10000000u;

			// Reset LastX times
			var player = (PlayerCreature)creature;
			player.LastAging = DateTime.Now;
			player.LastRebirth = DateTime.Now;
			player.RebirthCount++;

			// Location
			switch (location)
			{
				// Tir beginner area
				case RebirthLocation.Tir: creature.SetLocation(125, 21489, 76421); break;

				// Iria beginner area (Rano)
				// (TODO: Disable pre-G4?)
				case RebirthLocation.Iria: creature.SetLocation(3001, 164533, 161862); break;
			}

			// Just rebirthed switch (usage?)
			creature.Activate(CreatureStates.JustRebirthed);

			// Success
			Send.RequestRebirthR(creature, true);
		}

		/// <summary>
		/// Sent upon entering the rebirth screen.
		/// </summary>
		/// <remarks>
		/// A lack of a response does not lock the client.
		/// </remarks>
		/// <example>
		/// No parameters.
		/// </example>
		[PacketHandler(Op.EnterRebirth)]
		public void EnterRebirth(ChannelClient client, Packet packet)
		{
			var creature = client.GetCreatureSafe(packet.Id);

			Send.EnterRebirthR(creature);
			Send.PonsUpdate(creature, 9999);
		}
	}
}
