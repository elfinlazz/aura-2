// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence file in the main folder

namespace Aura.Shared.Database
{
	public class Card
	{
		public long Id { get; set; }
		public int Type { get; set; }
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
