// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using Aura.Shared.Database;

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
	}
}
