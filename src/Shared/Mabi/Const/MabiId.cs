// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

namespace Aura.Shared.Mabi.Const
{
	/// <summary>
	/// Various ids used by Mabi.
	/// </summary>
	public static class MabiId
	{
		// These ids are used as packet ids if there's no specific
		// target for the packet, or in situations where you're technically not
		// "identifiable" by a character id.
		public const long Login = 0x1000000000000010;
		public const long Channel = 0x1000000000000001;
		public const long Broadcast = 0x3000000000000000;

		// Id range starts
		public const long Cards = 0x0001000000000001;
		public const long Characters = 0x0010000000000001;
		public const long Pets = 0x0010010000000001;
		public const long Partners = 0x0010030000000001;
		public const long Npcs = 0x0010F00000000001;

		public const long Guilds = 0x0300000000500000;

		public const long Items = 0x0050000000000001;
		public const long QuestItems = 0x005000F000000001;
		public const long TmpItems = 0x0050F00000000001;

		// 00XX<short:region><short:area><short:id>
		public const long ClientProps = 0x00A0000000000000;
		public const long ServerProps = 0x00A1000000000000;
		public const long AreaEvents = 0x00B0000000000000;

		public const long Parties = 0x0040000000000001;

		// Quests is probably 0x0060000000000001, but we'll leave some space
		// between quests and (quest) items, just in case.
		public const long Quests = 0x006000F000000001;
		public const long QuestsTmp = 0x0060F00000000001;
		public const long QuestItemOffset = 0x0010000000000000;

		public const long Instances = 0x0100000000000001;

		// Default type for pet/partner cards.
		public const int PetCardType = 102;

		// NPCs
		public const long Nao = 0x0010FFFFFFFFFFFF;
		public const long Tin = 0x0010FFFFFFFFFFFE;
	}
}
