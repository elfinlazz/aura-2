// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Shared.Util;
using Aura.Shared.Util.Configuration;

namespace Aura.Login.Util
{
	public class LoginConf : BaseConf
	{
		/// <summary>
		/// login.conf
		/// </summary>
		public LoginConfFile Login { get; protected set; }

		public LoginConf()
		{
			this.Login = new LoginConfFile();
		}

		public override void Load()
		{
			this.LoadDefault();
			this.Login.Load();
		}
	}

	/// <summary>
	/// Represents login.conf
	/// </summary>
	public class LoginConfFile : ConfFile
	{
		public int Port { get; protected set; }
		public bool NewAccounts { get; protected set; }
		public bool EnableSecondaryPassword { get; protected set; }

		public bool ConsumeCharacterCards { get; protected set; }
		public bool ConsumePetCards { get; protected set; }
		public bool ConsumePartnerCards { get; protected set; }

		public int DeletionWait { get; protected set; }

		public void Load()
		{
			this.RequireAndInclude("{0}/conf/login.conf", "system", "user");

			this.Port = this.GetInt("login.port", 11000);
			this.NewAccounts = this.GetBool("login.new_accounts", true);
			this.EnableSecondaryPassword = this.GetBool("login.enable_secondary", false);

			this.ConsumeCharacterCards = this.GetBool("login.consume_character_cards", true);
			this.ConsumePetCards = this.GetBool("login.consume_pet_cards", true);
			this.ConsumePartnerCards = this.GetBool("login.consume_partner_cards", true);

			this.DeletionWait = this.GetInt("login.deletion_wait", 107);
		}
	}
}
