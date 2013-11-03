// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using Aura.Data;
using Aura.Data.Database;
using Aura.Shared.Database;
using Aura.Shared.Mabi.Const;

namespace Aura.Login.Database
{
	public class Account
	{
		public string Name { get; set; }
		public string Password { get; set; }
		public string SecondaryPassword { get; set; }
		public long SessionKey { get; set; }

		public byte Authority { get; set; }

		public DateTime Creation { get; set; }
		public DateTime LastLogin { get; set; }
		public string LastIp { get; set; }

		public string BannedReason { get; set; }
		public DateTime BannedExpiration { get; set; }

		public bool LoggedIn { get; set; }

		public List<Card> CharacterCards { get; set; }
		public List<Card> PetCards { get; set; }
		public List<Character> Characters { get; set; }
		public List<Character> Pets { get; set; }
		public List<Gift> Gifts { get; set; }

		public Account()
		{
			this.Creation = DateTime.Now;
			this.LastLogin = DateTime.Now;

			this.CharacterCards = new List<Card>();
			this.PetCards = new List<Card>();
			this.Gifts = new List<Gift>();

			this.Characters = new List<Character>();
			this.Pets = new List<Character>();
		}

		/// <summary>
		/// Returns character card with id, or null if it doesn't exist.
		/// </summary>
		public Card GetCharacterCard(long id)
		{
			return this.CharacterCards.FirstOrDefault(a => a.Id == id);
		}

		/// <summary>
		/// Returns pet/partner card with id, or null if it doesn't exist.
		/// </summary>
		public Card GetPetCard(long id)
		{
			return this.PetCards.FirstOrDefault(a => a.Id == id);
		}

		/// <summary>
		/// Returns gift with id, or null if it doesn't exist.
		/// </summary>
		public Gift GetGift(long id)
		{
			return this.Gifts.FirstOrDefault(a => a.Id == id);
		}

		/// <summary>
		/// Returns character with id, or null if it doesn't exist.
		/// </summary>
		public Character GetCharacter(long id)
		{
			return this.Characters.FirstOrDefault(a => a.Id == id);
		}

		/// <summary>
		/// Returns pet/partner with id, or null if it doesn't exist.
		/// </summary>
		public Character GetPet(long id)
		{
			return this.Pets.FirstOrDefault(a => a.Id == id);
		}

