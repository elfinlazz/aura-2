// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

namespace Aura.Shared.Network
{
	/// <summary>
	/// All Op codes
	/// </summary>
	public static class Op
	{
		// Login Server
		// ------------------------------------------------------------------
		public const int ClientIdent = 0x0FD1020A;
		public const int ClientIdentR = 0x1F;
		public const int Login = 0x0FD12002;
		public const int LoginR = 0x23;
		public const int ChannelStatus = 0x26;
		public const int CharacterInfoRequest = 0x29;
		public const int CharacterInfoRequestR = 0x2A;
		public const int CreateCharacter = 0x2B;
		public const int CreateCharacterR = 0x2C;
		public const int DeleteCharacterRequest = 0x2D;
		public const int DeleteCharacterRequestR = 0x2E;
		public const int EnterGame = 0x2F;
		public const int ChannelInfo = 0x30;
		public const int DeleteCharacter = 0x35;
		public const int DeleteCharacterR = 0x36;
		public const int RecoverCharacter = 0x37;
		public const int RecoverCharacterR = 0x38;
		public const int NameCheck = 0x39;
		public const int NameCheckR = 0x3A;
		public const int PetInfoRequest = 0x3B;
		public const int PetInfoRequestR = 0x3C;
		public const int CreatePet = 0x3D;
		public const int CreatePetR = 0x3E;
		public const int DeletePetRequest = 0x3F;
		public const int DeletePetRequestR = 0x40;
		public const int DeletePet = 0x41;
		public const int DeletePetR = 0x42;
		public const int RecoverPet = 0x43;
		public const int RecoverPetR = 0x44;
		public const int CreatePartner = 0x45;
		public const int CreatePartnerR = 0x46;
		public const int AccountInfoRequest = 0x47;
		public const int AccountInfoRequestR = 0x48;
		public const int AcceptGift = 0x49;
		public const int AcceptGiftR = 0x4A;
		public const int RefuseGift = 0x4B;
		public const int RefuseGiftR = 0x4C;
		public const int Disconnect = 0x4D;
		public const int PetCreationOptionsRequest = 0x50;
		public const int PetCreationOptionsRequestR = 0x51;
		public const int PartnerCreationOptionsRequest = 0x55;
		public const int PartnerCreationOptionsRequestR = 0x56;
		//public const int ? = 0x5A;  // Sent on login
		//public const int ?R = 0x5B; // ^ Response, only known parameter: 0 byte.
	}
}
