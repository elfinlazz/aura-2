// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Channel.World.Entities;

namespace Aura.Channel.Database
{
	public class Account
	{
		public string Id { get; set; }
		public long SessionKey { get; set; }

		public int Authority { get; set; }

		public List<Character> Characters { get; set; }
		public List<Pet> Pets { get; set; }

		public Account()
		{
			this.Characters = new List<Character>();
			this.Pets = new List<Pet>();
		}

		public Character GetCharacter(long entityId)
		{
			return this.Characters.FirstOrDefault(a => a.EntityId == entityId);
		}
	}
}