		/// <summary>
		/// Creates new character for this account. Returns true if successful,
		/// character's ids are also set in that case.
		/// </summary>
		/// <param name="character"></param>
		/// <returns></returns>
		public bool CreateCharacter(Character character, CharCardInfo cardInfo)
		{
			// Create start items for card and hair/face
			var cardItems = AuraData.CharCardSetDb.Find(cardInfo.SetId, character.Race);

			var items = this.CardItemsToItems(cardItems);
			this.GenerateItemColors(ref items, (this.Name + character.Race + character.SkinColor + character.Hair + character.HairColor + character.Age + character.EyeType + character.EyeColor + character.MouthType + character.Face));

			items.Add(new Item(character.Face, Pocket.Face, character.SkinColor, 0, 0));
			items.Add(new Item(character.Hair, Pocket.Hair, character.HairColor + 0x10000000u, 0, 0));

			// Start keywords
			var keywords = new List<ushort>() { 1, 2, 3, 37, 38 };

			// Start skills
			var skills = new List<Skill>();
			skills.Add(new Skill(SkillId.MeleeCombatMastery, SkillRank.RF));
			skills.Add(new Skill(SkillId.HiddenEnchant));
			skills.Add(new Skill(SkillId.HiddenResurrection));
			skills.Add(new Skill(SkillId.HiddenTownBack));
			skills.Add(new Skill(SkillId.HiddenGuildStoneSetting));
			skills.Add(new Skill(SkillId.HiddenBlessing));
			skills.Add(new Skill(SkillId.CampfireKit));
			skills.Add(new Skill(SkillId.SkillUntrainKit));
			skills.Add(new Skill(SkillId.BigBlessingWaterKit));
			skills.Add(new Skill(SkillId.Dye));
			skills.Add(new Skill(SkillId.EnchantElementalAllSlot));
			skills.Add(new Skill(SkillId.HiddenPoison));
			skills.Add(new Skill(SkillId.HiddenBomb));
			skills.Add(new Skill(SkillId.FossilRestoration));
			skills.Add(new Skill(SkillId.SeesawJump));
			skills.Add(new Skill(SkillId.SeesawCreate));
			skills.Add(new Skill(SkillId.DragonSupport));
			skills.Add(new Skill(SkillId.IceMine));
			skills.Add(new Skill(SkillId.Scan));
			skills.Add(new Skill(SkillId.UseSupportItem));
			skills.Add(new Skill(SkillId.TickingQuizBomb));
			skills.Add(new Skill(SkillId.ItemSeal));
			skills.Add(new Skill(SkillId.ItemUnseal));
			skills.Add(new Skill(SkillId.ItemDungeonPass));
			skills.Add(new Skill(SkillId.UseElathaItem));
			skills.Add(new Skill(SkillId.UseMorrighansFeather));
			skills.Add(new Skill(SkillId.PetBuffing));
			skills.Add(new Skill(SkillId.CherryTreeKit));
			skills.Add(new Skill(SkillId.ThrowConfetti));
			skills.Add(new Skill(SkillId.UsePartyPopper));
			skills.Add(new Skill(SkillId.HammerGame));
			skills.Add(new Skill(SkillId.SpiritShift));
			skills.Add(new Skill(SkillId.EmergencyEscapeBomb));
			skills.Add(new Skill(SkillId.EmergencyIceBomb));
			skills.Add(new Skill(SkillId.NameColorChange));
			skills.Add(new Skill(SkillId.HolyFlame));
			skills.Add(new Skill(SkillId.CreateFaliasPortal));
			skills.Add(new Skill(SkillId.UseItemChattingColorChange));
			skills.Add(new Skill(SkillId.InstallPrivateFarmFacility));
			skills.Add(new Skill(SkillId.ReorientHomesteadbuilding));
			skills.Add(new Skill(SkillId.GachaponSynthesis));
			skills.Add(new Skill(SkillId.MakeChocoStatue));
			skills.Add(new Skill(SkillId.Paint));
			skills.Add(new Skill(SkillId.MixPaint));
			skills.Add(new Skill(SkillId.PetSealToItem));
			skills.Add(new Skill(SkillId.FlownHotAirBalloon));
			skills.Add(new Skill(SkillId.ItemSeal2));
			skills.Add(new Skill(SkillId.CureZombie));
			skills.Add(new Skill(SkillId.ContinentWarp));
			skills.Add(new Skill(SkillId.AddSeasoning));

			if (!LoginDb.Instance.CreateCharacter(this.Name, character, items, keywords, skills))
				return false;

			this.Characters.Add(character);

			return true;
		}

		/// <summary>
		/// Creates new pet for this account. Returns true if successful,
		/// pet's ids are also set in that case.
		/// </summary>
		/// <param name="pet"></param>
		/// <returns></returns>
		public bool CreatePet(Character pet)
		{
			// Start skills
			var skills = new List<Skill>();
			skills.Add(new Skill(SkillId.MeleeCombatMastery, SkillRank.RF));

			if (!LoginDb.Instance.CreatePet(this.Name, pet, skills))
				return false;

			this.Pets.Add(pet);

			return true;
		}

