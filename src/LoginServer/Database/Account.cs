// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

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
			return this.Characters.FirstOrDefault(a => a.EntityId == id);
		}

		/// <summary>
		/// Returns pet/partner with id, or null if it doesn't exist.
		/// </summary>
		public Character GetPet(long id)
		{
			return this.Pets.FirstOrDefault(a => a.EntityId == id);
		}

		/// <summary>
		/// Creates new character for this account. Returns true if successful,
		/// character's ids are also set in that case.
		/// </summary>
		/// <param name="character"></param>
		/// <param name="cardInfo"></param>
		/// <returns></returns>
		public bool CreateCharacter(Character character, CharCardData cardInfo)
		{
			// Create start items for card and hair/face
			var cardItems = AuraData.CharCardSetDb.Find(cardInfo.SetId, character.Race);

			var items = this.CardItemsToItems(cardItems);
			this.GenerateItemColors(ref items, (this.Name + character.Race + character.SkinColor + character.Hair + character.HairColor + character.Age + character.EyeType + character.EyeColor + character.MouthType + character.Face));

			items.Add(new Item(character.Face, Pocket.Face, character.SkinColor, 0, 0));
			items.Add(new Item(character.Hair, Pocket.Hair, character.HairColor + 0x10000000u, 0, 0));

			if (!LoginServer.Instance.Database.CreateCharacter(this.Name, character, items))
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
			if (!LoginServer.Instance.Database.CreatePet(this.Name, pet))
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

			if (!LoginServer.Instance.Database.CreatePartner(this.Name, partner, items))
				return false;

			this.Pets.Add(partner);

			return true;
		}

		/// <summary>
		/// Returns list of items, based on CharCardSetInfo list.
		/// </summary>
		/// <param name="cardItems"></param>
		/// <returns></returns>
		private List<Item> CardItemsToItems(IEnumerable<CharCardSetData> cardItems)
		{
			return cardItems.Select(cardItem => new Item(cardItem.Class, (Pocket)cardItem.Pocket, cardItem.Color1, cardItem.Color2, cardItem.Color3)).ToList();
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
			var ihash = hash.Aggregate(5381, (current, ch) => current * 33 + (int)ch);

			var rnd = new MTRandom(ihash);
			foreach (var item in items.Where(a => a.Info.Pocket != Pocket.Face && a.Info.Pocket != Pocket.Hair))
			{
				var dataInfo = AuraData.ItemDb.Find(item.Info.Id);
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
		/// <param name="card"></param>
		public bool DeleteCharacterCard(Card card)
		{
			if (!LoginServer.Instance.Database.DeleteCard(card))
				return false;

			this.CharacterCards.Remove(card);

			return true;
		}

		/// <summary>
		/// Deletes pet card from account.
		/// </summary>
		/// <param name="card"></param>
		public bool DeletePetCard(Card card)
		{
			if (!LoginServer.Instance.Database.DeleteCard(card))
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

			LoginServer.Instance.Database.ChangeGiftToCard(gift.Id);
		}

		/// <summary>
		/// Deletes gift from account.
		/// </summary>
		/// <param name="gift"></param>
		public void DeleteGift(Gift gift)
		{
			LoginServer.Instance.Database.DeleteCard(gift);
			this.Gifts.Remove(gift);
		}
	}
}
