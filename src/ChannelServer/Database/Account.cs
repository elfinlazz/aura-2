// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Channel.Util;
using Aura.Channel.World.Entities;
using Aura.Channel.Scripting;
using Aura.Channel.World.Inventory;

namespace Aura.Channel.Database
{
	public class Account
	{
		public string Id { get; set; }
		public long SessionKey { get; set; }

		public int Authority { get; set; }

		public DateTime LastLogin { get; set; }

		public string BanReason { get; set; }
		public DateTime BanExpiration { get; set; }

		public List<Character> Characters { get; set; }
		public List<Pet> Pets { get; set; }

		public ScriptVariables Vars { get; protected set; }

		private int _autobanScore;
		private int _autobanCount;

		/// <summary>
		/// Account wide bank.
		/// </summary>
		public BankInventory Bank { get; protected set; }

		/// <summary>
		/// Account's current Autoban score. Don't set this directly
		/// as Autoban takes care of it.
		/// </summary>
		public int AutobanScore
		{
			get
			{
				return _autobanScore;
			}
			internal set
			{
				if (value < 0)
					value = 0;

				_autobanScore = value;
			}
		}

		/// <summary>
		/// Account's current Autoban count. Don't set this directly
		/// as Autoban takes care of it.
		/// </summary>
		public int AutobanCount
		{
			get
			{
				return _autobanCount;
			}
			internal set
			{
				if (value < 0)
					value = 0;

				_autobanCount = value;
			}
		}

		/// <summary>
		/// Last time this account had its autoban score reduced.
		/// Don't set this directly, as Autoban takes care of it.
		/// </summary>
		public DateTime LastAutobanReduction { get; internal set; }

		public Account()
		{
			this.Characters = new List<Character>();
			this.Pets = new List<Pet>();
			this.Vars = new ScriptVariables();
			this.Bank = new BankInventory();

			this.LastLogin = DateTime.Now;
		}

		public PlayerCreature GetCharacterOrPet(long entityId)
		{
			PlayerCreature result = this.Characters.FirstOrDefault(a => a.EntityId == entityId);
			if (result == null)
				result = this.Pets.FirstOrDefault(a => a.EntityId == entityId);
			return result;
		}

		public PlayerCreature GetCharacterOrPetSafe(long entityId)
		{
			var r = this.GetCharacterOrPet(entityId);
			if (r == null)
				throw new SevereViolation("Account doesn't contain character 0x{0:X}", entityId);

			return r;
		}

		public Pet GetPet(long entityId)
		{
			return this.Pets.FirstOrDefault(a => a.EntityId == entityId);
		}

		public Pet GetPetSafe(long entityId)
		{
			var r = this.GetPet(entityId);
			if (r == null)
				throw new SevereViolation("Account doesn't contain pet 0x{0:X}", entityId);

			return r;
		}
	}
}
