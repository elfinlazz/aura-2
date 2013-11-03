// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

namespace Aura.Shared.Database
{
	/// <summary>
	/// Represents an entry in card table.
	/// </summary>
	public class Card
	{
		/// <summary>
		/// Card id
		/// </summary>
		public long Id { get; set; }

		/// <summary>
		/// Card type
		/// </summary>
		/// <remarks>
		/// For character cards this is the chard card id.
		/// Pets are using different numbers, we default this to 102
		/// for pets and partners.
		/// </remarks>
		public int Type { get; set; }

		/// <summary>
		/// Race for this card
		/// </summary>
		/// <remarks>
		/// Race of the pet/partner, or 0 for char cards.
		/// </remarks>
		public int Race { get; set; }

		public Card()
		{ }

		public Card(int type, int race)
		{
			this.Type = type;
			this.Race = race;
		}

		public Card(long id, int type, int race)
			: this(type, race)
		{
			this.Id = id;
		}
	}
}
