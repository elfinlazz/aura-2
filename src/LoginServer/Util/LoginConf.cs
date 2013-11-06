// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Shared.Util;

namespace Aura.Login.Util
{
	public class LoginConf : BaseConf
	{
		// Login
		public int Port;
		public bool NewAccounts;
		public bool EnableSecondaryPassword;

		public bool ConsumeCharacterCards;
		public bool ConsumePetCards;
		public bool ConsumePartnerCards;

		public int DeletionWait;

		public override void Load()
		{
			this.LoadDefault();
			this.LoadLogin();
		}

		public void LoadLogin()
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