		/// <summary>
		/// Creates new partner for this account. Returns true if successful,
		/// pet's ids are also set in that case.
		/// </summary>
		/// <param name="partner"></param>
		/// <returns></returns>
		public bool CreatePartner(Character partner)
		{
			int setId = 0;
			if (partner.Race == 730201 || partner.Race == 730202 || partner.Race == 730204 || partner.Race == 730205)
				setId = 1000;
			else if (partner.Race == 730203)
				setId = 1001;
			else if (partner.Race == 730206)
				setId = 1002;

			// Create start items for card and hair/face
			var cardItems = AuraData.CharCardSetDb.Find(setId, partner.Race);

			// TODO: Hash seems to be incorrect.
			var items = this.CardItemsToItems(cardItems);
			this.GenerateItemColors(ref items, (this.Name + partner.Race + partner.SkinColor + partner.Hair + partner.HairColor + 1 + partner.EyeType + partner.EyeColor + partner.MouthType + partner.Face));

			items.Add(new Item(partner.Face, Pocket.Face, partner.SkinColor, 0, 0));
			items.Add(new Item(partner.Hair, Pocket.Hair, partner.HairColor + 0x10000000u, 0, 0));

			// Start skills
			var skills = new List<Skill>();
			skills.Add(new Skill(SkillId.MeleeCombatMastery, SkillRank.RF));

			if (!LoginDb.Instance.CreatePartner(this.Name, partner, items, skills))
				return false;

			this.Pets.Add(partner);

			return true;
		}

		/// <summary>
		/// Returns list of items, based on CharCardSetInfo list.
		/// </summary>
		/// <param name="cardItems"></param>
		/// <returns></returns>
		private List<Item> CardItemsToItems(List<CharCardSetInfo> cardItems)
		{
			var result = new List<Item>();

			foreach (var cardItem in cardItems)
			{
				result.Add(new Item(cardItem.Class, (Pocket)cardItem.Pocket, cardItem.Color1, cardItem.Color2, cardItem.Color3));
			}

			return result;
		}

		/// <summary>
		/// Changes item colors, using MTRandom and hash.
		/// </summary>
		/// <remarks>
		/// The hash is converted into an int, which is used as seed for
		/// MTRandom, the RNG Mabi is using. That is used to get specific
		/// "random" colors from the color map db.
		/// 
		/// Used to generate "random" colors on the equipment of
		/// new characters and partners.
		/// </remarks>
		private void GenerateItemColors(ref List<Item> items, string hash)
		{
			int ihash = 5381;
			foreach (var ch in hash)
				ihash = ihash * 33 + (int)ch;

			var rnd = new MTRandom(ihash);
			foreach (var item in items.Where(a => a.Info.Pocket != Pocket.Face && a.Info.Pocket != Pocket.Hair))
			{
				var dataInfo = AuraData.ItemDb.Find(item.Info.Class);
				if (dataInfo == null)
					continue;

				item.Info.Color1 = (item.Info.Color1 != 0 ? item.Info.Color1 : AuraData.ColorMapDb.GetRandom(dataInfo.ColorMap1, rnd));
				item.Info.Color2 = (item.Info.Color2 != 0 ? item.Info.Color2 : AuraData.ColorMapDb.GetRandom(dataInfo.ColorMap2, rnd));
				item.Info.Color3 = (item.Info.Color3 != 0 ? item.Info.Color3 : AuraData.ColorMapDb.GetRandom(dataInfo.ColorMap3, rnd));
			}
		}

		/// <summary>
		/// Deletes character card from account.
		/// </summary>
		/// <param name="cardId"></param>
		public bool DeleteCharacterCard(Card card)
		{
			if (!LoginDb.Instance.DeleteCard(card))
				return false;

			this.CharacterCards.Remove(card);

			return true;
		}

		/// <summary>
		/// Deletes pet card from account.
		/// </summary>
		/// <param name="cardId"></param>
		public bool DeletePetCard(Card card)
		{
			if (!LoginDb.Instance.DeleteCard(card))
				return false;

			this.PetCards.Remove(card);

			return true;
		}

		/// <summary>
		/// Changes gift to an ordinary card.
		/// </summary>
		/// <param name="gift"></param>
		public void ChangeGiftToCard(Gift gift)
		{
			this.Gifts.Remove(gift);

			if (gift.IsCharacter)
				this.CharacterCards.Add(gift);
			else
				this.PetCards.Add(gift);

			LoginDb.Instance.ChangeGiftToCard(gift.Id);
		}

		/// <summary>
		/// Deletes gift from account.
		/// </summary>
		/// <param name="gift"></param>
		public void DeleteGift(Gift gift)
		{
			LoginDb.Instance.DeleteCard(gift);
			this.Gifts.Remove(gift);
		}
	}
}
